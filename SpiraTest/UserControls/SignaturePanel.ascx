<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SignaturePanel.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.SignaturePanel" %>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
 
<br />
 
<asp:Panel runat="server" ID="dataPanel" Visible="false">
	

	<tstsc:GridViewEx ID="grdSignaturesList" CssClass="DataGrid" runat="server" PageSize="15" OnRowDataBound="grdSignaturesList_RowDataBound"
		AllowSorting="true" AllowCustomPaging="True" AllowPaging="True" ShowSubHeader="True"
		Width="100%" AutoGenerateColumns="False" EnableViewState="true">
		<HeaderStyle CssClass="Header" />
		<SubHeaderStyle CssClass="SubHeader" />
		<PagerStyle CssClass="Pagination" />
		<Columns>
			<tstsc:TemplateFieldEx HeaderText="FullName" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
				<ItemTemplate>
					<tstsc:LabelEx ID="lblFullName" runat="server" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx HeaderText="Requested Date" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
				<ItemTemplate>
					<tstsc:LabelEx ID="lblRequestedDate" runat="server" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx HeaderText="Updated Date" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
				<ItemTemplate>
					<tstsc:LabelEx ID="lblUpdatedDate" runat="server" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>

			<tstsc:TemplateFieldEx HeaderText="Status" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
				<ItemTemplate>
					<tstsc:LabelEx ID="lblStatus" runat="server" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx HeaderText="Meaning" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
				<ItemTemplate>
					<tstsc:LabelEx ID="lblMeaning" runat="server" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
		</Columns>
	</tstsc:GridViewEx>
</asp:Panel>

<asp:Panel runat="server" ID="noDataPanel" Visible="false">
	<tstsc:MessageBox 
                    ID="lblMessage" 
                    runat="server" 
                    SkinID="MessageBox"
					 Type="Information"
                    />
	
</asp:Panel>
