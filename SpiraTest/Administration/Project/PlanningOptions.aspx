<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="PlanningOptions.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.PlanningOptions" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">

    <h2>
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_Title %>" />
        <small>
                <tstsc:HyperLinkEx 
                ID="lnkAdminHome" 
                runat="server" 
                Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                >
                <tstsc:LabelEx ID="lblProjectName" runat="server" />
			</tstsc:HyperLinkEx>
        </small>
    </h2>


    <p class="my3">
        <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_String1 %>" />
    </p>
    <p class="my3">
        <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_String2 %>" />
        <tstsc:HyperLinkEx ID="lnkDataCaching1" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_DataCaching %>" />
        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_String3 %>" />
    </p>
    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
    <asp:ValidationSummary ID="vldSummary" runat="server" />



    <section class="u-wrapper width_md mt5">
        <div class="u-box_2">
            <%-- GENERAL --%>
            <div 
                class="u-box_group"
                data-collapsible="true"
                id="form-group_admin-product-planning-general" >
                <div 
                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                    aria-expanded="true">
                    <asp:Localize 
                        runat="server" 
                        Text="<%$Resources:Main,Admin_PlanningOptions_General %>" />
                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                </div>
                <ul class="u-box_list mt4">
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" ID="txtWorkingHoursPerDayLabel" AssociatedControlID="txtWorkingHoursPerDay" Required="true"
                            AppendColon="true" Text="<%$Resources:Main,Admin_PlanningOptions_WorkHoursPerDay %>" />
                        <span>
                            <tstsc:UnityTextBoxEx runat="server" CssClass="u-input mw6" DisabledCssClass="u-input w6 disabled" ID="txtWorkingHoursPerDay" MaxLength="2"/>
                            <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,Global_Hours %>" />
                            <asp:RequiredFieldValidator ControlToValidate="txtWorkingHoursPerDay" ErrorMessage="<%$Resources:Messages,Admin_PlanningOptions_WorkHoursPerDayRequired %>"
                                Text="*" runat="server" ID="vldWorkingHoursPerDay1" />
                            <asp:RegularExpressionValidator ControlToValidate="txtWorkingHoursPerDay" ErrorMessage="<%$Resources:Messages,Admin_PlanningOptions_WorkHoursPerDayNotInteger %>"
                                Text="*" runat="server" ID="vldWorkingHoursPerDay2" ValidationExpression='<%$GlobalFunctions:VALIDATION_REGEX_INTEGER%>' />
                        </span>
                    </li>
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" ID="txtWorkingDaysPerWeekLabel" AssociatedControlID="txtWorkingDaysPerWeek" Required="true" Text="<%$Resources:Main,Admin_PlanningOptions_WorkDaysPerWeek %>" AppendColon="true" />
                        <span>
                            <tstsc:UnityTextBoxEx runat="server" CssClass="u-input mw6" DisabledCssClass="u-input w6 disabled" ID="txtWorkingDaysPerWeek" MaxLength="1" />
                            <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Main,Global_Days %>" />
                            <asp:RequiredFieldValidator ControlToValidate="txtWorkingDaysPerWeek" ErrorMessage="<%$Resources:Messages,Admin_PlanningOptions_WorkDaysPerWeekRequired %>"
                                Text="*" runat="server" ID="RequiredFieldValidator9" />
                            <asp:RegularExpressionValidator ControlToValidate="txtWorkingDaysPerWeek" ErrorMessage="<%$Resources:Messages,Admin_PlanningOptions_WorkDaysPerWeekNotInteger %>"
                                Text="*" runat="server" ID="RegularExpressionValidator2" ValidationExpression='<%$GlobalFunctions:VALIDATION_REGEX_INTEGER%>' />
                        </span>
                    </li>
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" ID="txtNonWorkingHoursPerMonthLabel" AssociatedControlID="txtNonWorkingHoursPerMonth" Required="true" Text="<%$Resources:Main,Admin_PlanningOptions_NonWorkingHoursPerMonth %>" AppendColon="true" />
                        <span>
                            <tstsc:UnityTextBoxEx runat="server" CssClass="u-input mw6" DisabledCssClass="u-input w6 disabled" ID="txtNonWorkingHoursPerMonth" MaxLength="4" />
                            <asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Main,Global_Hours %>" />
                            <asp:RequiredFieldValidator ControlToValidate="txtNonWorkingHoursPerMonth" ErrorMessage="<%$Resources:Messages,Admin_PlanningOptions_NonWorkingHoursPerMonthRequired %>"
                                Text="*" runat="server" ID="RequiredFieldValidator10" />
                            <asp:RegularExpressionValidator ControlToValidate="txtNonWorkingHoursPerMonth" ErrorMessage="<%$Resources:Messages,Admin_PlanningOptions_NonWorkingHoursPerMonthNotInteger %>"
                                Text="*" runat="server" ID="RegularExpressionValidator3" ValidationExpression='<%$GlobalFunctions:VALIDATION_REGEX_INTEGER%>' />
                        </span>
                    </li>
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_EffortCalculations %>" ID="chkEffortTasksLabel" AssociatedControlID="chkEffortTasks" Required="true" AppendColon="true" />
                        <span class="u-checkbox-wrapper color-inherit">
                            <p>
                                <tstsc:CheckBoxEx ID="chkEffortTasks" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_IncludeTaskEffort %>" />
                            </p>
                            <p>
                                <tstsc:CheckBoxEx ID="chkEffortIncidents" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_IncludeIncidentEffort %>" />
                            </p>
                            <p>
                                <tstsc:CheckBoxEx ID="chkEffortTestCases" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_IncludeTestCaseEffort %>" />
                            </p>
                        </span>
                    </li>
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_DetectedRelease %>" ID="LabelEx1" AssociatedControlID="chkDetectedReleaseActiveOnly" Required="true" AppendColon="true" />
                        <span class="u-checkbox-wrapper color-inherit">
                            <tstsc:CheckBoxEx ID="chkDetectedReleaseActiveOnly" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_DetectedRelease_ActiveOnly %>" />
                        </span>
                    </li>
                </ul>
            </div>
        </div>


        <div class="u-box_2 mt5">
            <%-- REQUIREMENTS --%>
            <div 
                class="u-box_group"
                data-collapsible="true"
                id="form-group_admin-product-planning-requirements" >
                <div 
                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                    aria-expanded="true">
                    <asp:Localize 
                        runat="server" 
                        Text="<%$Resources:Main,Admin_PlanningOptions_Requirements %>" />
                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                </div>
                <ul class="u-box_list mt4">
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" ID="chkPlanUsingPointsLabel" AssociatedControlID="chkPlanUsingPoints" Required="true" Text="<%$Resources:Main,Admin_PlanningOptions_ReqPlanUsingPoints %>" AppendColon="true" />
                        <span>
                            <tstsc:CheckBoxYnEx ID="chkPlanUsingPoints" runat="server" />
                            <p class="Notes">
                                <asp:Localize runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_ReqPlanUsingPointsNotes1 %>" />
                            </p>                            
                            <p class="Notes">
                                <asp:Localize runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_ReqPlanUsingPointsNotes2 %>" />
                            </p>                            
                        </span>
                    </li>
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_DefaultEstimate %>" ID="txtReqDefaultEstimateLabel" AssociatedControlID="txtReqDefaultEstimate" 
                            Required="false" AppendColon="true" />
                        <span>
						    <tstsc:UnityTextBoxEx runat="server" CssClass="u-input mw6" DisabledCssClass="u-input w6 disabled" ID="txtReqDefaultEstimate" />
                            <asp:Localize ID="Localize13" runat="server" Text="<%$Resources:Main,Global_Points %>" />
                            <asp:RegularExpressionValidator ControlToValidate="txtReqDefaultEstimate" ErrorMessage="<%$Resources:Messages,PlanningOptions_DefaultEstimateInvalid %>"
                                Text="*" runat="server" ID="RegularExpressionValidator1" ValidationExpression='<%$GlobalFunctions:VALIDATION_REGEX_ESTIMATE_POINTS%>' />
                        </span>
                    </li>
                    <asp:PlaceHolder ID="plcPointEffort" runat="server">
                        <li class="ma0 pa0 mb3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_PointEffort %>" ID="txtPointEffortLabel" AssociatedControlID="txtPointEffort" 
                                Required="true" AppendColon="true" />
                            <span>
							    <tstsc:UnityTextBoxEx runat="server" CssClass="u-input mw6" DisabledCssClass="u-input w6 disabled" ID="txtPointEffort" />
                                <span class="mr3">
                                    <asp:Localize ID="Localize14" runat="server" Text="<%$Resources:Main,Global_Hours %>" />
                                </span>
                                <tstsc:ButtonEx ID="btnSuggestPointEffort" runat="server" Text="<%$Resources:Buttons,Suggest %>" ClientScriptMethod="display(event)" ClientScriptServerControlId="dlgSuggestedPointEffort" />
                                <asp:RegularExpressionValidator ControlToValidate="txtPointEffort" ErrorMessage="<%$Resources:Messages,PlanningOptions_PointEffortInvalid %>"
                                    Text="*" runat="server" ID="RegularExpressionValidator5" ValidationExpression='<%$GlobalFunctions:VALIDATION_REGEX_EFFORT_HOURS%>' />
                                <p class="Notes">
                                    <asp:Localize ID="Localize15" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_PointEffortNotes %>" />
                                </p>
                            </span>
                        </li>
                    </asp:PlaceHolder>
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" ID="chkAutoCreateTasksLabel" AssociatedControlID="chkAutoCreateTasks" Required="true" Text="<%$Resources:Main,Admin_PlanningOptions_AutoCreateTasks %>" AppendColon="true" />
                        <span class="u-checkbox-wrapper color-inherit">
                            <tstsc:CheckBoxEx ID="chkAutoCreateTasks" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_AutoCreateTasksLabel %>" />                            
                        </span>
                    </li>
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" ID="chkReqStatusAutoPlannedLabel" AssociatedControlID="chkReqStatusAutoPlanned" Required="true" Text="<%$Resources:Main,Admin_PlanningOptions_ReqStatusAutoPlanned %>" AppendColon="true" />
                        <span class="u-checkbox-wrapper color-inherit">
                            <tstsc:CheckBoxEx ID="chkReqStatusAutoPlanned" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_ReqStatusAutoPlannedLabel %>" />                            
                        </span>
                    </li>
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" ID="chkReqStatusChangedByTasksLabel" AssociatedControlID="chkReqStatusChangedByTasks" Required="true" Text="<%$Resources:Main,Admin_PlanningOptions_ReqStatusChangedByTasks %>" AppendColon="true" />
                        <span class="u-checkbox-wrapper color-inherit">
                            <tstsc:CheckBoxEx ID="chkReqStatusChangedByTasks" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_ReqStatusChangedByTasksLabel %>" />                            
                        </span>
                    </li>
                    <li class="ma0 pa0 mb3">
                        <tstsc:LabelEx runat="server" ID="chkReqStatusChangedByTestCaseLabel" AssociatedControlID="chkReqStatusChangedByTestCase" Required="true" Text="<%$Resources:Main,Admin_PlanningOptions_ReqStatusChangedByTestCase %>" AppendColon="true" />
                        <span class="u-checkbox-wrapper color-inherit">
                            <tstsc:CheckBoxEx ID="chkReqStatusChangedByTestCase" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_ReqStatusChangedByTestCaseLabel %>" />                            
                        </span>
                    </li>
                </ul>
            </div>
        </div>
        <asp:PlaceHolder ID="plcTasksIncidents" runat="server">
            <div class="u-box_2 mt5">
                <%-- TASKS and INCIDENTS --%>
                <div 
                    class="u-box_group"
                    data-collapsible="true"
                    id="form-group_admin-product-planning-tasksIncidents" >
                    <div 
                        class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                        aria-expanded="true">
                        <asp:Localize 
                            runat="server" 
                            Text="<%$Resources:Main,Admin_PlanningOptions_TasksIncidents %>" />
                        <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                    </div>
                    <ul class="u-box_list mt4">
                        <li class="ma0 pa0 mb3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_DefaultEffort %>" ID="txtTaskDefaultEffortLabel" AssociatedControlID="txtTaskDefaultEffort" 
                                Required="false" AppendColon="true" />    
                            <span>
							    <tstsc:UnityTextBoxEx runat="server" CssClass="u-input mw6" DisabledCssClass="u-input w6 disabled" ID="txtTaskDefaultEffort"/>
                                <asp:Localize ID="Localize9" runat="server" Text="<%$Resources:Main,Global_Hours %>" />
                                <asp:RegularExpressionValidator ControlToValidate="txtReqDefaultEstimate" ErrorMessage="<%$Resources:Messages,TaskDetails_EnterValidEstimatedEffort %>"
                                    Text="*" runat="server" ID="RegularExpressionValidator4" ValidationExpression='<%$GlobalFunctions:VALIDATION_REGEX_EFFORT_HOURS%>' />
                                <p class="Notes">
                                    <asp:Localize ID="Localize12" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_DefaultEffort_Tasks %>" />
                                </p>
                            </span>
                        </li>
                        <li class="ma0 pa0 mb3">
                            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_TimeTracking %>" ID="chkTimeTrackingTasksLabel" AssociatedControlID="chkTimeTrackingTasks" Required="true" AppendColon="true" />
                            <span class="u-checkbox-wrapper color-inherit">
                                <p><tstsc:CheckBoxEx ID="chkTimeTrackingTasks" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_EnabledForTasks %>" /></p>
                                <p><tstsc:CheckBoxEx ID="chkTimeTrackingIncidents" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_EnabledForIncidents %>" /></p>
                            </span>
                        </li>
                    </ul>
                </div>
            </div>
        </asp:PlaceHolder>
        <asp:PlaceHolder ID="plcKanbanWip" runat="server">
            <div class="u-box_2 mt5">
                <%-- WIP --%>
                <div 
                    class="u-box_group"
                    data-collapsible="true"
                    id="form-group_admin-product-planning-wip" >
                    <div 
                        class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                        aria-expanded="true">
                        <asp:Localize 
                            runat="server" 
                            Text="<%$Resources:Main,Admin_PlanningOptions_KanbanWip %>" />
                        <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                    </div>
                    <div class="u-box_item">
                        <p class="mt4">
                            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_KanbanWipIntro %>" />
                        </p>
                        <table class="DataGrid w-80">
                            <thead>
                                <tr class="Header">
                                    <th>
                                        <asp:Localize runat="server" Text="<%$Resources:Fields,Status %>" />
                                    </th>
                                    <th>
                                        <tstsc:ImageEx runat="server" ImageUrl="Images/artifact-Release.svg" CssClass="w4 h4 v-mid" />
                                        <span>
                                            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_KanbanWip_ReleaseWip %>" />
                                        </span>
                                    </th>
                                    <th>
                                        <tstsc:ImageEx runat="server" ImageUrl="Images/artifact-Iteration.svg" CssClass="w4 h4 v-mid" />
                                        <span>
                                            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_KanbanWip_IterationWip %>" />
                                        </span>
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater runat="server" ID="rptStatusWip">
                                    <ItemTemplate>
                                        <tr class="Normal">
                                            <td>
                                                <%#:((RequirementStatus)(Container.DataItem)).Name %>:
                                            </td>
                                            <td>
                                                <tstsc:TextBoxEx runat="server" ID="txtReleaseWipPercent" SkinID="NarrowControl" MetaData="<%#((RequirementStatus)(Container.DataItem)).RequirementStatusId %>" /> %
                                            </td>
                                            <td>
                                                <tstsc:TextBoxEx runat="server" ID="txtIterationWipPercent" SkinID="NarrowControl" MetaData="<%#((RequirementStatus)(Container.DataItem)).RequirementStatusId %>" /> %
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <tr class="Header">
                                    <th colspan="3"></th>
                                </tr>
                                <tr class="SubHeader">
                                    <th>
                                        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_KanbanWip_WipMultiplier %>" />:
                                    </th>
                                    <th>
                                        <tstsc:TextBoxEx runat="server" ID="txtReleaseWipMultiplier" SkinID="NarrowControl" />
                                        <span class="fas fa-times"></span> <span class="fas fa-users"></span>
                                    </th>
                                    <th>
                                        <tstsc:TextBoxEx runat="server" ID="txtIterationWipMultiplier" SkinID="NarrowControl" />
                                        <span class="fas fa-times"></span> <span class="fas fa-users"></span>
                                    </th>
                                </tr>
                            </tbody>
                        </table>
                        <p class="alert alert-warning">
                            <span class="fa fa-info-circle"></span>
                            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_KanbanWipNotes %>" />
                        </p>
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>


        <div class="u-box_3 mt4">
            <div class="btn-group">
                <tstsc:ButtonEx ID="btnUpdate" runat="server" Text="<%$Resources:Buttons,Save %>" CausesValidation="True" SkinID="ButtonPrimary"/>
                <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="false" />
            </div>
        </div>
    </section>
    


    <tstsc:DialogBoxPanel ID="dlgSuggestedPointEffort" runat="server" Title="<%$Resources:Dialogs,Admin_PlanningOptions_SuggestPointMetric %>" Modal="true" Width="400px">
        <p>
            <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_PlanningOptions_PointEffortSuggestExplanation %>" />:
        </p>
        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_DefaultEffort %>" ID="txtSuggestedPointEffortLabel" AssociatedControlID="txtSuggestedPointEffort" 
            Font-Bold="true" AppendColon="true" />    
        <tstsc:TextBoxEx runat="server" ID="txtSuggestedPointEffort" Width="100px" Enabled="false" />
        <asp:Localize runat="server" Text="<%$Resources:Main,Global_Hours %>" />
        <p style="margin-top: 1rem;">
            <tstsc:ButtonEx ID="btnSuggestedPointApply" runat="server" SkinID="ButtonPrimary" Text="<%$Resources:Buttons,Apply %>" ClientScriptMethod="btnSuggestedPointApply_click()" />            
            <tstsc:ButtonEx ID="btnSuggestedPointCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" ClientScriptServerControlId="dlgSuggestedPointEffort" ClientScriptMethod="close()" />            
        </p>
    </tstsc:DialogBoxPanel>


    <script type="text/javascript">
        SpiraContext.HasCollapsiblePanels = true;

        function btnSuggestedPointApply_click()
        {
            //Get the new value
            var suggestedPointEffort = '<%=this.SuggestedPointEffortMetric %>';
            if (suggestedPointEffort && suggestedPointEffort != '')
            {
                var txtPointEffort = $get('<%=txtPointEffort.ClientID %>');
                if (txtPointEffort)
                {
                    txtPointEffort.value = suggestedPointEffort;
                }
            }

            //Close the dialog box
            var dlgSuggestedPointEffort = $find('<%=dlgSuggestedPointEffort.ClientID %>');
            dlgSuggestedPointEffort.close();
        }
    </script>
</asp:Content>
