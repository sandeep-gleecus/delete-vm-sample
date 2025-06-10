<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SavedReports.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.SavedReports" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx ID="grdSavedReports" runat="server" DataMember="ReportSaved" SkinID="WidgetGrid">
    <HeaderStyle CssClass="SubHeader" />
    <Columns>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="True"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
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
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="True"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
			<HeaderTemplate>
				<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,Project %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<%#: ((SavedReportView)Container.DataItem).ProjectName%>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText="" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
		    <ItemTemplate>
		        <tstsc:LinkButtonEx ID="btnDelete" runat="server" CommandName="DeleteReport" CommandArgument='<%# ((SavedReportView) Container.DataItem).ReportSavedId%>'
		            Text="<%$Resources:Buttons,Delete %>" CausesValidation="false" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,SavedReports_DeleteConfirmation%>" />
		    </ItemTemplate>
		</tstsc:TemplateFieldEx>
    </Columns>
</tstsc:GridViewEx>
