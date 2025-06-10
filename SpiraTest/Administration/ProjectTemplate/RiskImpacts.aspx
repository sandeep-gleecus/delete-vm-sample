<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="RiskImpacts.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.RiskImpacts" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" Text="<%$Resources:Main,RiskImpacts_Title %>" runat="server" />
                    <small>
                        <tstsc:HyperLinkEx 
                            ID="lnkAdminHome" 
                            runat="server" 
                            Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                            >
                            <tstsc:LabelEx ID="lblTemplateName" runat="server" />
				        </tstsc:HyperLinkEx>
                    </small>
                </h2>
            </div>
        </div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="row">
            <div class="col-lg-9">
                <tstsc:MessageBox runat="server" SkinID="MessageBox" ID="lblMessage" />
                <asp:ValidationSummary ID="ValidationSummary" runat="server" />
                <p>
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,RiskImpacts_Intro %>" />
                </p>
                
                <div class="TabControlHeader">
                    <div class="display-inline-block">
                        <strong><asp:Localize runat="server" Text="<%$Resources:Main,Global_Displaying %>"/>:</strong>
                    </div>
                    <tstsc:DropDownListEx ID="ddlFilterType" runat="server" ClientIDMode="Static" AutoPostBack="true">
                         <asp:ListItem Text="3 x 3" Value="3" />
                        <asp:ListItem Text="4 x 4" Value="4" />
                        <asp:ListItem Text="5 x 5" Value="5" />
                    </tstsc:DropDownListEx>
                    <asp:HiddenField ID="selectedFilter" runat="server" ClientIDMode="Static" />
                </div>

                <tstsc:GridViewEx ID="grdEditRiskImpacts" runat="server"
                    OnRowDataBound="grdEditRiskImpacts_RowDataBound"
                    DataMember="RiskImpact" AutoGenerateColumns="False" ShowFooter="False"
                    ShowSubHeader="False" SkinID="DataGrid" Width="100%">
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# String.Format (GlobalFunctions.FORMAT_ID, ((RiskImpact) Container.DataItem).RiskImpactId) %>'
                                    ID="Label8" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" ItemStyle-Width="300px">
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((RiskImpact) Container.DataItem).RiskImpactId %>'
                                    CssClass="text-box" runat="server" Text='<%# ((RiskImpact) Container.DataItem).Name %>'
                                    Width="90%" MaxLength="40" ID="txtRiskImpactName" />
                                <asp:RequiredFieldValidator ID="Requiredfieldvalidator5" ControlToValidate="txtRiskImpactName"
                                    ErrorMessage="<%$Resources:Messages,RiskImpacts_NameRequired %>" Text="*" runat="server" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                         
                       <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Score %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((RiskImpact) Container.DataItem).RiskImpactId %>'
                                    CssClass="text-box" runat="server" Text='<%# ((RiskImpact) Container.DataItem).Score.ToString() %>'
                                    Width="90%" MaxLength="20" ID="txtScore" />
                                <asp:RequiredFieldValidator ControlToValidate="txtScore"
                                    ReadOnly="true"
                                    ErrorMessage="<%$Resources:Messages,Admin_Impacts_ScoreRequired %>" Text="*" runat="server" />
                                <asp:RegularExpressionValidator ValidationExpression="<%$ GlobalFunctions:VALIDATION_REGEX_INTEGER%>"
                                    ControlToValidate="txtScore" ErrorMessage="<%$Resources:Messages,Admin_Impacts_ScoreMustBeNumeric %>"
                                    Text="*" runat="server" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                       <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Position %>" ItemStyle-CssClass="priority3" HeaderStyle-CssClass="priority3">
                            <ItemTemplate>
                                <tstsc:TextBoxEx MetaData='<%# ((RiskImpact) Container.DataItem).RiskImpactId %>'
                                    CssClass="text-box" runat="server" Text='<%# ((RiskImpact) Container.DataItem).Position.ToString() %>'
                                    Width="90%" MaxLength="20" ID="txtPosition" />
                                <asp:RequiredFieldValidator ControlToValidate="txtPosition"
                                    ErrorMessage="<%$Resources:Messages,Admin_Impacts_PositionRequired %>" Text="*" runat="server" />
                                <asp:RegularExpressionValidator ValidationExpression="<%$ GlobalFunctions:VALIDATION_REGEX_INTEGER%>"
                                    ControlToValidate="txtPosition" ErrorMessage="<%$Resources:Messages,Admin_Impacts_PositionMustBeNumeric %>"
                                    Text="*" runat="server" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>

                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
                            <ItemTemplate>
                                <tstsc:CheckBoxYnEx runat="server" ID="chkActive" NoValueItem="false"
                                    Checked='<%# (((RiskImpact) Container.DataItem).IsActive) ? true : false %>' />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                    </Columns>
                </tstsc:GridViewEx>
                <div class="btn-group mt4">
                    <tstsc:ButtonEx ID="btnRiskImpactUpdate" SkinID="ButtonPrimary" runat="server" CausesValidation="True" Text="<%$Resources:Buttons,Save%>" />
                    
                </div>
            </div>
        </div>
    </div>

     <script type="text/javascript">

            $(document).ready(function () {

				$('*[id*=txtPosition]').each(function () {
					$(this).on('input', function (e) {
						var max = parseInt($('#selectedFilter').val()) + 1;
                        var positionValue = parseInt($(this).val());

                        if (!isNaN(positionValue) && (positionValue <= max && positionValue > 0)) {
                            var scoreValue = max - positionValue;
                            $(this).parent().prev().children('input').val(scoreValue);
                        }
                        else {
                            $(this).parent().prev().children('input').val(0);
                            $(this).val('');
						}
					});
                });

            });
	 </script>
</asp:Content>
