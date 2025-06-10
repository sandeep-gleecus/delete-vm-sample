<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestSetList.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.TestSetList" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx EnableViewState="false" id="grdTestSets" Runat="server" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService">
    <Columns>
	    <tstsc:TemplateFieldEx ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
		    <HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Name %>" />
		    </HeaderTemplate>
		    <ItemTemplate>
			    <tstsc:ImageEx ImageUrl="Images/artifact-TestSet.svg" CssClass="w4 h4 priority1" AlternateText="Test Set" ID="imgIcon" runat="server" />
			    <tstsc:HyperLinkEx Runat="server" Text='<%# GlobalFunctions.TruncateName(((TestSetView) Container.DataItem).Name, 60) %>' ID="lnkViewTestSet" />
			    <asp:Label ID="lblTestCount" Runat="server" Text='<%# ((TestSetView) Container.DataItem).CountNotRun %>' CssClass="badge"/>
		    </ItemTemplate>
	    </tstsc:TemplateFieldEx>
	    <tstsc:BoundFieldEx DataField="ProjectName" HeaderText="<%$Resources:Fields,Project %>" MaxLength="20" HtmlEncode="false" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" />
	    <tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False" ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
		    <HeaderTemplate>
				<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,DueDate %>" />
		    </HeaderTemplate>
		    <ItemTemplate>
		        <tstsc:LabelEx Runat="server" Tooltip='<%# (((TestSetView) Container.DataItem).PlannedDate.HasValue) ? String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(((TestSetView) Container.DataItem).PlannedDate.Value)) : "-" %>'
                    Text='<%# (((TestSetView) Container.DataItem).PlannedDate.HasValue) ? String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(((TestSetView) Container.DataItem).PlannedDate)) : "-" %>' ID="lblPlannedDate"/>
		    </ItemTemplate>
	    </tstsc:TemplateFieldEx>
	    <tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
		    <HeaderTemplate>
				<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,Status %>" />
		    </HeaderTemplate>
		    <ItemTemplate>
			    <asp:Label ID="Label16" Runat="server" Text='<%# GlobalFunctions.LocalizeFields("TestSet_Status_" + ((TestSetView) Container.DataItem).TestSetStatusId) %>'/>
		    </ItemTemplate>
	    </tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx runat="server" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
            <ItemTemplate>
                <tstsc:HyperLinkEx ID="lnkExecuteTestSet" Runat="server" NavigateUrl='<%# "javascript:grdTestSets_execute(" + ((TestSetView) Container.DataItem).ProjectId + "," + ((TestSetView) Container.DataItem).TestSetId + ")" %>' Tooltip='<%# Resources.Buttons.Execute + " " + GlobalFunctions.DISPLAY_SINGLE_QUOTE + ((TestSetView) Container.DataItem).Name + GlobalFunctions.DISPLAY_SINGLE_QUOTE%>' CssClass="btn btn-default">
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
    <asp:ServiceReference Path="~/Services/Ajax/TestSetService.svc" />  
    <asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
  </Services>  
</tstsc:ScriptManagerProxyEx>
<script type="text/javascript">
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    function grdTestSets_execute(projectId, testSetId)
    {
        var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');

        //Actually start the background process of creating the test runs
        ajxBackgroundProcessManager.display(projectId, 'TestSet_Execute', resx.TestSetList_ExecuteTestSet, resx.TestSetList_ExecuteTestSetDesc, testSetId);
    }
    function <%=WebPartUniqueId%>_ajxBackgroundProcessManager_success(msg, returnCode)
    {
        //Need to redirect to the test runs pending
        if (returnCode && returnCode > 0)
        {
            var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
            var projectId = ajxBackgroundProcessManager.get_projectId();
            var baseUrl = '<%=TestRunsPendingUrl %>';
            var url = baseUrl.replace(globalFunctions.artifactIdToken, returnCode).replace(globalFunctions.projectIdToken, projectId);
            window.location = url;
        }
    }
</script>
