<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="ReportDetails.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ReportDetails" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_ReportDetails %>" />
                    <small>
                        <tstsc:LabelEx CssClass="SubTitle" ID="lblReportName" runat="server" />
                    </small>
                </h2>
                <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkBackToList" runat="server" NavigateUrl="Reports.aspx">
                    <span class="fas fa-arrow-left"></span>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ReportDetails_BackToList %>" />
                </tstsc:HyperLinkEx>
                <p class="my4">
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_ReportDetails_Intro %>" />
                </p>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
            </div>
        </div>
        <div class="row data-entry-wide DataEntryForm">
            <div class="col-lg-12 col-sm-12">
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="txtNameLabel" runat="server" AssociatedControlID="txtName" Required="true" AppendColon="true" Text="<%$Resources:Fields,Name %>"/>
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
                        <tstsc:TextBoxEx ID="txtName" runat="server" MaxLength="50" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="txtDescriptionLabel" runat="server" AssociatedControlID="txtDescription" Required="false" AppendColon="true" Text="<%$Resources:Fields,Description %>"/>
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
                        <tstsc:TextBoxEx ID="txtDescription" runat="server" TextMode="MultiLine" RichText="false" Width="100%" Height="80px"/>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="txtHeaderLabel" runat="server" AssociatedControlID="txtHeader" Required="false" AppendColon="true" Text="<%$Resources:Fields,Header %>"/>
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
                        <tstsc:RichTextBoxJ ID="txtHeader" runat="server" Height="80px"/>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="txtFooterLabel" runat="server" AssociatedControlID="txtFooter" Required="false" AppendColon="true" Text="<%$Resources:Fields,Footer %>"/>
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
                        <tstsc:RichTextBoxJ ID="txtFooter" runat="server" Height="80px"/>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="chkActiveLabel" runat="server" AssociatedControlID="chkActive" Required="true" AppendColon="true" Text="<%$Resources:Fields,ActiveYn %>"/>
                    </div>
                    <div class="DataEntry col-sm-9">
                        <tstsc:CheckBoxYnEx ID="chkActive" runat="server" />
                    </div>
                </div>
                <div class="form-group row mb5">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="ddlCategoryLabel" runat="server" AssociatedControlID="ddlCategory" Required="true" AppendColon="true" Text="<%$Resources:Fields,Category %>"/>
                    </div>
                    <div class="DataEntry col-sm-9">
                        <tstsc:DropDownListEx ID="ddlCategory" runat="server" DataTextField="Name" DataValueField="ReportCategoryId" />
                    </div>
                </div>
                <div class="form-group row mb5">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="lblFormats" runat="server" Required="true" AppendColon="true" Text="<%$Resources:Fields,Formats %>"/>
                    </div>
                    <div class="DataEntry col-sm-9">
                        <tstsc:GridViewEx ID="grdFormats" runat="server" SkinID="DataGrid" DataKeyNames="ReportFormatId">
                            <Columns>
                                <tstsc:TemplateFieldEx HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                                    <ItemTemplate>
                                        <tstsc:CheckBoxEx ID="chkFormat" runat="server" MetaData="<%#((ReportFormat) Container.DataItem).ReportFormatId%>" />
                                    </ItemTemplate>
                                </tstsc:TemplateFieldEx>
                                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Format %>" HeaderColumnSpan="2"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                                    <ItemTemplate>
                                        <tstsc:ImageEx CssClass="w4 h4" runat="server" ID="imgIcon" ImageUrl='<%# "Images/Filetypes/" + ((ReportFormat) Container.DataItem).IconFilename%>' AlternateText="<%#((ReportFormat) Container.DataItem).IconFilename%>" />
                                    </ItemTemplate>
                                </tstsc:TemplateFieldEx>
                                <tstsc:TemplateFieldEx HeaderColumnSpan="-1"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                                    <ItemTemplate>
                                        <%#((ReportFormat) Container.DataItem).Name%>
                                    </ItemTemplate>
                                </tstsc:TemplateFieldEx>
                            </Columns>
                        </tstsc:GridViewEx>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="lblStandardSections" runat="server" Required="true" AppendColon="true" Text="<%$Resources:Main,Admin_ReportDetails_StandardSections %>"/>
                    </div>
                    <div class="DataEntry col-sm-9">
                        <div style="max-width: 500px">
                            <tstsc:GridViewEx ID="grdStandardSections" runat="server" Width="100%" SkinID="DataGrid" ShowHeader="true" ShowFooter="true" DataKeyNames="ReportSectionId">
                                <FooterStyle CssClass="Highlighted" />
                                <Columns>
                                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" HeaderColumnSpan="2" FooterColumnSpan="4" FooterStyle-Font-Bold="true" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" FooterStyle-CssClass="priority1">
                                        <ItemTemplate>
                                            <tstsc:ImageEx 
                                                CssClass="w4 h4"
                                                ID="imgIcon" 
                                                runat="server" 
                                                ImageUrl='<%# (((ReportSectionInstance) Container.DataItem).Section.ArtifactTypeId.HasValue) ? "Images/" + GlobalFunctions.GetIconForArtifactType((Artifact.ArtifactTypeEnum)((ReportSectionInstance) Container.DataItem).Section.ArtifactTypeId.Value) : "Images/Filetypes/Unknown.svg" %>' 
                                                />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <tstsc:HyperLinkEx ID="lnkAddReport" SkinID="ButtonLinkAdd" runat="server" Text="<%$Resources:Main,Admin_ReportDetails_AddNewStandardSection %>" NavigateUrl="javascript:void(0)" ClientScriptMethod="dlgStandardSection_load(event)" />
                                        </FooterTemplate>
                                    </tstsc:TemplateFieldEx>
                                    <tstsc:TemplateFieldEx HeaderColumnSpan="-1" FooterColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                                        <ItemTemplate>
                                            <tstsc:LabelEx ID="lblSectionName" runat="server" Text="<%#((ReportSectionInstance) Container.DataItem).Section.Name%>" Tooltip="<%#((ReportSectionInstance) Container.DataItem).Section.Description%>" />
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-HorizontalAlign="Center" FooterColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                                        <ItemTemplate>
                                            <asp:PlaceHolder ID="plcActive" runat="server" Visible="<%#((ReportSectionInstance) Container.DataItem).Section.IsActive%>">
                                                <span class="fas fa-check"></span>
                                            </asp:PlaceHolder>
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" FooterColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                                        <ItemTemplate>
                                            <div class="btn-group">
                                                <tstsc:HyperLinkEx ID="lnkCustomize" SkinID="ButtonDefault" runat="server" NavigateUrl='javascript:void(0)' ClientScriptMethod='<%#"dlgStandardSection_load(event," + ((ReportSectionInstance)Container.DataItem).ReportSectionId + ")"%>'>
                                                    <span class="far fa-edit"></span>
                                                    <asp:Localize runat="server" Text="<%$Resources:Buttons,Customize %>" />
                                                </tstsc:HyperLinkEx>
                                                <tstsc:HyperLinkEx ID="lnkDelete" SkinID="ButtonDefault" runat="server" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_ReportDetails_DeleteSectionConfirm %>"
                                                    NavigateUrl="javascript:void(0)" ClientScriptMethod='<%#"grdStandardSections_delete(" + ((ReportSectionInstance) Container.DataItem).ReportSectionId + ")" %>'>
                                                    <span class="fas fa-trash-alt"></span>
                                                    <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
                                                </tstsc:HyperLinkEx>
                                            </div>
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                </Columns>
                            </tstsc:GridViewEx>
                        </div>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="lblCustomSections" runat="server" Required="true" AppendColon="true" Text="<%$Resources:Main,Admin_ReportDetails_CustomSections %>"/>
                    </div>
                    <div class="DataEntry col-sm-9">
                        <div style="max-width: 500px">
                            <tstsc:GridViewEx ID="grdCustomSections" runat="server" SkinID="DataGrid" Width="100%" ShowHeader="true" ShowFooter="true" DataKeyNames="ReportCustomSectionId">
                                <FooterStyle CssClass="Highlighted" />
                                <Columns>
                                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" HeaderColumnSpan="2" FooterColumnSpan="4" FooterStyle-Font-Bold="true" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" FooterStyle-CssClass="priority1">
                                        <ItemTemplate>
                                            <tstsc:ImageEx ID="imgIcon" CssClass="w4 h4" runat="server" ImageUrl="Images/Filetypes/Unknown.svg" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <tstsc:HyperLinkEx ID="lnkAddReport" runat="server" SkinID="ButtonLinkAdd" Text="<%$Resources:Main,Admin_ReportDetails_AddNewCustomSection %>" NavigateUrl="javascript:void(0)" ClientScriptMethod="dlgCustomSection_load(event)" />
                                        </FooterTemplate>
                                    </tstsc:TemplateFieldEx>
                                    <tstsc:TemplateFieldEx HeaderColumnSpan="-1" FooterColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                                        <ItemTemplate>
                                            <tstsc:LabelEx ID="lblSectionName" runat="server" Text="<%#((ReportCustomSection) Container.DataItem).Name%>" Tooltip="<%#((ReportCustomSection) Container.DataItem).Description%>" />
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-HorizontalAlign="Center" FooterColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                                        <ItemTemplate>
                                            <asp:PlaceHolder ID="plcActive" runat="server" Visible="<%#((ReportCustomSection) Container.DataItem).IsActive%>">
                                                <span class="fas fa-check"></span>
                                            </asp:PlaceHolder>
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" FooterColumnSpan="-1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                                        <ItemTemplate>
                                             <div class="btn-group">
                                                <tstsc:HyperLinkEx ID="lnkEdit" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptMethod='<%#"dlgCustomSection_load(event," + ((ReportCustomSection) Container.DataItem).ReportCustomSectionId + ")"%>'>
                                                    <span class="far fa-edit"></span>
                                                    <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>" />
                                                </tstsc:HyperLinkEx>
                                                <tstsc:HyperLinkEx ID="lnkDelete" SkinID="ButtonDefault" runat="server" CausesValidation="false" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_ReportDetails_DeleteSectionConfirm %>"
                                                    NavigateUrl="javascript:void(0)" ClientScriptMethod='<%#"grdCustomSections_delete(" + ((ReportCustomSection) Container.DataItem).ReportCustomSectionId + ")" %>'>
                                                    <span class="fas fa-trash-alt"></span>
                                                    <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
                                                </tstsc:HyperLinkEx>
                                            </div>
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                </Columns>
                            </tstsc:GridViewEx>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <asp:HiddenField ID="hdnStandardSections" runat="server" EnableViewState="false" />
                    <asp:HiddenField ID="hdnCustomSections" runat="server" EnableViewState="false" />
                    <div class="col-sm-offset-3 col-lg-offset-2 btn-group pl3">
                        <tstsc:ButtonEx ID="btnUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save %>" />
                        <tstsc:ButtonEx ID="btnDeactivate" runat="server" Text="<%$Resources:Buttons,Deactivate %>" />
                        <tstsc:ButtonEx ID="btnActivate" runat="server" Text="<%$Resources:Buttons,Activate %>" />
                        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" />
                    </div>
                </div>

            </div>
        </div>
    </div>
    <tstsc:DialogBoxPanel ID="dlgStandardSection" runat="server" Title="<%$Resources:Main,ReportDetails_AddEditStandardSection %>"
        Modal="true">
        <div id="dlgStandardSection_container" style="overflow: auto; width: 85vw; padding-left: 10px; max-height: calc(100vh - 150px); /* to allow max screen height - as cannot set overflow on dialog*/">
            <div class="dib w7 v-top mb4">
                <tstsc:LabelEx ID="ddlStandardSectionNameLabel" runat="server" AssociatedControlID="ddlStandardSectionName" Required="true" AppendColon="true" Text="<%$Resources:Fields,Name %>"/>
            </div>
            <div class="dib">
                <tstsc:DropDownListEx ID="ddlStandardSectionName" runat="server" NoValueItem="false" DataValueField="ReportSectionId" DataTextField="Name"
                    Width="300px" ClientScriptMethod="populateStandardDescription" />
                <div class="display-inline-block mx3">
                    <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkStandardSectionGenerateTemplate" runat="server" Text="<%$Resources:Dialogs,Admin_ReportDetails_CreateDefaultTemplate %>" NavigateUrl="javascript:void(0)" ClientScriptMethod="populateStandardTemplate()" />
                </div>
                <tstsc:MessageBox ID="lblStandardSectionMessage" runat="server" SkinID="MessageBox" />
            </div>
           
            <div>
                <div class="dib w7 v-top mb4">
                    <tstsc:LabelEx CssClass="fw-b" ID="lblStandardSectionDescriptionLabel" runat="server" AssociatedControlID="lblStandardSectionDescription" Required="false" AppendColon="true" Text="<%$Resources:Fields,Description %>"/>
                </div>
                <div class="dib">
                    <tstsc:LabelEx ID="lblStandardSectionDescription" runat="server" />
                </div>
            </div>
            <div>
                <tstsc:LabelEx CssClass="fw-b" ID="txtStandardSectionHeaderLabel" runat="server" AssociatedControlID="txtStandardSectionHeader" Required="false" AppendColon="true" Text="<%$Resources:Fields,Header %>"/>
                <tstsc:RichTextBoxJ ID="txtStandardSectionHeader" runat="server" Height="80px"/>
                <tstsc:LabelEx CssClass="fw-b" ID="txtStandardSectionFooterLabel" runat="server" AssociatedControlID="txtStandardSectionFooter" Required="false" AppendColon="true" Text="<%$Resources:Fields,Footer %>"/>
                <tstsc:RichTextBoxJ ID="txtStandardSectionFooter" runat="server" Height="80px"/>
                <tstsc:LabelEx CssClass="fw-b" ID="txtStandardSectionTemplateLabel" runat="server" AssociatedControlID="txtStandardSectionTemplate" Required="false" AppendColon="true" Text="<%$Resources:Fields,Template %>"/>
                <tstsc:TextBoxEx ID="txtStandardSectionTemplate" runat="server" TextMode="MultiLine" RichText="false" Width="98%" Height="100px"/>
                <p>
                    <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Dialogs,Admin_ReportDetails_StandardTemplateLegend %>" />
                </p>
            </div>
        </div>
        <div class="btn-group ml3 mt4 mb3">
            <tstsc:ButtonEx ID="btnStandardSectionUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save %>" ClientScriptMethod="dlgStandardSection_update()" />
            <tstsc:ButtonEx ID="btnStandardSectionCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" ClientScriptServerControlId="dlgStandardSection" ClientScriptMethod="close()" />
        </div>
    </tstsc:DialogBoxPanel>

    <tstsc:DialogBoxPanel ID="dlgCustomSection" runat="server" Title="<%$Resources:Main,ReportDetails_AddEditCustomSection %>"
        Modal="true">
        <div id="dlgCustomSection_container" style="overflow: auto; width: 85vw; padding-left: 10px; max-height: calc(100vh - 150px); /* to allow max screen height - as cannot set overflow on dialog*/">
            <div class="dib w7 v-top mb4">
                <tstsc:LabelEx ID="txtCustomSectionNameLabel" runat="server" AssociatedControlID="txtCustomSectionName" Required="true" AppendColon="true" Text="<%$Resources:Fields,Name %>"/>
            </div>
            <div class="dib w9">
                <asp:HiddenField runat="server" ID="hdnReportCustomSectionId" />
                <tstsc:TextBoxEx ID="txtCustomSectionName" runat="server" MaxLength="100" Width="100%"/>
            </div>
           
            <div>
                <tstsc:LabelEx CssClass="db fw-b mt3 mb0" ID="txtCustomSectionDescriptionLabel" runat="server" AssociatedControlID="txtCustomSectionDescription" Required="false" AppendColon="true" Text="<%$Resources:Fields,Description %>"/>
                <tstsc:TextBoxEx ID="txtCustomSectionDescription" runat="server" TextMode="MultiLine" RichText="false" Width="98%" Height="60px"/>
                <tstsc:LabelEx CssClass="db fw-b mt3 mb0" ID="txtCustomSectionHeaderLabel" runat="server" AssociatedControlID="txtCustomSectionHeader" Required="false" AppendColon="true" Text="<%$Resources:Fields,Header %>"/>
                <tstsc:RichTextBoxJ ID="txtCustomSectionHeader" runat="server" Height="80px" CssClass="mb4"/>
                <tstsc:LabelEx CssClass="db fw-b mt3 mb0" ID="txtCustomSectionFooterLabel" runat="server" AssociatedControlID="txtCustomSectionFooter" Required="false" AppendColon="true" Text="<%$Resources:Fields,Footer %>"/>
                <tstsc:RichTextBoxJ ID="txtCustomSectionFooter" runat="server" Height="80px"/>
                <div>
                    <tstsc:LabelEx CssClass="fw-b mt3 w7" ID="chkCustomSectionActiveLabel" runat="server" AssociatedControlID="chkCustomSectionActive" Required="true" AppendColon="true" Text="<%$Resources:Fields,ActiveYn %>"/>
                    <tstsc:CheckBoxEx ID="chkCustomSectionActive" runat="server" />
                </div>
                
                <div>
                    <tstsc:LabelEx CssClass="fw-b mt3 w7" ID="txtCustomSectionQueryLabel" runat="server" AssociatedControlID="txtCustomSectionQuery" Required="true" AppendColon="true" Text="<%$Resources:Fields,Query %>"/>
                    <div class="btn-group">
                        <tstsc:DropDownListEx ID="ddlCustomSectionQueryNew" runat="server" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Admin_ReportDetails_AddNewQuery %>" DataValueField="Key" DataTextField="Value" Width="200px" ClientScriptMethod="ddlCustomSectionQueryNew_changed" />
                        <tstsc:HyperLinkEx ID="lnkCustomSectionQueryPreviewResults" runat="server"  NavigateUrl="javascript:void(0)" ClientScriptMethod="previewResults()" SkinID="ButtonDefault">
                            <span class="far fa-eye"></span>
                            <asp:Localize runat="server" Text="<%$Resources:Dialogs,Admin_ReportDetails_PreviewResults %>" />
                        </tstsc:HyperLinkEx>
                        <tstsc:HyperLinkEx ID="lnkCustomSectionQueryCreateTemplate" runat="server" NavigateUrl="javascript:void(0)" ClientScriptMethod="createDefaultTemplate()" SkinID="ButtonDefault">
                            <span class="fas fa-plus"></span>
                            <asp:Localize runat="server" Text="<%$Resources:Dialogs,Admin_ReportDetails_CreateDefaultTemplate %>" />
                        </tstsc:HyperLinkEx>
                    </div>
                </div>

                <tstsc:MessageBox ID="lblCustomQueryMessage" runat="server" SkinID="MessageBox" />
                <tstsc:TextBoxEx ID="txtCustomSectionQuery" runat="server" TextMode="MultiLine" RichText="false" Width="98%" Height="100px"/>
                <p class="alert alert-warning mt3">
                    <span class="fas fa-info-circle"></span>
                    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Dialogs,Admin_ReportDetails_CustomQueryLegend %>" />
                    <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Dialogs,Admin_ReportDetails_CustomQueryLegend2 %>" />
                </p>
                
                <tstsc:LabelEx CssClass="db fw-b mt4 mb0" ID="txtCustomSectionTemplateLabel" runat="server" AssociatedControlID="txtCustomSectionTemplate" Required="true" AppendColon="true" Text="<%$Resources:Fields,Template %>"/>
                <tstsc:TextBoxEx ID="txtCustomSectionTemplate" runat="server" TextMode="MultiLine" RichText="false" Width="98%" Height="100px"/>
                <p class="alert alert-warning mt3">
                    <span class="fas fa-info-circle"></span>
                    <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Dialogs,Admin_ReportDetails_CustomTemplateLegend %>" />
                </p>
            </div>
        </div>
        <div class="btn-group ml3 mt4 mb3">
            <tstsc:ButtonEx ID="btnCustomSectionUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save %>" ClientScriptMethod="dlgCustomSection_update()" />
            <tstsc:ButtonEx ID="btnCustomSectionCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" ClientScriptServerControlId="dlgCustomSection" ClientScriptMethod="close()" />
        </div>
    </tstsc:DialogBoxPanel>

    <tstsc:DialogBoxPanel ID="dlgCustomQueryResults" runat="server" Title="<%$Resources:Main,ReportDetails_CustomQueryResults %>"
        Modal="false" Width="700px" Height="300px">
        <div id="divCustomQueryResults" style="overflow-y: scroll; overflow-x: auto; height:270px; padding-left: 10px">
        </div>
        <div class="btn-group mt4 pl4">
            <tstsc:ButtonEx ID="btnCustomQueryResultsOK" runat="server" Text="<%$Resources:Buttons,OK %>" ClientScriptServerControlId="dlgCustomQueryResults" ClientScriptMethod="close()" />
        </div>
    </tstsc:DialogBoxPanel>
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/ReportsService.svc" />
        </Services>
    </tstsc:ScriptManagerProxyEx>

    <script type="text/javascript">
        /* Global Variables */
        var resx = Inflectra.SpiraTest.Web.GlobalResources;
        var g_standardSections = <%=StandardSectionsJson %>;
        var g_customSections = <%=CustomSectionsJson %>;
        var g_nextCustomSectionId = -1;

        /* Page Functions */

        function reportDetails_onSubmit()
        {
            //Make sure the report name is populated
            var txtName = $get('<%=txtName.ClientID %>');
            if (txtName.value == '')
            {
                alert (resx.ReportDetails_NameRequired);
                return false;
            }

            //Serialize the two arrays in hidden fields
            var hdnStandardSections  = $get('<%=hdnStandardSections.ClientID %>');
            var hdnCustomSections  = $get('<%=hdnCustomSections.ClientID %>');

            var standardSectionsJson = Sys.Serialization.JavaScriptSerializer.serialize(g_standardSections);
            hdnStandardSections.value = standardSectionsJson;
            var customSectionsJson = Sys.Serialization.JavaScriptSerializer.serialize(g_customSections);
            hdnCustomSections.value = customSectionsJson;
            return true;
        }

        /* Standard Sections */
        function dlgStandardSection_load(evt, reportSectionId)
        {
            //Tell the dropdowns that there is a scrollable element
            var ddlStandardSectionName = $find('<%=ddlStandardSectionName.ClientID %>');
            ddlStandardSectionName.set_scrollableParent($get('dlgStandardSection_container'));

            //Display the dialog
            var dlgStandardSection = $find('<%=dlgStandardSection.ClientID %>');
            dlgStandardSection.display(evt);

            //Either fill the fields or blank the fields
            if (reportSectionId)
            {
                //Find the matching report section
                var matchedSection = findStandardSection(reportSectionId);
                if (matchedSection)
                {
                    $find('<%=ddlStandardSectionName.ClientID %>').set_selectedItem(matchedSection.ReportSectionId);
                    CKEDITOR.instances['<%=txtStandardSectionHeader.ClientID %>'].setData(matchedSection.Header);
                    CKEDITOR.instances['<%=txtStandardSectionFooter.ClientID %>'].setData(matchedSection.Footer);
                    $get('<%=txtStandardSectionTemplate.ClientID %>').value = matchedSection.Template;
                }
            }
            else
            {
                CKEDITOR.instances['<%=txtStandardSectionHeader.ClientID%>'].setData('');
                CKEDITOR.instances['<%=txtStandardSectionFooter.ClientID%>'].setData('');
                $get('<%=txtStandardSectionTemplate.ClientID%>').value = '';
            }

            // handle ckeditor color mode
            ckeditorSetColorScheme();
        }
        function dlgStandardSection_update()
        {
            var reportSectionId = $find('<%=ddlStandardSectionName.ClientID %>').get_selectedItem().get_value();
            var sectionName = $find('<%=ddlStandardSectionName.ClientID %>').get_selectedItem().get_text();
            if (reportSectionId && reportSectionId != '')
            {
                //Find the matching report section
                var matchedSection = findStandardSection(reportSectionId);
                if (matchedSection)
                {
                    //Update the data item
                    matchedSection.Header = CKEDITOR.instances['<%=txtStandardSectionHeader.ClientID %>'].getData();
                    matchedSection.Footer = CKEDITOR.instances['<%=txtStandardSectionFooter.ClientID %>'].getData();
                    matchedSection.Template = $get('<%=txtStandardSectionTemplate.ClientID %>').value;

                    //Update the grid
                    var grdStandardSections = $get('<%=grdStandardSections.ClientID %>');
                    /*  Not needed since nothing needs to get updated
                    for (var i = 1; i < grdStandardSections.rows.length - 1; i++)
                    {
                        var tr = grdStandardSections.rows[i];
                        if (tr.getAttribute('tst:primaryKey') == reportSectionId)
                        {
                            //Name
                            var td =  tr.cells[1];
                            globalFunctions.clearContent(td);
                            td.appendChild(document.createTextNode());
                            break;
                        }
                    }*/
                }
                else
                {
                    //Add the data item
                    matchedSection = {};
                    matchedSection.ReportSectionId = reportSectionId;
                    matchedSection.Header = CKEDITOR.instances['<%=txtStandardSectionHeader.ClientID %>'].getData();
                    matchedSection.Footer = CKEDITOR.instances['<%=txtStandardSectionFooter.ClientID %>'].getData();                    matchedSection.Template = $get('<%=txtStandardSectionTemplate.ClientID %>').value;
                    g_standardSections.push(matchedSection);

                    //Add to the grid
                    var grdStandardSections = $get('<%=grdStandardSections.ClientID %>');
                    var tr = grdStandardSections.insertRow(grdStandardSections.rows.length - 1);
                    tr.setAttribute('tst:primaryKey', reportSectionId);
                    //Icon
                    var td = document.createElement('td');
                    tr.appendChild(td);
                    //Name
                    td = document.createElement('td');
                    td.appendChild(document.createTextNode(sectionName));
                    tr.appendChild(td);
                    //Active
                    td = document.createElement('td');
                    tr.appendChild(td);
                    //Operations
                    td = document.createElement('td');
                    var div = document.createElement('div');
                    td.appendChild(div);
                    div.className = 'btn-group';
                    var a = document.createElement('a');
                    a.className = 'btn btn-default';
                    var span = document.createElement('span');
                    span.className = 'far fa-edit';
                    span.appendChild(document.createTextNode('\xa0'));
                    a.appendChild(span);
                    a.appendChild(document.createTextNode(resx.Global_Customize));
                    a.href = 'javascript:dlgStandardSection_load(null,' + reportSectionId + ')';
                    div.appendChild(a);
                    a = document.createElement('a');
                    a.className = 'btn btn-default';
                    span = document.createElement('span');
                    span.className = 'fas fa-trash-alt';
                    span.appendChild(document.createTextNode('\xa0'));
                    a.appendChild(span);
                    a.appendChild(document.createTextNode(resx.Global_Delete));
                    a.href = 'javascript:grdStandardSections_delete(' + reportSectionId + ')';
                    div.appendChild(a);
                    tr.appendChild(td);
                }
            }

            //Close the dialog
            var dlgStandardSection = $find('<%=dlgStandardSection.ClientID %>');
            dlgStandardSection.close();
        }
        function grdStandardSections_delete(reportSectionId)
        {
            //Find the matching report section
            var matchedSection = findStandardSection(reportSectionId);
            var matchedSectionIndex = findStandardSectionIndex(reportSectionId);
            if (matchedSection && matchedSectionIndex != -1)
            {
                //Delete the item from the array
                g_standardSections.splice(matchedSectionIndex, 1);

                //Remove the item from the grid
                var grdStandardSections = $get('<%=grdStandardSections.ClientID %>');
                for (var i = 1; i < grdStandardSections.rows.length - 1; i++)
                {
                    var tr = grdStandardSections.rows[i];
                    if (tr.getAttribute('tst:primaryKey') == reportSectionId)
                    {
                        //Delete the row
                        grdStandardSections.deleteRow(i);
                        break;
                    }
                }

            }
        }
        function findStandardSection(reportSectionId)
        {
            //Find the matching report section
            var matchedSection = null;
            for (var i = 0; i < g_standardSections.length; i++)
            {
                if (g_standardSections[i].ReportSectionId == reportSectionId)
                {
                    matchedSection = g_standardSections[i];
                    return matchedSection;
                }
            }
            return null;
        }
        function findStandardSectionIndex(reportSectionId)
        {
            //Find the matching report section
            for (var i = 0; i < g_standardSections.length; i++)
            {
                if (g_standardSections[i].ReportSectionId == reportSectionId)
                {
                    return i;
                }
            }
            return -1;
        }

        /* Custom Sections */
        function dlgCustomSection_load(evt, reportCustomSectionId)
        {
            //Tell the dropdowns that there is a scrollable element
            var ddlCustomSectionQueryNew = $find('<%=ddlCustomSectionQueryNew.ClientID %>');
            ddlCustomSectionQueryNew.set_scrollableParent($get('dlgCustomSection_container'));

            //Display the dialog
            var dlgCustomSection = $find('<%=dlgCustomSection.ClientID %>');
            dlgCustomSection.display(evt);

            //Reset the query dropdown
            $find('<%=ddlCustomSectionQueryNew.ClientID %>').set_selectedItem('');

            //Either fill the fields or blank the fields
            if (reportCustomSectionId)
            {
                //Find the matching report section
                var matchedSection = findCustomSection(reportCustomSectionId);
                if (matchedSection)
                {
                    $get('<%=hdnReportCustomSectionId.ClientID %>').value = reportCustomSectionId;
                    $get('<%=txtCustomSectionName.ClientID%>').value = matchedSection.Name;
                    $get('<%=txtCustomSectionDescription.ClientID%>').value = matchedSection.Description;
                    CKEDITOR.instances['<%=txtCustomSectionHeader.ClientID %>'].setData(matchedSection.Header);
                    CKEDITOR.instances['<%=txtCustomSectionFooter.ClientID %>'].setData(matchedSection.Footer);
                    $get('<%=txtCustomSectionTemplate.ClientID %>').value = matchedSection.Template;
                    $get('<%=txtCustomSectionQuery.ClientID %>').value = matchedSection.Query;
                    $get('<%=chkCustomSectionActive.ClientID %>').checked = matchedSection.IsActive;
                }
            }
            else
            {
                $get('<%=hdnReportCustomSectionId.ClientID %>').value = '';
                $get('<%=txtCustomSectionName.ClientID%>').value = '';
                $get('<%=txtCustomSectionDescription.ClientID%>').value = '';
                CKEDITOR.instances['<%=txtCustomSectionHeader.ClientID%>'].setData('');
                CKEDITOR.instances['<%=txtCustomSectionFooter.ClientID%>'].setData('');
                $get('<%=txtCustomSectionTemplate.ClientID%>').value = '';
                $get('<%=txtCustomSectionQuery.ClientID%>').value = '';
                $get('<%=chkCustomSectionActive.ClientID %>').checked = true;
            }

            // handle ckeditor color mode
            ckeditorSetColorScheme();
        }
        function dlgCustomSection_update()
        {
            var reportCustomSectionId = $get('<%=hdnReportCustomSectionId.ClientID %>').value;
            var sectionName = $get('<%=txtCustomSectionName.ClientID %>').value;

            //Make sure a name was entered
            if (sectionName == '')
            {
                alert (resx.ReportDetails_NameRequired);
                return;
            }

            //Find the matching report section
            var matchedSection = findCustomSection(reportCustomSectionId);
            if (matchedSection)
            {
                //Update the data item
                matchedSection.Name = $get('<%=txtCustomSectionName.ClientID %>').value;
                matchedSection.Description = $get('<%=txtCustomSectionDescription.ClientID %>').value;
                matchedSection.IsActive = $get('<%=chkCustomSectionActive.ClientID %>').checked;
                matchedSection.Header = CKEDITOR.instances['<%=txtCustomSectionHeader.ClientID %>'].getData();
                matchedSection.Footer = CKEDITOR.instances['<%=txtCustomSectionFooter.ClientID %>'].getData();
                matchedSection.Template = $get('<%=txtCustomSectionTemplate.ClientID %>').value;
                matchedSection.Query = $get('<%=txtCustomSectionQuery.ClientID %>').value;

                //Update the grid
                var grdCustomSections = $get('<%=grdCustomSections.ClientID %>');
                for (var i = 1; i < grdCustomSections.rows.length - 1; i++)
                {
                    var tr = grdCustomSections.rows[i];
                    if (tr.getAttribute('tst:primaryKey') == reportCustomSectionId)
                    {
                        //Name
                        var td =  tr.cells[1];
                        globalFunctions.clearContent(td);
                        td.appendChild(document.createTextNode(matchedSection.Name));
                        break;
                    }
                }
            }
            else
            {
                //Get the next custom section id (negative to distinguish from those in the database)
                var newReportCustomSectionId = g_nextCustomSectionId;
                g_nextCustomSectionId--;

                //Add the data item
                matchedSection = {};
                matchedSection.ReportCustomSectionId = newReportCustomSectionId;
                matchedSection.Name = $get('<%=txtCustomSectionName.ClientID %>').value;
                matchedSection.Description = $get('<%=txtCustomSectionDescription.ClientID %>').value;
                matchedSection.IsActive = $get('<%=chkCustomSectionActive.ClientID %>').checked;
                matchedSection.Header = CKEDITOR.instances['<%=txtCustomSectionHeader.ClientID %>'].getData();
                matchedSection.Footer = CKEDITOR.instances['<%=txtCustomSectionFooter.ClientID %>'].getData();
                matchedSection.Template = $get('<%=txtCustomSectionTemplate.ClientID %>').value;
                matchedSection.Query = $get('<%=txtCustomSectionQuery.ClientID %>').value;
                g_customSections.push(matchedSection);

                //Add to the grid
                var grdCustomSections = $get('<%=grdCustomSections.ClientID %>');
                var tr = grdCustomSections.insertRow(grdCustomSections.rows.length - 1);
                tr.setAttribute('tst:primaryKey', newReportCustomSectionId);
                //Icon
                var td = document.createElement('td');
                tr.appendChild(td);
                //Name
                td = document.createElement('td');
                td.appendChild(document.createTextNode(sectionName));
                tr.appendChild(td);
                //Active
                td = document.createElement('td');
                tr.appendChild(td);
                //Operations
                td = document.createElement('td');
                var div = document.createElement('div');
                div.className = 'btn-group';
                td.appendChild(div);
                var a = document.createElement('a');
                a.className = 'btn btn-default';
                var span = document.createElement('span');
                span.className = 'far fa-edit';
                span.appendChild(document.createTextNode('\xa0'));
                a.appendChild(span);
                a.appendChild(document.createTextNode(resx.Global_Edit));
                a.href = 'javascript:dlgCustomSection_load(null,' + newReportCustomSectionId + ')';
                div.appendChild(a);
                a = document.createElement('a');
                a.className = 'btn btn-default';                
                span = document.createElement('span');
                span.className = 'fas fa-trash-alt';
                span.appendChild(document.createTextNode('\xa0'));
                a.appendChild(span);
                a.appendChild(document.createTextNode(resx.Global_Delete));
                a.href = 'javascript:grdCustomSections_delete(' + newReportCustomSectionId + ')';
                div.appendChild(a);
                tr.appendChild(td);
            }

            //Close the dialog
            var dlgCustomSection = $find('<%=dlgCustomSection.ClientID %>');
            dlgCustomSection.close();
        }
        function grdCustomSections_delete(reportCustomSectionId)
        {
            //Find the matching report section
            var matchedSection = findCustomSection(reportCustomSectionId);
            var matchedSectionIndex = findCustomSectionIndex(reportCustomSectionId);
            if (matchedSection && matchedSectionIndex != -1)
            {
                //Delete the item from the array
                g_customSections.splice(matchedSectionIndex, 1);

                //Remove the item from the grid
                var grdCustomSections = $get('<%=grdCustomSections.ClientID %>');
                for (var i = 1; i < grdCustomSections.rows.length - 1; i++)
                {
                    var tr = grdCustomSections.rows[i];
                    if (tr.getAttribute('tst:primaryKey') == reportCustomSectionId)
                    {
                        //Delete the row
                        grdCustomSections.deleteRow(i);
                        break;
                    }
                }
            }
        }
        function findCustomSection(reportCustomSectionId)
        {
            //Find the matching report section
            var matchedSection = null;
            for (var i = 0; i < g_customSections.length; i++)
            {
                if (g_customSections[i].ReportCustomSectionId == reportCustomSectionId)
                {
                    matchedSection = g_customSections[i];
                    return matchedSection;
                }
            }
            return null;
        }
        function findCustomSectionIndex(reportCustomSectionId)
        {
            //Find the matching report section
            for (var i = 0; i < g_customSections.length; i++)
            {
                if (g_customSections[i].ReportCustomSectionId == reportCustomSectionId)
                {
                    return i;
                }
            }
            return -1;
        }

        function ddlCustomSectionQueryNew_changed(item)
        {
            //Insert the standard Entity SQL select clause for this reportable entity
            if (item && item.get_value() != '')
            {
                var entitySqlFormat = '<%=Inflectra.SpiraTest.Business.ReportManager.REPORT_ENTITY_SQL_FORMAT %>';
                var defaultSql = entitySqlFormat.replace('{0}', item.get_value());
                var txtCustomSectionQuery = $get('<%=txtCustomSectionQuery.ClientID %>');
                txtCustomSectionQuery.value += defaultSql + '\n';
            }
        }
        //Preview the query results
        function previewResults()
        {
            //Make sure we have a query specified
            var txtCustomSectionQuery = $get('<%=txtCustomSectionQuery.ClientID %>');
            var sql = txtCustomSectionQuery.value;
            if (sql == '')
            {
                alert (resx.ReportDetails_NeedToSpecifyCustomQuery);
            }

            var projectId = <%=ProjectId %>;
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.ReportsService.Reports_RetrieveCustomQueryData(projectId, sql, Function.createDelegate(this, this.previewResults_success), Function.createDelegate(this, this.previewResults_failure));
        }
        function previewResults_success(results)
        {
            globalFunctions.hide_spinner();

            //Put the results into the DIV container
            var divCustomQueryResults = $get('divCustomQueryResults');
            divCustomQueryResults.innerHTML = results;

            //Display the dialog (position above the other dialog box)
            var dlgCustomSection = $find('<%=dlgCustomSection.ClientID %>');
            var dlgCustomQueryResults = $find('<%=dlgCustomQueryResults.ClientID %>');
            var left = dlgCustomSection.get_left() + 100;
            var top = dlgCustomSection.get_top() + 100;

            dlgCustomQueryResults.set_left(left);
            dlgCustomQueryResults.set_top(top);
            dlgCustomQueryResults.display();
        }
        function previewResults_failure(exception)
        {
            globalFunctions.hide_spinner();
            globalFunctions.display_error($get('<%=lblCustomQueryMessage.ClientID%>'), exception);
        }

        //Create the default XSLT template for the query
        function createDefaultTemplate()
        {
            //Make sure we have a query specified
            var txtCustomSectionQuery = $get('<%=txtCustomSectionQuery.ClientID %>');
            var sql = txtCustomSectionQuery.value;
            if (sql == '')
            {
                alert (resx.ReportDetails_NeedToSpecifyCustomQuery);
            }

            var projectId = <%=ProjectId %>;
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.ReportsService.Reports_RetrieveCustomQueryTemplate(projectId, sql, Function.createDelegate(this, this.createDefaultTemplate_success), Function.createDelegate(this, this.createDefaultTemplate_failure));
        }
        function createDefaultTemplate_success(template)
        {
            globalFunctions.hide_spinner();

            //Put the results into the textbox container
            var txtCustomSectionTemplate = $get('<%=txtCustomSectionTemplate.ClientID%>');
            txtCustomSectionTemplate.value = template;
        }
        function createDefaultTemplate_failure(exception)
        {
            globalFunctions.hide_spinner();
            globalFunctions.display_error($get('<%=lblCustomQueryMessage.ClientID%>'), exception);
        }
        function populateStandardDescription(selectedItem)
        {
            //Get the report section id
            var reportSectionId = selectedItem.get_value();
            if (reportSectionId && reportSectionId != '')
            {
                globalFunctions.display_spinner();
                Inflectra.SpiraTest.Web.Services.Ajax.ReportsService.Reports_RetrieveSectionDescription(reportSectionId, Function.createDelegate(this, this.populateStandardDescription_success), Function.createDelegate(this, this.populateStandardTemplate_failure));
            }        
        }
        function populateStandardDescription_success(desc)
        {
            globalFunctions.hide_spinner();
            var lblStandardSectionDescription = $get('<%=lblStandardSectionDescription.ClientID %>');
            globalFunctions.clearContent(lblStandardSectionDescription);
            if (desc && desc != '')
            {
                lblStandardSectionDescription.appendChild(document.createTextNode(desc));
            }
            else
            {
                lblStandardSectionDescription.appendChild(document.createTextNode('-'));
            }
        }
        function populateStandardTemplate()
        {
            //Get the report section id
            var ddlStandardSectionName = $find('<%=ddlStandardSectionName.ClientID %>');
            var reportSectionId = ddlStandardSectionName.get_selectedItem().get_value();
            if (reportSectionId && reportSectionId != '')
            {
                globalFunctions.display_spinner();
                Inflectra.SpiraTest.Web.Services.Ajax.ReportsService.Reports_RetrieveSectionDefaultTemplate(reportSectionId, Function.createDelegate(this, this.populateStandardTemplate_success), Function.createDelegate(this, this.populateStandardTemplate_failure));
            }
        }
        function populateStandardTemplate_success(template)
        {
            globalFunctions.hide_spinner();
            var txtStandardSectionTemplate = $get('<%=txtStandardSectionTemplate.ClientID %>');
            txtStandardSectionTemplate.value = template;
        }
        function populateStandardTemplate_failure(exception)
        {
            globalFunctions.hide_spinner();
            globalFunctions.display_error($get('<%=lblStandardSectionMessage.ClientID%>'), exception);
        }

        function ckeditorSetColorScheme() {
            setTimeout(function () {
                if (window && window.rct_comp_globalNav && window.rct_comp_globalNav.ckeditorSetColorScheme) {
                    window.rct_comp_globalNav.ckeditorSetColorScheme(document.body.dataset.colorscheme);
                }
            }, 500);
        }
    </script>
</asp:Content>
