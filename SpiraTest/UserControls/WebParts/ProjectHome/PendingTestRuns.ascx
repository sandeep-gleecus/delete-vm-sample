<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PendingTestRuns.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.PendingTestRuns" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:MessageBox runat="server" ID="msgPendingTestRuns" />
<tstsc:GridViewEx EnableViewState="false" id="grdSavedTestRuns" Runat="server" DataMember="TestRunsPending" SkinID="WidgetGrid">
	<HeaderStyle CssClass="SubHeader" />
	<Columns>
		<tstsc:TemplateFieldEx ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ImageUrl="Images/artifact-TestRun.svg" CssClass="w4 h4" AlternateText="Test Run" ID="imgIcon" runat="server" />
                <tstsc:LabelEx Runat="server" Tooltip='<%# ((TestRunsPending) Container.DataItem).Name %>' Text='<%# GlobalFunctions.TruncateName(((TestRunsPending) Container.DataItem).Name, 50) %>' ID="lblName" />
                <asp:Label ID="lblTestCount" Runat="server" Text='<%# ((TestRunsPending) Container.DataItem).CountNotRun %>' CssClass="badge"/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:BoundFieldEx DataField="Tester.FullName" HeaderText="<%$Resources:Fields,TesterId %>" MaxLength="20" HtmlEncode="false"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2"/>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False"  HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
			<HeaderTemplate>
                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,LastUpdated %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx Runat="server" Tooltip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME,  GlobalFunctions.LocalizeDate(((TestRunsPending) Container.DataItem).LastUpdateDate)) %>' Text='<%# String.Format(GlobalFunctions.FORMAT_DATE,  GlobalFunctions.LocalizeDate(((TestRunsPending) Container.DataItem).LastUpdateDate)) %>' ID="Label14" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" ItemStyle-HorizontalAlign="Center"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Progress %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:Equalizer ID="eqlProgress" runat="server" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx runat="server" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" ItemStyle-Wrap="false">
            <ItemTemplate>
				<div class="btn-group">
				    <tstsc:LinkButtonEx Runat="server" Authorized_Permission="ProjectAdmin" CommandName="DeletePending" CommandArgument='<%# ((TestRunsPending) Container.DataItem).TestRunsPendingId%>' Tooltip='<%# Resources.Buttons.Delete + " " + GlobalFunctions.DISPLAY_SINGLE_QUOTE +((TestRunsPending) Container.DataItem).Name + GlobalFunctions.DISPLAY_SINGLE_QUOTE %>' ID="btnDelete" CausesValidation="false" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,PendingTestRuns_DeleteConfirm %>">
                        <span class="fas fa-trash-alt"></span>
				    </tstsc:LinkButtonEx>
                    <tstsc:HyperLinkEx runat="server" Authorized_Permission="ProjectAdmin" NavigateUrl="javascript:void(0)" ClientScriptMethod='<%# "grdSavedTestRuns_showReassignDialog(" + ((TestRunsPending) Container.DataItem).ProjectId + "," + ((TestRunsPending) Container.DataItem).TestRunsPendingId + ")"%>' ToolTip="<%$Resources:Main,PendingTestRuns_Reassign%>"  CssClass="btn btn-default">
                        <span class="fas fa-users"></span>
                    </tstsc:HyperLinkEx>
				</div>
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:DialogBoxPanel ID="pnlReassignTestRuns" runat="server" Title="<%$Resources:Main,PendingTestRuns_Reassign%>" Modal="true" Width="500px">
    <p>
        <asp:Localize runat="server" Text="<%$Resources:Dialogs,PendingTestRuns_Reassign_Intro %>" />
    </p>
    <div class="Spacer"></div>
    <div class="form-group">
        <div class="DataLabel col-md-2">
            <tstsc:LabelEx ID="ddlAssigneeLabel" runat="server" AppendColon="true" Font-Bold="true" AssociatedControlID="ddlAssignee" Text="<%$Resources:Fields,TesterId %>" />
        </div>
        <div class="DataEntry col-md-10 data-entry-wide df items-center">
            <tstsc:DropDownUserList ID="ddlAssignee" runat="server" Width="200" DataValueField="UserId"
                DataTextField="FullName" NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>"
                NoValueItem="True" />
        </div>
    </div>
    <asp:HiddenField runat="server" ID="hdnTestRunsPendingId" />
    <div class="clearfix"></div>
    <p><br /></p>
    <div class="pull-right">
        <tstsc:ButtonEx runat="server" ID="btnReassignSubmit" SkinID="ButtonPrimary" Text="<%$Resources:Buttons,Assign %>" ClientScriptMethod="btnReassignSubmit_click()" />
		<tstsc:HyperLinkEx ID="btnCancel" runat="server" NavigateUrl="javascript:void(0)"
            ClientScriptServerControlId="pnlReassignTestRuns" ClientScriptMethod="close()" SkinID="ButtonDefault"
            Text="<%$Resources:Buttons,Cancel %>" />
    </div>
</tstsc:DialogBoxPanel>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>
        <asp:ServiceReference Path="~/Services/Ajax/UserService.svc" />
        <asp:ServiceReference Path="~/Services/Ajax/TestRunService.svc" />
    </Services>
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    function grdSavedTestRuns_showReassignDialog(projectId, testRunsPendingId)
    {
        //We need to load the list of users for the project that this test run belongs to
        $('#<%=hdnTestRunsPendingId.ClientID%>').val(testRunsPendingId);
        Inflectra.SpiraTest.Web.Services.Ajax.UserService.User_RetrieveActiveForProject(projectId, grdSavedTestRuns_showReassignDialog_success, grdSavedTestRuns_showReassignDialog_failure, { testRunsPendingId: testRunsPendingId });
    }
    function grdSavedTestRuns_showReassignDialog_success(users, context)
    {
        //Load users and display the dialog
        var pnlReassignTestRuns = $find('<%=pnlReassignTestRuns.ClientID%>');
        pnlReassignTestRuns.display();
        var ddlAssignee = $find('<%=ddlAssignee.ClientID%>');
        ddlAssignee.clearItems();
        ddlAssignee.set_dataSource(users);
        ddlAssignee.dataBind();
    }
    function grdSavedTestRuns_showReassignDialog_failure(ex, context)
    {
        //Display error
        globalFunctions.display_error($get('<%=msgPendingTestRuns.ClientID%>'), ex);
    }

	function btnReassignSubmit_click() {
		var testRunsPendingId = $('#<%=hdnTestRunsPendingId.ClientID%>').val();
		var ddlAssignee = $find('<%=ddlAssignee.ClientID%>');
        if (ddlAssignee.get_selectedItem() && ddlAssignee.get_selectedItem().get_value()) {
            var newAssigneeId = ddlAssignee.get_selectedItem().get_value();
			Inflectra.SpiraTest.Web.Services.Ajax.TestRunService.TestRun_ReassignPending(testRunsPendingId, newAssigneeId, btnReassignSubmit_click_success, btnReassignSubmit_click_failure, { testRunsPendingId: testRunsPendingId });
        }
        else {
			globalFunctions.globalAlert(resx.Global_SelectUserFromList, 'danger', true);
		}
    }
    function btnReassignSubmit_click_success() {
        //Close dialog box
		var pnlReassignTestRuns = $find('<%=pnlReassignTestRuns.ClientID%>');
        pnlReassignTestRuns.close();

        //Reload the page content
        var pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();
		pageRequestManager._doPostBack('', '');
    }
	function btnReassignSubmit_click_failure(ex, context) {
		//Display error
		globalFunctions.display_error($get('<%=msgPendingTestRuns.ClientID%>'), ex);
	}
</script>
