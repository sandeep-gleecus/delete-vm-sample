<%@ Page Language="c#" CodeBehind="RoleDetails.aspx.cs" AutoEventWireup="True" ValidateRequest="false" Inherits="Inflectra.SpiraTest.Web.Administration.RoleDetails" MasterPageFile="~/MasterPages/Administration.master" %>

<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True" DisplayMode="BulletList"
                    runat="server" ID="ValidationSummary1" />
                <h2>
                    <asp:Label ID="lblProjectRoleName" runat="server" />
                    <small>
                        <tstsc:LabelEx Text="<%$Resources:Main,SiteMap_EditRoleDetails %>" runat="server" />
                    </small>
                </h2>
                <div class="Spacer"></div>
                <asp:Localize runat="server" Text="<%$Resources:Main,Admin_RoleDetails_Text1 %>" /><br />
                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_RoleDetails_Text2 %>" />
            </div>
        </div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="row DataEntryForm view-edit">
            <div class="col-lg-9 col-sm-11">
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="txtNameLabel" runat="server" AssociatedControlID="txtName" Text="<%$Resources:Fields,RoleName %>" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:TextBoxEx CssClass="text-box" ID="txtName" runat="server" TextMode="SingleLine" MaxLength="50"/>
                        <asp:RequiredFieldValidator ID="vldName" runat="server" Text="*" ErrorMessage="<%$Resources:Messages,Admin_RoleDetails_NameRequired %>"
                            ControlToValidate="txtName" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="txtDescriptionLabel" runat="server" AssociatedControlID="txtDescription" Text="<%$Resources:Fields,Description %>" Required="false" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:TextBoxEx ID="txtDescription" runat="server" Height="80px" Width="100%" TextMode="MultiLine" RichText="false" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="ddlAdminYnLabel" runat="server" AssociatedControlID="chkAdminYn" Text="<%$Resources:Fields,ProjectAdmin %>" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:CheckBoxEx runat="server" ID="chkAdminYn" />
                        <p class="Notes">
                            <asp:Localize runat="server" Text="<%$Resources:Main,RoleDetails_AdminNotes %>" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="chkTemplateAdminYnLabel" runat="server" AssociatedControlID="chkTemplateAdminYn" Text="<%$Resources:Fields,TemplateAdmin %>" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:CheckBoxEx runat="server" ID="chkTemplateAdminYn" />
                        <p class="Notes">
                            <asp:Localize runat="server" Text="<%$Resources:Main,RoleDetails_TemplateAdminNotes %>" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="ddlLimitedViewLabel" runat="server" AssociatedControlID="chkLimitedView" Text="<%$Resources:Main,RoleDetails_LimitedView %>" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:CheckBoxEx runat="server" ID="chkLimitedView" />
                        <p class="Notes">
                            <asp:Localize ID="Localize12" runat="server" Text="<%$Resources:Main,RoleDetails_LimitedViewNotes %>" />
                        </p>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="ddlActiveYnLabel" runat="server" AssociatedControlID="chkActiveYn" Text="<%$Resources:Fields,ActiveYn %>" Required="true" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
                        <tstsc:CheckBoxEx runat="server" ID="chkActiveYn" />
                        <p class="Notes">
                            <asp:Localize ID="Localize13" runat="server" Text="<%$Resources:Main,RoleDetails_ActiveNotes %>" />
                        </p>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-9 col-sm-11">
                <h3>
                    <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_RoleDetails_ArtifactPermissions %>" />
                </h3>
                <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_RoleDetails_Text3 %>" />
                <div class="Spacer"></div>
                <div style="max-width: 600px">
                    <tstsc:GridViewEx ID="grdRolePermissions" runat="server" ShowHeader="true" CssClass="DataGrid priority1" ShowSubHeader="false" DataKeyField="ArtifactTypeId" AutoGenerateColumns="false" Width="100%">
                        <HeaderStyle CssClass="Header" />
                        <Columns>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ArtifactTypeId %>">
                                <ItemTemplate>
                                    <%#LocalizeArtifactType((string)((ArtifactType) Container.DataItem).Name) %>
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                        </Columns>
                    </tstsc:GridViewEx>
                </div>
                <div class="Spacer"></div>
                <h3>
                    <tstsc:LabelEx ID="LabelEx1" runat="server" Text="<%$Resources:Main,Admin_RoleDetails_OtherPermissions %>" />
                </h3>
                <p>
                    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_RoleDetails_Text4 %>" />
                </p>
                <p class="fs-i">
                    <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Admin_RoleDetails_SourceCodeExplanation %>" />
                </p>
                <div class="Spacer"></div>
                <div style="max-width: 600px">
                    <table class="DataGrid priority1" style="width: 100%">
                        <tr class="Header">
                            <th class="priority1">
                                <asp:Localize runat="server" Text="<%$Resources:Fields,ArtifactTypeId %>" />
                            </th>
                            <th style="text-align: center" class="priority1">
                                <asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Fields,Permission_Add %>" />
                            </th>
                            <th style="text-align: center" >
                                <asp:Localize ID="Localize8" runat="server" Text="<%$Resources:Fields,Permission_Edit %>" />
                            </th>
                            <th style="text-align: center" >
                                <asp:Localize ID="Localize9" runat="server" Text="<%$Resources:Fields,Permission_Delete %>" />
                            </th>
                            <th style="text-align: center">
                                <asp:Localize ID="Localize10" runat="server" Text="<%$Resources:Fields,Permission_View %>" />
                            </th>
                        </tr>
                        <tr>
                            <td>
                                <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Fields,Comment %>" />
                            </td>
                            <td style="text-align: center">
                                <tstsc:CheckBoxEx ID="chkDiscussionsAdd" runat="server" />
                            </td>
                            <td style="text-align: center">
                                -
                            </td>
                            <td style="text-align: center">
                                -
                            </td>
                            <td style="text-align: center">
                                -
                            </td>
                        </tr>
                        <tr>
                            <td class="priority1">
                                <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Fields,SourceCode %>" />
                            </td>
                            <td style="text-align: center">
                                -
                            </td>
                            <td style="text-align: center">
                                <tstsc:CheckBoxEx ID="chkSourceCodeEdit" runat="server" />
                            </td>
                            <td style="text-align: center">
                                -
                            </td>
                            <td style="text-align: center">
                                <tstsc:CheckBoxEx ID="chkSourceCodeView" runat="server" />
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="btn-group mt4">
                    <tstsc:ButtonEx SkinID="ButtonPrimary" ID="btnUpdate" runat="server" Text="<%$Resources:Buttons,Save %>" CausesValidation="True" />
                    <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="False" />
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        // we need to make sure you cannot be a template admin without being a product admin
        // this was added in 6.0 with the assumption that permissions would be fixed to avoid this code in the next release
        var productAdmin = document.getElementById('<%=chkAdminYn.ClientID%>');
        var templateAdmin = document.getElementById('<%=chkTemplateAdminYn.ClientID%>');

        templateAdmin.addEventListener("change", function () {
            if (this.checked) {
                productAdmin.checked = true;
            }
        })
        productAdmin.addEventListener("change", function () {
            if (!this.checked && templateAdmin.checked) {
                templateAdmin.checked = false;
            }
        })
    </script>
</asp:Content>
