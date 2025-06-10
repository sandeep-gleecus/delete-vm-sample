<%@ Page Language="c#" CodeBehind="DocumentWorkflowStep.aspx.cs" AutoEventWireup="True"
    Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.DocumentWorkflowStep" MasterPageFile="~/MasterPages/Administration.master" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h1 class="mt0 mb4 fs-h3">
                    <tstsc:HyperLinkEx ID="lnkWorkflowDetails" runat="server" NavigateUrl='<%# UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "DocumentWorkflowDetails") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString()%>' CssClass="btn btn-default"><span class="fas fa-angle-left"></span></tstsc:HyperLinkEx>
                    <asp:Literal ID="artifactName" runat="server" Text="<%$Resources:Main,Admin_EditDocumentWorkflows_Title %>"/>
                </h1>
                <h2>
                    <asp:Literal ID="lblStepName" runat="server" />
                    <small>
                        <asp:Literal Text="<%$Resources:Main,Admin_WorkflowStep_Title %>" runat="server" />
                    </small>
                </h2>    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <div class="Spacer"></div>
                <div class="WorkFlowDiagram-wrapper">
                    <table class="WorkflowDiagram">
                        <tr>
                            <td class="pa0">
                                <asp:DataGrid ID="grdIncomingTransitions" GridLines="None" runat="server" CssClass="WorkflowDiagram"
                                    ShowFooter="False" ShowHeader="False" AutoGenerateColumns="False" BorderStyle="None">
                                    <Columns>
                                        <asp:TemplateColumn ItemStyle-CssClass="pa0">
                                            <ItemTemplate>
                                                <div style="height: 2px; font-size: 1pt">
                                                </div>
                                                <div class="Step">
                                                    <tstsc:HyperLinkEx runat="server" Font-Italic="false" NavigateUrl='<%# UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "DocumentWorkflowTransition") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString() + "&" + GlobalFunctions.PARAMETER_WORKFLOW_TRANSITION_ID + "=" + ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition) Container.DataItem).WorkflowTransitionId%>'
                                                        ID="Hyperlink1"><%#: ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition)Container.DataItem).Name + " (" + GlobalFunctions.ARTIFACT_PREFIX_WORKFLOW_TRANSITION + ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition)Container.DataItem).WorkflowTransitionId + ")"%></tstsc:HyperLinkEx>
                                                </div>
                                                <div style="height: 2px; font-size: 1pt">
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn>
                                            <ItemTemplate>
                                                <span class="fas fa-arrow-right"></span>
                                            </ItemTemplate>
                                        </asp:TemplateColumn>
                                    </Columns>
                                </asp:DataGrid>
                            </td>
                            <td class="pa0">
                                <div class="Step">
                                    <asp:Label ID="lblStepName2" runat="server" />
                                </div>
                            </td>
                            <td class="pa0">
                                <asp:DataGrid ID="grdOutgoingTransitions" GridLines="None" runat="server" CssClass="WorkflowDiagram"
                                    ShowFooter="False" ShowHeader="False" AutoGenerateColumns="False" BorderStyle="None">
                                    <Columns>
                                        <asp:TemplateColumn>
                                            <ItemTemplate>
                                                <span class="fas fa-arrow-right"></span>
                                            </ItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn ItemStyle-CssClass="pa0">
                                            <ItemTemplate>
                                                <div style="height: 2px; font-size: 1pt">
                                                </div>
                                                <div class="Step">
                                                    <tstsc:HyperLinkEx runat="server" Font-Italic="false" NavigateUrl='<%# UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "DocumentWorkflowTransition") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString() + "&" + GlobalFunctions.PARAMETER_WORKFLOW_TRANSITION_ID + "=" + ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition) Container.DataItem).WorkflowTransitionId%>'><%#: ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition)Container.DataItem).Name + " (" + GlobalFunctions.ARTIFACT_PREFIX_WORKFLOW_TRANSITION + ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition)Container.DataItem).WorkflowTransitionId + ")"%></tstsc:HyperLinkEx>
                                                </div>
                                                <div style="height: 2px; font-size: 1pt">
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateColumn>
                                    </Columns>
                                </asp:DataGrid>
                            </td>
                        </tr>
                    </table>
                </div>
                <h3>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DocumentWorkflowStep_DocumentFields %>" />
                </h3>
                <p>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DocumentWorkflowStep_DocumentFieldsIntro %>" />
                </p>
                <div style="max-width: 550px">
                    <tstsc:GridViewEx ID="grdDocumentFields" runat="server" AutoGenerateColumns="False"
                        ShowHeader="True" Width="100%" ShowFooter="False" CssClass="DataGrid">
                        <HeaderStyle CssClass="Header" />
                        <Columns>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,FieldName %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <%# GetLocalizedFieldCaption(((Inflectra.SpiraTest.DataModel.ArtifactField)Container.DataItem).Caption)%>
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Default %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx 
                                        GroupName='<%# ((Inflectra.SpiraTest.DataModel.ArtifactField)Container.DataItem).Caption%>'
                                        ID="chkDefault" 
                                        MetaData='<%# ((Inflectra.SpiraTest.DataModel.ArtifactField) Container.DataItem).ArtifactFieldId %>'
                                        runat="server" 
                                        />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Hidden %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx 
                                        GroupName='<%# ((Inflectra.SpiraTest.DataModel.ArtifactField)Container.DataItem).Caption%>'
                                        ID="chkHidden" 
                                        MetaData='<%# ((Inflectra.SpiraTest.DataModel.ArtifactField) Container.DataItem).ArtifactFieldId %>'
                                        runat="server" 
                                        />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Disabled %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx
                                        GroupName='<%# ((Inflectra.SpiraTest.DataModel.ArtifactField)Container.DataItem).Caption%>'
                                        ID="chkDisabled" 
                                        MetaData='<%# ((Inflectra.SpiraTest.DataModel.ArtifactField) Container.DataItem).ArtifactFieldId %>'
                                        runat="server" 
                                        />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Required %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx 
                                        GroupName='<%# ((Inflectra.SpiraTest.DataModel.ArtifactField)Container.DataItem).Caption%>'
                                        ID="chkRequired" 
                                        MetaData='<%# ((Inflectra.SpiraTest.DataModel.ArtifactField) Container.DataItem).ArtifactFieldId %>'
                                        runat="server" 
                                        />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                        </Columns>
                    </tstsc:GridViewEx>
                </div>
                <div class="mt4 btn-group">
                    <tstsc:ButtonEx ID="btnSave1" runat="server" Text="<%$Resources:Buttons,Save %>" SkinID="ButtonPrimary" />
                    <tstsc:HyperLinkEx ID="HyperLinkEx2" runat="server" NavigateUrl='<%# UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "DocumentWorkflowDetails") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString()%>' Text="<%$Resources:Buttons,Cancel %>" CssClass="btn btn-default" />
                </div>
                <h3>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_DocumentWorkflowStep_DocumentCustomProperties %>" />
                </h3>
                <p>
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_DocumentWorkflowStep_DocumentCustomPropertiesIntro %>" />
                </p>
                <div style="max-width: 550px">
                    <tstsc:GridViewEx ID="grdDocumentCustomProperties" runat="server" AutoGenerateColumns="False"
                        ShowHeader="True" Width="100%" ShowFooter="False" CssClass="DataGrid">
                        <HeaderStyle CssClass="Header" />
                        <Columns>
                            <tstsc:BoundFieldEx DataField="Name" HeaderText="<%$Resources:Fields,Name %>" HtmlEncode="true" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" />
                            <tstsc:BoundFieldEx DataField="Type.Name" HeaderText="<%$Resources:Fields,Type %>" HtmlEncode="true" ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4" />
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Default %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx 
                                        GroupName='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty) Container.DataItem).Name %>'
                                        ID="chkDefault" 
                                        MetaData='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty) Container.DataItem).CustomPropertyId %>'
                                        runat="server" 
                                        />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Hidden %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx
                                        GroupName='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty) Container.DataItem).Name %>'
                                        ID="chkHidden" 
                                        MetaData='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty) Container.DataItem).CustomPropertyId %>'
                                        runat="server" 
                                        />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Disabled %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx 
                                        GroupName='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty) Container.DataItem).Name %>'
                                        ID="chkDisabled" 
                                        MetaData='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty) Container.DataItem).CustomPropertyId %>'
                                        runat="server" 
                                        />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Required %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx
                                        GroupName='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty) Container.DataItem).Name %>'
                                        ID="chkRequired" 
                                        MetaData='<%# ((Inflectra.SpiraTest.DataModel.CustomProperty) Container.DataItem).CustomPropertyId %>'
                                        runat="server" 
                                        />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                        </Columns>
                    </tstsc:GridViewEx>
                </div>
                <div class="mt4 btn-group">
                    <tstsc:ButtonEx ID="btnSave2" runat="server" Text="<%$Resources:Buttons,Save %>" SkinID="ButtonPrimary" />
                    <tstsc:HyperLinkEx ID="HyperLinkEx1" runat="server" NavigateUrl='<%# UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "DocumentWorkflowDetails") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString()%>' Text="<%$Resources:Buttons,Cancel %>" CssClass="btn btn-default" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
