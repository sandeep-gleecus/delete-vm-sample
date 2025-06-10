<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectOverview.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.ProjectOverview" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Microsoft.Security.Application" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<tstsc:RichTextLabel id="lblProjectDescription" Runat="server" />
<table class="NoBorderTable">
	<tr>
		<td>
			<div class="Spacer"></div>
			<b><asp:Localize runat="server" Text="<%$Resources:Fields,Program %>" />:</b>
		</td>
		<td>
			<div class="Spacer"></div>
            <tstsc:LinkButtonEx ID="btnProjectGroup" runat="server" SkinID="ButtonLink"/>
		</td>
	</tr>
	<tr>
		<td>
			<b><asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,WebSite %>" />:</b>
		</td>
		<td>
			<tstsc:HyperLinkEx ID="lnkProjectWebsite" CssClass="url-to-shorten external-link" Runat="server" Target="_blank" />
		</td>
	</tr>
	<tr>
		<td><b><asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,Owner_s_ %>" />:</b>
		</td>
		<td>
		    <asp:Repeater Runat="server" ID="rptProjectOwners" DataMember="User">
		        <SeparatorTemplate>, </SeparatorTemplate>
				<ItemTemplate>
					<asp:HyperLink Runat="server" NavigateUrl='<%# "mailto:" + ((User) Container.DataItem).EmailAddress %>' ID="Hyperlink1"><%# Microsoft.Security.Application.Encoder.HtmlEncode(((User) Container.DataItem).FullName) %></asp:HyperLink>
				</ItemTemplate>
			</asp:Repeater>
		</td>
	</tr>
    <tr>
		<td>
			<b><asp:Localize runat="server" Text="<%$Resources:Fields,Template %>" />:</b>
		</td>
		<td>
            <tstsc:LinkButtonEx ID="btnProjectTemplate" runat="server" SkinID="ButtonLink"/>
		</td>
	</tr>
</table>
