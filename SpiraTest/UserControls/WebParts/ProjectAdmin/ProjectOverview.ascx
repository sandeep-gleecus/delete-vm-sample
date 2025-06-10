<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectOverview.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin.ProjectOverview" %>
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
            <tstsc:HyperLinkEx ID="lnkProgram" runat="server" />
		</td>
	</tr>
	<tr>
		<td>
			<b><asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Template %>" />:</b>
		</td>
		<td>
			<tstsc:HyperLinkEx ID="lnkTemplate" runat="server" />
		</td>
	</tr>
	<tr>
		<td><b><asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,Owner_s_ %>" />:</b>
		</td>
		<td>
		    <asp:Repeater Runat="server" ID="rptProjectOwners" DataMember="User">
		        <SeparatorTemplate>, </SeparatorTemplate>
				<ItemTemplate>
					<asp:HyperLink Runat="server" NavigateUrl='<%# "~/Administration/UserDetailsEdit.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + ((User) Container.DataItem).UserId %>' ID="Hyperlink1" Enabled="<%#UserIsAdmin %>"><%# Microsoft.Security.Application.Encoder.HtmlEncode(((User) Container.DataItem).FullName) %></asp:HyperLink>
				</ItemTemplate>
			</asp:Repeater>
		</td>
	</tr>
    <asp:PlaceHolder runat="server" ID="plcBaselines" Visible="false">
        <tr>
		    <td>
			    <b><asp:Localize ID="Localize4" runat="server" Text="<%$Resources:ServerControls,TabControl_Baselines%>" />:</b>
		    </td>
		    <td>
			    <tstsc:LabelEx ID="lblBaselines" runat="server" />
		    </td>
	    </tr>
    </asp:PlaceHolder>
</table>


<h2>
    <small>
        <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_Title %>" />
    </small>
</h2>
<table class="NoBorderTable">
	<tr>
		<td>
			<tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_WorkHoursPerDay %>" AppendColon="true" />
		</td>
		<td>
            <asp:Literal ID="txtWorkingHoursPerDay" runat="server" Mode="Encode" />
		</td>
        <td rowspan="3">
            <tstsc:HyperLinkEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions%>" SkinID="ButtonDefault" ID="lnkPlanningOptions" />
        </td>
	</tr>
	<tr>
		<td>
			<tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_WorkDaysPerWeek %>" AppendColon="true" />
		</td>
		<td>
            <asp:Literal ID="txtWorkingDaysPerWeek" runat="server" Mode="Encode" />
		</td>
	</tr>
	<tr>
		<td>
			<tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_PlanningOptions_PointEffort %>" AppendColon="true" />
		</td>
		<td>
            <asp:Literal ID="txtPointEffort" runat="server" Mode="Encode" />
            <asp:Localize runat="server" Text="<%$Resources:Main,Global_Hours %>" />
		</td>
	</tr>
</table>
