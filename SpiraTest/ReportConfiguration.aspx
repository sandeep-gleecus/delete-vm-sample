<%@ Page 
    AutoEventWireup="True"
    CodeBehind="ReportConfiguration.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.ReportConfiguration" 
    Language="c#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
    
    <link href="https://cdnjs.cloudflare.com/ajax/libs/select2/3.5.3/select2.css" rel="stylesheet"/>
    <style type="text/css">
        #cplMainContent_lblMessage{
        display:none;
        }
        .bigdrop.select2-container .select2-results {max-height: 300px;}
        .bigdrop .select2-results {max-height: 300px;}
    </style>
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
	
    <script src="https://code.jquery.com/jquery-1.11.3.min.js"></script>
	<script src="https://cdnjs.cloudflare.com/ajax/libs/select2/3.5.3/select2.js"></script>


    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:SidebarPanel ID="pnlSidebar" runat="server"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.ReportsService"
                BodyHeight="300px" ErrorMessageControlId="lblMessage"
                HeaderCaption="<%$Resources:Main,ReportConfiguration_BackToHome %>">
                <div class="ma3">
                    <tstsc:DataListEx ID="lstRelatedReports" runat="server" ShowHeader="false" ShowFooter="false" BorderStyle="None">
                        <ItemStyle BorderStyle="None" />
                        <SeparatorStyle BorderStyle="None" Height="5px" />
                         <ItemTemplate>
                            <tstsc:HyperLinkEx ID="lnkRelatedreport" runat="server" NavigateUrl='<%# "ReportConfiguration.aspx?" + GlobalFunctions.PARAMETER_REPORT_ID + "=" + ((Report)Container.DataItem).ReportId%>'
                                ToolTip='<%# "<u>" + ((Report)Container.DataItem).Name + "</u><br />" + ((Report)Container.DataItem).Description %>'>
                                <%#:((Report)Container.DataItem).Name%>    
                            </tstsc:HyperLinkEx>
                        </ItemTemplate>
                        <SeparatorTemplate />
                    </tstsc:DataListEx>
                </div>
            </tstsc:SidebarPanel>
        </div>



        <div class="main-panel pl4 grow-1">
            <div class="w-100 pt5 pr6 pr0-sm">
                <div class="btn-group priority1 hidden-md hidden-lg" role="group">
                  <%--  <tstsc:HyperlinkEx ID="btnBack" runat="server" SkinID="ButtonDefault" ToolTip="<%$Resources:Main,ReportConfiguration_BackToHome%>">
                        <span class="fas fa-arrow-left"></span>
                    </tstsc:HyperlinkEx>--%>
                </div>
                <h2 class="dib">
                    <tstsc:LabelEx ID="lblReportTitle" runat="server" />
                </h2>
<!-- PSC Code -->
<% if(Int32.Parse(Request.QueryString["reportId"]) < 2000) {%>
                <p class="my3">
                    <asp:Localize runat="server" Text="<%$Resources:Main,ReportsConfiguration_Legend1 %>" /><br />
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,ReportsConfiguration_Legend2 %>" />
                </p>
<% } %>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True"
                    DisplayMode="BulletList" runat="server" ID="vldValidationSummary"  />
            </div>

            <div class="w10 mw-100 pt5">
                <div 
                    class="u-box_group mb4"
                    data-collapsible="true"
                    id="pnlReport_Format" >
<!-- PSC Code -->
<% if(Int32.Parse(Request.QueryString["reportId"]) > 2000) {%>
		<asp:Panel runat="server" ID="pnlReport_Format1">
			  <div class="dataFields">
                <br/>
                <div class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer" aria-expanded="false">
                        <asp:Localize ID="Localize4output" runat="server" Text="Select Report Format<!--<%$Resources:Main,ReportsConfiguration_ReportElements %>-->" />
                </div>
				 <div class="row">
				  <div class="Spacer">
				  </div>
					<table class="ReportConfigForm">
						 <tbody>
							<tr>
								 <td class="DataLabel">
									<label for="parameter" style="width:100px;">Output Type:</label>
									<asp:DropDownList class="text-box" style="width:120px;" runat="server" ClientIDMode="Static" ID="outputTypesDropDown" DataTextField="TypeDescription">
									</asp:DropDownList>
								 
								 </td>
							 </tr>
						 </tbody>
					</table>
				  </div>
			 </div>		 
		</asp:Panel>
<!-- End PSC Code -->
<%} else {%>
                    <div 
                        class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                        aria-expanded="true">
                        <asp:Localize 
                            runat="server" 
                            Text="<%$Resources:Main,ReportsConfiguration_ReportFormat %>" />
                        <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                    </div>

                    <tstsc:GridViewEx CssClass="ReportConfigForm u-box_item" AutoGenerateColumns="false" GridLines="None" ID="grdReportFormats" runat="server" ShowHeader="false">
                        <Columns>
                            <tstsc:TemplateFieldEx ItemStyle-CssClass="DataLabel">
                                <ItemTemplate>
                                    <tstsc:LabelEx ID="lblLegend" runat="server" Text='<%# Resources.Fields.Format + ":" %>' />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx ItemStyle-Width="20px">
                                <ItemTemplate>
                                    <tstsc:RadioButtonEx ID="radFormat" runat="server" GroupName="ReportFormat" MetaData='<%#((ReportFormat) Container.DataItem).ReportFormatId%>' />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx ItemStyle-CssClass="DataEntry">
                                <ItemTemplate>
                                    <tstsc:LabelEx ID="lblFormatName" AssociatedControlID="radFormat" runat="server"></tstsc:LabelEx>
                                    <tstsc:ImageEx CssClass="w4 h4" ID="imgFormatFiletype" runat="server" ImageAlign="AbsBottom" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                        </Columns>
                    </tstsc:GridViewEx>
                </div>


                <div 
                    class="u-box_group mb4"
                    data-collapsible="true"
                    id="pnlReport_Elements" >
                    <div 
                        class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                        aria-expanded="true">
                        <asp:Localize 
                            runat="server" 
                            Text="<%$Resources:Main,ReportsConfiguration_ReportElements %>" />
                        <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                    </div>

                    <tstsc:GridViewEx CssClass="ReportConfigForm u-box_item" AutoGenerateColumns="false" GridLines="None" ID="grdReportElements" runat="server" ShowHeader="false">
                        <Columns>
                            <tstsc:TemplateFieldEx ItemStyle-CssClass="DataLabel">
                                <ItemTemplate>
                                    <tstsc:LabelEx ID="lblLegend" runat="server" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx ItemStyle-Width="20px">
                                <ItemTemplate>
                                    <tstsc:CheckBoxEx ID="chkDisplayElement" runat="server" Checked="true" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                            <tstsc:TemplateFieldEx ItemStyle-CssClass="DataEntry">
                                <ItemTemplate>
                                    <tstsc:LabelEx ID="lblElementName" AssociatedControlID="chkDisplayElement" runat="server" />
                                </ItemTemplate>
                            </tstsc:TemplateFieldEx>
                        </Columns>
                    </tstsc:GridViewEx>
<% } %>
                </div>
                        <% if(Int32.Parse(Request.QueryString["reportId"]) > 2000) {%>
                            <!-- <div class="u-box_group mb4" data-collapsible="false" id="pnlReport_BatchInput" > -->
							<br/>
                                <div class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer" aria-expanded="false">
                                       <asp:Localize ID="Localize4ar" runat="server" Text="Create Batch Report<!--<%$Resources:Main,ReportsConfiguration_ReportElements %>-->" />
                                           
                                </div>
                                <table ID="grdReportBatchParameters"  Class="ReportConfigForm"></table>
                            
                        <% } %>


<!-- PSC Code -->
    <% if(Int32.Parse(Request.QueryString["reportId"]) > 2000) {%>
        <br/>
        <div class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer" aria-expanded="false">
            <asp:Localize ID="Localize5ar" runat="server" Text="Generate Report<!--<%$Resources:Main,ReportsConfiguration_ReportElements %>-->" />                               
        </div>
        <table ID="grdReportParameters"  Class="ReportConfigForm">
        </table>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <table>
            <tr>
                <td class="DataLabel"></td>
            </tr>
        </table>
    </div>
    <% } %>
<!-- End PSC Code -->


                <asp:Repeater ID="rptReportSection" runat="server">
                    <ItemTemplate>
                        <asp:HiddenField ID="hdnReportSection" runat="server" />
                        <div 
                            class="u-box_group mb4"
                            data-collapsible="true"
                            id="pnlReport_Section" >
                            <div 
                                class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                aria-expanded="true">
										<% if(Int32.Parse(Request.QueryString["reportId"]) > 2000) {%>
											<asp:Localize ID="Localize3a" runat="server" Text="Parameters List<!--<% $Resources:Main,ReportsConfiguration_StandardFieldFilters %>-->" />
										<%} else {%>
											<%#((ReportSectionInstance) Container.DataItem).Section.Name%> -
											<asp:Localize ID="Localize3" runat="server" Text="<% $Resources:Main,ReportsConfiguration_StandardFieldFilters %>" />
										<%}%>
                                <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                            </div>


                            <tstsc:GridViewEx ID="grdStandardFieldFilters" runat="server" CssClass="ReportConfigForm u-box_item"
                                OnRowCreated="grdStandardFieldFilters_RowCreated" OnRowDataBound="grdStandardFieldFilters_RowDataBound"
                                AutoGenerateColumns="false" ShowHeader="false" BorderStyle="None" GridLines="None" DataSource="<%#artifactFields %>">
                                <Columns>
                                    <tstsc:TemplateFieldEx ItemStyle-CssClass="DataLabel">
                                        <ItemTemplate>
                                            <tstsc:LabelEx ID="lblStandardFieldFilter" runat="server" MetaData='<%# ((ArtifactField) Container.DataItem).Caption%>' />
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                    <tstsc:TemplateFieldEx ItemStyle-Width="20px">
                                        <ItemTemplate>
                                            &nbsp;
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                    <tstsc:TemplateFieldEx ItemStyle-CssClass="DataEntry">
                                        <ItemTemplate>
                                            <tstsc:TextBoxEx 
                                                CssClass="text-box" 
                                                ID="txtStandardFilter" 
                                                MaxLength="50" 
                                                MetaData='<%# ((((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Text || (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.NameDescription) ? ((ArtifactField) Container.DataItem).ArtifactFieldId.ToString() : "" %>'
                                                runat="server" 
                                                TextMode="SingleLine"
                                                Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Text || (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.NameDescription%>'
                                                Width="90%" 
                                                />
                                            <tstsc:TextBoxEx ID="txtIDFilter" runat="server" style="max-width: 200px;" TextMode="SingleLine"
                                                MetaData='<%# ((((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Identifier) ? ((ArtifactField) Container.DataItem).ArtifactFieldId.ToString() : "" %>'
                                                Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Identifier%>'
                                                MaxLength="1000" CssClass="text-box" />
                                            <tstsc:DropDownListEx ID="ddlFlagFilter" runat="server" 
                                                MetaData='<%# ((((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Flag) ? ((ArtifactField) Container.DataItem).ArtifactFieldId.ToString() : "" %>'
                                                Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Flag%>'
                                                CssClass="DropDownList" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll%>" NoValueItem="True" />
                                            <tstsc:DropDownMultiList ID="lstStandardFilter" runat="server"  
                                                SelectionMode="Multiple" MetaData='<%# ((((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Lookup || (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.MultiList) ? ((ArtifactField) Container.DataItem).ArtifactFieldId.ToString() : "" %>'
                                                Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Lookup || (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.MultiList%>'
                                                CssClass="DropDownList" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll%>" NoValueItem="True" />
                                            <tstsc:DropDownListEx ID="ddlEqualizerFilter" runat="server"  
                                                MetaData='<%# ((((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Equalizer) ? ((ArtifactField) Container.DataItem).ArtifactFieldId.ToString() : "" %>'
                                                Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Equalizer%>'
                                                CssClass="DropDownList" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll%>" NoValueItem="True" />
                                            <tstsc:DropDownHierarchy ID="ddlHierarchyFilter" runat="server" 
                                                MetaData='<%# ((((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.HierarchyLookup) ? ((ArtifactField) Container.DataItem).ArtifactFieldId.ToString() : "" %>'
                                                Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.HierarchyLookup%>'
                                                CssClass="DropDownList" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll%>" NoValueItem="True" />
                                            <tstsc:DateRangeFilter ID="datStandardFilter" runat="server"
                                                MetaData='<%# ((((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.DateTime) ? ((ArtifactField) Container.DataItem).ArtifactFieldId.ToString() : "" %>'
                                                Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.DateTime%>'
                                                CssClass="DatePicker" />
                                            <tstsc:DecimalRangeFilter ID="decimalRangeFilter" runat="server"
                                                MetaData='<%# ((((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Decimal) ? ((ArtifactField) Container.DataItem).ArtifactFieldId.ToString() : "" %>'
                                                Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Decimal%>' />
                                            <tstsc:IntRangeFilter ID="intRangeFilter" runat="server" 
                                                MetaData='<%# ((((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Integer) ? ((ArtifactField) Container.DataItem).ArtifactFieldId.ToString() : "" %>'
                                                Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.Integer%>' />
                                            <tstsc:EffortRangeFilter ID="effortRangeFilter" runat="server"
                                                MetaData='<%# ((((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.TimeInterval) ? ((ArtifactField) Container.DataItem).ArtifactFieldId.ToString() : "" %>'
                                                Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.TimeInterval%>' />
                                            <asp:Label runat="server" Text="<%$Resources:Main,Global_Hours %>" Visible='<%# (((ArtifactField) Container.DataItem).ArtifactFieldTypeId) == (int)Artifact.ArtifactFieldTypeEnum.TimeInterval%>' Style="margin-left: 5px;" />
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                </Columns>
                            </tstsc:GridViewEx>

                            <asp:PlaceHolder ID="plcFolder" runat="server" Visible="false">
                                <table class="ReportConfigForm" style="margin-top: 10px;">
                                    <tr>
                                        <td class="DataLabel">
                                            <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,Folder %>" AssociatedControlID="ddlFolder" AppendColon="true" Font-Bold="true" /></td>
                                        <td style="width: 20px;">&nbsp;</td>
                                        <td class="DataEntry">
                                            <tstsc:DropDownHierarchy ID="ddlFolder" runat="server"
                                                MetaData='<%#((ReportSectionInstance) Container.DataItem).ReportSectionId%>'
                                                ItemImage="Images/FolderOpen.svg" SummaryItemImage="Images/FolderOpen.svg"
                                                CssClass="DropDownList" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll%>"
                                                NoValueItem="True" />
                                        </td>
                                    </tr>
                                </table>
                            </asp:PlaceHolder>
                        </div>





                        <div 
                            class="u-box_group mb4"
                            data-collapsible="true"
                            id="pnlReport_CustomProperties" >
                            <div 
                                class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                aria-expanded="true">
                                <asp:Localize 
                                    runat="server" 
                                    Text="<%$Resources:Main,ReportsConfiguration_CustomPropertyFilters %>" />
                                <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                            </div>

                            <tstsc:GridViewEx ID="grdCustomPropertyFilters" runat="server" CssClass="ReportConfigForm u-box_item" OnRowCreated="grdCustomPropertyFilters_RowCreated"
                                OnRowDataBound="grdCustomPropertyFilters_RowDataBound"
                                AutoGenerateColumns="false" ShowHeader="false" BorderStyle="None" GridLines="None" DataSource="<%#customProperties %>">
                                <Columns>
                                    <tstsc:TemplateFieldEx ItemStyle-CssClass="DataLabel">
                                        <ItemTemplate>
                                            <tstsc:LabelEx ID="lblCustomPropertyFilter" runat="server" Text="<%# ((CustomProperty) Container.DataItem).Name%>" AppendColon="true" />
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                    <tstsc:TemplateFieldEx ItemStyle-Width="20px">
                                        <ItemTemplate>
                                            &nbsp;
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                    <tstsc:TemplateFieldEx ItemStyle-CssClass="DataEntry">
                                        <ItemTemplate>
                                            <tstsc:TextBoxEx ID="txtCustomProperty" runat="server" Width="90%" TextMode="SingleLine"
                                                MetaData='<%# ((((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.Text) ? ((CustomProperty) Container.DataItem).CustomPropertyId.ToString() : "" %>'
                                                Visible='<%# (((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.Text%>'
                                                MaxLength="50" CssClass="text-box" />
                                            <tstsc:DropDownMultiList ID="lstCustomProperty" runat="server" 
                                                SelectionMode="Multiple" MetaData='<%# ((((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.List || (((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.MultiList) ? ((CustomProperty) Container.DataItem).CustomPropertyId.ToString() : "" %>'
                                                Visible='<%# (((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.List || (((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.MultiList%>'
                                                CssClass="DropDownList" DataValueField="CustomPropertyValueId" DataTextField="Name"
                                                NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll%>"
                                                NoValueItem="True" />
                                            <tstsc:DropDownMultiList ID="usrCustomProperty" runat="server" 
                                                SelectionMode="Multiple" MetaData='<%# ((((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.User) ? ((CustomProperty) Container.DataItem).CustomPropertyId.ToString() : "" %>'
                                                Visible='<%# (((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.User%>'
                                                CssClass="DropDownList" DataValueField="UserId" DataTextField="FullName"
                                                NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll%>"
                                                NoValueItem="True" />
                                            <tstsc:DropDownListEx ID="boolCustomProperty" runat="server"
                                                DataTextField="Value" DataValueField="Key"
                                                MetaData='<%# ((((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.Boolean) ? ((CustomProperty) Container.DataItem).CustomPropertyId.ToString() : "" %>'
                                                Visible='<%# (((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.Boolean%>'
                                                CssClass="DropDownList" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll%>" NoValueItem="True" />
                                            <tstsc:DateRangeFilter ID="datCustomProperty" runat="server" 
                                                MetaData='<%# ((((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.Date) ? ((CustomProperty) Container.DataItem).CustomPropertyId.ToString() : "" %>'
                                                Visible='<%# (((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.Date%>'
                                                CssClass="DatePicker" />
                                            <tstsc:DecimalRangeFilter ID="decimalCustomProperty" runat="server" 
                                                MetaData='<%# ((((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.Decimal) ? ((CustomProperty) Container.DataItem).CustomPropertyId.ToString() : "" %>'
                                                Visible='<%# (((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.Decimal%>' />
                                            <tstsc:IntRangeFilter ID="intCustomProperty" runat="server" 
                                                MetaData='<%# ((((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.Integer) ? ((CustomProperty) Container.DataItem).CustomPropertyId.ToString() : "" %>'
                                                Visible='<%# (((CustomProperty) Container.DataItem).CustomPropertyTypeId) == (int)CustomProperty.CustomPropertyTypeEnum.Integer%>' />
                                        </ItemTemplate>
                                    </tstsc:TemplateFieldEx>
                                </Columns>
                            </tstsc:GridViewEx>
                        </div>
                        <div runat="server" id="divSorts" style="padding-bottom: 10px; padding-top: 10px;">
                            <div class="ReportTitle2" style="width: 95%;">
                            </div>
                            <table class="ReportConfigForm" style="margin-top: 10px;">
                                <tr>
                                    <td class="DataLabel">
                                        <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,SortField %>" />:</td>
                                    <td style="width: 20px;">&nbsp;</td>
                                    <td>
                                        <tstsc:DropDownListEx ID="ddlSortField" runat="server" MetaData='<%#((ReportSectionInstance) Container.DataItem).ReportSectionId%>'
                                            NoValueItem="true" DataTextField="Caption" DataValueField="FieldId" Width="300px"
                                            NoValueItemText="<%$Resources:Main,ReportConfiguration_DefaultSortDDL %>" />
                                    </td>
                                </tr>
                                <tr>
                                    <td style="max-width: 150px;">&nbsp;</td>
                                    <td style="width: 20px;">&nbsp;</td>
                                    <td>
                                        <tstsc:CheckBoxEx ID="chkSortAscending" runat="server" Checked="true" Text="<%$Resources:Main,ReportsConfiguration_SortAscending %>"/>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <div class="Spacer">
                </div>


               <asp:PlaceHolder ID="plcCustomSectionFilters" runat="server" Visible="false">
                    <div 
                        class="u-box_group mb4"
                        data-collapsible="true"
                        id="pnlReport_Section" >
                        <div 
                            class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                            aria-expanded="true">
                            <asp:Localize 
                                runat="server" 
                                Text="<%$Resources:Main,ReportsConfiguration_CustomSectionFilters %>" />
                            <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                        </div>

                        <table class="ReportConfigForm" style="margin-top: 10px;">
                            <tr>
                                <td class="DataLabel">
                                    <tstsc:LabelEx runat="server" Text="<%$Resources:Fields,ReleaseId %>" AssociatedControlID="ddlCustomSectionReleaseFilter" AppendColon="true" Font-Bold="true" /></td>
                                <td style="width: 20px;">&nbsp;</td>
                                <td class="DataEntry">
                                    <tstsc:DropDownHierarchy ID="ddlCustomSectionReleaseFilter" runat="server"                                        
                                        CssClass="DropDownList" NoValueItemText="<%$Resources:Dialogs,Global_DropDownAll%>"
                                        ItemImage="Images/artifact-Release.svg"
                                        SummaryItemImage="Images/artifact-Release.svg"
                                        AlternateItemImage="Images/artifact-Iteration.svg"
                                        DataTextField="FullName"
                                        DataValueField="ReleaseId"
                                        IndentLevelField="IndentLevel"
                                        SummaryItemField="IsSummary"
                                        AlternateItemField="IsIteration"
                                        NoValueItem="True" />
                                </td>
                            </tr>
                        </table>

                    </div>
                </asp:PlaceHolder>

                <div class="Spacer">
                </div>

                <div class="ReportTitle3" style="width: 95%;">
                </div>

          
                <div style="width: 95%; margin-bottom: 20px;">
<!-- PSC Code -->
                    <% if (Int32.Parse(Request.QueryString["reportId"]) > 2000) { %>
                        <p><asp:Localize runat="server" Text="<%$Resources:Main,ReportsConfiguration_SaveReportLegend%>" visible="false" /></p>
                    <% } else { %>
<!-- End PSC Code -->
                    <p><asp:Localize runat="server" Text="<%$Resources:Main,ReportsConfiguration_SaveReportLegend %>" /></p>
                    <table class="ReportConfigForm">
                        <tr>
                            <td class="DataLabel">
                                <tstsc:LabelEx runat="server" AssociatedControlID="txtReportName" Text="<%$Resources:Fields,ReportName %>" AppendColon="true"/>
                            </td>
                            <td style="width: 20px">&nbsp;
                            </td>
                            <td class="DataEntry">
                                <tstsc:TextBoxEx ID="txtReportName" runat="server" Width="90%" MaxLength="50" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2"></td>
                            <td class="DataEntry">
                                <tstsc:CheckBoxEx ID="chkShareReport" runat="server" Text="<%$Resources:Main,ReportsConfiguration_ShareReport %>"   />
                                <div class="Spacer"></div>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2"></td>
                            <td class="DataEntry btn-group">
                                <tstsc:ButtonEx ID="btnCreate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,CreateReport %>" CausesValidation="True" />
                                <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="False" />
                            </td>
                        </tr>
                    </table>
                    <% } %>	
                </div>
            </div>
        </div>
   
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/BuildService.svc" />
		</Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
        </Scripts>
	</tstsc:ScriptManagerProxyEx>
	<script type="text/javascript">

	    //This page has collapsible panels
	    SpiraContext.pageId = "Inflectra.Spira.Web.ReportConfiguration";
	    SpiraContext.HasCollapsiblePanels = true;

		function ddlRelease_selectedItemChanged(item)
		{
			var clientId = '<%=GetBuildClientId() %>';
            if (clientId)
            {
			    var ddlBuild = $find(clientId);
                if (ddlBuild)
                {
        	        //Get the new releaseId
			        if (item.get_value() == '')
			        {
				        ddlBuild.clearItems();
				        ddlBuild.addItem('', '-- ' + resx.Global_All + ' --');
				        ddlBuild.set_selectedItem('');
			        }
			        else
			        {
				        var releaseId = item.get_value();
						var projectId = <%=ProjectId %>;
				        //Get the list of builds for this release from the web service
				        Inflectra.SpiraTest.Web.Services.Ajax.BuildService.GetBuildsForRelease(projectId, releaseId, ddlResolvedRelease_selectedItemChanged_success, ddlResolvedRelease_selectedItemChanged_failure);
                    }
                }
            }
        }
		function ddlResolvedRelease_selectedItemChanged_success (data)
		{
			//Clear values and databind
			var clientId = '<%=GetBuildClientId() %>';
            if (clientId)
            {
			    var ddlBuild = $find(clientId);
                if (ddlBuild)
                {
			        ddlBuild.clearItems();
			        ddlBuild.addItem('', '-- ' + resx.Global_All + ' --');
			        if (data)
			        {
				        ddlBuild.set_dataSource(data);
				        ddlBuild.dataBind();
				        ddlBuild.set_selectedItem('');
			        }
                }
            }
		}
		function ddlResolvedRelease_selectedItemChanged_failure (error)
		{
			var clientId = '<%=GetBuildClientId() %>';
            if (clientId)
            {
			    var ddlBuild = $find(clientId);
                if (ddlBuild)
                {
			        //ignore error, just clear dropdown values
			        ddlBuild.clearItems();
			        ddlBuild.addItem('', '-- ' + resx.Global_All + ' --');
			        ddlBuild.set_selectedItem('');
                }
            }
		}
    
/* END EXTERNAL SOURCE */

/* BEGIN EXTERNAL SOURCE */

//PSC Code
        $(function () {
			Api.GetReportByID();
        });

        

        var Api = {
            GetReportByID: async function () {
                let windowProtocol = window.location.protocol;
                let windowHost = window.location.host;

                console.log(windowProtocol);
                console.log(windowHost);

				var id = '<%=Request.QueryString["reportId"]%>';
                console.log(id);
                //let getTemplateByIdURL =  "http://" + windowHost + "/VMReporting/api/ReportApi/GetTemplateByID/" + id;
				let getTemplateByIdURL = windowProtocol + "//" + windowHost + "/ValidationMaster/api/ReportApi/GetTemplateByID/" + id;
                console.log(getTemplateByIdURL);
                $.ajax({
                    type: 'GET',

                    crossDomain: true,
                    url: getTemplateByIdURL,
                    success: function (data) {
                        console.log('success');
                        Api.FillReportTitle(data);
						Api.FillOutputFormat(data["TST_TEMPLATE_OUTTYPE"]);
						Api.FillParameters(data["TST_TEMPLATE_PARAMETER"]);
						Api.FillBatchParameters(data["TST_TEMPLATE_PARAMETER"]);
						Api.LoadTypeAhead();
                        Api.WireClick();
                    },
					fail: function (){
                        console.log('Fail');
					}
                });
                console.log('After Ajax');
            },

            FillReportTitle: async function(baseValues) {
                //let output = '<span id="mytitle">' + baseValues.TemplateName + '</span>';
                let output = baseValues.TemplateName;

                $('#cplMainContent_lblReportTitle').html(output);				
            },

            FillOutputFormat: async function(selectValues) {
                var output = [];

                $.each(selectValues, function (i, item) {
                    output.push('<option value="' + item.TypeDescription + '">' + item.TypeDescription + '</option>');					
                });
                $('#outputTypesDropDown').html(output.join(''));				
            },

            FillBatchParameters: function(selectValues) {
                var output = [];

                $.each(selectValues, function (i, item) {
                    //console.log(item.ParameterLabel);
					if(item.ParameterLabel == 'TestSetID')
					{
						output.push('<tr><td>&nbsp;&nbsp;&nbsp;&nbsp;No Batch Parameters are available for Test Sets</td></tr>');
					}
					else
                    {
                        var IDLocation = item.ParameterLabel.lastIndexOf("ID");
                        var labelText = item.ParameterLabel.slice(0,IDLocation);
                        labelText = labelText.replace(/([A-Z])/g, ' $1').trim();
						output.push('<tr><td class="DataLabel" style="width:200px;"><Label for="parameter">' + labelText + ' ID:</label><input style="width:120px;margin-left:30px;" class="text-box report-parameter" placeholder="1, 2, 3, 4-8, 9" type="text" id=' + item.ParameterLabel + ' onkeyup="sync(this)"></td></tr><tr><td class="btn-group report-btns"><input class="btn btn-default" onclick="Api.GenerateBatchReport()" type="button" value="Send Batch to SharePoint" /><asp:HyperLink CssClass="btn btn-default schedule" id="link1" runat="server" Text="Schedule Batch" /></td></tr>');
					}
                });
				$('#grdReportBatchParameters').html(output.join(''));
            },

			FillParameters: async function (selectValues) {
				var output = [];

				$.each(selectValues, async function (i, item) {
					console.log(item.ParameterLabel);
					if (item.ParameterLabel === "TestRunID") {
						// build TestRun Dropdown
						output.push('<tr><td class="DataLabel" style="width:200px;"><label for="parameter">Test Run:</label><input type="text" class="bigdrop" id="e8" style="width:310px;margin-left:30px;" /></td></tr>');
						output.push('<tr><td class="DataEntry btn-group report-btns"><input class="btn btn-default" onclick="Api.GenerateReport()" type="button" value="Create Report"><tstsc:ButtonEx ID="btnCancel8" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="False" /></td></tr>');
						$('#grdReportParameters').html(output.join(''));
					}

					if (item.ParameterLabel === "TestCaseID") {
						// build TestCase Dropdown
						output.push('<tr><td class="DataLabel" style="width:200px;"><label for="parameter">Test Case:</label><input type="text" class="bigdrop" id="e6" style="width:310px;margin-left:25px;" /></td></tr>');
						output.push('<tr><td class="DataEntry btn-group report-btns"><input class="btn btn-default" onclick="Api.GenerateReport()" type="button" value="Create Report"><tstsc:ButtonEx ID="btnCancel6" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="False" /></td></tr>');
						$('#grdReportParameters').html(output.join(''));
					}

					if (item.ParameterLabel === "TestSetID") {
						// build TestSet Dropdown
						output.push('<tr><td class="DataLabel" style="width:200px;"><label for="parameter">Test Set:</label><input type="text" class="bigdrop" id="e5" style="width:310px;margin-left:30px;" /></td></tr>');
						output.push('<tr><td class="DataEntry btn-group report-btns"><input class="btn btn-default" onclick="Api.GenerateTestSetBatchReport()" type="button" value="Send to SharePoint Now" /><asp:HyperLink CssClass="btn btn-default schedule" id="link5" runat="server" Text="Schedule Batch" /></td></tr>');
						$('#grdReportParameters').html(output.join(''));
					}

					if (item.ParameterLabel === "ProjectID") {
						// build Project Dropdown
						output.push('<tr><td class="DataLabel" style="width:200px;"><label for="parameter">Project:</label><input type="text" class="bigdrop" id="e7" style="width:310px;margin-left:35px;" /></td></tr>');
						output.push('<tr><td class="DataEntry btn-group report-btns"><input class="btn btn-default" onclick="Api.GenerateReport()" type="button" value="Create Report"><tstsc:ButtonEx ID="btnCancel7" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="False" /></td></tr>');
						$('#grdReportParameters').html(output.join(''));
					}
				});
			},
			
			LoadTypeAhead: function () {
                let windowProtocol = window.location.protocol;
                let windowHost = window.location.host;

				$("#e8").select2({
						placeholder: "Test Run Search",
						minimumInputLength: 1,
						ajax: { // instead of writing the function to execute the request we use Select2's convenient helper
							url: windowProtocol + "//" + windowHost + "/ValidationMaster/api/ReportApi/GetTestRun",
							dataType: 'json',
							quietMillis: 250,
							data: function (term, page) {
								return {
									q: term, // search term
								};
							},
						results: function (data, page) { // parse the results into the format expected by Select2.
						// since we are using custom formatting functions we do not need to alter the remote JSON data
							return { results: data };
						},
					   cache: true
					},
					formatResult: formatResult, // omitted for brevity, see the source of this page
					formatSelection: formatResult,  // omitted for brevity, see the source of this page
					dropdownCssClass: "bigdrop" // apply css that makes the dropdown taller
				});

				$("#e5").select2({
						placeholder: "Test Set Search",
						minimumInputLength: 1,
						ajax: { // instead of writing the function to execute the request we use Select2's convenient helper
							url: windowProtocol + "//" + windowHost + "/ValidationMaster/api/ReportApi/GetTestSet",
							dataType: 'json',
							quietMillis: 250,
							data: function (term, page) {
								return {
									q: term, // search term
								};
							},
						results: function (data, page) { // parse the results into the format expected by Select2.
						// since we are using custom formatting functions we do not need to alter the remote JSON data
							return { results: data };
						},
					   cache: true
					},
					formatResult: formatResult, // omitted for brevity, see the source of this page
					formatSelection: formatResult,  // omitted for brevity, see the source of this page
					dropdownCssClass: "bigdrop" // apply css that makes the dropdown taller
				});

				$("#e6").select2({
						placeholder: "Test Case Search",
						minimumInputLength: 1,
						ajax: { // instead of writing the function to execute the request we use Select2's convenient helper
							url: windowProtocol + "//" + windowHost + "/ValidationMaster/api/ReportApi/GetTestCase",
							dataType: 'json',
							quietMillis: 250,
							data: function (term, page) {
								return {
									q: term, // search term
								};
							},
						results: function (data, page) { // parse the results into the format expected by Select2.
						// since we are using custom formatting functions we do not need to alter the remote JSON data
							return { results: data };
						},
					   cache: true
					},
					formatResult: formatResult, // omitted for brevity, see the source of this page
					formatSelection: formatResult,  // omitted for brevity, see the source of this page
					dropdownCssClass: "bigdrop" // apply css that makes the dropdown taller
				});

				$("#e7").select2({
						placeholder: "Project Search",
						minimumInputLength: 1,
						ajax: { // instead of writing the function to execute the request we use Select2's convenient helper
							url: windowProtocol + "//" + windowHost + "/ValidationMaster/api/ReportApi/GetProject",
							dataType: 'json',
							quietMillis: 250,
							data: function (term, page) {
								return {
									q: term, // search term
								};
							},
						results: function (data, page) { // parse the results into the format expected by Select2.
						// since we are using custom formatting functions we do not need to alter the remote JSON data
							return { results: data };
						},
					   cache: true
					},
					formatResult: formatResult, // omitted for brevity, see the source of this page
					formatSelection: formatResult,  // omitted for brevity, see the source of this page
					dropdownCssClass: "bigdrop" // apply css that makes the dropdown taller
				});
			},

            GenerateReport: function(){
				var id = '<%=Request.QueryString["reportId"]%>';

                console.log(id);
                        var parameters = "";
                        var outputType = $('#outputTypesDropDown').val();
                
                    var projectId = $("#e7").select2("data").id;
                    if(projectId != null)
                    {
                        if(projectId.length > 0)
                        {
                                parameters += 'projectId=' + $("#e7").select2("data").id  
                        }
                    }
                    
                    var testRunId = $("#e8").select2("data").id;
                    if(testRunId != null)
                    {
                        if(testRunId.length > 0)
                        {
                                parameters += 'TestRunID=' + $("#e8").select2("data").id 
                        }
                    }
                    
                    var testCaseId = $("#e6").select2("data").id;
                    console.log(testCaseId);
                    if(testCaseId != null)
                    {
                        if(testCaseId.length > 0)
                        {
                                parameters += 'TestCaseID=' + $("#e6").select2("data").id  
                        }
                    }
                    
                    //var testSetId = $("#e5").select2("data").id;
                    //console.log(testSetId);
                    //if(testSetId != null)
                    //{
                    //	if(testSetId.length > 0)
                    //	{
                    //		 parameters += 'TestSetID=' + $("#e5").select2("data").id + '&'
                    //	}
                    //}
                    
                    if(parameters.length == 0) {
                        $('.report-parameter').each(function() {
                            console.log('report-parm:' + $(this).val());
                            parameters += $(this).attr('id') + '=' + $(this).val() + '&'
                        });
                    }
                    
                        console.log('parameters:' + parameters);
                        if(parameters == "")
                        {
                        parameters = "none";					
                        }

                    let windowProtocol = window.location.protocol;
                    let windowHost = window.location.host;
                    
                    var url = windowProtocol + "//" + windowHost + "/ValidationMaster/api/ReportApi/GenerateReport/"+id+"/"+parameters+"/" + outputType;
                    console.log('url:' + url);
                    window.open(url,'_blank');
            },

			GenerateBatchReport: function(){
				var id = '<%=Request.QueryString["reportId"]%>';

				console.log('ReportId:' + id);
				var parameters = "";
				var outputType = $('#outputTypesDropDown').val();
				var destination = $('#<%=batchDestination.ClientID%>').val();
				var batchFolder = $('#<%=batchFolder.ClientID%>').val();
            
                $('.report-parameter').each(function() {
                        console.log('report-parm:' + $(this).val());
                        parameters += $(this).attr('id') + '=' + $(this).val()  
                    });

                console.log('parameters:' + parameters);
                if(parameters == "")
                {
                    parameters = "none";					
                }
				
				console.log('index:' + parameters.indexOf('TestSetID'))
				
				if(parameters.indexOf('TestSetID') !== -1)
				{
				   console.log('Old TestSetId:' + id);
                   id = 2000; // $('#/***************************/').val();
				   console.log('New TestSetId:' + id);
				}
				
				var userDest = '<%= this.UserName %>';
				var emailAt = userDest.indexOf('@');
				if(emailAt > 0)
				{
					var emailLen = userDest.length;
					var emailName = userDest.substring(0, emailAt);
					var periodAt = emailName.indexOf('.');
					if(periodAt > 0)
					{
						var properName = emailName.charAt(0).toUpperCase() + emailName.slice(1);
						userDest = properName.slice(0,periodAt) + properName.charAt(periodAt + 1).toUpperCase() + properName.slice(periodAt + 2, properName.length);						
					}
					else
					{
						userDest = emailName;
					}
				}
                console.log('userDest:' + userDest);
                
                let windowProtocol = window.location.protocol;
                let windowHost = window.location.host;
				var url = windowProtocol + "//" + windowHost + "/ValidationMaster/api/ReportApi/GenerateBatchReport/"+id+"/"+parameters+"/"+outputType+"/"+destination+"/"+batchFolder+"/"+userDest;
                console.log('url:' + url);
                //window.open(url,'_blank');
                var request = new XMLHttpRequest();
				//alert(url);
                request.open('GET', url, true)
                request.onload = function() {
                    if (request.status >= 200 && request.status < 400) {
                        console.log('SUCCESS');
                        alert("Batch Process Started.");
                    } else {
                        console.log('error');
                        console.log(request.responseText);
                        alert("There was an error creating the batch. \n\nPlease ensure your personal batch folder exists in SharePoint or contact your Administrator.");
                    }                
                }
                request.onerror = function () {
                    console.log('Network Error' + request.responseText);
                    alert("Network Error: There was an error creating the batch.");
                };
                request.send()
            },

			GenerateTestSetBatchReport: function(){
				var id = '<%=Request.QueryString["reportId"]%>';

				console.log('ReportId:' + id);
				var parameters = "";
				var outputType = $('#outputTypesDropDown').val();
				var destination = $('#<%=batchDestination.ClientID%>').val();
				var batchFolder = $('#<%=batchFolder.ClientID%>').val();

                //PARAMETER BUILDING
				var projectId = $("#e7").select2("data").id;
				if(projectId != null)
				{
					if(projectId.length > 0)
					{
						 parameters += 'projectId=' + $("#e7").select2("data").id  
					}
				}
				
				var testRunId = $("#e8").select2("data").id;
				if(testRunId != null)
				{
					if(testRunId.length > 0)
					{
						 parameters += 'TestRunID=' + $("#e8").select2("data").id  
					}
				}
				
				var testCaseId = $("#e6").select2("data").id;
				if(testCaseId != null)
				{
					console.log('TestCaseId: ' + testCaseId);
					if(testCaseId.length > 0)
					{
						 parameters += 'TestCaseID=' + $("#e6").select2("data").id  
					}
				}
				
				var testSetId = $("#e5").select2("data").id;
				if(testSetId != null)
				{
					console.log('TestSetId' + testSetId);
					if(testSetId.length > 0)
					{
						 parameters += 'TestSetID=' + $("#e5").select2("data").id  
					}
				}
				
				if(parameters.length == 0) {
					$('.report-parameter').each(function() {
						console.log('report-parm:' + $(this).val());
						parameters += $(this).attr('id') + '=' + $(this).val() 
					});
				}
					
					            
                $('.report-parameter').each(function() {
                        console.log('report-parm:' + $(this).val());
                        parameters += $(this).attr('id') + '=' + $(this).val()  
                    });

                console.log('parameters:' + parameters);

            
                $('.report-parameter').each(function() {
                        console.log('report-parm:' + $(this).val());
                        parameters += $(this).attr('id') + '=' + $(this).val()  
                    });

                console.log('parameters:' + parameters);
                if(parameters == "")
                {
                    parameters = "none";					
                }

				console.log('index:' + parameters.indexOf('TestSetID'))
				
				if(parameters.indexOf('TestSetID') !== -1)
				{
				   console.log('Old TestSetId:' + id);
                   id = 2000; //$('#/***************************/').val();
				   console.log('New TestSetId:' + id);
				}				
				
				var userDest = '/******************/';
				var emailAt = userDest.indexOf('@');
				if(emailAt > 0)
				{
					var emailLen = userDest.length;
					var emailName = userDest.substring(0, emailAt);
					var periodAt = emailName.indexOf('.');
					if(periodAt > 0)
					{
						var properName = emailName.charAt(0).toUpperCase() + emailName.slice(1);
						userDest = properName.slice(0,periodAt) + properName.charAt(periodAt + 1).toUpperCase() + properName.slice(periodAt + 2, properName.length);						
					}
					else
					{
						userDest = emailName;
					}
				}
                console.log('userDest:' + userDest);
                let windowProtocol = window.location.protocol;
                let windowHost = window.location.host;
                
				var url = windowProtocol + "//" + windowHost + "/ValidationMaster/api/ReportApi/GenerateBatchReport/"+id+"/"+parameters+"/"+outputType+"/"+destination+"/"+batchFolder+"/"+userDest;
                console.log('url:' + url);
                //window.open(url,'_blank');
                var request = new XMLHttpRequest();
				//alert(url);
                request.open('GET', url, true)
                request.onload = function() {
                    if (request.status >= 200 && request.status < 400) {
                        console.log('SUCCESS');
                        alert("Batch Process Started.");
                    } else {
                        console.log('error');
                        console.log(request.responseText);
                        alert("There was an error creating the batch. \n\nPlease ensure your personal batch folder exists in SharePoint or contact your Administrator.");
                    }                
                }
                request.onerror = function () {
                    console.log('Network Error' + request.responseText);
                    alert("Network Error: There was an error creating the batch.");
                };
                request.send()
            },

            WireClick: function(){
				$('#<%=link1.ClientID%>').click(function () { $('#dialog').dialog(); });
				console.log('#<%=link1.ClientID%> Click Wired');
                $('#<%=link5.ClientID%>').click(function(){$('#dialog').dialog();});
				console.log('#<%=link5.ClientID%> Click Wired');
            },

            CreateSchedule (){
				var id = $('#scheduleReportId').val();
				var parameters = $('#scheduleParameterName').val() + "=" + $('#scheduleParameterValue').val();
				var outputType = $('#<%=outputTypesDropDown.ClientID%>').val();
				var date = $('#datepicker').val();
				var datearray = date.split("/");
				var scheduleDate = datearray[0] + '-' + datearray[1] + '-' + datearray[2];
                var scheduleTime = $('#selectedTime').val();
                scheduleTime = scheduleTime.replace(":", "%3A").replace(" ","_");
				var destination = $('#<%=batchDestination.ClientID%>').val();
                var batchFolder = $('#<%=batchFolder.ClientID%>').val();
				var batchUserFolder = $('#<%=batchUserFolder.ClientID%>').val();
                
                var reportId = '<%=Request.QueryString["reportId"]%>';

                let windowProtocol = window.location.protocol;
                let windowHost = window.location.host;
                var schedulerAPIEndPoint = $('#<%=schedulerAPIEndPoint.ClientID%>').val();

				var userDest = '<%= this.UserName %>';
				var emailAt = userDest.indexOf('@');
				if(emailAt > 0)
				{
					var emailLen = userDest.length;
					var emailName = userDest.substring(0, emailAt);
					var periodAt = emailName.indexOf('.');
					if(periodAt > 0)
					{
						var properName = emailName.charAt(0).toUpperCase() + emailName.slice(1);
						userDest = properName.slice(0,periodAt) + properName.charAt(periodAt + 1).toUpperCase() + properName.slice(periodAt + 2, properName.length);						
					}
					else
					{
						userDest = emailName;
					}
                }

				var url = windowProtocol + "//" + windowHost + "/ValidationMaster/api/ReportApi/CreateSchedule/" + reportId + "/" + parameters+"/"+outputType+"/"+scheduleDate+"/"+scheduleTime+"/"+destination+'/'+batchFolder+"/"+userDest;
                console.log("ScheduleURL: " + url);

                var request = new XMLHttpRequest()
                request.open('GET', url, true)
                request.onload = function() {
                    if (request.status >= 200 && request.status < 400) {
                        console.log('SUCCESS');
                        alert("Scheduled Created.");
                        $("#dialog").dialog("close");
                    } else {
                        console.log('error');
                        console.log(request.responseText);
                        alert("There was an error creating the schedule. ");
                    }                
                }
                request.onerror = function () {
                    console.log('Network Error' + request.responseText);
                    alert("Network Error: There was an error creating the schedule.");
                };
                request.send()
            }
        }

        function sync(textbox){
            document.getElementById('scheduleParameterValue').value = textbox.value;
            document.getElementById('scheduleParameterName').value = textbox.id;
        }
		//END PSC Code

        function formatResult(result) { 
            return "<div class='select2-user-result'>" + result.name + "</div>"; 
        }
	</script>

     
 <link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
<script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>  

  
<script>
     
    $( function() {
        $("#datepicker").datepicker();
    });

     
    $('#<%=outputTypesDropDown.ClientID%>').change(function(){
        var outType = $('#<%=outputTypesDropDown.ClientID%>').val();
        $("#scheduleOutputType").val(outType);
    });
    $("#Closer").click(function () {
        $("#dialog").dialog("close");
    });
</script>
      
<div style="display:none">
    <div style="width: auto; min-height: 200px; max-height: none; height: auto;" class="ui-dialog-content ui-widget-content" id="dialog"  title="Schedule Batch Report">
        <p>Select a Date and Time for this report to execute.</p>
        <p>Date: <input type="text" id="datepicker"></p>
        <p>Time: 
        <select ID="selectedTime">
        <option value="12:00 AM">12:00 AM</option>
        <option value="12:30 AM">12:30 AM</option>
        <option value="1:00 AM">1:00 AM</option>
        <option value="1:30 AM">1:30 AM</option>
        <option value="2:00 AM">2:00 AM</option>
        <option value="2:30 AM">2:30 AM</option>
        <option value="3:00 AM">3:00 AM</option>
        <option value="3:30 AM">3:30 AM</option>
        <option value="4:00 AM">4:00 AM</option>
        <option value="4:30 AM">4:30 AM</option>
        <option value="5:00 AM">5:00 AM</option>
        <option value="5:30 AM">5:30 AM</option>
        <option value="6:00 AM">6:00 AM</option>
        <option value="6:30 AM">6:30 AM</option>
        <option value="7:00 AM">7:00 AM</option>
        <option value="7:30 AM">7:30 AM</option>
        <option value="8:00 AM">8:00 AM</option>
        <option value="8:30 AM">8:30 AM</option>
        <option value="9:00 AM">9:00 AM</option>
        <option value="9:30 AM">9:30 AM</option>
        <option value="10:00 AM">10:00 AM</option>
        <option value="10:30 AM">10:30 AM</option>
        <option value="11:00 AM">11:00 AM</option>
        <option value="11:30 AM">11:30 AM</option>
        <option value="12:00 PM">12:00 PM</option>
        <option value="12:30 PM">12:30 PM</option>
        <option value="1:00 PM">1:00 PM</option>
        <option value="1:30 PM">1:30 PM</option>
        <option value="2:00 PM">2:00 PM</option>
        <option value="2:30 PM">2:30 PM</option>
        <option value="3:00 PM">3:00 PM</option>
        <option value="3:30 PM">3:30 PM</option>
        <option value="4:00 PM">4:00 PM</option>
        <option value="4:30 PM">4:30 PM</option>
        <option value="5:00 PM">5:00 PM</option>
        <option value="5:30 PM">5:30 PM</option>
        <option value="6:00 PM">6:00 PM</option>
        <option value="6:30 PM">6:30 PM</option>
        <option value="7:00 PM">7:00 PM</option>
        <option value="7:30 PM">7:30 PM</option>
        <option value="8:00 PM">8:00 PM</option>
        <option value="8:30 PM">8:30 PM</option>
        <option value="9:00 PM">9:00 PM</option>
        <option value="9:30 PM">9:30 PM</option>
        <option value="10:00 PM">10:00 PM</option>
        <option value="10:30 PM">10:30 PM</option>
        <option value="11:00 PM">11:00 PM</option>
        <option value="11:30 PM">11:30 PM</option>
        </select>
        </p>
        <input type="hidden" value='<%=Request.QueryString["reportId"]%>' id="scheduleReportId">
        <input type="hidden" value='' id="scheduleParameterValue">
        <input type="hidden" value='' id="scheduleParameterName">
        <input type="hidden" value='' id="scheduleOutputType">
        <input type="hidden" ClientIDMode="static" id="testRunReportId" runat="server" value='<%$ AppSettings:TestRunReportId %>' />
        <input type="hidden" ClientIDMode="static" id="batchDestination" runat="server" value='<%$ AppSettings:BatchDestination %>' />
        <input type="hidden" ClientIDMode="static" id="batchFolder" runat="server" value='<%$ AppSettings:DeliveryLocation %>' />
        <input type="hidden" ClientIDMode="static" id="batchUserFolder" runat="server" value='<%$ AppSettings:DeliveryLocation %>' />
        <input type="hidden" ClientIDMode="static" id="schedulerAPIEndPoint" runat="server" value='<%$ AppSettings:SchedulerAPI %>' />
        <div class="form-group col-lg-12 col-md-12" role="group">
            <button id="Closer">Close</button>
            <input type="button" value="Schedule" class="btn btn-default" style="float: right;" onclick='Api.CreateSchedule()' />
        </div>
        <script>
            $("#Closer").click(function () {
                $("#dialog").dialog("close");
            });
		</script>
    </div>
</div>

<div style="display:none">
    <div style="width: auto; min-height: 100px; max-height: none; height: auto;" class="ui-dialog-content ui-widget-content" id="dialogComplete"  title="Schedule Batch Results">
        <p>The Schedule Restults ____________</p>
        <div class="form-group col-lg-12 col-md-12" role="group">
                <button id="dialogCloser" style="float: right;">Close</button>
        </div>
    </div>
</div>

</script>


</asp:Content>
