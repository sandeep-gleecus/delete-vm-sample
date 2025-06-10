<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Components.aspx.cs" MasterPageFile="~/MasterPages/Administration.master"
    Inherits="Inflectra.SpiraTest.Web.Administration.Project.Components" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <h2>
        <asp:Localize ID="Localize1" Text="<%$Resources:Main,Admin_EditComponents %>" runat="server" />
        <small>
                <tstsc:HyperLinkEx 
                ID="lnkAdminHome" 
                runat="server" 
                Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                >
                <asp:Label id="lblProjectName" Runat="server" />
			</tstsc:HyperLinkEx>
        </small>
    </h2>

    <p>
        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_EditComponents_Intro %>" />
    </p>
    <asp:ValidationSummary ID="ValidationSummary" runat="server" />
    <tstsc:MessageBox runat="server" SkinID="MessageBox" ID="lblMessage" />


    <div class="TabControlHeader mt6">
        <div class="display-inline-block">
            <strong><asp:Localize runat="server" Text="<%$Resources:Main,Global_Displaying %>"/>:</strong>
        </div>
        <tstsc:DropDownListEx ID="ddlFilterType" runat="server" AutoPostBack="true">
            <asp:ListItem Text="<%$Resources:Dialogs,Global_AllActive %>" Value="allactive" />
            <asp:ListItem Text="<%$Resources:Dialogs,Global_AllButDeleted %>" Value="allbutdeleted" />
            <asp:ListItem Text="<%$Resources:Dialogs,Global_All %>" Value="all" />
        </tstsc:DropDownListEx>
    </div>


	<tstsc:GridViewEx 
        ID="grdComponents" 
        runat="server" 
        DataKeyNames="ComponentId" 
        ShowSubHeader="False" 
        SkinID="DataGrid" 
        ShowFooter="true" 
        ShowHeaderWhenEmpty="true" 
        style="min-width:700px"
        >
        <FooterStyle CssClass="AddValueFooter" />
		<EmptyDataTemplate>
			<asp:Literal ID="ltrNoComponents" runat="server" Text="<%$ Resources:Main,Admin_Components_NoComponentsDefined %>" />
			<tstsc:LinkButtonEx runat="server" Text="<%$ Resources:Main,Admin_Components_AddComponent %>" ID="lnkNewRow" CommandName="addnew" />
		</EmptyDataTemplate>
		<Columns>
            <tstsc:TemplateFieldEx FooterColumnSpan="6" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" FooterStyle-CssClass="priority1">
                <ItemTemplate>
                    <tstsc:ImageEx CssClass="w4 h4" runat="server" ImageUrl="Images/org-Component.svg" AlternateText="<%$Resources:Fields,ComponentId %>" />
                </ItemTemplate>
				<FooterTemplate>
					<tstsc:LinkButtonEx ID="LinkButtonEx2" runat="server" Text="<%$ Resources:Main,Admin_Components_AddComponent %>" CommandName="AddNew" SkinID="ButtonLinkAdd"/>
				</FooterTemplate>
            </tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>" FooterColumnSpan="-1" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" >
				<ItemTemplate>
				    <asp:Literal ID="Label2" runat="server" Text='<%# "[" + Component.ARTIFACT_PREFIX + ":" %>' /><asp:Literal runat="server" ID="ltrComponentId" Text='<%# ((Component)Container.DataItem).ComponentId %>' /><asp:Literal ID="Label3" runat="server" Text=']' />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" FooterColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" >
				<ItemTemplate>
					<tstsc:TextBoxEx runat="server" Text='<%# ((Component)Container.DataItem).Name %>' Width="400px" MaxLength="50" ID="txtName" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx  HeaderText="<%$Resources:Fields,ActiveYn %>" FooterColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" >
                <ItemStyle Width="50px" />
                <ItemTemplate>
                    <tstsc:CheckBoxEx runat="server" ID="chkActive" NoValueItem="false" Checked='<%# (((Component) Container.DataItem).IsActive) ? true : false %>' Width="50px" />
                </ItemTemplate>
            </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx  HeaderText="<%$Resources:Fields,Status %>" FooterColumnSpan="-1" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" >
                <ItemTemplate>
                    <asp:Literal runat="server" Text="<%#GetStatus((Component)Container.DataItem) %>" />
                </ItemTemplate>
            </tstsc:TemplateFieldEx>
			<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" FooterColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" >
				<ItemTemplate>
                    <asp:PlaceHolder runat="server" Visible="<%#((Component)Container.DataItem).ComponentId > 0 %>">
					    <tstsc:LinkButtonEx ID="btnDelete" runat="server" CommandName="Remove" CommandArgument="<%# ((Component)Container.DataItem).ComponentId %>" Visible="<%#!((Component)Container.DataItem).IsDeleted %>">
                            <span class="fas fa-trash-alt"></span>
                            <asp:Localize runat="server" Text="<%$ Resources:Buttons,Delete %>" />
					    </tstsc:LinkButtonEx>
                        <tstsc:LinkButtonEx ID="btnUndelete" runat="server" CommandName="Undelete" CommandArgument="<%# ((Component)Container.DataItem).ComponentId %>" Visible="<%#((Component)Container.DataItem).IsDeleted %>">
                            <span class="fas fa-undo"></span>
                            <asp:Localize runat="server" Text="<%$ Resources:Buttons,Undelete %>" />
                        </tstsc:LinkButtonEx>
                    </asp:PlaceHolder>
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
		</Columns>
	</tstsc:GridViewEx>
    <div class="btn-group mt4">
        <tstsc:ButtonEx ID="btnSave" SkinID="ButtonPrimary" runat="server" CausesValidation="True" Text="<%$Resources:Buttons,Save%>" />
        <tstsc:ButtonEx ID="btnCancel" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Cancel%>" />
    </div>
</asp:Content>
