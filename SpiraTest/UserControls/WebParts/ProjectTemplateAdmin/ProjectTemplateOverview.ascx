<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectTemplateOverview.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectTemplateAdmin.ProjectTemplateOverview" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<tstsc:RichTextLabel id="lblProjectTemplateDescription" Runat="server" />
<table class="NoBorderTable">
	<tr>
		<td><b><asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,Owner_s_ %>" />:</b>
		</td>
		<td>
		    <asp:Repeater Runat="server" ID="rptTemplateAdmins" DataMember="User">
		        <SeparatorTemplate>, </SeparatorTemplate>
				<ItemTemplate>
					<asp:HyperLink Runat="server" NavigateUrl='<%# "~/Administration/UserDetailsEdit.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + ((User) Container.DataItem).UserId %>' ID="Hyperlink1" Enabled="<%#UserIsAdmin %>"><%# Microsoft.Security.Application.Encoder.HtmlEncode(((User) Container.DataItem).FullName) %></asp:HyperLink>
				</ItemTemplate>
			</asp:Repeater>
		</td>
	</tr>
    <tr>
		<td>
			<b runat="server" title="<%$Resources:Messages,ProjectTemplate_CanBulkChangeStatus %>">
				<asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,ProjectTemplate_CanBulkChangeStatus%>"/>:
			</b>
		</td>
		<td>
			<tstsc:LabelEx ID="lblCanBulkChangeStatus" runat="server"  />
		</td>
	</tr>
</table>
