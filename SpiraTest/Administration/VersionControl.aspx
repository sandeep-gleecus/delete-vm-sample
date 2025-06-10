<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="VersionControl.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.VersionControl" Title="Untitled Page" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-11">
                <h2>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_VersionControl_Title %>" />
                </h2>
                <div class="my3">
                    <p>
                        <%=this.productName%>
                        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_VersionControl_Intro %>" />
                    </p>
                    <p>
                        <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_VersionControl_Intro2 %>" />                    
                    </p>
                </div>
				<tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
                <tstsc:MessageBox ID="msgStatus" runat="server" SkinID="MessageBox" />
				<asp:ValidationSummary id="ValidationSummary" Runat="server" CssClass="ValidationMessage" DisplayMode="BulletList"
					ShowSummary="True" ShowMessageBox="False" />
                <div class="Spacer"></div>
                <tstsc:GridViewEx ID="grdVersionControlProviders" Runat="server" AutoGenerateColumns="False" CssClass="DataGrid" ShowHeader="true" ShowFooter="false" ShowSubHeader="false" Width="100%" DataMember="VersionControlSystem">
	                <HeaderStyle CssClass="Header" />
	                <Columns>
		                <tstsc:BoundFieldEx DataField="Name" HeaderText="<%$Resources:Fields,Provider %>"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
		                <tstsc:BoundFieldEx DataField="Description" HeaderText="<%$Resources:Fields,Description%>"  HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3"/>
		                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ProjectSettings%>" ItemStyle-Wrap="false"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
			                <ItemTemplate>
                                <tstsc:DropDownHierarchy runat="server" ID="ddlProjectSettings" NoValueItem="true"
                                    AutoPostBack="false" DataTextField="Name" DataValueField="ProjectId" ItemImage="Images/org-Project-outline.svg" SummaryItemImage="Images/org-Project.svg"
                                />
			                </ItemTemplate>
		                </tstsc:TemplateFieldEx>
		                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn%>"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
		                    <ItemTemplate>
		                        <tstsc:LabelEx ID="lblActive" runat="server" Text='<%# GlobalFunctions.DisplayYnFlag(((VersionControlSystem)Container.DataItem).IsActive) %>' />
		                    </ItemTemplate>
		                </tstsc:TemplateFieldEx>
		                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations%>" ItemStyle-Wrap="false"  HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
			                <ItemTemplate>
			                    <div class="btn-group">
				                    <tstsc:LinkButtonEx Runat="server" CausesValidation="false" CommandName="EditProvider" CommandArgument='<%# ((VersionControlSystem)Container.DataItem).VersionControlSystemId %>' ID="lnlEdit" Confirmation="false" Authorized_Permission="SystemAdmin">
                                        <span class="far fa-edit"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>" />
				                    </tstsc:LinkButtonEx>
    				                <tstsc:LinkButtonEx Runat="server" CausesValidation="false" CommandName="DeleteProvider" CommandArgument='<%# ((VersionControlSystem)Container.DataItem).VersionControlSystemId %>' ID="lnkDelete" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_VersionControl_DeleteConfirm %>" Authorized_Permission="SystemAdmin" >
                                        <span class="fas fa-trash-alt"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
    				                </tstsc:LinkButtonEx>
                                </div>
			                </ItemTemplate>
		                </tstsc:TemplateFieldEx>
	                </Columns>													
                </tstsc:GridViewEx>
                <div class="my3">
                    <tstsc:ButtonEx id="btnAdd" SkinID="ButtonPrimary" Runat="server" Text="<%$Resources:Buttons,Add %>" CausesValidation="false" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
