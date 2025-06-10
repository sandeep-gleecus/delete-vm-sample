<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="AutomationEngines.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.AutomationEngines" Title="Untitled Page" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_AutomationEngines_Title%>" />
                </h2>
                <div class="my4">
                    <p>
                        <%= this.productName %>
                        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_AutomationEngines_Intro1%>" />
                    </p>
                    <p>
                        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_AutomationEngines_Intro2%>" />
                    </p>
                </div>
				<tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
				<asp:ValidationSummary id="ValidationSummary" Runat="server" CssClass="ValidationMessage" DisplayMode="BulletList"
					ShowSummary="True" ShowMessageBox="False" />
                <tstsc:GridViewEx ID="grdAutomationEngines" Runat="server" AutoGenerateColumns="False" CssClass="DataGrid" ShowHeader="true" ShowFooter="false" ShowSubHeader="false" Width="100%">
	                <HeaderStyle CssClass="Header" />
	                <Columns>
		                <tstsc:BoundFieldEx DataField="Name" HeaderText="<%$Resources:Fields,Name %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
                        <tstsc:BoundFieldEx DataField="Token" HeaderText="<%$Resources:Fields,Token %>"  HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4"/>
		                <tstsc:BoundFieldEx DataField="Description" HeaderText="<%$Resources:Fields,Description %>"  HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4"/>
		                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
		                    <ItemTemplate>
		                        <tstsc:LabelEx ID="lblActive" runat="server" Text='<%#GlobalFunctions.DisplayYnFlag(((AutomationEngine) Container.DataItem).IsActive) %>' />
		                    </ItemTemplate>
		                </tstsc:TemplateFieldEx>
		                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations%>" ItemStyle-Wrap="false" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
			                <ItemTemplate>
                                <div class="btn-group">
    				                <tstsc:HyperLinkEx Runat="server" CausesValidation="false" NavigateUrl='<%# "AutomationEngineDetails.aspx?" + GlobalFunctions.PARAMETER_AUTOMATION_ENGINE_ID + "=" + ((AutomationEngine) Container.DataItem).AutomationEngineId%>' ID="lnlEdit" Confirmation="false" Authorized_Permission="SystemAdmin" SkinID="ButtonDefault">
                                        <span class="far fa-edit"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit%>" />
    				                </tstsc:HyperLinkEx>
	    			                <tstsc:LinkButtonEx Runat="server" CausesValidation="false" CommandName="DeleteEngine" CommandArgument='<%# ((AutomationEngine) Container.DataItem).AutomationEngineId%>' ID="lnkDelete" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_AutomationEngines_DeleteConfirm %>" Authorized_Permission="SystemAdmin">
                                        <span class="fas fa-trash-alt"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
	    			                </tstsc:LinkButtonEx>
                                </div>
			                </ItemTemplate>
		                </tstsc:TemplateFieldEx>
	                </Columns>													
                </tstsc:GridViewEx>
                <div class="my4">
                    <tstsc:ButtonEx id="btnAdd" Runat="server" SkinID="ButtonPrimary" Text="<%$Resources:Buttons,Add %>" CausesValidation="false" />
                </div>
			</div>
		</div>
	</div>
</asp:Content>
