<%@ Page Language="C#"
	MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true"
	CodeBehind="TaraVault_ProjectList.aspx.cs"
	Inherits="Inflectra.SpiraTest.Web.Administration.TaraVault_ProjectList" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h1>
                    <tstsc:LabelEx ID="LabelEx1" runat="server" Text="<%$ Resources:Main,Admin_TaraVaultProjects %>" />
                </h1>
                <div class="btn-group" role="group">
	                <tstsc:HyperLinkEx runat="server" ID="lnkBackToTaraVault" NavigateUrl="TaraVault.aspx" SkinID="ButtonDefault">
                        <span class="fas fa-arrow-left"></span>
                        <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_TaraVault_BackToHome %>" />
	                </tstsc:HyperLinkEx>
                </div>
	            <p class="my3">
		            <%-- Introduction to what TaraVault is all about. --%>
		            <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_TaraVaultProjects_Intro %>" />
	            </p>
				<tstsc:GridViewEx ID="grdProjectList" CssClass="DataGrid" runat="server" AllowSorting="true"
					AllowCustomPaging="False" AllowPaging="False" ShowSubHeader="False" Width="100%"
					AutoGenerateColumns="False" EnableViewState="true" HeaderStyle-CssClass="Header"
					ShowHeaderWhenEmpty="true" DataKeyNames="ProjectId">
					<EmptyDataTemplate>
						<asp:Literal runat="server" Text="<%$ Resources:Main,Admin_TaraVault_Config_NoProjectsActivated %>" />
					</EmptyDataTemplate>
					<Columns>
						<tstsc:FilterSortFieldEx DataField="Name" HeaderText="<%$ Resources:Fields,ProjectName %>" FilterField="Name"
							FilterType="TextBox" Sortable="false" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2"/>
						<tstsc:FilterSortFieldEx DataField="TaraVault.Name" HeaderText="<%$ Resources:Fields,Name %>" FilterField="TaraVault.Name"
							FilterType="TextBox" Sortable="false" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
						<tstsc:FilterSortFieldEx DataField="TaraVault.VaultType.Name" HeaderText="<%$ Resources:Fields,Type %>" FilterField="TaraVault.VaultType.Name"
							FilterType="TextBox" Sortable="false" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
						<tstsc:FilterSortFieldEx DataField="TaraVault.VaultId" HeaderText="<%$ Resources:Fields,ID %>" FilterField="TaraVault.VaultId"
							FilterType="TextBox" Sortable="false" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2"/>
						<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
							<ItemTemplate>
                                <div class="btn-group" role="group">
								    <tstsc:HyperLinkEx ID="lnkEdit" SkinID="ButtonDefault" runat="server" NavigateUrl='<%#UrlRewriterModule.RetrieveProjectAdminUrl(((Inflectra.SpiraTest.DataModel.Project)Container.DataItem).ProjectId, "TaraVaultProjectSettings")%>'>
                                        <span class="fas fa-edit"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>" />
								    </tstsc:HyperLinkEx>
                                </div>
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
					</Columns>
				</tstsc:GridViewEx>
            </div>
        </div>
    </div>
</asp:Content>
