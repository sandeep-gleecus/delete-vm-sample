<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AssignedApprovers.aspx.cs" MasterPageFile="~/MasterPages/Administration.master" Inherits="Inflectra.SpiraTest.Web.Administration.Project.AssignedApprovers" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<h2>
		<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_AssignedApprovers_Title%>" />
		<small>
			<tstsc:HyperLinkEx
				ID="lnkAdminHome"
				runat="server"
				Title="<%$Resources:Main,Admin_Project_BackToHome %>">
                <asp:Label id="lblProjectName" Runat="server" />
			</tstsc:HyperLinkEx>
		</small>
	</h2>
	
    <p class="mb5">
		<a href="BaselineDetails.aspx">BaselineDetails.aspx</a>
	    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_AssignedApprovers_String1 %>" /><br />
	    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_AssignedApprovers_String2 %>" />
    </p>

	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage"
		DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />


	<div class="toolbar btn-toolbar-mid-page flex items-center my4">
		<div class="btn-group priority2" role="group">
			<tstsc:DropMenu ID="btnProjectMembershipUpdate" SkinID="ButtonPrimary" GlyphIconCssClass="fas fa-save mr3" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Save %>"></tstsc:DropMenu>
		</div>

		 
	</div>

	<tstsc:GridViewEx ID="grdUserMembership" CssClass="DataGrid" runat="server"
		DataMember="ProjectUser" AutoGenerateColumns="False" Width="100%">
		<HeaderStyle CssClass="Header" />
		<Columns>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,FullName %>" 
                HeaderStyle-CssClass="priority1" 
                ItemStyle-CssClass="priority1" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblFullName" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser)Container.DataItem).FullName) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,UserName %>" 
                HeaderStyle-CssClass="priority3" 
                ItemStyle-CssClass="priority3" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblUserName" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser)Container.DataItem).UserName) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,Department %>" 
                HeaderStyle-CssClass="priority1" 
                ItemStyle-CssClass="priority1" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblDepartment" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser)Container.DataItem).User.Profile.Department) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
             
			 
			<tstsc:TemplateFieldEx HeaderText="Is Test Approver?" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
				<ItemTemplate>
					 <tstsc:CheckBoxYnEx ID="chkActive" runat="server" ClientScriptMethod="testApproverCheckboxClicked(e);"/>
					 <tstsc:LabelEx 
                        ID="lblUserId" 
						Visible="false"
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser)Container.DataItem).User.UserId.ToString()) %>'
                        />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx HeaderText="TestCase Approver Order Id" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
				<ItemTemplate>
					 <tstsc:TextBoxEx ID="txtOrderId" runat="server" Width="20px" />
                     <asp:CompareValidator runat="server" Operator="DataTypeCheck" Type="Integer" 
                                                        ControlToValidate="txtOrderId" ErrorMessage="*" EnableClientScript="true" 
                                                        ForeColor="Red"
                                                        Display="Dynamic"/>
                     <asp:RangeValidator runat="server" Type="Integer" ID="txtRangeValidator"
                                                        MinimumValue="1" MaximumValue="400" ControlToValidate="txtOrderId" 
                                                        ForeColor="Red"
                                                        ErrorMessage="*" />

				</ItemTemplate>
			</tstsc:TemplateFieldEx>
		</Columns>
	</tstsc:GridViewEx>

	<script type="text/javascript">
		function testApproverCheckboxClicked(e) {
            if (e.target.checked) {
				$(this).siblings("input[id *= 'txtOrderId']").each(function () {
					$(this).removeAttr('disabled');
                });
                
            }
            else {
				$(this).siblings("input[id *= 'txtOrder']").each(function () {
                    $(this).val('');
					$(this).attr('disabled', 'disabled');
				});
			}
        };
	</script>
</asp:Content>
