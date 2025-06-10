<%@ Page Language="c#" CodeBehind="DocumentWorkflowDetails.aspx.cs" AutoEventWireup="True"
    Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.DocumentWorkflowDetails" MasterPageFile="~/MasterPages/Administration.master" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-10">
                <h1 class="mt0 mb4 fs-h3">
                    <tstsc:HyperLinkEx ID="lnkWorkflowList" runat="server" CssClass="btn btn-default"><span class="fas fa-angle-left"></span></tstsc:HyperLinkEx>
                    <asp:Literal ID="artifactName" runat="server" Text="<%$Resources:Main,Admin_EditDocumentWorkflows_Title %>"/>
                </h1>
                <h2>
                    <asp:Literal ID="ltrWorkflowName" runat="server"/>
                    <small>
                        <asp:Literal runat="server" Text="<%$Resources:Main,Admin_WorkflowDetails_Title %>" />
                    </small>
                </h2>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <p>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_DocumentWorkflowDetails_Intro %>" />
                </p>
                <tstsc:GridViewEx ID="grdWorkflowSteps" runat="server" AutoGenerateColumns="False" Width="100%" CssClass="DataGrid WorkflowDiagram workflow-list" ShowFooter="False" ShowHeader="True">
                    <HeaderStyle CssClass="Header" />
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,workflow_step_name %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1 v-mid">
                            <ItemTemplate>
                                <div class="Step display-inline-block ml4" data-workflowStep="<%# ((Inflectra.SpiraTest.DataModel.DocumentStatus) Container.DataItem).Name%>">
                                    <tstsc:HyperLinkEx runat="server" CssClass="large" NavigateUrl='<%#UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "DocumentWorkflowStep") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString() + "&" + GlobalFunctions.PARAMETER_DOCUMENT_STATUS_ID + "=" + ((Inflectra.SpiraTest.DataModel.DocumentStatus) Container.DataItem).DocumentStatusId%>'><%#: ((Inflectra.SpiraTest.DataModel.DocumentStatus) Container.DataItem).Name%></tstsc:HyperLinkEx>
                                    <asp:Label runat="server" Visible='<%# ((Inflectra.SpiraTest.DataModel.DocumentStatus) Container.DataItem).IsDefault%>'
                                        CssClass="badge badge-inline-text" ID="Label1"><small><asp:Localize runat="server" Text="<%$Resources:Main,Admin_WorkflowDetails_Default %>"/></small></asp:Label>
                                </div>
                                <span class="fas fa-arrow-right my3 mx4"></span>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,workflow_transitions %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemTemplate>
                                <div class="Spacer"></div>
                                <asp:Repeater runat="server" OnItemCommand="rptWorkflowStepTransitions_ItemCommand" ID="rptWorkflowStepTransitions">
                                    <ItemTemplate>
                                        <div class="btn-group">
                                            <tstsc:HyperLinkEx Font-Italic="true" SkinID="ButtonDefault" runat="server" NavigateUrl='<%#UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "DocumentWorkflowTransition") + "?" + GlobalFunctions.PARAMETER_WORKFLOW_ID + "=" + workflowId.ToString() + "&" + GlobalFunctions.PARAMETER_WORKFLOW_TRANSITION_ID + "=" + ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition) Container.DataItem).WorkflowTransitionId%>'><%#: ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition) Container.DataItem).Name%></tstsc:HyperLinkEx>
                                            <tstsc:LinkButtonEx runat="server" CommandName="Delete" CommandArgument='<%# ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition) Container.DataItem).WorkflowTransitionId%>' CausesValidation="false"
                                                ID="Linkbutton2" Tooltip="<%$Resources:Main,Admin_WorkflowDetails_Delete %>">
                                                <span class="fas fa-trash-alt"></span>
                                            </tstsc:LinkButtonEx>
                                        </div>
                                        <span class="fas fa-arrow-right mx4 "></span>
                                        <div class="Step display-inline-block muted" data-workflowStep="<%# ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition)Container.DataItem).OutputDocumentStatus.Name%>">
                                            <tstsc:LabelEx runat="server">
                                                <%#: ((Inflectra.SpiraTest.DataModel.DocumentWorkflowTransition)Container.DataItem).OutputDocumentStatus.Name%>
                                            </tstsc:LabelEx>
                                        </div>
                                        <div class="Spacer">
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,workflow_status_id %>" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
                            <ItemTemplate>
                                <%# String.Format(GlobalFunctions.FORMAT_ID, ((Inflectra.SpiraTest.DataModel.DocumentStatus)Container.DataItem).DocumentStatusId)%>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Main,Global_Operations %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:HyperLinkEx runat="server" NavigateUrl="javascript:void(0)" ClientScriptMethod='<%# "pnlAddTransition_display(event," + ((Inflectra.SpiraTest.DataModel.DocumentStatus) Container.DataItem).DocumentStatusId + ")"%>'
                                    ID="lnkAddTransition" Text="<%$Resources:Buttons,AddTransition %>" CssClass="btn btn-default btn-add" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                    </Columns>
                </tstsc:GridViewEx>
            </div>
        </div>
    </div>
    <tstsc:DialogBoxPanel ID="pnlAddTransition" runat="server" Title="<%$Resources:Main,Admin_WorkflowDetails_AddNewTransitionTitle %>" Modal="false" Persistent="true" Width="500px">
        <asp:ValidationSummary runat="server" ID="ValidationSummary1" />
        <table>
            <tr>
                <td style="white-space: nowrap; padding-right: 10px">
                    <tstsc:LabelEx runat="server" ID="txtTransitionNameLabel" AssociatedControlID="txtTransitionName" Text="<%$Resources:Fields,Name %>" Required="true" />:
                </td>
                <td>
                    <tstsc:TextBoxEx ID="txtTransitionName" runat="server" CssClass="text-box" Width="250px" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtTransitionName" ErrorMessage="<%$Resources:Messages,Admin_WorkflowDetails_NameRequired %>" />
                </td>
            </tr>
            <tr>
                <td style="white-space: nowrap; padding-right: 10px; padding-bottom: 5px;">
                    <tstsc:LabelEx runat="server" ID="lblSourceStepLabel" AssociatedControlID="lblSourceStep" Text="<%$Resources:Main,Admin_WorkflowDetails_OriginatingStep %>" Required="true" />:
                </td>
                <td style="padding-bottom: 5px;">
                    <tstsc:LabelEx ID="lblSourceStep" runat="server" />
                    <asp:HiddenField ID="hdnSourceStep" runat="server" />
                </td>
            </tr>
            <tr>
                <td style="white-space: nowrap; padding-right: 10px">
                    <tstsc:LabelEx runat="server" ID="ddlDestinationStepLabel" AssociatedControlID="ddlDestinationStep" Text="<%$Resources:Main,Admin_WorkflowDetails_DestinationStep %>" Required="true" />:
                </td>
                <td>
                    <tstsc:DropDownListEx ID="ddlDestinationStep" runat="server" CssClass="DropDownList"
                        DataTextField="Name" DataValueField="DocumentStatusId" Width="250px" />
                </td>
            </tr>
            <tr>
                <td>&nbsp;
                </td>
                <td style="padding-top: 15px">
                    <tstsc:ButtonEx ID="btnAdd" runat="server" Text="<%$Resources:Buttons,Add %>" CausesValidation="True" SkinID="ButtonPrimary" />
                    <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel%>" ClientScriptServerControlId="pnlAddTransition" ClientScriptMethod="close()" />
                </td>
            </tr>
        </table>
    </tstsc:DialogBoxPanel>
    <script type="text/javascript">
        //Get the list of statuses as a dictionary
        var resx = Inflectra.SpiraTest.Web.GlobalResources;
        var incidentStatuses = <%=GetDocumentStatusesAsJson() %>;

        //Hover effects to highlight all match steps on the page
        $('.Step').hover(
            function() {
                var nameOnHover = $(this).attr("data-workflowstep");
                $(".Step[data-workflowStep='" + nameOnHover + "']").addClass('step-has-hover');
            }, function() {
                $('.Step').removeClass('step-has-hover');
            }
        );

        //Fix scroll bug with client-side validation
        //http://connect.microsoft.com/VisualStudio/feedback/details/299399/validationsummary-control-causes-the-page-to-scroll-to-the-top
        var ValidationSummaryOnSubmitOrig = ValidationSummaryOnSubmit; 
        var ValidationSummaryOnSubmit = function()
        { 
            var scrollToOrig = window.scrollTo; 
            window.scrollTo = function() { }; 
            ValidationSummaryOnSubmitOrig(); 
            window.scrollTo = scrollToOrig; 
        } 


        //Displays the 'Add Transition' dialog box
        function pnlAddTransition_display(evt, incidentStatusId)
        {
            //Clear the text box
            var txtTransitionName = $get('<%=txtTransitionName.ClientID %>');
            txtTransitionName.value = '';

            //Populate the existing status label
            var lblSourceStep = $get('<%=lblSourceStep.ClientID %>');
            lblSourceStep.innerHTML = incidentStatuses[incidentStatusId];
            var hdnSourceStep = $get('<%=hdnSourceStep.ClientID %>');
            hdnSourceStep.value = incidentStatusId;

            //Reset the validators
            globalFunctions.clearValidators();

            //Populate the destination status dropdown list (assuming it has values)
            var destStatuses = {};
            var count = 0;
            var firstItem = '';
            for (var i in incidentStatuses)
            {
                if (i != incidentStatusId)
                {
                    destStatuses[globalFunctions.keyPrefix + i] = incidentStatuses[i];
                    count++;
                    if (firstItem == '')
                    {
                        firstItem =  i;
                    }
                }
            }
            var ddlDestinationStep = $find('<%=ddlDestinationStep.ClientID %>');
            ddlDestinationStep.clearItems();
            if (count > 0)
            {
                ddlDestinationStep.set_dataSource(destStatuses);
                ddlDestinationStep.set_selectedItem(firstItem);
                ddlDestinationStep.dataBind();
            }
            else
            {
                ddlDestinationStep.addItem('', resx.Global_PleaseSelect);
                ddlDestinationStep.set_selectedItem('');
            }

            var pnlAddTransition = $find('<%=pnlAddTransition.ClientID%>');
            pnlAddTransition.display(evt);
        }
    </script>
</asp:Content>
