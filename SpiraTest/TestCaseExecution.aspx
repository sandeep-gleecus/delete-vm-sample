<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" 
    AutoEventWireup="True" CodeBehind="TestCaseExecution.aspx.cs" Inherits="Inflectra.SpiraTest.Web.TestCaseExecution" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="TestExecutionStylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server"> 
    <div class="container-fluid display-none" id="te-execution-panel">
        <tstsc:RichTextBoxJ runat="server" RenderOnlyScripts="true" /><!-- Used to load the scripts for ckeditor only -->
        <div class="row te-header mt3" id="te-header">
            <div class="col-md-6">
                <h2>
                    <span data-bind="text: name"></span>
                    <small id="te-title-small">
                        <asp:Literal runat="server" ID="litRelease" Text="<%$Resources:Fields,Release%>"></asp:Literal>
                        <span data-bind="text: releaseVersion"></span>
                    </small>
                </h2>
            </div>
            <div class="col-md-6">
                <div class="te-display-select pull-right pull-left-xs" id="te-display-toolbar">
                    <div class="display-inline-block py2">
                        <asp:Literal runat="server" ID="litDisplayBtnTitle" Text="<%$Resources:ClientScript,NavigationBar_Display %>"></asp:Literal>
                    </div>
                    <div class="display-inline-block">
                        <div class="btn-group priority1 te-display-toolbar-prim" data-toggle="buttons">
                            <label class="btn btn-default" for="display-tb_radio-splitView" id="lbl-display-tb_radio-splitView" >
                                <input id="display-tb_radio-splitView" type="radio"/>
                                <asp:Literal runat="server" ID="litDisplayBtnSplitTitle"  Text="<%$Resources:Buttons,Split %>"></asp:Literal>
                            </label>
                            <label class="btn btn-default" for="display-tb_radio-gridView" id="lbl-display-tb_radio-gridView">
                                <input id="display-tb_radio-gridView" type="radio"/>
                                <asp:Literal runat="server" ID="litDisplayBtnTableTitle" Text="<%$Resources:Buttons,Table %>"></asp:Literal>
                            </label>
                            <label class="btn btn-default" for="display-tb_radio-miniView" id="lbl-display-tb_radio-miniView">
                                <input id="display-tb_radio-miniView" type="radio"/>
                                <asp:Literal runat="server" ID="litDisplayBtnMiniTitle" Text="<%$Resources:Buttons,Mini %>"></asp:Literal>
                            </label>
                        </div>
                        <div class="te-display-toolbar-sec py2">
                            <div class="child">
                                <div class="btn-group priority1" data-toggle="buttons" id="btn-group-splitView">
                                    <label class="btn btn-default py0" for="btn-group-splitView_inspectorLarge" id="lbl-btn-group-splitView_inspectorLarge" >
                                        <input id="btn-group-splitView_inspectorLarge" type="radio"/>
                                        <span class="fas fa-minus fa-rotate-90" style="margin-right: -3px;"></span>
                                        <span class="fas fa-square"></span>
                                    </label>
                                    <label class="btn btn-default" for="btn-group-splitView_gridLarge" id="lbl-btn-group-splitView_gridLarge">
                                        <input id="btn-group-splitView_tableLarge" type="radio"/>
                                        <span class="fas fa-square" style="margin-right: -3px;"></span>
                                        <span class="fas fa-minus fa-rotate-90"></span>
                                    </label>
                                </div>
                            </div>
                            <div class="child">
                                <div class="btn-group priority1" id="btn-group-gridView">
                                    <button class="btn btn-default" id="btn-group-gridView_expand">
                                        <span class="fas fa-arrows-alt-v"></span>
                                    </button>
                                    <button class="btn btn-default" id="btn-group-gridView_collapse">
                                        <span class="fas fa-compress" style="transform: rotate(135deg);"></span>
                                    </button>
                                </div>
                            </div>
                            <div class="child">
                                <div class="btn-group priority1" data-toggle="buttons" id="btn-group-miniView">
                                    <label class="btn btn-default" for="btn-group-miniView_full" id="lbl-btn-group-miniView_full" >
                                        <input id="btn-group-miniView_full" type="radio"/>
                                        <span class="fas fa-square" style="margin-right: -7px;"></span>
                                        <span class="fas fa-square"></span>
                                    </label>
                                    <label class="btn btn-default" for="btn-group-miniView_iframe" id="lbl-btn-group-miniView_iframe">
                                        <input id="btn-group-miniView_iframe" type="radio" />
                                        <span class="fas fa-minus fa-rotate-90" style="margin-right: -3px;"></span>
                                        <span class="fas fa-minus fa-globe"></span>
                                    </label>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="btn-group te-display-toolbar-options display-inline-block" id="te-display-toolbar-options">
                        <button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                            <span class="fas fa-bars"></span>
                        </button>
                        <ul class="dropdown-menu pull-right">
                            <li><a href="#" id="dropdown-menu-refresh-btn"><span class="fas fa-sync mr2 ml2"></span><asp:Literal runat="server" ID="litOptionsRefresh" Text="<%$Resources:Buttons,Refresh %>"></asp:Literal></a></li>
                            <li role="separator" class="divider"></li>
                            <li>
                                <a href="#">
                                    <input id="te-menu_always-show-case-description" class="fancy-checkbox" type="checkbox" data-bind="checked: $root.userSettingsAlwaysShowTestRunDetails">
                                    <label for="te-menu_always-show-case-description" id="lblAlwaysShowTestCase" class="fancy-checkbox-label w-100">
                                        <asp:Localize runat="server" ID="litOptionsAlwaysShowTestCase" Text="<%$Resources:Main,TestCaseExecution_AlwaysShowTestCase%>" />
                                    </label>
                                </a>
                            </li>
                            <li>
                                <a href="#">
                                    <input id="te-menu-show-custom-properties" class="fancy-checkbox" type="checkbox" data-bind="checked: $root.userSettingsShowCustomProperties">
                                    <label for="te-menu-show-custom-properties" id="lblShowCustomProperties" class="fancy-checkbox-label w-100">
                                        <asp:Localize runat="server" ID="litOptionsShowCustomProperties" Text="<%$Resources:Main,TestCaseExecution_ShowCustomProperties%>" />
                                    </label>
                                </a>
                            </li>
                            <li role="separator" class="divider"></li>
                            <li><a href="#" id="te-menu-show-guided-tour"><span class="far fa-eye mr2 ml2"></span><asp:Literal runat="server" ID="litOptionsShowGuidedTour" Text="<%$Resources:Buttons,ShowGuidedTour %>"></asp:Literal></a></li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
        <div class="row sticky top-nav bg-white z-2" id="te-header-progress-bar">
            <div class="col-sm-12">
                <h3 class="my0">
                    <asp:Localize runat="server" ID="litProgressBarProgress" Text="<%$Resources:Fields,Progress%>" />
                    <small>
                        ( 
                        <span data-bind="text: liveTotalRun"></span>
                        &nbsp;/&nbsp; 
                        <span data-bind="text: totalTestSteps"></span>
                        <span>
                            <asp:Localize runat="server" ID="litProgressBarComplete" Text="<%$Resources:Buttons,Steps%>" />
                            )
                        </span>
                    </small>
                </h3>
                <div class="te-progress-bar btn-group w-100 my3" id="te-progress-bar">
                    <button id="pb-btn-previous" class="btn btn-default te-pb-btn" data-bind="css: { disabled: progressBarPosition().atStart }, click: pageModelMoveFocus.progressBarBack">
                        <span class="fas fa-chevron-left"></span>
                    </button>
                    <button id="pb-btn-next" class="btn btn-default te-pb-btn" data-bind="css: { disabled: progressBarPosition().atEnd }, click: pageModelMoveFocus.progressBarForward">
                        <span class="fas fa-chevron-right"></span>
                    </button>
                    <button id="pb-btn-play-pause" class="btn btn-default te-pb-btn">
                        <span class="fas fa-pause"></span>
                    </button>
                    <div class="te-progress-steps pull-left mx0">
                        <!-- ko if: progressBarDisplaySteps -->
                            <!-- ko foreach: testRun -->
                                <div class="class-divide"></div>
                                <!-- ko foreach: testRunStep -->
                                    <div class="te-progress-item" data-bind="css: executionStatusClass, attr: { 'data-selected': $data == $root.chosenTestStep() }, click: pageModelSetChosen.selectTestFromProgressBar, event: { mouseover: pageTooltips.displayTestStepTooltip.bind($data), mouseout: pageTooltips.hideTestStepTooltip.bind() }">&nbsp;</div>
                                <!-- /ko -->
                            <!-- /ko -->
                        <!-- /ko -->
                        <!-- ko ifnot: progressBarDisplaySteps -->
                            <!-- ko foreach: testRun -->
                                <div class="te-progress-item" data-bind="css: executionStatusClass, attr: { 'data-selected': $data == $root.chosenTestStepParent() }, click: pageModelSetChosen.selectTestFromRunFromProgressBar, event: { mouseover: pageTooltips.displayTestRunTooltip.bind($data), mouseout: pageTooltips.hideTestRunTooltip.bind() }">&nbsp;</div>
                            <!-- /ko -->
                        <!-- /ko -->
                    </div>
                    <div id="te-end-buttons-marker"></div>
                    <button runat="server" id="pbBtnLeave" class="btn btn-default te-pb-btn" data-bind="css: { 'is-at-end': !allRunsHaveBeenRun()}" title="<%$Resources:Buttons,Leave%>">
                        <span class="fas fa-eject"></span>    
                    </button>
                    <tstsc:LinkButtonEx id="pbBtnFinish" SkinID="ButtonPrimary" runat="server" data-bind="css: { 'is-hidden': !allRunsHaveBeenRun()}"
                        CausesValidation="false" Authorized_ArtifactType="TestRun" Authorized_Permission="Create" title="<%$Resources:Buttons,Finish%>"
                        Confirmation="true" ConfirmationMessage="<%$Resources:Messages,TestCaseExecution_FinishConfirm %>">
                        <span class="fas fa-stop"></span>
                    </tstsc:LinkButtonEx>
                         
                </div>
            </div>
        </div>


        
        <tstsc:MessageBox id="divNewIncidentsMessage" Runat="server" SkinID="MessageBox" />
        <tstsc:MessageBox id="divNewTasksMessage" Runat="server" SkinID="MessageBox" />
        <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
        
        
        
        <div class="row te-main-content my3 display-none" id="te-main-content">
            <div class="te-grid" id="te-grid">
                <div class="border" data-bind="foreach: testRun">
                    <div class="row te-grid-row te-grid-row-tc pa3" data-bind="css: { selected: $data == $root.chosenTestStepParent() }, click: pageModelSetChosen.selectTestFromRun">
                        <div data-bind="css: {cannotEdit: cannotEdit}">
                            <h4>
                                <span class="fa-stack te-tc-status v-mid">
                                    <i class="fa-stack-2x execution-status-bg" data-bind="css: executionStatusIcon"></i>
                                    <i class="fa-stack-1x execution-status-glyph" data-bind="css: executionStatusGlyph"></i>
                                </span>
                                <strong><span data-bind="text: name"></span></strong>
                                <small class="mx3" data-bind="if: testCaseId">
                                    (TC <span data-bind="text: testCaseId"></span>)
                                </small>
                            </h4>
                            <div class="text" data-bind="html: cleanAndFilterXss(description)"></div>
                            <button
                                id="btnExecutionStatusPassAll_grid"
                                runat="server"
                                type="button"
                                class="btn btn-default show-grid-only te-btn execution-status ExecutionStatusPassed pull-right" 
                                data-bind="click: pageModelStatusUpdate.handlePassAllFromGrid.bind($data, false), clickBubble: false"
                                >
                                <span class="fas fa-check fa-fw" style="transform: translateY(0.25em);"></span>
                                <span class="fas fa-check" style="margin-left: -1.3125em; transform: translateY(-0.25em)"></span>
                            </button>
                        </div>
                    </div>
                    <!-- ko foreach: testRunStep -->
                        <div class="row te-grid-row te-grid-row-ts py2 px3" data-bind="css: { selected: $data == $root.chosenTestStep() }, click: pageModelSetChosen.selectTest">
                            <div class="col-sm-12 te-grid-row-heading pr0 pl3" data-bind="css: {cannotEdit: cannotEdit}">
                                <div class="btn-group show-grid-only te-btn-grp mb3 pull-right pr0 ml3 w-auto" data-bind="css: {cannotEdit: cannotEdit}">
                                    <div class="btn-group" role="group">
                                        <button class="btn btn-default dropdown-toggle mobile-rounded" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                            <span class="fas fa-plus fa-fw"></span>
                                            <span class="caret"></span>
                                        </button>
                                        <ul class="dropdown-menu pull-right">
                                            <li><a href="#" data-bind="click: function() { $root.actualResultNeedsFocus(true) }"><asp:Localize ID="lnkAddActualResult" runat="server" Text="<%$Resources:Main,TestCaseExecution_ActualResult%>" /></a></li>
                                            <li role="separator" class="divider"></li>
                                            <li>
                                                <tstsc:HyperLinkEx ID="lnkAddNewAttachment" runat="server" NavigateUrl="javascript:void(0)" ClientScriptMethod="tstucAttachmentPanel.upload_attachment(event)" Authorized_Permission="Modify" Authorized_ArtifactType="Document">
                                                    <img class="w4 h4" data-bind="attr: {src: SpiraContext.BaseThemeUrl + 'Images/artifact-Document.svg'}" />
                                                    <asp:Localize Text="<%$Resources:Main,TestCaseExecution_AddNewAttachment %>" runat="server" ID="lblAddNewAttachment" />
                                                </tstsc:HyperLinkEx>
                                            </li>
                                            <li>
                                                <tstsc:HyperLinkEx ID="lnkAddExistingAttachment" runat="server" NavigateUrl="javascript:void(0)" ClientScriptMethod="tstucAttachmentPanel.add_existing(event)" Authorized_Permission="Modify" Authorized_ArtifactType="Document">
                                                    <span class="fas fa-link"></span>
                                                    <asp:Localize Text="<%$Resources:Main,TestCaseExecution_LinkExistingAttachment %>" runat="server" ID="lblAddExistingAttachment" />
                                                </tstsc:HyperLinkEx>
                                            </li>
                                            <!-- ko if: pageProps.canViewTasks -->
                                                <li role="separator" class="divider"></li>
                                                <li  class="hidden-xs">
                                                    <a href="javascript:void(0)" onclick="pageManageTasks.openTaskModal()">
                                                        <img class="w4 h4" data-bind="attr: {src: SpiraContext.BaseThemeUrl + 'Images/artifact-Task.svg'}" />
                                                        <asp:Localize runat="server" ID="Localize2" Text="<%$Resources:Dialogs,TaskList_NewTask %>" />
                                                    </a>
                                                </li>
                                            <!-- /ko -->
                                            <li role="separator" class="divider"></li>
                                            <li  class="hidden-xs">
                                                <a href="javascript:void(0)" onclick="pageManageIncidentForm.openIncidentAssociationModal()">
                                                    <img class="w4 h4" data-bind="attr: {src: SpiraContext.BaseThemeUrl + 'Images/artifact-Incident.svg'}" />
                                                    <asp:Localize runat="server" ID="lblAddNewIncident" Text="<%$Resources:ServerControls,TabControl_NewIncident %>" />
                                                </a>
                                            </li>
                                            <li>
                                                <tstsc:HyperLinkEx 
                                                    Authorized_ArtifactType="Incident" 
                                                    Authorized_Permission="Modify" 
                                                    ClientScriptMethod="pageManageIncidentForm.associateExistingIncident('associateIncidentInsideModal', 'associateIncidentModal')" 
                                                    ID="lnkAddExistingIncident" 
                                                    NavigateUrl="javascript:void(0)"
                                                    runat="server" 
                                                    >
                                                    <span class="fas fa-link"></span>
                                                    <asp:Localize runat="server" ID="lblAddExistingIncident" Text="<%$Resources:Main,TestCaseExecution_LinkExisting %>" />
                                                </tstsc:HyperLinkEx>       
                                            </li>
                                        </ul>
                                    </div>
                                    <button class="btn btn-default hidden-mobile" data-toggle="modal" data-target="#te-grid-modal-screenshot">
                                        <span class="fas fa-image fa-fw" ></span>
                                    </button>
                                </div>
                                <div class="btn-group show-grid-only te-btn-grp te-btn-grp-status mb3 pull-right pr0 w-auto" data-bind="css: {cannotEdit: cannotEdit}">
                                    <button class="btn btn-default execution-status ExecutionStatusPassed" data-bind="click: pageModelStatusUpdate.handleStatusUpdateFromGrid.bind($data, 2), css: { selected: executionStatusId() == 2 }, disable: $root.appIsBusy">
                                        <span class="fas fa-check fa-fw"></span>
                                    </button>
                                    <button 
                                        id="btnExecutionStatusBlocked_grid"
                                        runat="server"
                                        type="button"
                                        class="btn btn-default execution-status ExecutionStatusBlocked" 
                                        data-bind="click: pageModelStatusUpdate.handleStatusUpdateFromGrid.bind($data, 5), css: { selected: executionStatusId() == 5 }, disable: $root.appIsBusy"
                                        >
                                        <span class="fas fa-ban fa-fw"></span>
                                    </button>
                                    <button 
                                        id="btnExecutionStatusCaution_grid"
                                        runat="server"
                                        type="button"
                                        class="btn btn-default execution-status ExecutionStatusCaution" 
                                        data-bind="click: pageModelStatusUpdate.handleStatusUpdateFromGrid.bind($data, 6), css: { selected: executionStatusId() == 6 }, disable: $root.appIsBusy"
                                        >
                                        <span class="fas fa-exclamation-triangle fa-fw"></span>
                                    </button>
                                    <button class="btn btn-default execution-status ExecutionStatusFailed" data-bind="click: pageModelStatusUpdate.handleStatusUpdateFromGrid.bind($data, 1), css: { selected: executionStatusId() == 1 }, disable: $root.appIsBusy">
                                        <span class="fas fa-times fa-fw"></span>
                                    </button>
                                    <button 
                                        id="btnExecutionStatusNA_grid"
                                        runat="server"
                                        type="button"
                                        class="btn btn-default execution-status ExecutionStatusNotApplicable" 
                                        data-bind="click: pageModelStatusUpdate.handleStatusUpdateFromGrid.bind($data, 4), css: { selected: executionStatusId() == 4 }, disable: $root.appIsBusy"
                                        >
                                        <span class="fas fa-minus fa-fw"></span>
                                    </button>
                                </div>
                                <div class="te-grid-row-ts-heading">
                                    <span class="fa-stack te-ts-status v-mid">
                                        <i class="fa-stack-2x execution-status-bg" data-bind="css: executionStatusIcon"></i>
                                        <i class="fa-stack-1x execution-status-glyph" data-bind="css: executionStatusGlyph"></i>
                                    </span>
                                    <p class="di">
                                        <strong><span data-bind="text: positionTitle"></span>:</strong>
                                        <span class="text" data-bind="html: cleanAndFilterXss(description)"></span>
                                    </p>

                                </div>
                            </div>
                            <div class="clearfix"></div>
                            <div class="details show-grid-only" data-bind="css: {cannotEdit: cannotEdit}">
                                <div class="details-item" data-bind="if: expectedResult">
                                    <div class="col-md-2 col-sm-3 label-div">
                                        <label><asp:Literal runat="server" ID="lblExpectedResultGrid" Text="<%$Resources:Main,TestCaseExecution_ExpectedResult%>" /></label>
                                    </div>
                                    <div class="col-md-10 col-sm-9 field-div">
                                        <div class="text" data-bind="html: cleanAndFilterXss(expectedResult)"></div>
                                        <div class="btn btn-default btn-xs te-grid-height-toggle" >
                                            <span class="fas fa-fw fa-caret-down te-grid-glyph-expand"></span>
                                            <span class="fas fa-fw fa-caret-up te-grid-glyph-collapse"></span>
                                        </div>
                                    </div>
                                </div>
                                <div class="details-item" data-bind="if: sampleData">
                                    <div class="clearfix"></div>
                                    <div class="col-md-2 col-sm-3 label-div">
                                        <label><asp:Literal runat="server" ID="lblSampleDataGrid" Text="<%$Resources:Main,TestCaseExecution_SampleData%>" /></label>
                                    </div>
                                    <div class="col-md-10 col-sm-9 field-div">
                                        <div class="text" data-bind="html: cleanAndFilterXss(sampleData)"></div>
                                        <div class="btn btn-default btn-xs te-grid-height-toggle">
                                            <span class="fas fa-fw fa-caret-down te-grid-glyph-expand"></span>
                                            <span class="fas fa-fw fa-caret-up te-grid-glyph-collapse"></span>
                                        </div>
                                    </div>
                                </div>
                                <div class="details-item" data-bind="visible: ($data == $root.chosenTestStep() && $root.actualResultNeedsFocus() ? false : actualResult)">
                                    <div class="clearfix"></div>
                                    <div class="col-md-2 col-sm-3 label-div">
                                        <label><asp:Localize runat="server" ID="lblActualResultGrid" Text="<%$Resources:Main,TestCaseExecution_ActualResult%>" /></label>
                                    </div>
                                    <div class="col-md-10 col-sm-9 field-div">
                                        <div class="text" data-bind="html: cleanAndFilterXss(actualResult()), click: pageModelStatusUpdate.openGridActualResult, clickBubble: false"></div>
                                        <div class="btn btn-default btn-xs te-grid-height-toggle">
                                            <span class="fas fa-fw fa-caret-down te-grid-glyph-expand"></span>
                                            <span class="fas fa-fw fa-caret-up te-grid-glyph-collapse"></span>
                                        </div>
                                    </div>
                                </div>
                                <div class="details-item" data-bind="if: ($data == $root.chosenTestStep() && $root.userDisplayIsGrid()), visible: $root.enableActualResultInGrid()">
                                    <div class="clearfix"></div>
                                    <div class="col-md-2 col-sm-3 label-div">
                                        <label><asp:Localize runat="server" ID="lblActualResultGrid2" Text="<%$Resources:Main,TestCaseExecution_ActualResult%>" /></label>
                                    </div>
                                    <div class="col-md-10 col-sm-9 field-div">
                                        <button class="btn btn-xs btn-default te-grid-close-toggle" data-bind="css: {disabled: $root.chosenPendingStatusUpdate}, click: function () { pageModelStatusUpdate.closeGridActualResult() }">
                                            <span class="fas fa-times fa-fw"></span>
                                        </button>
                                        <p class="alert alert-info alert-narrow" data-bind="visible: $root.chosenPendingStatusUpdate">
                                            <span data-bind="text: $root.chosenPendingWarningMessage"></span>
                                        </p>
                                        <div data-bind="css: {cannotEdit: cannotEdit}">
                                            <textarea 
                                                class="text" 
                                                id="txtGridActualResults" 
                                                data-bind="richText: $root.chosenTestStep().actualResult, uniqueName: true, field: 'actualResult'"
                                                >
                                            </textarea>
                                        </div>
                                        <button type="button" class="btn btn-default execution-status mb3" data-bind="visible: $root.chosenPendingStatusUpdate, css: $root.chosenPendingStatusClass, click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, $root.chosenPendingStatusUpdate())">
                                            <span class="fas fa-fw" data-bind="css: $root.chosenPendingStatusGlyph"></span>
                                            <span class="label-text" data-bind="text: $root.chosenPendingStatusName"></span>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    <!-- /ko -->
                </div>
            </div>
            <div class="modal" id="te-grid-modal-screenshot">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <p class="alert alert-info center col-lg-8">
                            <asp:Literal runat="server" ID="msgAddScreenshotUsingEditor" Text="<%$Resources:Messages,TestCaseExecution_AddScreenshotUsingEditor %>"></asp:Literal>
                        </p>
                    </div>
                </div>
            </div>
            



            <%-- INSPECTOR VIEW --%>
            <div class="te-inspector" id="te-inspector">
                <div class="border u-wrapper width_md">
                    <div class="row mx0" data-bind="css: {cannotEdit: chosenTestStep().cannotEdit}">
                        <div class="btn-group col-sm-12 te-execution-status-btn-grp mb3 px0" data-bind="with: chosenTestStep" id="te-inspector-execution-status-btn-grp">
                            <div class="btn-group">
                                <button type="button" class="btn btn-default execution-status ExecutionStatusPassed" data-bind="click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, 2), css: { selected: executionStatusId() == 2 }, disable: $root.appIsBusy">
                                    <span class="fas fa-check fa-fw"></span>
                                    <span class="label-text"><asp:Literal runat="server" ID="lblBtnPass" Text="<%$Resources:Buttons,Pass %>"></asp:Literal></span>
                                </button>
                                <button 
                                    id="btnExecutionStatusPassAll_inspector"
                                    runat="server"
                                    type="button" 
                                    class="btn btn-default dropdown-toggle execution-status ExecutionStatusPassed" 
                                    data-toggle="dropdown" 
                                    aria-haspopup="true" 
                                    aria-expanded="false" 
                                    data-bind="visible: $root.displayTestRunInInspector, disable: $root.appIsBusy"
                                    >
                                    <span class="caret"></span>
                                    <span class="label-text caret-dropdown-mobile-fix">&nbsp;</span>
                                    <span class="sr-only">Toggle Dropdown</span>
                                </button>
                                <ul class="dropdown-menu ExecutionStatusPassed execution-status-bg" data-bind="visible: $root.displayTestRunInInspector">
                                    <li><a href="#" class="mb0" data-bind="click: pageModelStatusUpdate.handlePassAll.bind($data, true)">
                                        <span class="fas fa-check fa-fw" style="transform: translateY(0.25em);"></span>
                                        <span class="fas fa-check" style="margin-left: -1.3125em; transform: translateY(-0.25em)"></span>
                                        <asp:Literal runat="server" ID="lblBtnPassAll" Text="<%$Resources:Buttons,PassAll %>"></asp:Literal>
                                    </a></li>
                                </ul>
                            </div>
                            <button 
                                id="btnExecutionStatusBlocked_inspector"
                                runat="server"
                                type="button"
                                class="btn btn-default execution-status ExecutionStatusBlocked" 
                                data-bind="click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, 5), css: { selected: executionStatusId() == 5 }, disable: $root.appIsBusy"
                                >
                                <span class="fas fa-ban fa-fw"></span>
                                <span class="label-text"><asp:Literal runat="server" ID="lblBtnBlocked" Text="<%$Resources:Buttons,Blocked %>"></asp:Literal></span>
                            </button>
                            <button 
                                id="btnExecutionStatusCaution_inspector"
                                runat="server"
                                type="button"
                                class="btn btn-default execution-status ExecutionStatusCaution" 
                                data-bind="click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, 6), css: { selected: executionStatusId() == 6 }, disable: $root.appIsBusy"
                                >
                                <span class="fas fa-exclamation-triangle fa-fw"></span>
                                <span class="label-text"><asp:Literal runat="server" ID="lblBtnCaution" Text="<%$Resources:Buttons,Caution %>"></asp:Literal></span>
                            </button>
                            <button class="btn btn-default execution-status ExecutionStatusFailed" data-bind="click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, 1), css: { selected: executionStatusId() == 1 }, disable: $root.appIsBusy">
                                <span class="fas fa-times fa-fw"></span>
                                <span class="label-text"><asp:Literal runat="server" ID="lblBtnFail" Text="<%$Resources:Buttons,Fail %>"></asp:Literal></span>
                            </button>
                            <button 
                                id="btnExecutionStatusNA_inspector"
                                runat="server"
                                type="button"
                                class="btn btn-default execution-status ExecutionStatusNotApplicable" 
                                data-bind="click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, 4), css: { selected: executionStatusId() == 4 }, disable: $root.appIsBusy"
                                >
                                <span class="fas fa-minus fa-fw"></span>
                                <span class="label-text">N/A</span>
                            </button>
                        </div>
                    </div>
                    <div class="te-tc mx0" data-bind="with: chosenTestStepParent, visible: displayTestRunInInspector, css: {cannotEdit: $root.chosenTestStep().cannotEdit}">
                            <h3 class="my3 dib">
                                <span class="fa-stack te-tc-status">
                                    <i class="fas fa-stack-2x execution-status-bg" data-bind="css: executionStatusIcon"></i>
                                    <i class="fas fa-stack-1x execution-status-glyph" data-bind="css: executionStatusGlyph"></i>
                                </span>
                                <span data-bind="text: name"></span>
                                <small data-bind="if: testCaseId">
                                    (TC <span data-bind="text: testCaseId"></span>)
                                </small>
                            </h3>
                            <div class="text" data-bind="html: cleanAndFilterXss(description)"></div>
                    </div>
                    




                    <%-- TEST RUN CASE CUSTOM PROPERTIES --%>
                    <div data-bind="stopBinding: true, visible: $root.userSettingsShowCustomPropertiesOfTestCase">
                        <tstsc:AjaxFormManager 
                            AutoLoad="false" 
                            CheckUnsaved="false"
                            ID="ajxTestRunCaseFormManager" 
                            ErrorMessageControlId="lblFirstPageMessages" 
                            runat="server" 
                            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService" 
                            WorkflowEnabled="true"
                            >
                        </tstsc:AjaxFormManager>
                        <div
                            class="u-box_children-1 children-read-only"
                            ID="pnlTestRunCaseCustomProperties" 
                            >
                            <ul 
                                class="u-box_list pl4"
                                id="customFieldsTestRunCaseDefault"
                                runat="server"
                                ></ul>
                        </div>
                        <div class="u-box_3 u-cke_is-minimal children-read-only" onclick="function(e) {e.preventDefault()}" >
                            <ul 
                                class="u-box_list pl4 labels_absolute"
                                id="customFieldsTestRunCaseRichText"
                                runat="server"
                                ></ul>
                        </div>
                    </div>
                    



                    <%-- TEST RUN STEP FIELDS --%>
                    <div class="te-ts u-box_3" data-bind="with: chosenTestStep, css: {cannotEdit: chosenTestStep().cannotEdit}">
                        <div class="my3">
                            <h3 class="my3">
                                <span class="fa-stack te-ts-status">
                                    <i class="fas fa-stack-2x execution-status-bg" data-bind="css: executionStatusIcon"></i>
                                    <i class="fas fa-stack-1x execution-status-glyph" data-bind="css: executionStatusGlyph"></i>
                                </span>
                                <span data-bind="text: positionTitle"></span>
                            </h3>
                        </div>
                        <ul class="u-box_list children-read-only">
                            <li class="ma0 pa0 mb3">
                                <label><asp:Literal runat="server" ID="lblTestStepDescription" Text="<%$Resources:Fields,Description%>"></asp:Literal></label>
                                <div class="u-box_list_control text" data-bind="html: cleanAndFilterXss(description)">
                                </div>
                            </li>
                            <li class="ma0 pa0 mb3" data-bind="if: expectedResult">
                                <label><asp:Literal runat="server" ID="lblExpectedResultInspector" Text="<%$Resources:Main,TestCaseExecution_ExpectedResult%>"></asp:Literal></label>
                                <div class="u-box_list_control text" data-bind="html: cleanAndFilterXss(expectedResult)"></div>
                            </li>
                            <li class="ma0 pa0 mb3" data-bind="if: sampleData">
                                <label><asp:Literal runat="server" ID="lblSampleDataInspector" Text="<%$Resources:Main,TestCaseExecution_SampleData%>"></asp:Literal></label>
                                <div class="u-box_list_control text" data-bind="html: cleanAndFilterXss(sampleData)"></div>
                            </li>
                        </ul>
                    </div>
                    



                    <%-- TEST RUN STEP CUSTOM PROPERTIES --%>
                    <div data-bind="stopBinding: true, visible: $root.userSettingsShowCustomProperties">
                        <tstsc:AjaxFormManager 
                            AutoLoad="false" 
                            CheckUnsaved="false" 
                            ErrorMessageControlId="lblFirstPageMessages" 
                            ID="ajxTestRunStepFormManager" 
                            runat="server" 
                            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestStepService" 
                            WorkflowEnabled="true"
                            HideOnAuthorizationFailure="true"
                            >
                        </tstsc:AjaxFormManager>
                        <div
                            class="u-box_children-1 children-read-only"
                            ID="pnlTestRunStepCustomProperties" 
                            >
                            <ul 
                                class="u-box_list pl4"
                                id="customFieldsTestRunStepDefault"
                                runat="server"
                                ></ul>
                        </div>
                        <div class="u-box_3 u-cke_is-minimal children-read-only" >
                            <ul 
                                class="u-box_list pl4 labels_absolute"
                                id="customFieldsTestRunStepRichText"
                                runat="server"
                                ></ul>
                        </div>
                    </div>
                    



                    <%-- TABS --%>
                    <div class="row px4">
                        <ul class="nav nav-tabs te-tab-buttons mb3 mt4" role="tablist">
                            <li role="presentation" class="active">
                                <a href="#te-inspector_tab_actual-results" role="tab" data-toggle="tab" id="te-inspector-tab-buttons_actual-results">
                                    <i class="fas fa-clipboard-list mr3"></i>
                                    <asp:Localize ID="lblActualResultInspector" runat="server" Text="<%$Resources:Main,TestCaseExecution_ActualResult%>" />
                                </a>
                            </li>
                            <li role="presentation">
                                <a href="#te-inspector_tab_attachments" role="tab" data-toggle="tab" id="te-inspector-tab-buttons_attachments">
                                    <tstsc:ImageEx 
                                        CssClass="w4 h4 mr3"
                                        ImageUrl="Images/artifact-Document.svg"
                                        runat="server"
                                        />
                                    <asp:Localize ID="lblTabAttachments" runat="server" Text="<%$Resources:ServerControls,TabControl_Attachments%>" />
                                </a>
                            </li>
                            <li role="presentation" data-bind="if: pageProps.canViewTasks">
                                <a href="#te-inspector_tab_tasks" role="tab" data-toggle="tab" id="te-inspector-tab-buttons_tasks">
                                    <tstsc:ImageEx 
                                        CssClass="w4 h4 mr3"
                                        ImageUrl="Images/artifact-Task.svg"
                                        runat="server"
                                        />
                                    <asp:Localize ID="lblTasks" runat="server" Text="<%$Resources:Fields,Tasks%>" />
                                </a>
                            </li>
                            <li role="presentation">
                                <a href="#te-inspector_tab_incidents" role="tab" data-toggle="tab" id="te-inspector-tab-buttons_incidents" data-bind="click: function() { pageControls.handleIncidentTabClick() }">
                                    <tstsc:ImageEx 
                                        CssClass="w4 h4 mr3"
                                        ImageUrl="Images/artifact-Incident.svg"
                                        runat="server"
                                        />
                                    <asp:Localize ID="lblTabIncidents" runat="server" Text="<%$Resources:ServerControls,TabControl_Incidents%>" />
                                    <i class="fas fa-asterisk ml1 light-silver fs-80 fw-b" data-bind="visible: _pageInfo.requireIncident && $root.chosenTestStepIncidents().length"></i>
                                </a>
                            </li>
                        </ul>
                    </div>





                    <div class="tab-content" data-bind="css: {cannotEdit: chosenTestStep().cannotEdit}">
                        
                        
                        <%-- ACTUAL RESULTS PANEL --%>
                        <div role="tabpanel" class="tab-pane active" id="te-inspector_tab_actual-results" data-bind="if: $root.enableActualResultInInspector()">
                            <p class="alert alert-warning alert-narrow" data-bind="visible: $root.chosenPendingStatusUpdate">
                                <span data-bind="text: $root.chosenPendingWarningMessage"></span>
                            </p>
                            <div>
                                <textarea 
                                    data-bind="richText: chosenTestStep().actualResult, field: 'actualResult'" 
                                    id="txtInspectorActualResults"
                                    >
                                </textarea>
                            </div>
                            <button type="button" class="btn btn-default execution-status mb3" data-bind="visible: $root.chosenPendingStatusUpdate, css: $root.chosenPendingStatusClass, click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, $root.chosenPendingStatusUpdate())">
                                <span class="fas fa-fw" data-bind="css: $root.chosenPendingStatusGlyph"></span>
                                <span data-bind="text: $root.chosenPendingStatusName"></span>
                            </button>
                        </div>




                        <%-- ATTACHMENT PANEL --%>
                        <div role="tabpanel" class="tab-pane" id="te-inspector_tab_attachments" data-bind="stopBinding: true">
                            <p class="alert alert-narrow alert-info center">
                                <asp:Literal runat="server" ID="msgAddScreenshotUsingEditor2" Text="<%$Resources:Messages,TestCaseExecution_AddScreenshotUsingEditor %>"></asp:Literal>
                            </p>
                            <asp:Panel runat="server" ID="pnlTestExecution_Attachments">
                                <tstuc:AttachmentPanel id="tstAttachmentPanel" Runat="server" ArtifactTypeEnum="TestRun" />
                            </asp:Panel>
                        </div>




                        <%-- INCIDENT PANEL --%>
                        <div role="tabpanel" class="tab-pane" id="te-inspector_tab_incidents">
                            <p class="pa3" id="existing-incidents-legend">

                                <span 
                                    class="mr4"
                                    data-bind="visible: ($root.chosenTestStepIncidents().length === 0)"
                                    >
                                    <asp:literal runat="server" ID="msgNoExistingIncidents" Text="<%$Resources:Main,TestCaseExecution_NoExistingIncident%>" />
                                </span>
                                <span 
                                    class="mr4"
                                    data-bind="visible: $root.chosenTestStepIncidents().length > 0"
                                    >
                                    <asp:literal runat="server" ID="Literal1" Text="<%$Resources:Main,TestCaseExecution_ExistingIncidents%>" />
                                </span>
                                
                                
                                <tstsc:HyperLinkEx 
                                    Authorized_ArtifactType="Incident" 
                                    Authorized_Permission="Modify" 
                                    ClientScriptMethod="pageManageIncidentForm.associateExistingIncident('associateIncidentPanel')" 
                                    ID="btnLinkExistingIncident" 
                                    NavigateUrl="javascript:void(0)" 
                                    runat="server" 
                                    SkinID="ButtonDefaultSmall" 
                                    Text="<%$Resources:Main,TestCaseExecution_LinkExisting %>" 
                                    />
                            </p>

                            <div id="associateIncidentPanel"></div>
                            
                            <div data-bind="visible: $root.chosenTestStepIncidents().length > 0"> 
                                <table id="tblExistingIncidents" class="DataGrid" style="width:100%; margin-top:10px;">
                                    <thead>
                                        <tr class="Header">
                                            <th class="priority3"><asp:Localize ID="lblIncidentIdGrid" runat="server" Text="<%$Resources:Fields,IncidentId %>" /></th>
                                            <th class="priority1"><asp:Localize ID="lblIncidentNameGrid" runat="server" Text="<%$Resources:Fields,Name %>" /></th>
                                            <th class="priority1"><asp:Localize ID="lblIncidentTypeIdGrid" runat="server" Text="<%$Resources:Fields,IncidentTypeId %>" /></th>
                                            <th class="priority2"><asp:Localize ID="lblIncidentStatusIdGrid" runat="server" Text="<%$Resources:Fields,IncidentStatusId %>" /></th>
                                            <th class="priority4"><asp:Localize ID="lblIncidentPriorityIdGrid" runat="server" Text="<%$Resources:Fields,PriorityId %>" /></th>
                                            <th class="priority4"><asp:Localize ID="lblIncidentSeverityIdGrid" runat="server" Text="<%$Resources:Fields,SeverityId %>" /></th>
                                            <th class="priority2"><asp:Localize ID="lblIncidentOwnerIdGrid" runat="server" Text="<%$Resources:Fields,OwnerId %>" /></th>
                                            <th class="priority4"><asp:Localize ID="lblIncidentCreationDateGrid" runat="server" Text="<%$Resources:Fields,CreationDate %>" /></th>
                                            <th class="priority4"><asp:Localize ID="lblIncidentOpenerIdGrid" runat="server" Text="<%$Resources:Fields,OpenerId %>" /></th>
                                        </tr>                            
                                    </thead>
                                    <tbody data-bind="foreach: $root.chosenTestStepIncidents">
                                        <tr class="Normal">
                                            <td data-bind="text: incidentId" class="priority3">
                                            </td>
                                            <td class="priority1">
                                                <tstsc:ImageEx ID="imgIncident" runat="server" CssClass="w4 h4" ImageUrl="Images/artifact-Incident.svg" AlternateText="Incident" />
                                                <a data-bind="text: name, 
                                                    attr: { href: _pageInfo.get_incidentBaseUrl().replace(globalFunctions.artifactIdToken, $data.incidentIdIntValue) },
                                                    event: { mouseover: pageTooltips.displayIncidentTooltip.bind($data), mouseout: pageTooltips.hideIncidentTooltip.bind() }">
                                                </a>
                                            </td>
                                            <td class="priority1" data-bind="text: incidentType"></td>
                                            <td class="priority2" data-bind="text: incidentStatus"></td>
                                            <td class="priority4" data-bind="text: globalFunctions.displayIfDefined(priorityValue), style: { backgroundColor: (priorityId && priorityClass) ? '#' + priorityClass : '', whiteSpace: 'nowrap' }" ></td>
                                            <td class="priority4" data-bind="text: globalFunctions.displayIfDefined(severityValue), style: { backgroundColor: (severityId && severityClass) ? '#' + severityClass : '', whiteSpace: 'nowrap' }"></td>
                                            <td class="priority2" data-bind="text: globalFunctions.displayIfDefined(owner)"></td>
                                            <td class="priority4" data-bind="text: creationDate"></td>
                                            <td class="priority4" data-bind="text: opener"></td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>





                            <%-- INCIDENT FORM FOR ADDING NEW INCIDENT --%>
                            <asp:Panel runat="server" ID="pnlTestExecution_Incidents" class="modal-inWaiting-modal">
                                <div class="modal-inWaiting-dialog">
                                    <div class="modal-inWaiting-content"> 
                                        


                                        <section class="u-wrapper width_md">
                                            <div class="u-box_3">
                                                <tstsc:MessageBox 
                                                    ID="msgNewIncidentMessageInline" 
                                                    runat="server" 
                                                    SkinID="MessageBox" 
                                                    />
                                                <div class="u-box_group" >
                                                    <div 
                                                        class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3"
                                                        aria-expanded="false">
                                                        <asp:Localize 
                                                            runat="server" 
                                                            Text="<%$Resources:ServerControls,TabControl_NewIncident %>" />
                                                    </div>
                                                    
                                                    <%-- warning message to show when an incident is required and has not yet been added --%>
                                                    <p class="alert alert-error my4 fw-110" id="msgIncidentRequired" data-bind="visible: $root.showAddIncidentMessage">
                                                        <asp:Localize 
                                                            ID="Localize1" 
                                                            runat="server" 
                                                            Text="<%$Resources:Main,TestCaseExecution_IncidentRequired %>" 
                                                            />
                                                    </p>

                                                    <p class="my3 fs-110 pa3 bg-info br3">
                                                        <asp:Localize 
                                                            ID="lblEnterIncidentInfo" 
                                                            runat="server" 
                                                            Text="<%$Resources:Main,TestCaseExecution_EnterIncidentInfo %>" 
                                                            />
                                                    </p>
                                                    <div class="u-box_3">
                                                        <ul class="u-box_list">
                                                            <li class="ma0 pa0 mb4 fs-h4">
                                                                <tstsc:LabelEx 
                                                                    AppendColon="true" 
                                                                    AssociatedControlID="txtIncidentName" 
                                                                    ID="lblIncidentName" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,Name %>" 
                                                                    />
                                                                <asp:HiddenField ID="hdnIncidentStatus" runat="server" />
                                                                <tstsc:UnityTextBoxEx 
                                                                    CssClass="u-input is-active" 
                                                                    ClientIdMode="Static"
                                                                    ID="txtIncidentName" 
                                                                    MaxLength="255" 
                                                                    PlaceHolder="<%$ Resources:Fields,TestCase_Execution_EnterIncidentName %>"
                                                                    runat="server" 
                                                                    TextMode="SingleLine" 
                                                                    />
                                                            </li>
                                                        </ul>
                                                    </div>
                                                    <div class="u-box_1">
                                                        <ul class="u-box_list">
                                                            <li class="mao pa0">
                                                                <tstsc:LabelEx 
                                                                    AppendColon="true" 
                                                                    AssociatedControlID="ddlIncidentType" 
                                                                    ID="lblIncidentType" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,IncidentTypeId %>" 
                                                                    />
                                                                <tstsc:UnityDropDownListEx 
                                                                    CssClass="u-dropdown"
                                                                    DataMember="IncidentType" 
                                                                    DataTextField="Name" 
                                                                    DataValueField="IncidentTypeId" 
                                                                    DisabledCssClass="u-dropdown disabled"
                                                                    ID="ddlIncidentType" 
                                                                    runat="server" 
                                                                    />
                                                            </li>
                                                            <li class="ma0 pa0">
                                                                <tstsc:LabelEx 
                                                                    AppendColon="true" 
                                                                    AssociatedControlID="ddlResolvedRelease" 
                                                                    ID="lblResolvedRelease" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,ResolvedReleaseId %>" 
                                                                    />
                                                                <tstsc:UnityDropDownHierarchy 
                                                                    ActiveItemField="Isactive" 
                                                                    DataTextField="FullName" 
                                                                    DataValueField="ReleaseId" 
                                                                    ID="ddlResolvedRelease" 
                                                                    NoValueItem="True" 
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>" 
                                                                    runat="server" 
                                                                    />
                                                            </li>
                                                            <li class="ma0 pa0">
                                                                <tstsc:LabelEx 
                                                                    AppendColon="true" 
                                                                    AssociatedControlID="ddlVerifiedRelease" 
                                                                    ID="lblVerifiedRelease" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,VerifiedReleaseId %>" 
                                                                    />
                                                                <tstsc:DropDownHierarchy 
                                                                    ActiveItemField="IsActive" 
                                                                    DataTextField="FullName" 
                                                                    DataValueField="ReleaseId" 
                                                                    ID="ddlVerifiedRelease" 
                                                                    NoValueItem="True" 
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>" 
                                                                    runat="server" 
                                                                    />
                                                            </li>
                                                        </ul>
                                                        <ul 
                                                            class="u-box_list"
                                                            id="customFieldsIncidentsUsers"
                                                            runat="server"
                                                            >
                                                            <li class="ma0 pa0">
                                                                <tstsc:LabelEx 
                                                                    AppendColon="true" 
                                                                    AssociatedControlID="ddlOwner" 
                                                                    ID="lblOwner" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,OwnerId %>" 
                                                                    />
                                                                <tstsc:UnityDropDownUserList 
                                                                    CssClass="u-dropdown u-dropdown_user" 
                                                                    ID="ddlOwner"
                                                                    DataTextField="FullName" 
                                                                    DataValueField="UserId" 
                                                                    DisabledCssClass="u-dropdown u-dropdown_user disabled"
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>" 
                                                                    NoValueItem="True" 
                                                                    runat="server" 
                                                                    />
                                                            </li>
                                                        </ul>
                                                    </div>
                                                    <div class="u-box_1">
                                                        <ul 
                                                            class="u-box_list"
                                                            id="customFieldsIncidentsDefault"
                                                            runat="server"
                                                            >
                                                            <li class="mao pa0">
                                                                <tstsc:LabelEx 
                                                                    AssociatedControlID="ddlPriority" 
                                                                    ID="lblPriority" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,PriorityId %>" 
                                                                    />
                                                                <tstsc:UnityDropDownListEx 
                                                                    CssClass="u-dropdown"
                                                                    DataMember="IncidentPriority" 
                                                                    DataTextField="Name" 
                                                                    DataValueField="PriorityId"
                                                                    DisabledCssClass="u-dropdown disabled"
                                                                    ID="ddlPriority" 
                                                                    NoValueItem="True" 
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>" 
                                                                    runat="server" 
                                                                    />
                                                            </li>
                                                            <li class="mao pa0">
                                                                <tstsc:LabelEx 
                                                                    AppendColon="true" 
                                                                    AssociatedControlID="ddlSeverity" 
                                                                    ID="lblSeverity" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,SeverityId %>" 
                                                                    />
                                                                <tstsc:UnityDropDownListEx 
                                                                    CssClass="u-dropdown"
                                                                    DataMember="IncidentSeverity" 
                                                                    DataTextField="Name" 
                                                                    DataValueField="SeverityId"
                                                                    DisabledCssClass="u-dropdown disabled"
                                                                    ID="ddlSeverity" 
                                                                    NoValueItem="True" 
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>" 
                                                                    runat="server" 
                                                                    />
                                                            </li>
                                                            <li class="mao pa0">
                                                                <tstsc:LabelEx 
                                                                    AppendColon="true" 
                                                                    AssociatedControlID="ddlComponent" 
                                                                    ID="ddlComponentLabel" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,ComponentId %>" 
                                                                    />
                                                                <tstsc:UnityDropDownMultiList 
                                                                    ActiveItemField="IsActive" 
                                                                    CssClass="u-dropdown" 
                                                                    DataTextField="Name" 
                                                                    DataValueField="ComponentId" 
                                                                    DisabledCssClass="u-dropdown disabled"
                                                                    ID="ddlComponent" 
                                                                    NoValueItem="True" 
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                                                    runat="server" 
                                                                    SelectionMode="Multiple" 
                                                                    />
                                                            </li>
                                                        </ul>
                                                    </div>
                                                    <div class="u-box_1">
                                                        <ul 
                                                            class="u-box_list"
                                                            id="customFieldsIncidentsDates"
                                                            runat="server"
                                                            ></ul>
                                                    </div>
                                                    <div class="u-box_3 u-cke_is-minimal">
                                                        <ul 
                                                            class="u-box_list labels_absolute"
                                                            id="customFieldsIncidentsRichText"
                                                            runat="server"
                                                            ></ul>
                                                    </div>
                                                </div>
                                            </div>
                                        </section>

                                        <div class="pa5 pt3 btn-group">
                                            <tstsc:HyperLinkEx 
                                                SkinID="ButtonPrimary" 
                                                id="newIncidentAdd" 
                                                runat="server" 
                                                NavigateUrl="javascript:void(0)" 
                                                Text="<%$Resources:Buttons,Add %>" 
                                                ClientScriptMethod="pageManageIncidentForm.newIncidentAdd_click()" 
                                                />
                                            <button 
                                                class="btn btn-default modal-inWaiting-footer" 
                                                id="newIncidentCancel" 
                                                data-dismiss="modal" 
                                                aria-hidden="true"
                                                type="button"
                                                >
                                                <asp:Literal runat="server" ID="btnNewIncidentCancel" Text="<%$Resources:Buttons,Cancel %>" />
                                            </button>
                                        </div>
                                    </div>
                                </div>
                                <tstsc:AjaxFormManager ID="ajxIncidentFormManager" runat="server" ArtifactTypeName="<%$Resources:Fields,TestRun %>" AutoLoad="true" CheckUnsaved="false"
                                        ErrorMessageControlId="divNewIncidentsMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" WorkflowEnabled="true">
                                    <ControlReferences>
                                        <tstsc:AjaxFormControl ControlId="txtIncidentName" DataField="Name" Direction="InOut" />
                                        <tstsc:AjaxFormControl ControlId="ddlOpener" DataField="OpenerId" Direction="InOut" />
                                        <tstsc:AjaxFormControl ControlId="ddlIncidentType" DataField="IncidentTypeId" Direction="InOut" ChangesWorkflow="true" />
                                        <tstsc:AjaxFormControl ControlId="ddlPriority" DataField="PriorityId" Direction="InOut" />
                                        <tstsc:AjaxFormControl ControlId="ddlSeverity" DataField="SeverityId" Direction="InOut" />
                                        <tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
                                        <tstsc:AjaxFormControl ControlId="ddlResolvedRelease" DataField="ResolvedReleaseId" Direction="InOut" />
                                        <tstsc:AjaxFormControl ControlId="ddlVerifiedRelease" DataField="VerifiedReleaseId" Direction="InOut" />
                                        <tstsc:AjaxFormControl ControlId="ddlComponent" DataField="ComponentIds" Direction="InOut" />
                                        <tstsc:AjaxFormControl ControlId="hdnIncidentStatus" DataField="IncidentStatusId" Direction="In" IsWorkflowStep="true" />
                                    </ControlReferences>
                                </tstsc:AjaxFormManager>      
                            </asp:Panel>





                            <asp:Panel ID="pnlNoTestStepsFound" runat="server" Style="display:none">
                                <div class="ma4">
                                    <h2>
                                        <asp:Localize runat="server" ID="msgNoStepsAvailableForTestCase" Text="<%$Resources:Main,TestCaseExecution_NoStepsAvailableForTestCase %>" />
                                    </h2>
                                    <p>
                                        <asp:Localize  runat="server" ID="msgTestCaseExecution_ChooseTestStep1" Text="<%$Resources:Main,TestCaseExecution_ChooseTestStep1 %>" />
                                        <tstsc:LinkButtonEx ID="btnNoStepsBackToTests" runat="server" CausesValidation="false" Text="<%$Resources:Main,TestCaseExecution_ChooseTestStep2 %>" />
                                        <asp:Localize runat="server" ID="msgTestCaseExecution_ChooseTestStep2" Text="<%$Resources:Main,TestCaseExecution_ChooseTestStep3 %>" />
                                    </p>
                                </div>
                            </asp:Panel>
                        </div>






                         <%-- TASKS PANEL --%>
                        <div 
                            role="tabpanel" 
                            class="tab-pane" 
                            id="te-inspector_tab_tasks" 
                            data-bind="if: pageProps.canViewTasks"
                            >

                            <%-- PANEL FOR ADDING NEW TASK --%>
                            <asp:Panel runat="server" ID="pnlTestExecution_Tasks" class="modal-inWaiting-modal">
                                <div class="modal-inWaiting-dialog">
                                    <div class="modal-inWaiting-content"> 
                                        
                                        <section class="u-wrapper width_md">
                                            <div class="u-box_3" >
                                                <div class="u-box_group" >
                                                    <div 
                                                        class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3"
                                                        aria-expanded="false">
                                                        <asp:Localize 
                                                            runat="server" 
                                                            Text="<%$Resources:Dialogs,TaskList_NewTask %>" />
                                                    </div>
                                                    <div class="u-box_3">
                                                        <ul class="u-box_list u-cke_is-minimal labels_absolute">
                                                            <li class="ma0 pa0 mb4 fs-h4">
                                                                <tstsc:UnityTextBoxEx
                                                                    CssClass="u-input is-active"
                                                                    data-bind="textInput: taskName"
                                                                    ID="txtTaskName"
                                                                    MaxLength="255"
                                                                    PlaceHolder="<%$ Resources:Fields,TestCase_Execution_EnterTaskName %>"
                                                                    runat="server"
                                                                    TextMode="SingleLine"
                                                                    />
                                                            </li>
                                                            <li class="ma0 pa0">
                                                    
                                                            </li>
                                                            <li class="ma0 pa0 mb2">
                                                                <textarea
                                                                    data-bind="richText: taskDescription, field: 'taskDescription', isTestRun: true"
                                                                    id="taskDescription"
                                                                    >
                                                                </textarea>
                                                                <label class="pl1 fw-b ov-hidden ws-nowrap" style="width: 100%;">
                                                                    <asp:Localize
                                                                        runat="server"
                                                                        ID="lblTaskDescription"
                                                                        Text="<%$Resources:Fields,Description%>"
                                                                        />
                                                                    <asp:HyperLink 
                                                                        class="light-gray transition-all fs-90 pointer ml2"
                                                                        onclick="pageManageTasks.setDescriptionFromTestStep();" 
                                                                        runat="server"
                                                                        Text="<%$Resources:Main,TestCaseExecution_SetTaskDescriptionFromTestStep%>"
                                                                        Title="<%$Resources:Main,TestCaseExecution_SetTaskDescriptionFromTestStep%>"
                                                                        />
                                                                </label>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                    <div class="u-box_1">
                                                        <ul class="u-box_list">
                                                            <li class="mx0 pa0 mtn4 mb2">
                                                                <tstsc:LabelEx
                                                                    AppendColon="true"
                                                                    AssociatedControlID="ddlTaskOwner"
                                                                    ID="lblTaskOwner"
                                                                    runat="server"
                                                                    Text="<%$Resources:Fields,OwnerId %>"
                                                                    />
                                                                <tstsc:UnityDropDownUserList
                                                                    CssClass="u-dropdown u-dropdown_user"
                                                                    ID="ddlTaskOwner"
                                                                    DataTextField="FullName"
                                                                    DataValueField="UserId"
                                                                    DisabledCssClass="u-dropdown u-dropdown_user disabled"
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>"
                                                                    NoValueItem="True"
                                                                    runat="server"
                                                                    />
                                                            </li>
                                                        </ul>
                                                    </div>




                                                    <!-- list of existing tasks  -->
                                                    <div class="u-box_3" data-bind="if: tasks().length > 0">
                                                        <ul class="pa0 df flex-wrap items-start" data-bind="foreach: tasks">
                                                            <li class="pr4 py3 ma3 ba b-light-gray br2 dib w8 dif">
                                                                <div 
                                                                    class="fl w3 myn3 mr4"
                                                                    data-bind="style: { backgroundColor: '#' + priorityColor }"
                                                                    ></div>
                                                                <a data-bind="attr: { href: url }" target="_blank">
                                                                    <span data-bind="text: taskToken"></span>&nbsp;
                                                                    <span data-bind="text: name"></span>
                                                                </a>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                            </div>
                                        </section>

                                        <div class="px4 py3 btn-group">
                                            <button 
                                                class="btn btn-primary"
                                                data-bind="enable: hasTaskName"
                                                id="btnNewTask"
                                                type="button"
                                                >
                                                <asp:Literal runat="server" Text="<%$Resources:Buttons,Add %>" />
                                            </button>
                                            <button 
                                                class="btn btn-default modal-inWaiting-footer" 
                                                id="newTaskCancel" 
                                                data-dismiss="modal" 
                                                aria-hidden="true"
                                                type="button"
                                                >
                                                <asp:Literal runat="server" ID="Literal2" Text="<%$Resources:Buttons,Cancel %>" />
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </asp:Panel>
                        </div>
                    </div>
                </div>
            </div>






            <div class="te-iframe">
                <div class="border pa3">
                    <div class="input-group my3 w-100">
                        <div class="input-group-btn">
                            <div class="btn btn-default" id="te-iframe-btn-back">
                                <span class="fas fa-arrow-left"></span>
                            </div>
                            <div class="btn btn-default" id="te-iframe-btn-forward">
                                <span class="fas fa-arrow-right"></span>
                            </div>
                        </div>
                        <input type="text" class="form-control" placeholder="url..." id="te-iframe-input-url" />
                        <div class="input-group-btn" id="te-iframe-btn-go">
                            <div class="btn btn-default">
                                <span class="fas fa-share"></span>
                            </div>
                        </div>
                    </div>
                    <iframe src="about:blank" name="te-iframe-iframe" id="te-iframe-iframe">
                    </iframe>
                </div>
            </div>
        </div>
    </div>





    <!-- Add New Incident Modal Popup for use in Table View -->
    <div 
        class="modal fade" 
        id="associateIncidentModal" 
        tabindex="-1" 
        role="dialog" 
        >
        <div class="modal-dialog mvw-100 mw1280" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" onclick="pageManageIncidentForm.closeIncidentAssociationModal()" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h4 class="modal-title" id="myModalLabel">
                            <asp:literal runat="server" Text="<%$Resources:Main,TestCaseExecution_LinkExisting %>" />
                        </h4>
                </div>
                <div class="modal-body">
                    <div id="associateIncidentInsideModal"></div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" onclick="pageManageIncidentForm.closeIncidentAssociationModal()">
                            <asp:literal runat="server" Text="<%$Resources:Buttons,Close %>" />
                    </button>
                </div>
            </div>
        </div>
    </div>





    <!-- initial wizard screen -->
    <div 
        class="w-100 pt6 mtn3 mbn4 ov-y-auto bg-near-white h-insideHeadAndFoot" 
        id="te-wizard-panel"
        >
        <section class="py4 px5 br3 ov-visible bg-white w-70 shadow-a-mid-gray gray mx-auto mt0 mb5">
            <h2>
                <tstsc:LabelEx 
                    ID="txtWizardTitle" 
                    runat="server" 
                    />
            </h2>
            <tstsc:MessageBox 
                id="lblFirstPageMessages" 
                Runat="server" 
                SkinID="MessageBox" 
                />
            <p 
                class="alert alert-danger alert-narrow" 
                id="no-testRunsFound-message"
                style="display: none" 
                >
                <span class="fas fa-exclamation-circle"></span>
                <asp:Localize 
                    ID="litAlertMessageNoTestRunsFound" 
                    runat="server" 
                    Text="<%$Resources:Messages,TestExecutionService_NoTestCasesWithStepsFound%>" 
                    />
            </p>
            <p class="mb0">
                <asp:Localize 
                    ID="lblChooseReleaseAndCustomProps" 
                    runat="server" 
                    Text="<%$Resources:Main,TestCaseExecution_ChooseReleaseAndCustomProps %>" 
                    />
            </p>
            <p class="mb4">
                <asp:Localize 
                    ID="lblCustomPropertiesPresetLegend" 
                    runat="server" 
                    Text="<%$Resources:Main,TestCaseExecution_CustomPropertiesPresetLegend %>" 
                    />
            </p>




            <!-- fields -->
            <div 
                class="u-wrapper width_md clearfix" 
                id="tblTestRunReleaseCustomProps" 
                enableviewstate="false"
                >
                <div class="u-box_1">
                    <ul class="u-box_list">
                        <li class="ma0 pa0">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ddlRelease" 
                                ID="ddlReleaseLabel" 
                                Required="true" 
                                runat="server" 
                                Text="<%$Resources:Fields,Release %>" 
                                />
                            <tstsc:UnityDropDownHierarchy 
                                AlternateItemField="IsIteration" 
                                AlternateItemImage="Images/artifact-Iteration.svg" 
                                AutoPostBack="false" 
                                ClientScriptMethod="pageWizard.ddlRelease_selectedItemChanged" 
                                DataTextField="FullName"
                                DataValueField="ReleaseId"
                                ID="ddlRelease" 
                                IndentLevelField="IndentLevel" 
                                ItemImage="Images/artifact-Release.svg" 
                                Runat="server" 
                                SummaryItemField="IsSummary" 
                                SummaryItemImage="Images/artifact-Release.svg"
                                />                                
                        </li>
                        <li 
                            class="ma0 pa0"
                            id="liBuildInfo"
                            runat="server"
                            >
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ddlBuild" 
                                ID="ddlBuildLabel" 
                                Required="false" 
                                runat="server" 
                                Text="<%$Resources:Fields,BuildId %>" 
                                />
                            <tstsc:UnityDropDownListEx 
                                CssClass="u-dropdown" 
                                DataTextField="Name" 
                                DataValueField="BuildId" 
                                DisabledCssClass="u-dropdown disabled" 
                                id="ddlBuild" 
                                NoValueItem="true"
                                NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                Runat="server" 
                                />
                        </li>
                    </ul>
                </div>


                <%-- CUSTOM FIELDS --%>
                <div class="u-box_1">
                    <ul 
                        class="u-box_list"
                        id="customFieldsTestRunDefault"
                        runat="server"
                        >
                    </ul>
                </div>
                <div class="u-box_3 u-cke_is-minimal">
                    <ul 
                        class="u-box_list labels_absolute"
                        id="customFieldsTestRunRichText"
                        runat="server"
                        >
                    </ul>
                </div>
                <div class="u-box_3 pl4">
                    <div class="btn-group">
                        <tstsc:ButtonEx 
                            Authorized_ArtifactType="TestRun" 
                            Authorized_Permission="Create"
                            ClientScriptMethod="ajxTestRunFormManager_saveData(event)" 
                            ID="btnSubmitRelease" 
                            runat="server" 
                            SkinID="ButtonPrimary" 
                            Text="<%$Resources:Buttons,Next %>" 
                            />
                        <tstsc:ButtonEx 
                            CausesValidation="false" 
                            ID="btnCancel" 
                            runat="server" 
                            Text="<%$Resources:Buttons,Cancel %>" 
                            />
                    </div>
                </div>
            </div>
            <tstsc:AjaxFormManager 
                ArtifactTypeName="<%$Resources:Fields,TestRun %>" 
                AutoLoad="true" 
                CheckUnsaved="false"
                ErrorMessageControlId="lblFirstPageMessages" 
                ID="ajxTestRunFormManager" 
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestExecutionService" 
                WorkflowEnabled="true"
                >
                <ControlReferences>
                    <tstsc:AjaxFormControl ControlId="txtWizardTitle" Direction="InOut" DataField="WizardTitle" PropertyName="textValue" />
                    <tstsc:AjaxFormControl ControlId="ddlRelease" Direction="InOut" DataField="ReleaseId" PropertyName="intValue" />
                    <tstsc:AjaxFormControl ControlId="ddlBuild" Direction="InOut" DataField="BuildId" PropertyName="intValue" />
                </ControlReferences>
            </tstsc:AjaxFormManager>
        </section>
    </div>

    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>  
            <asp:ServiceReference Path="~/Services/Ajax/TestExecutionService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />  
            <asp:ServiceReference Path="~/Services/Ajax/TestStepService.svc" />  
            <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/AssociationService.svc" /> 
            <asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/BuildService.svc" />
        </Services>  
        <Scripts>
            <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.hopscotch.min.js" Assembly="Web" />
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
        </Scripts>
    </tstsc:ScriptManagerProxyEx>
</asp:Content>




<asp:Content ContentPlaceHolderID="cplScripts" ID="teCplScripts" runat="server">
    <script type="text/javascript">
        SpiraContext.HasCollapsiblePanels = false;

        _pageInfo = {
            projectId: <%=ProjectId%>,
            userId: <%=UserId%>,
            testRunsPendingId: '<%=testRunsPendingId%>',
            clientId: '<%=lblMessage.ClientID%>',
            ddlRelease: '<%=ddlRelease.ClientID %>',
            ddlBuild: '<%=ddlBuild.ClientID %>',
            pbBtnLeave: '<%=pbBtnLeave.ClientID %>',
            btnSubmitRelease: '<%=btnSubmitRelease.ClientID %>',
            appBusy: false,
            pnlTestExecution_Attachments: '<%=pnlTestExecution_Attachments.ClientID%>',
            artifactTypeFilter_TestRun:'<%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.TestRun%>',
            artifactTypeFilter_TestCase:'<%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.TestCase%>',
            artifactTypeFilter_TestStep:'<%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.TestStep%>',
            txtIncidentName: '<%=txtIncidentName.ClientID%>',
            ajxIncidentFormManager: '<%=ajxIncidentFormManager.ClientID%>',
            ajxTestRunFormManager: '<%=ajxTestRunFormManager.ClientID%>',
            btnLinkExistingIncident: '<%=btnLinkExistingIncident.ClientID%>',
            divNewIncidentsMessage: '<%=divNewIncidentsMessage.ClientID%>',
            msgNewIncidentMessageInline : '<%=msgNewIncidentMessageInline.ClientID%>',
            ajxTestRunCaseFormManager: '<%=ajxTestRunCaseFormManager.ClientID%>',
            ajxTestRunStepFormManager: '<%=ajxTestRunStepFormManager.ClientID%>',
            addIncidentArea: '<%=pnlTestExecution_Incidents.ClientID%>',
            referrerUrl: '<%=ReferrerUrl%>',
            fullReferrerUrl: '<%=FullReferrerUrl%>',
            incidentBaseUrl: '<%=IncidentBaseUrl%>',
            get_incidentBaseUrl: function() {
                return _pageInfo.incidentBaseUrl;
            },
            screenshotUploadUrl: '<%=ScreenshotUploadUrl%>',
            ddlTaskOwner: '<%=ddlTaskOwner.ClientID%>',
            actualResultAlwaysRequired: '<%=ActualResultAlwaysRequired%>' == "true",
            requireIncident: '<%=RequireIncident%>' == "true",
            addTaskArea: '<%=pnlTestExecution_Tasks.ClientID%>',
            allowTasks: '<%=AllowTasks%>' == "true"
        };

        //fx to save the data, and load the main page. Coded here as opposed to through the btn control
        //this allows us to disable the button given that this save operation can take a while and is a necessary gateway to the main page 
        //disabling means users won't double click, and get visual feedback on what is going on
        ajxTestRunFormManager_saveData=function(event) {
            var saveBtn = document.getElementById("<%=btnSubmitRelease.ClientID%>");
            saveBtn.disabled=true; 
            saveBtn.value=resx.GlobalFunctions_SpinnerText;
            $find(_pageInfo.ajxTestRunFormManager).save_data(event, null, true)
        }

        //this function deals with [IN:4666] where users with bad formatted and imported data see html and body tags in their test execution
        //previously we just applied filterXSS to the string in question, so now, before that, we remove html tags that bother customers
        cleanAndFilterXss = function (string) {
            // only get rid of the tags if they are at the very start and very end of the string
            var cleanedString = string;
            var regexStart = /^\<html\>[^a-zA-Z0-9]*\<body\>/;
            var regexEnd = /\<\/body\>[^a-zA-Z0-9]*\<\/html\>$/;
            cleanedString = cleanedString.replace(regexStart, "").replace(regexEnd, "");
            return filterXSS(cleanedString, filterXssInlineStyleOptions);
        }

    </script>
    <script src='<%= Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl("~/TestCaseExecution.aspx.js") + "?v=" + GlobalFunctions.SYSTEM_VERSION_NUMBER %>' type="text/javascript"></script>
</asp:Content>
