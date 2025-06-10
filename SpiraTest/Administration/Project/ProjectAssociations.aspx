<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="ProjectAssociations.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.ProjectAssociations" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">

    <h1>
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_ProjectAssociations %>" />
        <small>
            <tstsc:HyperLinkEx 
                ID="lnkAdminHome" 
                runat="server" 
                Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                >
                <asp:Label id="lblProjectName" Runat="server" />
			</tstsc:HyperLinkEx>
        </small>
    </h1>
    <p class="mb4">
        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectAssociations_Intro %>" />
    </p>
            
    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />


    <div class="toolbar btn-toolbar-mid-page mt5">
        <div class="btn-group priority2" role="group">
            <tstsc:DropMenu ID="btnAdd" SkinID="ButtonPrimary" GlyphIconCssClass="mr3 fas fa-plus" runat="server" Text="<%$Resources:Buttons,Add%>" ClientScriptServerControlId="dlgAddAssociation" ClientScriptMethod="display()" />
            <tstsc:DropMenu ID="btnRemove" runat="server" GlyphIconCssClass="mr3 fas fa-trash-alt" Text="<%$Resources:Buttons,Remove%>" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_ProjectAssociations_ConfirmRemove %>" />
        </div>
    </div>
                
    <tstsc:GridViewEx ID="grdLinkedProjects" CssClass="DataGrid" runat="server" AutoGenerateColumns="False" Width="100%">
        <HeaderStyle CssClass="Header" />
        <Columns>
            <tstsc:TemplateFieldEx HeaderStyle-CssClass="TickIcon priority3" ItemStyle-CssClass="priority3">
                <ItemTemplate>
                    <tstsc:CheckBoxEx runat="server" ID="chkDeleteAssociation" MetaData="<%#((IGrouping<Project, ProjectArtifactSharing>)Container.DataItem).Key.ProjectId %>" />
                </ItemTemplate>
            </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,ProjectName %>" 
                HeaderStyle-CssClass="priority1" 
                ItemStyle-CssClass="priority1" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblProjectName" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((IGrouping<Project, ProjectArtifactSharing>)Container.DataItem).Key.Name) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,ProjectGroupName %>" 
                HeaderStyle-CssClass="priority1" 
                ItemStyle-CssClass="priority1" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblProjectGroupName" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((IGrouping<Project, ProjectArtifactSharing>)Container.DataItem).Key.Group.Name) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" HeaderText="<%$Resources:Main,Admin_ProjectAssociations_ArtifactTypes %>">
                <ItemTemplate>
                    <asp:Repeater ID="rptArtifactTypes" runat="server" DataSource="<%#((IGrouping<Project, ProjectArtifactSharing>)Container.DataItem).ToList() %>">
                        <ItemTemplate>
                            <tstsc:ImageEx CssClass="w4 h4" runat="server" ImageUrl='<%# "Images/" + GlobalFunctions.GetIconForArtifactType(((ProjectArtifactSharing)Container.DataItem).ArtifactTypeId)%>' />
                            <asp:Label runat="server" Text="<%# ((ProjectArtifactSharing)Container.DataItem).ArtifactType.Name%>" />
                        </ItemTemplate>
                        <SeparatorTemplate>,</SeparatorTemplate>
                    </asp:Repeater>
                </ItemTemplate>
            </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" HeaderText="<%$Resources:Fields,Operations %>">
                <ItemTemplate>
                    <tstsc:HyperLinkEx ID="lnkEdit" runat="server" ClientScriptMethod="grdLinkedProjects_edit(this)" SkinID="ButtonDefault" NavigateUrl="javascript:void(0)">
                        <span class="fas fa-edit"></span>
                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>" />
                    </tstsc:HyperLinkEx>
                </ItemTemplate>
            </tstsc:TemplateFieldEx>
        </Columns>
    </tstsc:GridViewEx>


    <p class="alert alert-warning alert-narrow mt4">
        <span class="fas fa-info-circle"></span>
        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectAssociations_Notes %>" />
    </p>


    <tstsc:DialogBoxPanel ID="dlgAddAssociation" runat="server" Title="<%$Resources:Main,Admin_ProjectAssociations_AddAssociation %>" Modal="true" Width="500px">
        <div class="my3">
            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectAssociations_AddAssociation_Intro %>" />
        </div>
        <div class="my3 form-group">
            <div class="DataLabel col-md-4">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Project %>" AppendColon="true" AssociatedControlID="ddlNewProject" Required="true" />
            </div>
            <div class="DataEntry col-md-8">
                <tstsc:DropDownListEx runat="server" ID="ddlNewProject" DataTextField="Name" DataValueField="ProjectId" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_ChooseProject %>" />
            </div>
        </div>
        <div class="clearfix"></div>
        <div class="my3 form-group">
            <div class="DataLabel col-md-4">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_ProjectAssociations_ArtifactTypes %>" AppendColon="true" AssociatedControlID="ddlArtifactTypesNew" Required="false" />
            </div>
            <div class="DataEntry col-md-8">
                <tstsc:DropDownMultiList runat="server" ID="ddlArtifactTypesNew" DataTextField="Name" DataValueField="ArtifactTypeId" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" SelectionMode="Multiple" />
            </div>
        </div>
        <div class="clearfix"></div>
        <div class="my3 btn-group ml4">
            <tstsc:ButtonEx runat="server" ID="btnAddAssociation" Text="<%$Resources:Buttons,Add %>" SkinID="ButtonPrimary" />
            <tstsc:ButtonEx runat="server" ClientScriptMethod="close()" ClientScriptServerControlId ="dlgAddAssociation" Text="<%$Resources:Buttons,Cancel %>" />
        </div>
    </tstsc:DialogBoxPanel>

    <tstsc:DialogBoxPanel ID="dlgEditAssociation" runat="server" Title="<%$Resources:Main,Admin_ProjectAssociations_EditAssociation %>" Modal="true" Width="500px">
        <div class="my3">
            <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ProjectAssociations_EditAssociation_Intro %>" />
        </div>
        <div class="my3 form-group">
            <div class="DataLabel col-md-4">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Project %>" AppendColon="true" AssociatedControlID="spnDestProject" Required="true" />
            </div>
            <div class="DataEntry col-md-8">
                <span runat="server" id="spnDestProject" />
            </div>
        </div>
        <div class="clearfix"></div>
        <div class="my3 form-group">
            <div class="DataLabel col-md-4">
                <tstsc:LabelEx runat="server" Text="<%$Resources:Main,Admin_ProjectAssociations_ArtifactTypes %>" AppendColon="true" AssociatedControlID="ddlArtifactTypesEdit" Required="false" />
            </div>
            <div class="DataEntry col-md-8">
                <tstsc:DropDownMultiList runat="server" ID="ddlArtifactTypesEdit" DataTextField="Name" DataValueField="ArtifactTypeId" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" SelectionMode="Multiple" />
            </div>
        </div>
        <div class="clearfix"></div>
            <asp:HiddenField runat="server" ID="hdnDestProjectId" />
        <div class="my3 btn-group ml4">
            <tstsc:ButtonEx runat="server" ID="btnSaveAssociation" Text="<%$Resources:Buttons,Save %>" SkinID="ButtonPrimary" />
            <tstsc:ButtonEx runat="server" ClientScriptMethod="close()" ClientScriptServerControlId ="dlgEditAssociation" Text="<%$Resources:Buttons,Cancel %>" />
        </div>

    </tstsc:DialogBoxPanel>

    <script type="text/javascript">
        function grdLinkedProjects_edit(sender)
        {
            //Set the project id, name
            var destProjectId = sender.getAttribute('data-project-id');
            var projectName = sender.getAttribute('data-project-name');
            $('#<%=spnDestProject.ClientID%>').html(projectName);
            $('#<%=hdnDestProjectId.ClientID%>').val(destProjectId);

            //Set the list of artifacts - need to convert them to ints
            var artifactTypeIds = sender.getAttribute('data-artifact-ids');
            var artifactTypeIdsAsInts = artifactTypeIds.split(",").map(function (x) {
                return parseInt(x);
            }).join();
            var ddlArtifactTypesEdit = $find('<%=ddlArtifactTypesEdit.ClientID%>');
            ddlArtifactTypesEdit.set_selectedItem(artifactTypeIdsAsInts);    //Pass in comma-separated list of ids

            var dlgEditAssociation = $find('<%=dlgEditAssociation.ClientID%>');
            dlgEditAssociation.display();
        }
    </script>
</asp:Content>

