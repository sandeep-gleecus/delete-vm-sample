<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestCaseList.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.TestCaseList" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx EnableViewState="false" id="grdTestCases" Runat="server" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService">
	<Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" ItemStyle-CssClass="Icon"  HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ImageUrl="Images/artifact-TestCase.svg" AlternateText="Test Case" ID="imgIcon" runat="server" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
			<ItemTemplate>
				<tstsc:HyperLinkEx Runat="server" Text='<%# GlobalFunctions.TruncateName(((TestCaseView) Container.DataItem).Name, 60) %>' ID="lnkViewTest" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:BoundFieldEx DataField="ProjectName" HeaderText="<%$Resources:Fields,Project %>" MaxLength="20" HtmlEncode="false" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" />
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Status %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx ID="lblExecutionStatus" Runat="server"
                    Text='<%# GlobalFunctions.LocalizeFields("TestCase_ExecutionStatus_" + ((TestCaseView) Container.DataItem).ExecutionStatusId) %>'
                    ToolTip='<%# GlobalFunctions.LocalizeFields("TestCase_ExecutionStatus_" + ((TestCaseView) Container.DataItem).ExecutionStatusId) + " - " + ((((TestCaseView)Container.DataItem).ExecutionDate.HasValue) ? String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(((TestCaseView)Container.DataItem).ExecutionDate.Value)) : "") %>'/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,LastExecuted %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx runat="server" Text='<%# (((TestCaseView)Container.DataItem).ExecutionDate.HasValue) ? String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate((DateTime)((TestCaseView)Container.DataItem).ExecutionDate.Value)) : "-" %>'
					ToolTip='<%# (((TestCaseView)Container.DataItem).ExecutionDate.HasValue) ? String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(((TestCaseView)Container.DataItem).ExecutionDate.Value)) : "-" %>'
					ID="lblExecutionDate" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:BoundFieldEx DataField="TestCaseStatusName" HeaderText="<%$Resources:Fields,Workflow %>" HtmlEncode="false" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" />
        <tstsc:TemplateFieldEx runat="server" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
            <ItemTemplate>
                <tstsc:HyperLinkEx 
                    Runat="server" 
                    NavigateUrl='<%# "javascript:grdTestCases_execute(" + ((TestCaseView) Container.DataItem).ProjectId + "," + ((TestCaseView) Container.DataItem).TestCaseId + ")" %>' 
                    Tooltip='<%# Resources.Buttons.Execute + " " + GlobalFunctions.DISPLAY_SINGLE_QUOTE + ((TestCaseView) Container.DataItem).Name + GlobalFunctions.DISPLAY_SINGLE_QUOTE%>' 
                    ID="btnExecuteTest" 
                    CssClass="btn btn-default"
                    >
                    <span class="fas fa-play"></span>
                </tstsc:HyperLinkEx>
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:BackgroundProcessManager ID="ajxBackgroundProcessManager" runat="server" ErrorMessageControlClientId="<%#this.MessageBoxClientID %>"
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />
        <asp:ServiceReference Path="~/Services/Ajax/TestRunService.svc" />
        <asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
    </Services>
    <Scripts>
        <asp:ScriptReference Path="~/TypeScript/rct_comp_testRunsPendingExecuteNewOrExisting.js" />
    </Scripts>
</tstsc:ScriptManagerProxyEx>
<script type="text/javascript">
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    var grdTestCases_projectId = 0;
    var grdTestCases_testCaseId = 0;

    function grdTestCases_execute(projectId, testCaseId)
    {

        //verify we have one project and one test case specified
        if (projectId && testCaseId) {
            //store the information on the page for later retrieval
            grdTestCases_projectId = projectId;
            grdTestCases_testCaseId = testCaseId;

            //First check if the user has an existing testrunpending for this test case
            Inflectra.SpiraTest.Web.Services.Ajax.TestRunService.RetrievePendingByUserIdAndTestCase(
                projectId,
                testCaseId,
                AspNetAjax$Function.createDelegate(this, this.retrieveExistingPending_success),
                AspNetAjax$Function.createDelegate(this, this.execute_test_case_process)
            );
        }
    }
    function retrieveExistingPending_success(data) {
        if (data && data.length) {
            // make sure the message dialog is clear then render
            globalFunctions.dlgGlobalDynamicClear();
            ReactDOM.render(
                React.createElement(RctTestRunsPendingExecuteNewOrExisting, {
                    data: data,
                    newTestName: resx.TestCaseList_ExecuteTestCase,
                    executeFunction: this.execute_test_case_process
                }, null),
                document.getElementById('dlgGlobalDynamic')
            );
        }
        else {
            execute_test_case_process();
        }
    }
    function execute_test_case_process(testRunsPendingId)
    {
        //store the test case ids in function so we can clear the global var (safety)
        var projectId = grdTestCases_projectId;
        var testCaseId = grdTestCases_testCaseId;
        grdTestCases_testCaseId = 0;
        grdTestCases_projectId = 0;

        //Actually start the background process of creating the test runs
        if (!testRunsPendingId)
        {
            var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
            ajxBackgroundProcessManager.display(projectId, 'TestCase_Execute', resx.TestCaseList_ExecuteTestCase, resx.TestCaseList_ExecuteTestCaseDesc, testCaseId);
        }
        else
        {
            window.open(globalFunctions.getArtifactDefaultUrl(SpiraContext.BaseUrl, SpiraContext.ProjectId, "TestExecute", testRunsPendingId), "_self");
        }
    }



    function <%=WebPartUniqueId%>_ajxBackgroundProcessManager_success(msg, returnCode)
    {
        //Need to redirect to the test runs pending
        if (returnCode && returnCode > 0)
        {
            var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
            var projectId = ajxBackgroundProcessManager.get_projectId();
            var baseUrl = msg === "testcase_executeexploratory" ? '<%=TestRunsPendingExploratoryUrl %>' : '<%=TestRunsPendingUrl %>';
            var url = baseUrl.replace(globalFunctions.artifactIdToken, returnCode).replace(globalFunctions.projectIdToken, projectId);
            window.location = url;
        }
    }
</script>
