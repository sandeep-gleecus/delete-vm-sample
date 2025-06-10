/* global _pageInfo */
/* global CKEDITOR */
/* global hideddrivetip */
/* global ddrivetip */
/* global Inflectra */
/* global globalFunctions */
/* global ko */
/* global $ */
//Variables constants used to control display
var resx = Inflectra.SpiraTest.Web.GlobalResources;
Type.registerNamespace('Inflectra.SpiraTest.Web');
var pageProps = {
    loadedOK: false,
    associationWithoutPanel: true,
    isTestExecution: true,
    oneRem: 24,
    widthClass: {
        narrow: 'te-col-narrow',
        wide: 'te-col-wide',
        full: 'te-col-full',
        hide: 'te-col-hide'
    },
    heightClass: {
        min: 'te-grid-height-min',
        max: 'te-grid-height-max'
    },
    glyphs: {
        play: 'fas fa-play',
        pause: 'fas fa-pause',
        leave: 'fas fa-eject',
        end: 'fas fa-stop'
    },
    partClass: {
        grid: 'te-grid',
        inspector: 'te-inspector',
        iframe: 'te-iframe'
    },
    displayMode: {
        split: 1,
        grid: 2,
        mini: 3,
        modeInUse: 0,
        subModeInUse: 1,
        splitName: 'split',
        gridName: 'grid',
        miniName: 'mini'
    },
    toolbarDisplaySecondary: {
        $btnGrp: {
            split: $('#btn-group-splitView'),
            grid: $('#btn-group-gridView'),
            mini: $('#btn-group-miniView')
        }
    },
    progressBar: {
        $bar: $('.te-progress-bar'),
        $playPause: $('#pb-btn-play-pause')
    },
    actualResultId: '',
    taskDescriptionId: 'taskDescription',
    statusEnum: {
        fail: 1,
        passed: 2,
        notRun: 3,
        notApplicable: 4,
        blocked: 5,
        caution: 6
        
    },
    temp: {
        statusId: '',
        isPassAll: '',
        shouldMoveForward: '',
        shouldLeave: '',
        
    },
    timeManagement: {
        start: '',
        end: '',
        duration: '',
        isPaused: false,
        skipTimeUpdate: false
    },
    previousChosen: {
        testRunId: '',
        testCaseId: ''
    },
    screenshotUploadUrl: _pageInfo.screenshotUploadUrl,
    canViewIncidents: globalFunctions.isAuthorized(globalFunctions.permissionEnum.View, globalFunctions.artifactTypeEnum.incident) == globalFunctions.authorizationStateEnum.authorized,
    ajxService: Inflectra.SpiraTest.Web.Services.Ajax.TestExecutionService,
    canViewTasks: _pageInfo.allowTasks
        && globalFunctions.isAuthorized(globalFunctions.permissionEnum.Create, globalFunctions.artifactTypeEnum.task) == globalFunctions.authorizationStateEnum.authorized
        && globalFunctions.isAuthorized(globalFunctions.permissionEnum.View, globalFunctions.artifactTypeEnum.task) == globalFunctions.authorizationStateEnum.authorized,
    data: {}
};
var pageWizard = {
    ajxTestRunFormManager_loaded: function() {
    //If we have a release selected, need to load the builds
        var ddlRelease = $find(_pageInfo.ddlRelease);
        if (ddlRelease && ddlRelease.get_selectedItem().get_value() != '') {
            var releaseId = parseInt(ddlRelease.get_selectedItem().get_value());
            pageWizard.updateBuildList(releaseId);
        }
    },
    ajxTestRunFormManager_dataSaved: function (operation, releaseVersionNumber) {
        $(_pageInfo.btnSubmitRelease).prop("disabled", "disabled");
        //Retrieve the information to populate the main page
        pageProps.ajxService.TestExecution_RetrieveTestRunsPending(
            _pageInfo.projectId,
            _pageInfo.testRunsPendingId,
            pageCallbacks.TestExecution_RetrieveTestRunsPending_Success,
            pageCallbacks.TestExecution_RetrieveTestRunsPending_Failure
            );
    },
    ajxTestRunFormManager_dataFailure: function (exception) {
        //Reset the next button
        var saveBtn = document.getElementById(_pageInfo.btnSubmitRelease);
        saveBtn.disabled = false;
        saveBtn.value = resx.Guide_Global_Next;
    },
    ddlRelease_selectedItemChanged: function (item) {
        //Get the new releaseId
        if (item.get_value() != '') {
            var releaseId = item.get_value();
            pageWizard.updateBuildList(releaseId);
        }
    },
    updateBuildList: function(releaseId) {
        //Get the list of builds for this release from the web service
        globalFunctions.display_spinner();
        Inflectra.SpiraTest.Web.Services.Ajax.BuildService.GetBuildsForRelease(_pageInfo.projectId, releaseId, pageWizard.updateBuildList_success, pageWizard.updateBuildList_failure);
    },
    updateBuildList_success: function(data) {
        //Clear values and databind
        var ddlBuild = $find(_pageInfo.ddlBuild);
        if (ddlBuild) {
            ddlBuild.clearItems();
            ddlBuild.addItem('', '-- ' + resx.Global_None + ' --');
            if (data) {
                ddlBuild.set_dataSource(data);
                ddlBuild.dataBind();
                var ajxTestRunFormManager = $find(_pageInfo.ajxTestRunFormManager);
                if (ajxTestRunFormManager && ajxTestRunFormManager.get_dataItem()) {
                    var dataItem = ajxTestRunFormManager.get_dataItem();
                    if (dataItem.Fields.BuildId) {
                        var buildId = dataItem.Fields.BuildId.intValue;
                        if (buildId) {
                            ddlBuild.set_selectedItem(buildId);
                        }
                        else {
                            ddlBuild.set_selectedItem('');
                        }
                    }
                    else {
                        ddlBuild.set_selectedItem('');
                    }
                }
                else {
                    ddlBuild.set_selectedItem('');
                }
            }
        }
        //Hide spinner
        globalFunctions.hide_spinner();
    },
    updateBuildList_failurefunction: function(error) {
        var ddlBuild = $find(_pageInfo.ddlBuild);
        //build can be disabled
        if (ddlBuild) {
            ddlBuild.clearItems();
            ddlBuild.addItem('', '-- ' + resx.Global_None + ' --');
            ddlBuild.set_selectedItem('');
        }
        //Hide spinner, ignore error, just clear dropdown values
        globalFunctions.hide_spinner();
    }
};
var pageControls = {
    //Utility functions to control display based on different view modes
    showHideBtnGroups: function (toShow) {
        pageProps.toolbarDisplaySecondary.$btnGrp.split.hide();
        pageProps.toolbarDisplaySecondary.$btnGrp.grid.hide();
        pageProps.toolbarDisplaySecondary.$btnGrp.mini.hide();
        pageProps.toolbarDisplaySecondary.$btnGrp[toShow].show();
    },
    displayClass: function (cls) {
        var x = $('.' + cls).attr('class');
        return x.replace(cls, '');
    },
    changeClass: function (a, b, c, isMiniRichText) {
        $('.' + pageProps.partClass.grid).removeClass(pageControls.displayClass(pageProps.partClass.grid));
        $('.' + pageProps.partClass.grid).addClass(a);
        $('.' + pageProps.partClass.inspector).removeClass(pageControls.displayClass(pageProps.partClass.inspector));
        $('.' + pageProps.partClass.inspector).removeClass(isMiniRichText ? '' : 'rich-text-mini');
        $('.' + pageProps.partClass.inspector).addClass(b);
        $('.' + pageProps.partClass.inspector).addClass(isMiniRichText ? 'rich-text-mini' : '');
        $('.' + pageProps.partClass.iframe).removeClass(pageControls.displayClass(pageProps.partClass.iframe));
        $('.' + pageProps.partClass.iframe).addClass(c);
    },
    //To set the display mode on page first load
    setInitialDisplayMode: function (callback) {
        //Enter correct display mode (split is default)
        if ($(window).width() < 501 && $(window).width() > 0) {
            $('#lbl-display-tb_radio-splitView').trigger('click');
        //Handle errors if no model has been created
        
        } else if (teViewModel.vm.userSettingsDisplayModeInUse() == pageProps.displayMode.mini) {
            $('#lbl-display-tb_radio-miniView').trigger('click');
        } else if (teViewModel.vm.userSettingsDisplayModeInUse() == pageProps.displayMode.grid) {
            $('#lbl-display-tb_radio-gridView').trigger('click');
        } else {
            teViewModel.vm.userSettingsDisplayModeInUse(pageProps.displayMode.split);
            $('#lbl-display-tb_radio-splitView').trigger('click');
        }
        callback && callback();
    },
    //Functions to shift between specific display modes
    displaySplit: function () {
        //destroy the editor if coming from grid mode (it has a different editor instance)
        if (pageProps.displayMode.grid === pageProps.displayMode.modeInUse) {
            pageProps.loadedOK ? pageCKE.destroy() : void (null);

            // and hide all tabs on the inspector to make sure there dialogs will be visible here
            // Two lines of jquery are used to make sure the code works in IE11
            $('#te-inspector_tab_attachments').css('display', '');
            $('#te-inspector_tab_incidents').css('display', '');
        }
        //do nothing if already in that mode
        if (pageProps.displayMode.split != pageProps.displayMode.modeInUse) {
            pageProps.displayMode.modeInUse = pageProps.displayMode.split;
            teViewModel.vm.userSettingsDisplayModeInUse(pageProps.displayMode.split);
            pageControls.showHideBtnGroups(pageProps.displayMode.splitName);

            // Make sure states are reset in case coming from table view
            pageControls.turnOffModalInWaiting();
            // Two lines of jquery are used to make sure the code works in IE11
            $('#te-inspector_tab_attachments').css('display', '');
            $('#te-inspector_tab_incidents').css('display', '');

            // Set the correct sub mode
            if (teViewModel.vm.userSettingsDisplaySubModeInUse() == 2) {
                $('#lbl-btn-group-splitView_gridLarge').trigger('click');
            } else {
                $('#lbl-btn-group-splitView_inspectorLarge').trigger('click');
            }
        }
    },
    displayGrid: function () {
        //do nothing if already in that mode
        if (pageProps.displayMode.grid != pageProps.displayMode.modeInUse) {
            // first destroy any existing CK Editor instance
            pageProps.loadedOK ? pageCKE.destroy() : void(null);
            
            pageProps.displayMode.modeInUse = pageProps.displayMode.grid;
            teViewModel.vm.userSettingsDisplayModeInUse(pageProps.displayMode.grid);
            pageControls.showHideBtnGroups(pageProps.displayMode.gridName);
            pageControls.changeClass(pageProps.widthClass.full, pageProps.widthClass.hide, pageProps.widthClass.hide);
            pageControls.turnOnModalInWaiting();
            $('#btn-group-gridView_expand').trigger('click');

            // Unhide all tabs on the inspector to make sure there dialogs will be visible here
            // Two lines of jquery are used to make sure the code works in IE11
            $('#te-inspector_tab_attachments').css('display', 'block');
            $('#te-inspector_tab_incidents').css('display', 'block');
        }
    },
    displayMini: function () {
        //destroy the editor if coming from grid mode (it has a different editor instance)
        if (pageProps.displayMode.grid === pageProps.displayMode.modeInUse) {
            pageProps.loadedOK ? pageCKE.destroy() : void (null);

            // and hide all tabs on the inspector to make sure there dialogs will be visible here
            // Two lines of jquery are used to make sure the code works in IE11
            $('#te-inspector_tab_attachments').css('display', '');
            $('#te-inspector_tab_incidents').css('display', '');
        }
        //do nothing if already in that mode
        if (pageProps.displayMode.mini != pageProps.displayMode.modeInUse) {
            pageProps.displayMode.modeInUse = pageProps.displayMode.mini;
            teViewModel.vm.userSettingsDisplayModeInUse(pageProps.displayMode.mini);
            pageControls.showHideBtnGroups(pageProps.displayMode.miniName);
            pageControls.turnOffModalInWaiting();
            if (teViewModel.vm.userSettingsDisplaySubModeInUse() === 2) {
                $('#lbl-btn-group-miniView_iframe').trigger('click');
            } else {
                $('#lbl-btn-group-miniView_full').trigger('click');
            }
        }
    },
    displayGridSizing: function (a, b) {
        $('.te-grid.te-col-full .field-div .text').addClass(a);
        $('.te-grid.te-col-full .field-div .text').removeClass(b);
        setTimeout(function() {
            pageControls.hideShowPerFieldHeightButtons();
        }, 300);
    },
    displaySplitInspectorLarge: function () {
        pageControls.changeClass(pageProps.widthClass.narrow, pageProps.widthClass.wide, pageProps.widthClass.hide);
        pageControls.checkInspectorWidth();
        teViewModel.vm.userSettingsDisplaySubModeInUse(1);
        pageCallbacks.logDisplayOptions(true);
    },
    displaySplitGridLarge: function () {
        pageControls.changeClass(pageProps.widthClass.wide, pageProps.widthClass.narrow, pageProps.widthClass.hide);
        pageControls.checkInspectorWidth();
        teViewModel.vm.userSettingsDisplaySubModeInUse(2);
        pageCallbacks.logDisplayOptions(true);
    },
    displayGridExpand: function () {
        pageControls.displayGridSizing(pageProps.heightClass.max, pageProps.heightClass.min);
        teViewModel.vm.userSettingsDisplaySubModeInUse(1);
        pageCallbacks.logDisplayOptions(true);
    },
    displayGridCollapse: function () {
        pageControls.displayGridSizing(pageProps.heightClass.min, pageProps.heightClass.max);
        teViewModel.vm.userSettingsDisplaySubModeInUse(2);
        pageCallbacks.logDisplayOptions(true);
    },
    displayMiniFull: function () {
        pageControls.changeClass(pageProps.widthClass.hide, pageProps.widthClass.full, pageProps.widthClass.hide);
        pageControls.checkInspectorWidth();
        teViewModel.vm.userSettingsDisplaySubModeInUse(1);
        pageCallbacks.logDisplayOptions(true);
    },
    displayMiniIframe: function () {
        pageControls.changeClass(pageProps.widthClass.hide, pageProps.widthClass.narrow, pageProps.widthClass.wide, true);
        pageControls.checkInspectorWidth();
        teViewModel.vm.userSettingsDisplaySubModeInUse(2);
        pageCallbacks.logDisplayOptions(true);
    },
    //Adjusts the width of label and field elements inside the inspector as it dynamically changes its width
    //Special account is taken of dataLabels/dataEntries from the incident form
    checkInspectorWidth: function () {
        $('.' + pageProps.partClass.inspector).on('transitionend webkitTransitionEnd oTransitionEnd otransitionend MSTransitionEnd',
            function (event) {
                if (event.target !== this) {
                    return;
                }
                // for unity wrappers with boxes
                uWrapper_onResize();
            }
        );
    },

    //Functions for the progress bar
    //toggle which button is shown, 
    playPauseChange: function () {
        $('#pb-btn-play-pause > *').toggleClass(pageProps.glyphs.pause);
        $('#pb-btn-play-pause > *').toggleClass(pageProps.glyphs.play);
        pageControls.playPauseCheck();
        
    },
    //checks to make sure the right tooltips are applied, and that timing functions are being handled properly
    playPauseCheck: function () {
        if($('#pb-btn-play-pause > *').hasClass(pageProps.glyphs.play)) {
            $('#pb-btn-play-pause').addClass('is-glowing');
            $('#pb-btn-play-pause').prop('title', resx.TestCaseExecution_Resume);
            pageProps.timeManagement.isPaused = true;
            pageTimeManagement.setLocalEnd(function() {
                pageTimeManagement.setLocalDuration(function() {
                    pageTimeManagement.setActualEndAndDuration()
                });
            });
        } else {
            pageProps.timeManagement.isPaused = false;
            pageTimeManagement.setLocalStart(function() {
                pageTimeManagement.setActualStart()
            });
            $('#pb-btn-play-pause').removeClass('is-glowing');
            $('#pb-btn-play-pause').prop('title', resx.TestCaseExecution_Pause);
        }
    },
    //Redirects back to the referring page when clicked (and after time functions successfully saved)
    leave: function() {
        pageProps.temp.shouldLeave = true;
        pageCallbacks.logChosenStepTiming();
    },
    
    ///Functions for the grid view
    //Managing the grid height
    setGridHeightOnSplit: function () {
        $('.' + pageProps.partClass.grid + '.' + pageProps.widthClass.narrow + ' .border').css('max-height', 'calc(100vh - 20px - ' + parseInt($('.' + pageProps.partClass.grid).offset().top) + 'px');
        $('.' + pageProps.partClass.grid + '.' + pageProps.widthClass.wide + ' .border').css('max-height', 'calc(100vh - 20px - ' + parseInt($('.' + pageProps.partClass.grid).offset().top) + 'px');
    },
    //Setting scrolling on the grid (only if not in mobile phone view)
    scrollGridToCurrentStep: function (scrollToRun) {
        if ($(window).width() > 501) {
            //scroll to the case if cases are shown in the progress bar, steps if steps are being displayed
            var scrollTarget = scrollToRun ? ' .te-grid-row-tc.selected' : ' .te-grid-row-ts.selected',
                scrollTo = $('.' + pageProps.partClass.grid + scrollTarget),
                container;
            if (teViewModel.vm.userSettingsDisplayModeInUse() === pageProps.displayMode.grid) {
                container = $('html, body');
                container.animate({
                    scrollTop: scrollTo.offset().top - $('.te-progress-bar').height() - 130
                }, 200);
            } else {
                container = $('.' + pageProps.partClass.grid + ' .border');
                container.animate({
                    scrollTop: scrollTo.offset().top - container.offset().top + container.scrollTop()
                }, 200);
            }
        }
    },
    //Manage the grid field heights on resize and set display to ensure responsiveness
    onWindowResize: function() {
        if ($(window).width() === 0 || !teViewModel.vm) {
            return
        } else {
            if ($(window).width() < 501) {
                $('#lbl-display-tb_radio-splitView').trigger('click');
            } else if (teViewModel.vm.userSettingsDisplayModeInUse() == pageProps.displayMode.grid) {
                pageControls.hideShowPerFieldHeightButtons();
            }
        }
    },
    //Hide the grid view button in expanded view if cell only has one line of text
    hideShowPerFieldHeightButtons: function () {
        $(".te-grid .field-div .text." + pageProps.heightClass.max).each(function () {
            if ($(this).height() <= pageProps.oneRem) {
                $(this).closest('.field-div').find('.te-grid-height-toggle').hide();
            } else {
                $(this).closest('.field-div').find('.te-grid-height-toggle').show();
            }
        });
    },

    //group function to manage clicking on the tab controls in the inspector to show the incident form
    handleIncidentTabClick: function() {
        pageCallbacks.getChosenIncidents();
        setTimeout(function () { uWrapper_onResize(); }, 0);
        
    },

    //functions to switch the incident form from inline in inspector view to bootstrap modal in the grid view 
    turnOnModalInWaiting: function() {
       $('.modal-inWaiting-modal').addClass('modal fade');
       $('.modal-inWaiting-dialog').addClass('modal-dialog mw1280 w1280');
       $('.modal-inWaiting-content').addClass('modal-content');
       $('.modal-inWaiting-modal').attr('tabindex', '-1');
       $('.modal-inWaiting-modal').attr('role', 'dialog');
       $('.modal-inWaiting-modal').attr('aria-hidden', 'true');
       $('.modal-inWaiting-footer').show();
    },
    turnOffModalInWaiting: function() {
       $('.modal-inWaiting-modal').removeClass('modal fade');
       $('.modal-inWaiting-dialog').removeClass('modal-dialog mw1280 w1280');
       $('.modal-inWaiting-content').removeClass('modal-content');
       $('.modal-inWaiting-modal').css('display', '');
       $('.modal-inWaiting-modal').attr('tabindex', '');
       $('.modal-inWaiting-modal').attr('role', '');
       $('.modal-inWaiting-modal').attr('aria-hidden', '');
       $('.modal-inWaiting-footer').hide();
    },
    
    //JS to manage IE9 and IE10 specific quirks
    handleIe: function() {
        var isIe8 = browserCapabilities.isIE8 || false,
            isIe9 = browserCapabilities.isIE9 || false,
            isIe10 = browserCapabilities.isIE10 || false,
            ieIsOld = isIe8 || isIe9 || isIe10;
        
        if(ieIsOld) {
            //Set the width of each progress bar item to make it responsive
            var count = $('.te-progress-item').length,
                width = 100 / count;
            $('.te-progress-item').css('width', width + '%');
        }
    },
    // JS for mobile touch events
    resetBtnAfterTouch: function() {
        var el = this;
        var par = el.parentNode;
        var next = el.nextSibling;
        par.removeChild(el);
        setTimeout(function() {par.insertBefore(el, next);}, 0)
    }
};
var teViewModel = { };

//Functions that are called by knockout viewmodel
var pageModelUtilities = {
    addSubArray: function (receivingArray, sendingArray, childViewModel, parentId, passedOnField) {
        if(sendingArray && sendingArray.length > 0) {
            for (var i = 0, l = sendingArray.length; i < l; i++) {
                sendingArray[i].ParentId = parentId;
                sendingArray[i].PassedOnField = passedOnField;  
                receivingArray.push(new childViewModel(sendingArray[i]));
            }
        }
    },
    arrayLengthsMatch: function(arrayOriginal, arrayNew) {
        return arrayOriginal.length === arrayNew.length;
    },
    //for cycling through a sub array to check length - assumes the sub array is an observable
    allChildrenArraysLengthsMatch: function(arrayOriginal, arrayNew, childArrayOriginal, childArrayNew, childArrayOriginalNotObservable) {
        var i = 0,
            l = arrayNew.length,
            allLengthsMatch = true;
        for (i; i < l; i++) {
            var arrayMatch;
            if (childArrayOriginalNotObservable) {
                arrayMatch = pageModelUtilities.arrayLengthsMatch(arrayOriginal[i][childArrayOriginal], arrayNew[i][childArrayNew]);
            } else {
                arrayMatch = pageModelUtilities.arrayLengthsMatch(arrayOriginal[i][childArrayOriginal](), arrayNew[i][childArrayNew]);
            }
            if(!arrayMatch) {
                allLengthsMatch = false;
                return;
            }
        }
        return allLengthsMatch;
    },
    updateModelArray: function(receivingArray, sendingArray, receivingId, sendingId, receivingField, sendingField, receivingFieldNotObservable) {
        var i = 0,
            recIndex = 0,
            lSend = sendingArray.length,
            lRec = receivingArray.length;
        if (sendingArray && lSend > 0) {
            //First look for a match by ID (assumed to not be observable)
            for (i; i < lSend; i++) {
                if (receivingArray[i][receivingId] === sendingArray[i][sendingId]) {
                    recIndex = i;
                //If the arrays are in a different order run a full check
                } else {
                    for (var j = 0; j < lRec; j++) {
                        //IDs are assumed to always be non-observable in the knockout model
                        if (receivingArray[j][receivingId] === sendingArray[i][sendingId]) {
                            recIndex = j;
                        } else {
                            //Fail quietly
                        }
                    } 
                }
                //perform actual update to the model - default is that the field is an observable
                if(sendingArray[i][sendingField]) {
                    if (receivingFieldNotObservable) {
                        receivingArray[recIndex][receivingField] = sendingArray[i][sendingField];
                    } else {
                        receivingArray[recIndex][receivingField](sendingArray[i][sendingField]);
                    }
                }
            }
        }
    },
    countInArray: function(array, field, search, isNotObservable) {
        var result = 0;
        ko.utils.arrayForEach(array, function (item) {
            if(isNotObservable) {
                if (item[field] === search) {
                    result +=1;   
                }
            } else {
                if (item[field]() === search) {
                    result +=1;
                } 
            }
        });
        return result;
    },
    arrayIndexOfByField: function(array, field, fieldInArray) {
        for (var i = 0, j = array.length; i < j; i++) {
            if(array[i][fieldInArray] === field) {
                return i;
            }
        }
        return -1;
    },
    sumInArray: function(array, field, isNotObservable) {
        var result = 0;
        ko.utils.arrayForEach(array, function (item) {
            result = isNotObservable ? result += item[field] : result += item[field]();
        });
        return result;
    },
    executionStatusProps: function (input) {
        var glyph, status, message, name;
        switch (input) {
            case pageProps.statusEnum.fail: 
                glyph = 'fas fa-times';
                status = 'ExecutionStatusFailed';
                message = resx.TestCaseExecution_ActualResultNeeded;
                name = resx.TestCaseExecution_Fail;
                break;
            case pageProps.statusEnum.passed: 
                glyph = 'fas fa-check';
                status = 'ExecutionStatusPassed';
                message = resx.TestCaseExecution_ActualResultNeeded;
                name = resx.TestCaseExecution_Pass;
                break;
            case pageProps.statusEnum.notRun: 
                glyph = '';
                status = '';
                break;
            case pageProps.statusEnum.notApplicable:
                glyph = 'fas fa-minus';
                status = 'ExecutionStatusNotApplicable';
                message = resx.TestCaseExecution_ActualResultNeeded;
                name = resx.TestCaseExecution_NotApplicable;
                break;
            case pageProps.statusEnum.blocked:
                glyph = 'fas fa-ban';
                status = 'ExecutionStatusBlocked';
                message = resx.TestCaseExecution_ActualResultNeeded;
                name = resx.TestCaseExecution_Blocked;
                break;
            case pageProps.statusEnum.caution: 
                glyph = 'fas fa-exclamation-triangle';
                status = 'ExecutionStatusCaution';
                message = resx.TestCaseExecution_ActualResultNeeded;
                name = resx.TestCaseExecution_Caution;
                break;
            default: 
                glyph = '';
                status = '';
                message = '';
                name = '';
        }
        return {"glyph": glyph, "status": status, "message": message, "name": name};
    }
};
var pageCKE = {
    destroy: function(callback) {
        var editor = CKEDITOR.instances[pageProps.actualResultId];  
        try { 
            if (editor) {
                CKEDITOR.remove(editor);
                editor.removeAllListeners();
                if(teViewModel.vm.userSettingsDisplayModeInUse() == pageProps.displayMode.grid) {
                    setTimeout(pageControls.hideShowPerFieldHeightButtons(), 300);
                }
            };
        } catch (e) {
            //fail quietly
        }
        callback && callback();
    },
    updateScreenshotUrl: function () {
        //First set the custom url 
        var initialUrl = _pageInfo.screenshotUploadUrl,
            artId = teViewModel.vm.chosenTestStepParent().testRunId,
            artifactTypeId = _pageInfo.artifactTypeFilter_TestRun;
        pageProps.screenshotUploadUrl = initialUrl.replace("{art}", artId).replace("{artifactType}", artifactTypeId);
    },
    options: {
        toolbar: [{
                name: 'styles',
                items: ['Format', 'Font', 'FontSize']
            }, {
                name: 'basicstyles',
                items: ['Bold', 'Italic', 'Underline', '-', 'RemoveFormat']
            }, {
                name: 'colors',
                items: ['TextColor', 'BGColor']
            }, {
                name: 'paragraph',
                items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight']
            }, {
                name: 'insert',
                items: ['Link', 'Unlink', '-', 'Image', 'CodeSnippet', 'Table', 'HorizontalRule', '-', 'PasteFromWord', '-', 'CreateToken']
            }, {
                name: 'tools',
                items: ['Maximize', '-', 'Source', '-', 'Templates', 'UIColor', 'ShowBlocks']
            }
        ],
        height: '12em',
		disallowedContent: 'img{width,height}',
		disableNativeSpellChecker: false
    },
    setColorMode: function () {
        if (window && window.rct_comp_globalNav && window.rct_comp_globalNav.ckeditorSetColorScheme) {
            window.rct_comp_globalNav.ckeditorSetColorScheme(document.body.dataset.colorscheme);
        }
    }  
};
var pageModelSetChosen = {
    setInitialTestStepPosition: function () {
        var self = teViewModel.vm,
            testRunIndex = 0,
            testStepIndex = 0;
        
        //If we can turn user setting IDs into positions in the current array for navigation purposes
        if(self.userSettingsCurrentRunId() && self.userSettingsCurrentStepId()) {
            var testRunIndexOf = pageModelUtilities.arrayIndexOfByField(self.testRun(), self.userSettingsCurrentRunId(), 'testRunId'),
                testRunIndex = testRunIndexOf === -1 ? 0 : testRunIndexOf,
                testStepIndexOf = pageModelUtilities.arrayIndexOfByField(self.testRun()[testRunIndex].testRunStep(), self.userSettingsCurrentStepId(), 'testRunStepId'),
                testStepIndex = testStepIndexOf === -1 ? 0 : testStepIndexOf;
        }
        try {
            self.chosenTestStep(self.testRun()[testRunIndex].testRunStep()[testStepIndex]);
            self.chosenTestStepParent(self.testRun()[testRunIndex]);
            //set the attachment panel's test run id so attachments can be added from all views
            pageManageAttachments.linkAttachmentsToChosenStep();
            pageManageAttachments.getChosenAttachments();
        } catch(e) {};
        //Handle timing issues
        pageTimeManagement.setLocalStart(function(){
            pageTimeManagement.setActualStart()
        });
        pageManageCustomProperties.getTestCaseAndStep();

        //Update upload url for ckeditor
        pageCKE.updateScreenshotUrl();  
    },
    isMovingStepAllowed: function () {
        var result = true;
        pageModelStatusUpdate.syncViewModelAndEditorActualResult( function () {
            var self = teViewModel.vm,
                actualResultIsEmpty = self.chosenTestStep().actualResult() ? false : true,
                currentStatus = self.chosenTestStep().executionStatusId(),
                stepHasExecutionAttempt = currentStatus != 3,
                // actual result required if the project settings say so, or if the status is not pass
                actualResultRequired = stepHasExecutionAttempt && (_pageInfo.actualResultAlwaysRequired || currentStatus != 2);
            result = actualResultRequired && actualResultIsEmpty ? false : true;

            // if the project settings require an incident then check if we have one and update result as needed - only if the status is not pass
            if (stepHasExecutionAttempt && _pageInfo.requireIncident && currentStatus != 2) {
                var stepHasIncidents = self.chosenTestStepIncidents() && self.chosenTestStepIncidents().length > 0;
                if (!stepHasIncidents) {
                    result = false;
                }
            }
        });
        return result;
    },
    selectTest: function (input) {
        if (pageModelSetChosen.isMovingStepAllowed()) {
            var that = input || this,
                testIsAlreadySelected = that.testRunStepId === teViewModel.vm.chosenTestStep().testRunStepId;
            if (testIsAlreadySelected) {
                return;
            } else {
                pageModelSetChosen.manageChosenStepChange(that)
                pageModelMoveFocus.resetChosenStepSettingsOnMove();
            }
        } else {
            pageModelStatusUpdate.handleStatusUpdateAttempt(teViewModel.vm.chosenTestStep().executionStatusId());
        };
    },
    selectTestFromRun: function (input, callback) {
        if (pageModelSetChosen.isMovingStepAllowed()) {
            //selects the first step in the chosen run only if the user is allowed to move steps (ie does not need to enter actual result) 
            var that = input.testRunStep()[0] ||    this.testRunStep()[0];
            pageModelSetChosen.manageChosenStepChange(that);
            pageModelMoveFocus.resetChosenStepSettingsOnMove();
        } else {
            pageModelStatusUpdate.handleStatusUpdateAttempt(teViewModel.vm.chosenTestStep().executionStatusId());
        }
        if (typeof(callback) == "function") {
            callback && callback();
        }
            
    },
    selectTestFromProgressBar: function () {
        if (pageModelSetChosen.isMovingStepAllowed()) {
            var that = this;
            pageModelSetChosen.manageChosenStepChange(that);
            pageControls.scrollGridToCurrentStep(true);
            pageModelMoveFocus.resetChosenStepSettingsOnMove();
        } else {
            pageModelStatusUpdate.handleStatusUpdateAttempt(teViewModel.vm.chosenTestStep().executionStatusId());
        }
    },
    selectTestFromRunFromProgressBar: function () {
        if (pageModelSetChosen.isMovingStepAllowed()) {
            var that = this.testRunStep()[0];
            pageModelSetChosen.manageChosenStepChange(that);
            pageControls.scrollGridToCurrentStep(true);
            pageModelMoveFocus.resetChosenStepSettingsOnMove();
        } else {
            pageModelStatusUpdate.handleStatusUpdateAttempt(teViewModel.vm.chosenTestStep().executionStatusId());
        }
    },
    //The function that carries out the search and sets required observables
    setNewRunAndStep: function (that) {
        var self = teViewModel.vm;
        var search = that.testRunStepParentId,
            parent,
            runIndex,
            stepIndex,
            isFirst = false;
            //Simply set the new chosen step
        self.chosenTestStep(that);

        //Now find and select the correct parent test run (case) and populate the model with it
        if (!search) {
            return null;
        } else {
            parent = ko.utils.arrayFirst(self.testRun(), function (item) {
                return (item.testRunId == search);
            });
            runIndex = ko.utils.arrayIndexOf(self.testRun(), parent);
            stepIndex = ko.utils.arrayIndexOf(parent.testRunStep(), that);
            isFirst = stepIndex === 0;
        }
        self.chosenTestStepParent(parent);
        
        //Set the chosen run / step ids as the latest position
        self.userSettingsCurrentRunId(self.chosenTestStepParent().testRunId);
        self.userSettingsCurrentStepId(self.chosenTestStep().testRunStepId);
        
		//update ckeditor - try catch needed as of 6.5.2 for IE11 that fails at this stetp
		try {
			if (pageProps.actualResultId) {
				$('#' + pageProps.actualResultId).val(self.chosenTestStep().actualResult());
			}
        } catch (err) {
            // fail quietly
        }
		
        //set start times running for newly chosen step
        pageTimeManagement.setLocalStart(function() {
            pageTimeManagement.setActualStart()
        })
        
        //set the attachment panel's test run id so attachments can be added from all views
        pageManageAttachments.linkAttachmentsToChosenStep();

        //and then update the attachments panel to get the correct attachments for the step
        pageManageAttachments.getChosenAttachments();
        
        //Send the new position back to the server
        pageCallbacks.sendBackPosition();
        
        //Update custom properties as required
        pageManageCustomProperties.getTestCaseAndStep();

        //reset the associate incident panel
        pageManageIncidentForm.closeIncidentAssociationPanel();
        
        //Update upload url for ckeditor
        pageCKE.updateScreenshotUrl();
    },
    //Wrapper function to manage the different functions and callbacks
    manageChosenStepChange: function (that, callback) {
        //save current id information for later checks
        pageProps.previousChosen.testCaseId = teViewModel.vm.chosenTestStepParent().testCaseId;
        pageProps.previousChosen.testRunId = teViewModel.vm.chosenTestStepParent().testRunId;
        
        //deal with time management
        if(!pageProps.timeManagement.skipTimeUpdate) {
            if(!pageProps.timeManagement.isPaused) {
                pageTimeManagement.setAllEndTimes();
            }
        }
        pageModelStatusUpdate.syncViewModelAndEditorActualResult( function() { 
            if(teViewModel.vm.userSettingsDisplayModeInUse() === pageProps.displayMode.grid) {
                pageCKE.destroy();
            }
            pageModelSetChosen.setNewRunAndStep(that)
        });

        pageCKE.setColorMode();

        callback && callback();
    }
};
var pageTimeManagement = {
    
    setLocalStart: function(callback) {
        pageProps.timeManagement.start = Date.now();
        callback && callback();
    },
    setLocalEnd: function(callback) {
        pageProps.timeManagement.end = Date.now();
        callback && callback();
    },
    setLocalDuration: function(callback) {
        pageProps.timeManagement.duration = pageProps.timeManagement.end - pageProps.timeManagement.start;
        callback && callback();
    },
    setAllLocalTimes: function(callback) {
        pageTimeManagement.setLocalStart();
        pageTimeManagement.setLocalEnd(function () {
            pageTimeManagement.setLocalDuration()
        });
        callback && callback();
    },
    resetLocalTimes: function() {
        pageProps.timeManagement.start = '';
        pageProps.timeManagement.end = '';
        pageProps.timeManagement.duration = '';
    },
    convertToWcf: function(date) {
        if(date) {
            return "/Date(" + date + ")/";
        }
    },
    setActualStart: function() {
        var start = pageTimeManagement.convertToWcf(pageProps.timeManagement.start);
        if(teViewModel.vm.chosenTestStep().startDate()) {
            return;
        } else {
            teViewModel.vm.chosenTestStep().startDate(start); 
        } 
    },
    setActualEndAndDuration: function() {
        var end = pageTimeManagement.convertToWcf(pageProps.timeManagement.end),
            actualDurationInitial = teViewModel.vm.chosenTestStep().actualDuration() || 0,
            localDuration = Math.round(pageProps.timeManagement.duration / 60000);
        teViewModel.vm.chosenTestStep().endDate(end);
        teViewModel.vm.chosenTestStep().actualDuration(actualDurationInitial + localDuration);
    },
    setAllEndTimes: function(callback) {
        pageTimeManagement.setLocalEnd(function () {
            pageTimeManagement.setLocalDuration(function() {
                pageTimeManagement.setActualEndAndDuration();
            });
        });
        callback && callback();
    }
};
//Determines whether an attempted status update on a step can be carried out immediately or if further action is needed
var pageModelStatusUpdate = {
    handleStatusUpdateFromGrid: function(statusId) {
        pageModelSetChosen.selectTest(this);
        pageModelStatusUpdate.handleStatusUpdateAttempt(statusId, false);
    },
    handleStatusUpdateAttempt: function (statusId, moveForward) {
        pageProps.temp.statusId = statusId;
        pageProps.temp.isPassAll = false;
        pageProps.temp.shouldMoveForward = moveForward || false;

        //make sure the actual result has been synced to the model
        pageModelStatusUpdate.syncViewModelAndEditorActualResult( function () {
            //make sure an actual result is entered if one is required 
            var self = teViewModel.vm,
                actualResultIsEmpty = globalFunctions.trim(self.chosenTestStep().actualResult()) == '' ? true : false,
                currentStatus = pageProps.temp.statusId,
                stepHasExecutionAttempt = currentStatus != 3,
                // actual result required if the project settings say so, or if the status is not pass
                actualResultRequired = stepHasExecutionAttempt && _pageInfo.actualResultAlwaysRequired || currentStatus != 2;

            pageProps.temp.isPassAll = false;
            //don't move on if the actual result field is blank and either the step isn't passed or if the projectSetting always requires an actual result
            if (actualResultRequired && actualResultIsEmpty) {
                pageModelStatusUpdate.makeUserEnterActualResult();
            } else if (_pageInfo.requireIncident && currentStatus != 2) {
                // if the project settings require an incident (and the status is not pass) then check if we have one and update result as needed
                var stepHasIncidents = self.chosenTestStepIncidents() && self.chosenTestStepIncidents().length > 0;
                if (!stepHasIncidents) {
                    pageManageIncidentForm.makeUserAddIncident();
                } else {
                    pageCallbacks.handleStatusCallback();
                }
            } else {
                pageCallbacks.handleStatusCallback();
            }
        }); 
    },
    handlePassAllFromGrid: function(moveForward) {
        pageModelSetChosen.selectTestFromRun(this, function() {
            pageModelStatusUpdate.handlePassAll(moveForward);
        });
    },
    handlePassAll: function (moveForward) {
        pageProps.temp.statusId = pageProps.statusEnum.passed;
        pageProps.temp.isPassAll = true;
        pageProps.temp.shouldMoveForward = moveForward || false;
        
        //make sure the actual result has been synced to the model
        pageModelStatusUpdate.syncViewModelAndEditorActualResult(
            //make sure an actual result is entered if one is required 
            function() {
                pageCallbacks.handleStatusCallback();
            }
        );
    },
    makeUserEnterActualResult: function () {
        teViewModel.vm.chosenPendingStatusUpdate(pageProps.temp.statusId);
        pageModelStatusUpdate.openActualResult();
    },
    syncViewModelAndEditorActualResult: function (callback) {
        //unless ckeditor is in focus just before clicking an action button, the editor and model should be in sync already
        //so a quick check upfront speeds up the function to avoid the set interval
        if (teViewModel.vm.isEditorAndModelSynced()) {
            callback && callback();
        } else {

            //but if a ckeditor has focus (which sets the isEditorAndModelSynced to false as soon as the editor is focused on by user)
            //then there is a danger that the custom binding event on the ckeditor to get the model in sync with the editor won't be done before other code runs to, say, move step
            //a quick loop of checks should be more than enough as the likely delay is going to be milliseconds
            //100ms chosen here is an arbitary and pretty small amount
            //it should definitely not take more than five loops
            var counter = 0;
            var checkSyncStatus = setInterval(function () {
                if (teViewModel.vm.isEditorAndModelSynced()) {
                    callback && callback();
                    clearInterval(checkSyncStatus);
                } else {
                    counter++;
                }

                if (counter === 5) {
                    clearInterval(checkSyncStatus);
                    //no callback to make sure wrong data is not sent anywhere
                    //display error
                    globalFunctions.display_error($get(_pageInfo.clientId), resx.Global_PleaseTryAgain);
                    console.warn("The ckeditor was delayed in synchronising with the model, as monitored by the function 'syncViewModelAndCKEditors'");
                }
            }, 100);
        }
    },
    openActualResult: function () {
        if (teViewModel.vm.userSettingsDisplayModeInUse() != pageProps.displayMode.grid) {
            $('#te-inspector-tab-buttons_actual-results').trigger('click');
        }
        teViewModel.vm.actualResultNeedsFocus(true);
        //set focus on the actual result with a small delay to make sure the cursor gets positioned
        setTimeout(function() {
            CKEDITOR.instances[pageProps.actualResultId].focus();
        }, 100);
    },
    openGridActualResult: function (input) {
        //only attempt to set focus on the actual result editor if it has already been created (ie not on first entry to a test step)
        var that = input || this,
            stepAlreadySelected = that.testRunStepId === teViewModel.vm.chosenTestStep().testRunStepId;
        if (stepAlreadySelected) {
            pageModelStatusUpdate.openActualResult();
        }
        return true;
    },      
    closeGridActualResult: function () {
        pageModelStatusUpdate.syncViewModelAndEditorActualResult( function() {
            var self = teViewModel.vm,
                actualResultIsEmpty = self.chosenTestStep().actualResult() ? false : true,
                currentStatus = self.chosenPendingStatusUpdate(),
                stepHasExecutionAttempt = currentStatus != 3,
                // actual result required if the project settings say so, or if the status is not pass
                actualResultRequired = stepHasExecutionAttempt && (_pageInfo.actualResultAlwaysRequired || currentStatus != 2);

			//Only let the text box close when a non-pass/not-run status is set (or if the settings always requires an actual result) if an actual result is entered
            self.actualResultNeedsFocus(actualResultRequired && actualResultIsEmpty);
        });
    }
};
//Move to the previous or next test step
var pageModelMoveFocus = {
    //Send the correct new array object as the selected test set step
    stepForward: function () {
        var self = teViewModel.vm;
        var currentStepIndex = self.itemArrayProps().stepCurrentIndex,
            currentRunIndex = self.itemArrayProps().runCurrentIndex,
            newStepIndex = currentStepIndex + 1,
            newRunIndex = currentRunIndex + 1;
        if (pageModelSetChosen.isMovingStepAllowed()) {
            if (self.itemPositions().isVeryLastStep) {
                pageModelMoveFocus.resetChosenStepSettingsOnMove();
                return;
            } else if (self.itemPositions().isLastStepInRun) {
                pageModelSetChosen.manageChosenStepChange(self.testRun()[newRunIndex].testRunStep()[0]);
            } else {
                pageModelSetChosen.manageChosenStepChange(self.testRun()[currentRunIndex].testRunStep()[newStepIndex]);
            }
            pageControls.scrollGridToCurrentStep();
            pageModelMoveFocus.resetChosenStepSettingsOnMove();
        } else {
            pageModelStatusUpdate.handleStatusUpdateAttempt(teViewModel.vm.chosenTestStep().executionStatusId());
        }
    },
    stepBack: function () {
        var self = teViewModel.vm;
        var currentStepIndex = self.itemArrayProps().stepCurrentIndex,
            currentRunIndex = self.itemArrayProps().runCurrentIndex,
            newStepIndex = currentStepIndex == 0 ? 0 : currentStepIndex - 1,
            newRunIndex = currentRunIndex == 0 ? 0 : currentRunIndex - 1,
            lastStepIndexInNewRun = self.testRun()[newRunIndex].testRunStep().length - 1;
        if (pageModelSetChosen.isMovingStepAllowed()) {
            if (self.itemPositions().isVeryFirstStep) {
                pageModelMoveFocus.resetChosenStepSettingsOnMove();
                return;
            } else if (self.itemPositions().isFirstStepInRun) {
                pageModelSetChosen.manageChosenStepChange(self.testRun()[newRunIndex].testRunStep()[lastStepIndexInNewRun]);
            } else {
                pageModelSetChosen.manageChosenStepChange(self.testRun()[currentRunIndex].testRunStep()[newStepIndex]);
            }
            pageControls.scrollGridToCurrentStep();
            pageModelMoveFocus.resetChosenStepSettingsOnMove();
        } else {
            pageModelStatusUpdate.handleStatusUpdateAttempt(teViewModel.vm.chosenTestStep().executionStatusId());
        }
    },
    //if test runs are shown in the progress bar, send the correct object into test set step
    runForward: function () {
        var self = teViewModel.vm;
        var currentRunIndex = self.itemArrayProps().runCurrentIndex,
            newRunIndex = currentRunIndex + 1;
        if (pageModelSetChosen.isMovingStepAllowed()) {
            if (self.itemPositions().isLastRun) {
                return;
            } else {
                pageModelSetChosen.manageChosenStepChange(self.testRun()[newRunIndex].testRunStep()[0]);
            }
            pageControls.scrollGridToCurrentStep(true);
            pageModelMoveFocus.resetChosenStepSettingsOnMove();
        } else {
            pageModelStatusUpdate.handleStatusUpdateAttempt(teViewModel.vm.chosenTestStep().executionStatusId());
        }
    },
    runBack: function () {
        var self = teViewModel.vm;
        var currentRunIndex = self.itemArrayProps().runCurrentIndex,
            newRunIndex = currentRunIndex == 0 ? 0 : currentRunIndex - 1;
        if (pageModelSetChosen.isMovingStepAllowed()) {
            if (self.itemPositions().isFirstRun) {
                return;
            } else {
                pageModelSetChosen.manageChosenStepChange(self.testRun()[newRunIndex].testRunStep()[0]);
            }
            pageControls.scrollGridToCurrentStep(true);
            pageModelMoveFocus.resetChosenStepSettingsOnMove();
        } else {
            pageModelStatusUpdate.handleStatusUpdateAttempt(teViewModel.vm.chosenTestStep().executionStatusId());
        }
    },
    //set which type of action the progress bar should have (based on what is displayed in the progress bar)
    progressBarForward: function () {
        var self = teViewModel.vm;
        return self.progressBarDisplaySteps() ? pageModelMoveFocus.stepForward() : pageModelMoveFocus.runForward();
    },
    progressBarBack: function () {
        var self = teViewModel.vm;
        return self.progressBarDisplaySteps() ? pageModelMoveFocus.stepBack() : pageModelMoveFocus.runBack();
    },
    resetChosenStepSettingsOnMove: function () {
        var self = teViewModel.vm,
            selfActualResult = self.chosenTestStep().actualResult();
        //Reset chosen step specific metadata
        self.chosenPendingStatusUpdate(null);
        self.actualResultNeedsFocus(false);
        self.showAddIncidentMessage(false);
        pageProps.temp.statusId = '';
        pageProps.temp.isPassAll = '';
        pageProps.temp.shouldLeave = false;
        pageProps.temp.shouldMoveForward = true;
        
        //Clear the grid of existing linked incidents
        self.chosenTestStepIncidents([]);
        self.chosenLinkedIncidentsSuccess(false);
        
        //Clear the incident form
        pageManageIncidentForm.clearIncidentFields();

        //Get incidents if the project settings require an incident for every non pass step
        //We get the incidents here so that we have them ahead of an attempt to set an execution status to avoid async issues
        if (_pageInfo.requireIncident) {
            pageCallbacks.getChosenIncidents();
        }
        
        //Reset the tabs on the inspector
        $('.te-inspector .tab-pane').removeClass('active');
        $('#te-inspector-tab-buttons_actual-results').trigger('click');
        $('#te-inspector_tab_actual-results').addClass('active');
        
        //Reset timing parameters
        pageProps.timeManagement.duration = 0;
        pageProps.timeManagement.skipTimeUpdate = false;

        
    }
};
//Attachment Panel
var pageManageAttachments = {
    //Set the attachment panel's test run id so it can upload new attachments
    linkAttachmentsToChosenStep: function() {
        if (tstucAttachmentPanel.set_artifactId) {
            if (teViewModel.vm.chosenTestStepParent().testerId === _pageInfo.userId) {
                tstucAttachmentPanel.set_artifactId(teViewModel.vm.chosenTestStepParent().testRunId);
            } else {
                tstucAttachmentPanel.set_artifactId(-1);
            }
        }
    },
    //Display the list of existing attachments for this test run, test case or test step
    getChosenAttachments: function () {
        var pnlAttachments = $get(_pageInfo.pnlTestExecution_Attachments),
            filters = {},
            chosenTestRunId = teViewModel.vm.chosenTestStepParent().testRunId,
            chosenTestRunTestCaseId = teViewModel.vm.chosenTestStepParent().testCaseId,
            chosenTestStepId = teViewModel.vm.chosenTestStep().testStepId,
            chosenTestStepTestCaseId = teViewModel.vm.chosenTestStep().testCaseId;
        filters['ArtifactId'] = globalFunctions.serializeValueInt(chosenTestRunId);
        filters['ArtifactType'] = globalFunctions.serializeValueInt(_pageInfo.artifactTypeFilter_TestRun);
        if (chosenTestStepTestCaseId) {
            filters['AdditionalArtifact_' + _pageInfo.artifactTypeFilter_TestCase] = globalFunctions.serializeValueInt(chosenTestStepTestCaseId);
        } else {
            filters['AdditionalArtifact_' + _pageInfo.artifactTypeFilter_TestCase] = globalFunctions.serializeValueInt(chosenTestRunTestCaseId);
        }
        if (chosenTestStepId) {
            filters['AdditionalArtifact_' + _pageInfo.artifactTypeFilter_TestStep] = globalFunctions.serializeValueInt(chosenTestStepId);
        }

        //Set the artifact type/id for use in permission checking
        SpiraContext.ArtifactTypeId = globalFunctions.artifactTypeEnum.testRun;
        SpiraContext.ArtifactId = chosenTestRunId;

        var loadNow = (pnlAttachments.style.display != 'none');
        tstucAttachmentPanel.load_data(filters, loadNow);
    }
};
//Manage incident form manager
var pageManageIncidentForm = {
    //Returns the following:   null => no incident to log;   undefined => validation error;    dataItem => log an incident
    captureIncidentData: function() {
        //If incident name specified, need to save item and send with data
        var txtIncidentName = $get(_pageInfo.txtIncidentName);
        var incidentName = txtIncidentName.value.trim();
        if (incidentName == '') {
            return null;
        }
        var ajxIncidentFormManager = $find(_pageInfo.ajxIncidentFormManager);
        //Just return true, but leave incident data unpopulated since no incident entered
        if (!ajxIncidentFormManager.validate_and_update()) {
            //Validation failed, so return undefined
            return undefined;
        }
        //Return the populated incident data item
        return ajxIncidentFormManager.get_dataItem();
    },
    clearIncidentFields: function() {
        //Refresh the new incident panel - ignore any unsaved changes
        var ajxIncidentFormManager = $find(_pageInfo.ajxIncidentFormManager);
        ajxIncidentFormManager.set_unsavedChanges(false);
        ajxIncidentFormManager.load_data(false, false, true);
    },
    //Adding a new incident via button click (in grid view)
    newIncidentAdd_click: function() {
        if (teViewModel.vm.appIsBusy()) {
            return;
        }
        //set the callback data
        var self = teViewModel.vm,
            chosenStep = self.chosenTestStep(),
            data = {};
        data.projectId = _pageInfo.projectId;
        data.testRunsPendingId = self.testRunsPendingId;
        data.testRunId = chosenStep.testRunStepParentId;
        data.testRunStepId = chosenStep.testRunStepId;
        pageModelStatusUpdate.syncViewModelAndEditorActualResult( function () {
            data.actualResult = chosenStep.actualResult();
        });
        //Get any incident data (returns undefined if validation error)
        data.incidentDataItem = pageManageIncidentForm.captureIncidentData();
        
        //Make the callback
        if (data.incidentDataItem) {
            teViewModel.vm.appIsBusy(true);
            pageProps.ajxService.TestExecution_LogIncident(data.projectId, data.testRunsPendingId, data.testRunId, data.testRunStepId, data.actualResult, data.incidentDataItem, pageCallbacks.newIncidentAdd_success, pageCallbacks.newIncidentAdd_failure);
        } else {
            globalFunctions.display_error_message(
                //$get(_pageInfo.msgNewIncidentMessageInline),
                null,
                resx.TestCaseExecution_IncidentEntryInvalid,
                true, // makes the popup modal
                "fas fa-exclamation-triangle mr3"
            );
        }
    },
    
    associateExistingIncident: function (panelId, modalId) {
        //we set the art id into spira context as this is the generic place where the association panel js looks for the id.
        SpiraContext.ArtifactId = teViewModel.vm.chosenTestStep().testRunStepId;
        SpiraContext.ArtifactTypeId = globalFunctions.artifactTypeEnum.testRunStep;

        //populate general data into the global panelAssociationAdd object, so it is accessible by React on render
        panelAssociationAdd.lnkAddBtnId = _pageInfo.btnLinkExistingIncident;
        panelAssociationAdd.addPanelId = panelId;
        panelAssociationAdd.displayType = globalFunctions.displayTypeEnum.TestRun_Incidents;
        panelAssociationAdd.messageBox = _pageInfo.divNewIncidentsMessage;
        panelAssociationAdd.customSaveSuccessFunction = pageManageIncidentForm.associateExistingIncident_success;
        panelAssociationAdd.listOfViewableArtifactTypeIds = '3';

        //now render the panel
        panelAssociationAdd.showPanel();

        //if a modal Id is passed in (used when in table view) then display the modal
        if(modalId) {
            $("#" + modalId).modal();
        }
    },

    associateExistingIncident_success: function () {
        //Reload the incident grid
        teViewModel.vm.chosenLinkedIncidentsSuccess(false);
        pageCallbacks.getChosenIncidents();

        //Hide any message saying an incident is required to complete this step
        if (teViewModel.vm.showAddIncidentMessage()) {
            teViewModel.vm.showAddIncidentMessage(false);
            //scroll to the top of the page so the pass/fail buttons are visible - these are likely the next thing the user will want to interact with
            window.scroll(0, 0)
        }
    },

    makeUserAddIncident: function () {
        teViewModel.vm.showAddIncidentMessage(true);
        pageManageIncidentForm.openeIncidentAssociationPanel(true);
    },

    openeIncidentAssociationPanel: function (focusOnNameField) {
        //Make sure we have permssions
        if (globalFunctions.isAuthorized(globalFunctions.permissionEnum.Create, globalFunctions.artifactTypeEnum.incident) != globalFunctions.authorizationStateEnum.authorized) {
            alert(resx.AjaxFormManager_NotAuthorizedToCreate);
        }
        if (teViewModel.vm.userSettingsDisplayModeInUse() != pageProps.displayMode.grid) {
            $('#te-inspector-tab-buttons_incidents').trigger('click');
            //set focus on the Name field 
            if (focusOnNameField) {
                document.getElementById("txtIncidentName").focus();
            }
        } else {
            pageManageIncidentForm.openIncidentAssociationModal()
        }
    },

    openIncidentAssociationModal: function () {
        //Make sure we have permssions
        if (globalFunctions.isAuthorized(globalFunctions.permissionEnum.Create, globalFunctions.artifactTypeEnum.incident) != globalFunctions.authorizationStateEnum.authorized) {
            alert(resx.AjaxFormManager_NotAuthorizedToCreate);
        }
        else {
            $("#" + _pageInfo.addIncidentArea).modal();
            setTimeout(function () {
                //update the layout of the columns inside the modal
                uWrapper_onResize();
            }, 300);
        }
    },

    closeIncidentAssociationPanel: function () {
        $("#" + _pageInfo.btnLinkExistingIncident).removeClass('disabled');
        //only unmount the panel if it is currently mounted
        var domId = panelAssociationAdd.addPanelId;
        if (typeof SpiraContext.uiState[domId] != "undefined" && SpiraContext.uiState[domId].isMounted) {
            SpiraContext.uiState[domId].isMounted = false;
            ReactDOM.unmountComponentAtNode(document.getElementById(domId));
        }
    },

    closeIncidentAssociationModal: function () {
        $("#associateIncidentModal").modal("hide");
        pageManageIncidentForm.closeIncidentAssociationPanel();
    }
};


var pageManageCustomProperties = {
    getTestCase: function() {
        var testCaseId = teViewModel.vm.chosenTestStepParent().testCaseId,
            show = teViewModel.vm.userSettingsShowCustomPropertiesOfTestCase(),
            ajxTestRunCaseFormManager = $find(_pageInfo.ajxTestRunCaseFormManager),
            testCaseHasChanged = pageProps.previousChosen.testCaseId != testCaseId;
        if(show && testCaseHasChanged) {
            ajxTestRunCaseFormManager.set_projectId(_pageInfo.projectId);
            ajxTestRunCaseFormManager.set_primaryKey(testCaseId);
            ajxTestRunCaseFormManager.load_data(false, false, true);
            //Make sure any custom properties shown on page are disabled
            ajxTestRunCaseFormManager.add_loaded(function () {
                pageManageCustomProperties.disableAllInputs()
            });
        }
    },
    getTestStep: function() {
        var testStepId = teViewModel.vm.chosenTestStep().testStepId,
            show = teViewModel.vm.userSettingsShowCustomProperties(),
            ajxTestRunStepFormManager = $find(_pageInfo.ajxTestRunStepFormManager);
        if(show) {
            ajxTestRunStepFormManager.set_projectId(_pageInfo.projectId);
            ajxTestRunStepFormManager.set_primaryKey(testStepId);
            ajxTestRunStepFormManager.load_data(false, false, true);
            //Make sure any custom properties shown on page are disabled
            ajxTestRunStepFormManager.add_loaded(function () {
                pageManageCustomProperties.disableAllInputs()
            });
        }

        //Once we have a step, we need to also get incidents if the project settings require an incident for every non pass step
        //We get the incidents here so that we have them ahead of an attempt to set an execution status to avoid async issues
        if (_pageInfo.requireIncident) {
            pageCallbacks.getChosenIncidents();
        }

    },
    getTestCaseAndStep: function() {
        this.getTestCase();
        this.getTestStep();
    },
    disableAllInputs: function() {
        $('.children-read-only input').prop('disabled', true);
    }
};





var pageManageTasks = {
    //Overwrites any existing description in the task form with some combination of the description and actual result of the currently selected test step
    setDescriptionFromTestStep: function () {
        var chosenStep = teViewModel.vm.chosenTestStep(),
            stepDescription = chosenStep.description,
            stepActualResult = chosenStep.actualResult(),
            newTaskDescription = "";

        // if both fields are filled in (normal case) then create headings
        if (stepDescription && stepActualResult) {
            newTaskDescription = "<h2>" + resx.Global_Description + "</h2>" + stepDescription + "<br>" + "<h2>" + resx.TestCaseExecution_ActualResult + "</h2>" + stepActualResult;
            // if there's only one field filled in then just return that 
        } else {
            newTaskDescription = stepDescription + stepActualResult;
        }

        teViewModel.vm.taskDescription(newTaskDescription);
    },

    //Adding a new task via button click (in grid view)
    newTask_click: function () {
        if (teViewModel.vm.appIsBusy()) {
            return;
        }

        if (teViewModel.vm.taskName() == "") {
            alert(resx.TaskDetails_SplitTaskNameRequired);
        } else {
            //set the callback data
            var self = teViewModel.vm,
                chosenStep = self.chosenTestStep(),
                projectId = _pageInfo.projectId,
                testCaseId = chosenStep.testCaseId,
                testRunsPendingId = self.testRunsPendingId,
                testRunId = self.chosenTestStepParent().testRunId,
                testRunStepId = chosenStep.testRunStepId,
                taskName = self.taskName(),
                taskDescription = self.taskDescription(),
                taskOwnerId = $find(_pageInfo.ddlTaskOwner).get_selectedItem().get_value() == "" ? null : $find(_pageInfo.ddlTaskOwner).get_selectedItem().get_value();


            teViewModel.vm.appIsBusy(true);

            //Make the callback
            pageProps.ajxService.TestExecution_LogTask(
                projectId,
                testCaseId,
                testRunsPendingId,
                testRunId,
                testRunStepId,
                taskName,
                taskDescription,
                taskOwnerId,
                pageManageTasks.newTask_success,
                pageManageTasks.newTask_failure
            );
        }
    },
    newTask_success: function (data) {
        teViewModel.vm.appIsBusy(false);
        //add task to tasks model array
        newTask = {
            name: teViewModel.vm.taskName(),
            taskId: data,
            taskToken: "TK:" + data,
            owner: $find(_pageInfo.ddlTaskOwner).get_selectedItem().get_value() == "" ? null : $find(_pageInfo.ddlTaskOwner).get_selectedItem().get_value(),
            type: "",
            status: "",
            creator: "",
            priorityColor: "d0d0d0",
            creationDate: "",
            url: SpiraContext.BaseUrl + SpiraContext.ProjectId + "/Task/" + data + ".aspx"
        }
        teViewModel.vm.tasks.push(newTask);

        //reset task fields
        teViewModel.vm.taskName("");
        teViewModel.vm.taskDescription("");

        //hide the popup - only relevant in table mode
        $('#newTaskCancel').trigger('click');

    },
    newTask_failure: function (err) {
        teViewModel.vm.appIsBusy(false);
    },


    loadTasks: function (testRunId) {
        globalFunctions.display_spinner();
        Inflectra.SpiraTest.Web.Services.Ajax.TasksService.Task_RetrieveByTestRunId(
            _pageInfo.projectId,
            testRunId,
            pageManageTasks.loadTasksSuccess,
            pageManageTasks.loadTasksFailure
        );
    },
    loadTasksSuccess: function (data) {
        globalFunctions.hide_spinner();
        pageModelUtilities.addSubArray(teViewModel.vm.tasks, data.items, tasksViewModel);
    },
    loadTasksFailure: function (exception) {
        globalFunctions.hide_spinner();
        globalFunctions.display_error($get(_pageInfo.clientId), exception);
    },


    openTaskModal: function () {
        //Make sure we have permssions
        if (globalFunctions.isAuthorized(globalFunctions.permissionEnum.Create, globalFunctions.artifactTypeEnum.task) != globalFunctions.authorizationStateEnum.authorized) {
            alert(resx.AjaxFormManager_NotAuthorizedToCreate);
        }
        else {
            $("#" + _pageInfo.addTaskArea).modal();
            setTimeout(function () {
                //update the layout of the columns inside the modal
                uWrapper_onResize();
            }, 300);
        }
    },
};




//load and use the tooltips
var pageTooltips = {
    _incidentIsOverNameDesc: false,
    displayTestStepTooltip: function (data) {
        //Now create the custom HTML for the tooltip message
        var innerHtml = '<u>' + data.positionTitle() + ' - TC' + data.testCaseId + ' / TS' + data.testStepId + '' + '</u><br />'
            + data.description + '<br /><i>' + (data.actualResult() ? data.actualResult() : data.expectedResult) + '</i>';
        ddrivetip(innerHtml);
    },
    hideTestStepTooltip: function () {
        hideddrivetip();
    },
    displayTestRunTooltip: function (data) {
        //Now create the custom HTML for the tooltip message
        var stepWordToUse = data.countSteps() > 1 ? resx.TestStep_StepPlural : resx.TestStep_Step;
        var innerHtml = '<u>' + data.name + ' - TC' + data.testCaseId + ' (' + data.countSteps() + ' ' + stepWordToUse + ')' + '</u><br />'
            + data.description;
        ddrivetip(innerHtml);
    },
    hideTestRunTooltip: function () {
        hideddrivetip();
    },
    displayIncidentTooltip: function(data) {
        //Display the loading message
        ddrivetip(resx.GlobalFunctions_TooltipLoading);
        pageTooltips._incidentIsOverNameDesc = true;   //Set the flag since asynchronous
        var incidentId = data.incidentIdIntValue;
        //Now get the real tooltip via Ajax web-service call
        Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.RetrieveNameDesc(_pageInfo.projectId, incidentId, null, pageTooltips.displayIncidentTooltip_success, pageTooltips.displayIncidentTooltip_failure);
    },
    displayIncidentTooltip_success: function(tooltipData) {
        if (pageTooltips._incidentIsOverNameDesc) {
            ddrivetip(tooltipData);
        }
    },
    displayIncidentTooltip_failure: function(exception) {
        //Fail quietly
    },
    hideIncidentTooltip: function() {
        hideddrivetip();
        pageTooltips._incidentIsOverNameDesc = false;
    },
};
var pageCallbacks = {
    TestExecution_RetrieveTestRunsPending_Success: function(data) {
        pageProps.data = data;
        //only set the model and show the main parts of the page if actual data was returned
        if (data != null && data.TestRun.length != 0) {
            teViewModel.vm = new testRunPendingViewModel();
            pageModelSetChosen.setInitialTestStepPosition();
            ko.applyBindings(teViewModel.vm);

            //start the tour on first time accessing the page
            teViewModel.vm.userSettingsGuidedTourSeen() ? void (null) : hopscotch.startTour(teTour);

            $('.te-main-content').show();
            $('.te-main-content').removeClass('display-none');
            pageControls.setInitialDisplayMode(function() {
                pageProps.loadedOK = true;
            });
            //hide the wizard, show the main page - with subtle animation effect 
            $('#te-wizard-panel').animate({
                opacity: 0
                }, 200, function() {
                    $('#te-wizard-panel').hide();
                    $('#te-execution-panel').show().animate({ opacity: 1 }, 200, function () {
                        //resize any unity wrapper areas
                        uWrapper_onResize();
                    });
                    //affix the progress bar using Bootstrap affix js property - only in IE
                    if (browserCapabilities.isIE9 || browserCapabilities.isIE10 || browserCapabilities.isIE11) {
                        $('.te-progress-bar').affix({
                            offset: {
                                top: function () { return pageprops.progressbar.$bar.offset().top - parseint($('body').css('margintop')) }
                            }
                        });
                    }

                    pageControls.setGridHeightOnSplit();
                    //Scroll the grid if needed
                    pageControls.scrollGridToCurrentStep();
                }
            );
            
            //Carry out some checks on the page - to make sure timing functions and old IE browsers handled correctly
            pageControls.playPauseCheck();
            pageControls.handleIe();
            
            //Make sure any custom properties shown on page are disabled
            pageManageCustomProperties.disableAllInputs();

            //retrieve any tasks (if required permissions met)
            if (pageProps.canViewTasks) {
                pageManageTasks.loadTasks(data.TestRun[0].TestRunId);
            }
            
            
        } else {
            $('#no-testRunsFound-message').show();
        }
    },
    TestExecution_RetrieveTestRunsPending_Failure: function (exception) {
        globalFunctions.hide_spinner();
        globalFunctions.display_error($get(_pageInfo.clientId), exception);
    },
	getChosenIncidents: function () {
        var chosenStep = teViewModel.vm.chosenTestStep();
        if (teViewModel.vm.chosenLinkedIncidentsSuccess()) {
            return
        } else {
            //Check permissions before showing existing incidents
            if (globalFunctions.isAuthorized(globalFunctions.permissionEnum.View, globalFunctions.artifactTypeEnum.incident) == globalFunctions.authorizationStateEnum.authorized) {
                pageCallbacks.loadIncidents(chosenStep.testRunStepId, chosenStep.testStepId);
            }
            else {
                $('#existing-incidents-legend').addClass('hidden');
            }
        }
    },
    loadIncidents: function (testRunStepId, testStepId) {
        globalFunctions.display_spinner();
        Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.RetrieveByTestRunStepId(_pageInfo.projectId, testRunStepId, testStepId, Function.createDelegate(this, pageCallbacks.loadIncidentsSuccess),Function.createDelegate(this, pageCallbacks.loadIncidentsFailure));
    },
    loadIncidentsSuccess: function(data) {
        teViewModel.vm.chosenTestStepIncidents([]);
        teViewModel.vm.chosenLinkedIncidentsSuccess(true);
        globalFunctions.hide_spinner();
        pageModelUtilities.addSubArray(teViewModel.vm.chosenTestStepIncidents, data.items, incidentsViewModel);
    },
    loadIncidentsFailure: function(exception) {
        teViewModel.vm.chosenLinkedIncidentsSuccess(false);
        globalFunctions.hide_spinner();
        globalFunctions.display_error($get(_pageInfo.clientId), exception);
    },
    
    //sending back current position to store in user settings
    sendBackPosition: function() {
        var projectId = _pageInfo.projectId,
            currentStep = teViewModel.vm.userSettingsCurrentStepId(),
            currentRun = teViewModel.vm.userSettingsCurrentRunId();
        pageProps.ajxService.TestExecution_LogCurrentPosition(projectId, currentStep, currentRun, pageCallbacks.sendBackPosition_success, pageCallbacks.sendBackPosition_failure);
    },
    sendBackPosition_success: function() {
        //succeed quietly
    },
    sendBackPosition_failure: function() {
        //fail quietly
    },
    
    //sending back current user display settings
    logDisplayOptions: function (newVal) {
        //we pass in the value from the subscription event - the event from knockout fires twice
        //one time returns a true/false, the other an undefined (order varies by browser). We only want to send the new value (true/false)
        if (typeof newVal !== "undefined") {
            var projectId = _pageInfo.projectId,
                displayModeMain = teViewModel.vm.userSettingsDisplayModeInUse() || 1,
                displayModeSub = teViewModel.vm.userSettingsDisplaySubModeInUse() || 1,
                alwaysShowTestRun = teViewModel.vm.userSettingsAlwaysShowTestRunDetails(),
                showCustomProperties = teViewModel.vm.userSettingsShowCustomProperties(),
                guidedTourSeen = teViewModel.vm.userSettingsGuidedTourSeen();
            alwaysShowTestRun = (alwaysShowTestRun === true ? true : (alwaysShowTestRun === false ? false : false));
            pageProps.ajxService.TestExecution_LogDisplaySettings(projectId, displayModeMain, displayModeSub, alwaysShowTestRun, showCustomProperties, guidedTourSeen, pageCallbacks.logDisplayOptions_success, pageCallbacks.logDisplayOptions_failure);
            return true;
        }
    },
    logDisplayOptions_success: function() {
        //succeed quietly
    },
    logDisplayOptions_failure: function() {
        //fail quietly
    },
    
    //Recording confirmed statuses
    handleStatusCallback: function() {
        if (teViewModel.vm.appIsBusy()) {
            return;
        }
        //run the timing updates needed - set the start if in pause mode to ensure all fields complete and duration is 0
        if(pageProps.timeManagement.isPaused) {
            pageTimeManagement.setLocalStart(function() {
                pageTimeManagement.setActualStart()
            });
        }
        pageTimeManagement.setAllEndTimes();
        //set the callback data
        var self = teViewModel.vm,
            chosenStep = self.chosenTestStep(),
            data = {};
        data.projectId = _pageInfo.projectId;
        data.testRunsPendingId = self.testRunsPendingId;
        data.testRunStepId = chosenStep.testRunStepId;
        data.actualResult = chosenStep.actualResult();
        data.startDate = chosenStep.startDate();
        data.endDate = chosenStep.endDate();
        data.actualDuration = chosenStep.actualDuration();

        //Get any incident data (returns undefined if validation error)
        data.incidentDataItem = pageManageIncidentForm.captureIncidentData();
        if(pageProps.temp.isPassAll) {
            data.testRunId = self.chosenTestStepParent().testRunId;
        } else {
            data.testRunId = chosenStep.testRunStepParentId;  
        }
        if (typeof(data.incidentDataItem) != 'undefined') {
            //Call the webservice
            teViewModel.vm.appIsBusy(true);
            globalFunctions.display_spinner();
            if(pageProps.temp.isPassAll) {
                pageProps.ajxService.PassAllTestRunSteps(data.projectId, data.testRunsPendingId, data.testRunId, data.actualResult, data.testRunStepId, data.startDate, data.endDate, data.actualDuration, pageCallbacks.recordStatusUpdateResult_success, pageCallbacks.recordStatusUpdateResult_failure);
            } else {
                switch (pageProps.temp.statusId) {
                    case 1: //fail
                        pageProps.ajxService.FailTestRunStep(data.projectId, data.testRunsPendingId, data.testRunId, data.testRunStepId, data.actualResult, data.startDate, data.endDate, data.actualDuration, data.incidentDataItem, pageCallbacks.recordStatusUpdateResult_success, pageCallbacks.recordStatusUpdateResult_failure);
                        break;
                    case 2: //pass
                        pageProps.ajxService.PassTestRunStep(data.projectId, data.testRunsPendingId, data.testRunId, data.testRunStepId, data.actualResult, data.startDate, data.endDate, data.actualDuration, data.incidentDataItem, pageCallbacks.recordStatusUpdateResult_success, pageCallbacks.recordStatusUpdateResult_failure);
                        break;
                    case 4: //not applicable
                        pageProps.ajxService.NotApplicableTestRunStep(data.projectId, data.testRunsPendingId, data.testRunId, data.testRunStepId, data.actualResult, data.startDate, data.endDate, data.actualDuration, data.incidentDataItem, pageCallbacks.recordStatusUpdateResult_success, pageCallbacks.recordStatusUpdateResult_failure);
                        break;
                    case 5: //blocked
                        pageProps.ajxService.BlockTestRunStep(data.projectId, data.testRunsPendingId, data.testRunId, data.testRunStepId, data.actualResult, data.startDate, data.endDate, data.actualDuration, data.incidentDataItem, pageCallbacks.recordStatusUpdateResult_success, pageCallbacks.recordStatusUpdateResult_failure);
                        break;
                    case 6: //caution
                        pageProps.ajxService.CautionTestRunStep(data.projectId, data.testRunsPendingId, data.testRunId, data.testRunStepId, data.actualResult, data.startDate, data.endDate, data.actualDuration, data.incidentDataItem, pageCallbacks.recordStatusUpdateResult_success, pageCallbacks.recordStatusUpdateResult_failure);
                        break;
                }
            }
        } 
    },
    recordStatusUpdateResult_success: function(data, e) {
        //manage success and record the requested new status into the model
        globalFunctions.hide_spinner();
        teViewModel.vm.appIsBusy(false);
        //update test run status
        if (teViewModel.vm.chosenTestStepParent().testRunId === Number(data.kTestRunId)) {
            var newStatus = Number(data.kTestRunExecutionStatusId);
            isNaN(newStatus) ? void(null) : teViewModel.vm.chosenTestStepParent().executionStatusId(newStatus);
        }
        //update test run step status(es)
        if (pageProps.temp.isPassAll) {
            //assign status update to every step in the run
            var parent = teViewModel.vm.chosenTestStepParent().testRunStep(),
                l = parent.length;
            for (var i = 0; i < l; i++) {
                parent[i].executionStatusId(pageProps.temp.statusId);
                if(i === (l - 1) ) {
                    pageProps.temp.shouldMoveForward ? pageModelMoveFocus.runForward() : pageModelMoveFocus.resetChosenStepSettingsOnMove();
                }
            };
        } else {  
            //assign status to the chosen step only and move position as needed
            pageProps.timeManagement.skipTimeUpdate = true;
            teViewModel.vm.chosenTestStep().executionStatusId(pageProps.temp.statusId);
            pageProps.temp.shouldMoveForward ? pageModelMoveFocus.stepForward() : pageModelMoveFocus.resetChosenStepSettingsOnMove();
        }
    },
    recordStatusUpdateResult_failure: function(exception, e) {
        globalFunctions.hide_spinner();
        teViewModel.vm.appIsBusy(false);
        if (exception.get_exceptionType && exception.get_message && exception.get_exceptionType() == 'DataValidationExceptionEx') {
            var ajxIncidentFormManager = $find(_pageInfo.ajxIncidentFormManager);
            if (ajxIncidentFormManager && ajxIncidentFormManager.display_validation_messages) {
                //The message is really a serialized array of validation messages
                var messages = eval ("(" + exception.get_message() + ")");
                ajxIncidentFormManager.display_validation_messages(messages);
            } else {
                globalFunctions.display_error($get(_pageInfo.clientId), exception);
            }
        } else {
            globalFunctions.display_error($get(_pageInfo.clientId), exception);
        }
    },
	newIncidentAdd_success: function () {
        globalFunctions.hide_spinner();
        teViewModel.vm.appIsBusy(false);
        pageManageIncidentForm.clearIncidentFields();
        $('#newIncidentCancel').trigger('click');
        
        //get the incidents so that you can see the new incident straight away - if we are in the inspector mode (ie not grid mode)
        if (teViewModel.vm.userSettingsDisplayModeInUse() != pageProps.displayMode.grid) {
			var chosenStep = teViewModel.vm.chosenTestStep();
			//Check permissions before showing existing incidents
			if (globalFunctions.isAuthorized(globalFunctions.permissionEnum.View, globalFunctions.artifactTypeEnum.incident) == globalFunctions.authorizationStateEnum.authorized) {
				pageCallbacks.loadIncidents(chosenStep.testRunStepId, chosenStep.testStepId);
			}
        }

        //Hide any message saying an incident is required to complete this step
        if (teViewModel.vm.showAddIncidentMessage()) {
            teViewModel.vm.showAddIncidentMessage(false);
            //scroll to the top of the page so the pass/fail buttons are visible - these are likely the next thing the user will want to interact with
            window.scroll(0, 0)
        }
    },
    newIncidentAdd_failure: function (exception, e)
    {
        globalFunctions.hide_spinner();
        teViewModel.vm.appIsBusy(false);

        if (exception.get_exceptionType && exception.get_message && exception.get_exceptionType() == 'DataValidationExceptionEx')
        {
            var ajxIncidentFormManager = $find(_pageInfo.ajxIncidentFormManager);
            if (ajxIncidentFormManager && ajxIncidentFormManager.display_validation_messages)
            {
                //The message is really a serialized array of validation messages
                var messages = eval("(" + exception.get_message() + ")");
                ajxIncidentFormManager.display_validation_messages(messages, $get(_pageInfo.msgNewIncidentMessageInline));
            } else
            {
                globalFunctions.display_error($get(_pageInfo.msgNewIncidentMessageInline), exception);
            }
        } else
        {
            globalFunctions.display_error($get(_pageInfo.msgNewIncidentMessageInline), exception);
        }
    },
    
    userRequestedRefresh: function() {
        globalFunctions.display_spinner();
        pageProps.ajxService.TestExecution_RetrieveTestRunsPending(_pageInfo.projectId, _pageInfo.testRunsPendingId, 
            pageCallbacks.userRequestedRefresh_Success, pageCallbacks.TestExecution_RetrieveTestRunsPending_Failure);
    },
    //updated the parts of the model that could have changed
    userRequestedRefresh_Success: function(data) {
        globalFunctions.hide_spinner();
        if(data != null) {
            var self = teViewModel.vm,
                allLengthsMatch = true;
            //Check that the array lengths in the data match those in model (first checking the number of test runs is the same)
            if(pageModelUtilities.arrayLengthsMatch(self.testRun(), data.TestRun)) {
                //Next check that each testRun has the same number of steps
                var allChildrenMatch = pageModelUtilities.allChildrenArraysLengthsMatch(self.testRun(), data.TestRun, 'testRunStep', 'TestRunStep');
                allLengthsMatch = allChildrenMatch;
            } else {
                allLengthsMatch = false;
            }
            if (!allLengthsMatch) {
                //TODO provide some kind of error message or fail quietly?  
            //If the data looks good we can update the model
            } else {
                //First update the execution status Id for testRuns
                pageModelUtilities.updateModelArray(self.testRun(), data.TestRun, 'testRunId', 'TestRunId', 'executionStatusId', 'ExecutionStatusId');
                //Now update the testRunStep arrays
                var i = 0,
                    l = self.testRun().length;
                for (i; i < l; i++) {
                    //Update the execution status id, then the actual results
                    pageModelUtilities.updateModelArray(self.testRun()[i].testRunStep(), data.TestRun[i].TestRunStep, 'testRunStepId', 'TestRunStepId', 'executionStatusId', 'ExecutionStatusId');
                    pageModelUtilities.updateModelArray(self.testRun()[i].testRunStep(), data.TestRun[i].TestRunStep, 'testRunStepId', 'TestRunStepId', 'actualResult', 'ActualResult');
                }
            }
            
        } else {
        }
    },
    logChosenStepTiming: function () {
        //only log if the current step is enabled - otherwise server throws an error
        if (!teViewModel.vm.chosenTestStep().cannotEdit) {
            pageTimeManagement.setActualStart();
            pageTimeManagement.setAllEndTimes(function callback() {
                var projectId = _pageInfo.projectId,
                    testRunsPendingId = teViewModel.vm.testRunsPendingId,
                    testRunId = teViewModel.vm.chosenTestStep().testRunStepParentId,
                    testRunStepId = teViewModel.vm.chosenTestStep().testRunStepId,
                    startDate = teViewModel.vm.chosenTestStep().startDate(),
                    endDate = teViewModel.vm.chosenTestStep().endDate(),
                    actualDuration = teViewModel.vm.chosenTestStep().actualDuration();
                pageProps.ajxService.TestExecution_LogStepTiming(projectId, testRunsPendingId, testRunId, testRunStepId, startDate, endDate, actualDuration, function () { pageCallbacks.logChosenStepTiming_success() }, function () { pageCallbacks.logChosenStepTiming_failure() });
            });
        // if we should not log the time (on a disabled step) we still need to pass success so we can leave page if needed
        } else {
            pageCallbacks.logChosenStepTiming_success();
        }
    },
    logChosenStepTiming_success: function () {
        pageProps.temp.shouldLeave ? window.location.href = _pageInfo.fullReferrerUrl : void(null);
    },
    logChosenStepTiming_failure: function (err) {
        pageProps.temp.shouldLeave = false;
        var divLinkIncidentsMessage = $get(_pageInfo.divLinkIncidentsMessage);
        globalFunctions.display_error(divLinkIncidentsMessage, err);
    },


    UpdateTestRunActualResult: function (testRunStepId, textField) {
        var projectId = _pageInfo.projectId,
            testRunId = teViewModel.vm.chosenTestStepParent().testRunId,
            testRunsPendingId = teViewModel.vm.testRunsPendingId;

        pageProps.ajxService.UpdateTestRunActualResult(
            projectId,
            testRunsPendingId,
            testRunId,
            testRunStepId,
            textField,
            pageCallbacks.UpdateTestRunActualResult_success,
            pageCallbacks.UpdateTestRunActualResult_failure
        );
    },
    UpdateTestRunActualResult_success: function () {
        //succeed quietly
    },
    UpdateTestRunActualResult_failure: function () {
        //fail quietly
    },
};

/// DOM triggered jquery events
//On load
$(document).ready(function () {
    $('#te-execution-panel').animate({opacity: 0}, 0);
    $('#te-wizard-panel').show();
    $('#global-nav-keyboard-shortcuts #shortcuts-test-execution').removeClass('dn');

    //Hide new incident logging if we don't have permissions
    if (globalFunctions.isAuthorized(globalFunctions.permissionEnum.Create, globalFunctions.artifactTypeEnum.incident) != globalFunctions.authorizationStateEnum.authorized) {
        $('#' + _pageInfo.addIncidentArea).addClass('hidden');
    }
});
//On window resize
$(window).on("resize", function() {
    pageControls.onWindowResize()
});

//Click events
//display menu
$('#lbl-display-tb_radio-splitView').on("click", function () {
    pageControls.displaySplit();
});
$('#lbl-display-tb_radio-gridView').on("click", function () {
    pageControls.displayGrid();
});
$('#lbl-display-tb_radio-miniView').on("click", function () {
    pageControls.displayMini();
});
$('#lbl-btn-group-splitView_inspectorLarge').on("click", function () {
    pageControls.displaySplitInspectorLarge();
});
$('#lbl-btn-group-splitView_gridLarge').on("click", function () {
    pageControls.displaySplitGridLarge();
});
$('#btn-group-gridView_expand').on("click", function (event) {
    event.preventDefault();
    pageControls.displayGridExpand();
});
$('#btn-group-gridView_collapse').on("click", function (event) {
    event.preventDefault();
    pageControls.displayGridCollapse();
});
$('#lbl-btn-group-miniView_full').on("click", function () {
    pageControls.displayMiniFull();
});
$('#lbl-btn-group-miniView_iframe').on("click", function () {
    pageControls.displayMiniIframe();
});

//options menu - see model code as well at self.userSettings
$('#te-menu-show-guided-tour').on("click", function() {
    hopscotch.startTour(teTour); 
});

//progress bar
pageProps.progressBar.$playPause.on("click", function (event) {
    event.preventDefault();
    pageControls.playPauseChange();
});
$('#' + _pageInfo.pbBtnLeave).on("click", function (event) {
    event.preventDefault();
    pageControls.leave();
});
$('#te-iframe-btn-back').on("click", function () {
    document.getElementById('te-iframe-iframe').contentWindow.history.back();
});
$('#te-iframe-btn-forward').on("click", function () {
    document.getElementById('te-iframe-iframe').contentWindow.history.forward();
});
$('#te-iframe-btn-go').on("click", function () {
    var u = $('#te-iframe-input-url');
    $('#te-iframe-iframe').attr('src', u.val());
    u.val('');
});

//On pressing Enter in the iframe input field
$('#te-iframe-input-url').bind("enterKey", function iframeInputUrl_enterKey(e) {
    $('#te-iframe-btn-go').trigger('click');
});
$('#te-iframe-input-url').keyup(function iframeInputUrl_keyUp(e) {
    if (e.keyCode == 13) {
        $(this).trigger("enterKey");
    }
});

//add new tasks
$('#btnNewTask').on("click", function (event) {
    event.preventDefault();
    pageManageTasks.newTask_click();
});

//Toggle the grid view on a per field basis between expanded and collapsed
$(".te-grid").on("click", ".te-grid-height-toggle", function teGridHeightToggle(e) {
    $(this).closest('.field-div').find('.text').toggleClass(pageProps.heightClass.min);
    $(this).closest('.field-div').find('.text').toggleClass(pageProps.heightClass.max);
});

//To refresh the page for updates to actual results and execucution statuses
$('#dropdown-menu-refresh-btn').on("click", function PRESSME() {
   pageCallbacks.userRequestedRefresh(); 
});

//Keyboard shortcut events using moustreap
//For changing main display mode
Mousetrap.bind('d 1', function() { 
    if ($(window).width() > 501) {
        $('#lbl-display-tb_radio-splitView').trigger('click');
    }
}); 
Mousetrap.bind('d 2', function() { 
    if ($(window).width() > 501) {
        $('#lbl-display-tb_radio-gridView').trigger('click');
    }
});
Mousetrap.bind('d 3', function() { 
    if ($(window).width() > 501) {
        $('#lbl-display-tb_radio-miniView').trigger('click');
    }
});

//For changing sub display mode
Mousetrap.bind('d j', function() { 
    if ($(window).width() > 501) {
        if (teViewModel.vm.userSettingsDisplayModeInUse() == pageProps.displayMode.mini) {
            $('#lbl-btn-group-miniView_full').trigger('click');
        } else if (teViewModel.vm.userSettingsDisplayModeInUse() == pageProps.displayMode.grid) {
            $('#btn-group-gridView_expand').trigger('click');
        } else {
            teViewModel.vm.userSettingsDisplayModeInUse(pageProps.displayMode.split);
            $('#lbl-btn-group-splitView_inspectorLarge').trigger('click');;
        }
    }
});
Mousetrap.bind('d k', function() { 
    if ($(window).width() > 501) {
        if (teViewModel.vm.userSettingsDisplayModeInUse() == pageProps.displayMode.mini) {
            $('#lbl-btn-group-miniView_iframe').trigger('click');
        } else if (teViewModel.vm.userSettingsDisplayModeInUse() == pageProps.displayMode.grid) {
            $('#btn-group-gridView_collapse').trigger('click');
        } else {
            teViewModel.vm.userSettingsDisplayModeInUse(pageProps.displayMode.split);
            $('#lbl-btn-group-splitView_gridLarge').trigger('click');;
        }
    }
});

//Other UI functions
Mousetrap.bind('r', function() {
    pageCallbacks.userRequestedRefresh();
});
Mousetrap.bind('d d', function() {
    teViewModel.vm.userSettingsAlwaysShowTestRunDetails(
        !teViewModel.vm.userSettingsAlwaysShowTestRunDetails()
    );
    pageCallbacks.logDisplayOptions(true);
});
Mousetrap.bind('d c', function () {
    teViewModel.vm.userSettingsShowCustomProperties(
        !teViewModel.vm.userSettingsShowCustomProperties()
    );
    pageCallbacks.logDisplayOptions(true);
});
//Switch inspector tabs for results, attachments, incidents
Mousetrap.bind('d r', function() {
    $('#te-inspector-tab-buttons_actual-results').trigger('click'); 
});
Mousetrap.bind('d t', function () {
    $('#te-inspector-tab-buttons_tasks').trigger('click');
});
Mousetrap.bind('d a', function() {
    $('#te-inspector-tab-buttons_attachments').trigger('click'); 
});
Mousetrap.bind('d i', function() {
    $('#te-inspector-tab-buttons_incidents').trigger('click'); 
});

//Pause
Mousetrap.bind('shift+alt+p', function() {
    pageControls.playPauseChange();
});

//Add actual results
Mousetrap.bind('alt+r', function() {
    pageModelStatusUpdate.makeUserEnterActualResult();
});

//For updating a status
Mousetrap.bind('alt+f', function() {
    pageModelStatusUpdate.handleStatusUpdateAttempt(pageProps.statusEnum.fail, true);
});
Mousetrap.bind('alt+p', function() {
    pageModelStatusUpdate.handleStatusUpdateAttempt(pageProps.statusEnum.passed, true);
});
Mousetrap.bind('alt+c', function() {
    pageModelStatusUpdate.handleStatusUpdateAttempt(pageProps.statusEnum.caution, true);
});
Mousetrap.bind('alt+b', function() {
    pageModelStatusUpdate.handleStatusUpdateAttempt(pageProps.statusEnum.blocked, true);
});
Mousetrap.bind('alt+n', function() {
    pageModelStatusUpdate.handleStatusUpdateAttempt(pageProps.statusEnum.notApplicable, true);
});

//Navigating between steps
Mousetrap.bind('k', function() {
    pageModelMoveFocus.progressBarBack();
});
Mousetrap.bind('j', function() {
    pageModelMoveFocus.progressBarForward();
});
Mousetrap.bind('shift+j', function() {
    pageModelMoveFocus.runBack();
});
Mousetrap.bind('shift+k', function() {
    pageModelMoveFocus.runForward();
});

//Knockout view model
function testRunPendingViewModel() {
    var data = pageProps.data;
    var self = this;
    self.testRunsPendingId = data.TestRunsPendingId || '';
    self.projectId = data.ProjectID || '';
    self.testSetId = data.TestSetId || '';
    self.releaseId = data.ReleaseId || '';
    self.releaseVersion = data.ReleaseVersion || '';
    self.name = data.Name || '';
    self.countPassed = ko.observable(data.CountPassed || '');
    self.countFailed = ko.observable(data.CountFailed || '');
    self.countBlocked = ko.observable(data.CountBlocked || '');
    self.countCaution = ko.observable(data.CountCaution || '');
    self.countNotRun = ko.observable(data.CountNotRun || '');
    self.countNotApplicable = ko.observable(data.CountNotApplicable || '');
    
    //Set the user settings
    self.userSettingsDisplayModeInUse = ko.observable(data.Settings ? data.Settings[0].DisplayModeMain : 1);
    self.userSettingsDisplaySubModeInUse = ko.observable(data.Settings ? data.Settings[0].DisplayModeSub : 1);
    self.userSettingsAlwaysShowTestRunDetails = ko.observable(data.Settings ? data.Settings[0].AlwaysShowTestRun : false);
    self.userSettingsShowCustomProperties = ko.observable(data.Settings ? data.Settings[0].ShowCustomProperties : false);
    self.userSettingsGuidedTourSeen = ko.observable(data.Settings ? data.Settings[0].GuidedTourSeen : false);
    self.userSettingsCurrentRunId = ko.observable(data.Settings ? data.Settings[0].CurrentTestRunId : '' );
    self.userSettingsCurrentStepId = ko.observable(data.Settings ? data.Settings[0].CurrentTestRunStepId : '');

    self.userSettingsShowCustomProperties.subscribe(function (newVal) {
        pageCallbacks.logDisplayOptions(newVal);
        pageManageCustomProperties.getTestCaseAndStep();
    });
    self.userSettingsAlwaysShowTestRunDetails.subscribe(function (newVal) {
        pageCallbacks.logDisplayOptions(newVal);
    });
    
    self.userDisplayIsGrid = ko.computed(function() {
        return self.userSettingsDisplayModeInUse() == pageProps.displayMode.grid;
    });
        
    //Create empty KO array, loop over all test runs (cases) in the set and push them to the array
    self.testRun = ko.observableArray();
    pageModelUtilities.addSubArray(self.testRun, data.TestRun, testRunViewModel);

    //Allow knockout to make a control have focus
    self.chosenPendingStatusUpdate = ko.observable();
    self.chosenPendingStatusClass = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.chosenPendingStatusUpdate()).status;
    });
    self.chosenPendingStatusGlyph = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.chosenPendingStatusUpdate()).glyph;
    });
    self.chosenPendingWarningMessage = ko.pureComputed(function () {
        return pageModelUtilities.executionStatusProps(self.chosenPendingStatusUpdate()).message;
    });
    self.chosenPendingStatusName = ko.pureComputed(function () {
        return pageModelUtilities.executionStatusProps(self.chosenPendingStatusUpdate()).name;
    });

    self.actualResultNeedsFocus = ko.observable(false);
    self.enableActualResultInGrid = ko.computed(function() {
       return self.actualResultNeedsFocus() && (self.userSettingsDisplayModeInUse() == pageProps.displayMode.grid);
    });
    self.enableActualResultInInspector = ko.computed(function() {
       return self.userSettingsDisplayModeInUse() != pageProps.displayMode.grid;
    });

    self.showAddIncidentMessage = ko.observable(false);

    
    //To select a specific test run step
    self.chosenTestStep = ko.observable();
    self.chosenTestStepParent = ko.observable();
    self.chosenTestStepIncidents = ko.observableArray();

    ////Computeds to ascertain behaviour for moving to the previous or next test step
    //First get information about the arrays and current position of the chosen test set within them
    self.itemArrayProps = ko.computed(function () {
        if (self.chosenTestStep() && self.chosenTestStepParent()) {
            return {
                stepArrayLength: self.chosenTestStepParent().testRunStep().length,
                stepCurrentIndex: ko.utils.arrayIndexOf(self.chosenTestStepParent().testRunStep(), self.chosenTestStep()),
                runArrayLength: self.testRun().length,
                runCurrentIndex: ko.utils.arrayIndexOf(self.testRun(), self.chosenTestStepParent())
            };
        }
    });
    

    
    //Then determine whether the current step and run are the first or last in the respective arrays
    self.itemPositions = ko.computed(function () {
        if (self.chosenTestStep() && self.chosenTestStepParent()) {
            return {
                isFirstStepInRun: self.itemArrayProps().stepCurrentIndex === 0 ? true : false,
                isVeryFirstStep: self.itemArrayProps().stepCurrentIndex === 0 && self.itemArrayProps().runCurrentIndex === 0 ? true : false,
                isFirstRun: self.itemArrayProps().runCurrentIndex === 0 ? true : false,
                isLastStepInRun: self.itemArrayProps().stepCurrentIndex === (self.itemArrayProps().stepArrayLength - 1) ? true : false,
                isVeryLastStep: self.itemArrayProps().stepCurrentIndex === (self.itemArrayProps().stepArrayLength - 1) && self.itemArrayProps().runCurrentIndex === (self.itemArrayProps().runArrayLength - 1) ? true : false,
                isLastRun: self.itemArrayProps().runCurrentIndex === (self.itemArrayProps().runArrayLength - 1) ? true : false
            };
        }
    });
    
    self.displayTestRunInInspector = ko.computed(function () {
        if (self.chosenTestStep() && self.chosenTestStepParent()) {
            return (self.itemPositions().isFirstStepInRun || self.userSettingsAlwaysShowTestRunDetails() ) ? true : false; 
        }
    });
    
    self.userSettingsShowCustomPropertiesOfTestCase = ko.computed(function() {
       return  self.userSettingsShowCustomProperties() && self.displayTestRunInInspector();
    });
    
    //The progress bar forward and back buttons are enabled or disabled based on if the selected item is the last step / run (based on which is displayed in the progress bar)
    self.progressBarPosition = ko.computed(function () {
        if (self.chosenTestStep() && self.chosenTestStepParent()) {
            return {
                atStart: self.progressBarDisplaySteps() ? self.itemPositions().isVeryFirstStep : self.itemPositions().isFirstRun,
                atEnd: self.progressBarDisplaySteps() ? self.itemPositions().isVeryLastStep : self.itemPositions().isLastRun
            };
        } else {
            return {
                atStart: true,
                atEnd: true
            }
        }
    });

    //Calculate the total number of test runs and test steps in the test set
    self.totalTestRuns = ko.pureComputed(function () {
        return data.TestRun.length;
    });
    self.totalTestSteps = ko.pureComputed(function () {
        var count = 0;
        ko.utils.arrayForEach(data.TestRun, function (item) {
            count += item.TestRunStep.length;
        });
        return count;
    });

    //Calculate the counts of all statuses
    self.liveCountFail = ko.computed(function() {
        return pageModelUtilities.sumInArray(self.testRun(), 'liveCountFail');
    });
    self.liveCountPassed = ko.computed(function() {
        return pageModelUtilities.sumInArray(self.testRun(), 'liveCountPassed');
    });
    self.liveCountNotRun = ko.computed(function() {
        return pageModelUtilities.sumInArray(self.testRun(), 'liveCountNotRun');
    });
    self.liveCountNotApplicable = ko.computed(function() {
        return pageModelUtilities.sumInArray(self.testRun(), 'liveCountNotApplicable');
    });
    self.liveCountBlocked = ko.computed(function() {
        return pageModelUtilities.sumInArray(self.testRun(), 'liveCountBlocked');
    });
    self.liveCountCaution = ko.computed(function() {
        return pageModelUtilities.sumInArray(self.testRun(), 'liveCountCaution');
    });
    self.liveTotalRun = ko.computed(function() {
       return  self.liveCountFail() + self.liveCountPassed() + self.liveCountNotApplicable() + self.liveCountBlocked() + self.liveCountCaution();
    });
    self.allRunsHaveBeenRun = ko.computed(function() {
        return pageModelUtilities.countInArray(self.testRun(), 'executionStatusId', pageProps.statusEnum.notRun) === 0;
    });
    
    //Manage the progress bar display
    self.maxStepsToShowInProgressBar = 50;
    self.progressBarDisplaySteps = ko.pureComputed(function () {
        var showSteps = true;
        if (self.totalTestSteps() > self.maxStepsToShowInProgressBar && self.totalTestSteps() > self.totalTestRuns()) {
            showSteps = false;
        }
        return showSteps;
    });

    // control the busy status of the app to help manage what is enabled / disabled
    self.appIsBusy = ko.observable(false);


    /*
     * TASK FIELDS
     */
    self.taskName = ko.observable("");
    self.taskDescription = ko.observable("");
    self.hasTaskName = ko.computed(function () {
        return self.taskName() != "";
    });

    //Create empty KO array for existing tasks / newly created tasks
    self.tasks = ko.observableArray();


    //managing CK editor
    self.isEditorAndModelSynced = ko.observable(true);


    //Manage Callbacks
    self.chosenLinkedIncidentsSuccess = ko.observable(false);
};
function testRunViewModel(testRunObj) {
    var self = this;
    self.testCaseId = testRunObj.TestCaseId || '';
    self.testRunId = testRunObj.TestRunId || '';
    self.releaseId = testRunObj.ReleaseId || '';
    self.testerId = testRunObj.TesterId || '';
    self.name = testRunObj.Name || '';
    self.description = testRunObj.Description || '';
    self.executionStatusId = ko.observable(testRunObj.ExecutionStatusId || '');
    self.startDate = ko.observable(globalFunctions.parseJsonDate(testRunObj.StartDate) || '');
    self.endDate = ko.observable(globalFunctions.parseJsonDate(testRunObj.EndDate) || '');
    self.actualDuration = ko.observable(testRunObj.ActualDuration || '');
    
    //Computed function to check if logged in user can take actions on test run
    self.cannotEdit = ko.pureComputed(function () {
       return self.testerId != _pageInfo.userId; 
    });

    //Computed functions that set display classes based on current execution status
    self.executionStatusClass = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.executionStatusId()).status;
    });
    self.executionStatusGlyph = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.executionStatusId()).status;
    });
    self.executionStatusIcon = ko.computed(function () {
        return self.executionStatusClass() ? ('fas fa-sticky-note ' + self.executionStatusClass()) : 'far fa-sticky-note';
    });

    //Create empty KO array, loop over all test steps in the run / case and push them to the array
    self.testRunStep = ko.observableArray();
    pageModelUtilities.addSubArray(self.testRunStep, testRunObj.TestRunStep, testRunStepViewModel, testRunObj.TestRunId, self.cannotEdit());
    
    self.countSteps = ko.computed(function () {
        return self.testRunStep().length;
    });
    self.liveCountFail = ko.computed(function() {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.fail);
    });
    self.liveCountPassed = ko.computed(function() {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.passed);
    });
    self.liveCountNotRun = ko.computed(function() {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.notRun);
    });
    self.liveCountNotApplicable = ko.computed(function() {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.notApplicable);
    });
    self.liveCountBlocked = ko.computed(function() {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.blocked);
    });
    self.liveCountCaution = ko.computed(function() {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.caution);
    });
};
function testRunStepViewModel(testRunStepObj) {
    var self = this;
    self.testRunStepId = testRunStepObj.TestRunStepId || '';
    self.testStepId = testRunStepObj.TestStepId || '';
    self.testCaseId = testRunStepObj.TestCaseId || '';
    self.testRunStepParentId = testRunStepObj.ParentId || '';
    self.description = testRunStepObj.Description || '';
    self.position = testRunStepObj.Position || '';
    self.positionTitle = ko.computed(function () {
        return resx.TestStep_Step + ' ' + self.position;
    });
    self.expectedResult = testRunStepObj.ExpectedResult || '';
    self.sampleData = testRunStepObj.SampleData || '';
    self.actualResult = ko.observable(testRunStepObj.ActualResult || '');
    self.executionStatusId = ko.observable(testRunStepObj.ExecutionStatusId || '');
    self.startDate = ko.observable(globalFunctions.parseJsonDate(testRunStepObj.StartDate) || '');
    self.endDate = ko.observable(globalFunctions.parseJsonDate(testRunStepObj.EndDate) || '');
    self.actualDuration = ko.observable(testRunStepObj.ActualDuration || '');
    self.cannotEdit = testRunStepObj.PassedOnField; 

    //Computed functions that set display classes based on current execution status
    self.executionStatusClass = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.executionStatusId()).status;
    });
    self.executionStatusGlyph = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.executionStatusId()).glyph;
    });
    self.executionStatusIcon = ko.computed(function () {
        return self.executionStatusClass() ? ('fas fa-circle ' + self.executionStatusClass()) : 'far fa-circle';
    });
};

function incidentsViewModel(incidentObj) {
    var self = this;
    self.name = incidentObj.Fields.Name.textValue || '';
    //self.description = incidentObj.Fields.Description.textValue || '';
    self.incidentId = incidentObj.Fields.IncidentId.textValue || '';
    self.incidentIdIntValue = incidentObj.Fields.IncidentId.intValue || '';
    self.incidentType = incidentObj.Fields.IncidentTypeId.textValue || '';
    self.incidentStatus = incidentObj.Fields.IncidentStatusId.textValue || '';
    self.priorityId = incidentObj.Fields.PriorityId || '';
    self.priorityValue = incidentObj.Fields.PriorityId.textValue || '';
    self.priorityClass = incidentObj.Fields.PriorityId.cssClass || '';
    self.severityId = incidentObj.Fields.SeverityId || '';
    self.severityValue = incidentObj.Fields.SeverityId.textValue || '';
    self.severityClass = incidentObj.Fields.SeverityId.cssClass || '';
    self.owner = incidentObj.Fields.OwnerId.textValue || '';
    self.creationDate = incidentObj.Fields.CreationDate.textValue || '';
    self.opener = incidentObj.Fields.OpenerId.textValue || '';
};


function tasksViewModel(taskObj) {
    var self = this;
    self.name = taskObj.Fields.Name.textValue || '';
    self.taskId = taskObj.Fields.TaskId.intValue || '';
    self.taskToken = taskObj.Fields.TaskId.textValue || '';
    self.owner = taskObj.Fields.OwnerId.textValue || '';
    self.type = taskObj.Fields.TaskTypeId.textValue || '';
    self.status = taskObj.Fields.TaskStatusId.textValue || '';
    self.creator = taskObj.Fields.CreatorId.textValue || '';
    self.priorityColor = taskObj.Fields.TaskPriorityId.cssClass || '';
    self.creationDate = taskObj.Fields.CreationDate.textValue || '';
    self.url = SpiraContext.BaseUrl + SpiraContext.ProjectId + "/Task/" + self.taskId + ".aspx"
};

//Custom Bindings
//Stopping the current view model from applying bindings to the node and descendants this binding is applied to 
ko.bindingHandlers.stopBinding = {
    init: function() {
        return { controlsDescendantBindings: true };
    }
};
//For creating a ckEditor rich text box from a textarea
ko.bindingHandlers.richText = {
    init: function (element, valueAccessor, allBindings, bindingContext) {

        var fieldName = allBindings.get("field") || false,
            isTestRun = allBindings.get("isTestRun");

        //first, as this is a custom binding, we need to manually set the dom val of the element to the value of the model
        //use ko.unwrap to make sure to get the plain, not observable, value - ie the actual html
        $(element).val(ko.unwrap(valueAccessor()));

        //now create the ckeditor using the general options for all editors on the apge
        $(element).ckeditor(pageCKE.options);

        //with the element created we can get the specific editor object
        var txtBoxID = $(element).attr("id");
        var editor = CKEDITOR.instances[txtBoxID];

        //set the initial upload URL
        editor.config.uploadUrl = pageProps.screenshotUploadUrl;

        //this let's us create event listeners so that every time the editor gains/loses focus the knockout model is updated
        //this uses the CKEDitor built in events
        editor.on('focus', function (e) {
            //reset here so that we are sure that only changes made in the UI (not from navigating steps) set the dirty flag
            editor.resetDirty();

            teViewModel.vm.isEditorAndModelSynced(false);

            // update the uploadUrl - all fields are attached to the test run so run update if we have changed test run
            // NOTE: the same url is going to be used by any tasks that are to be created - but this seems OK as we are adding the screenshot before we have the task itself created
            var chosenTestRunId = teViewModel.vm.chosenTestStepParent().testRunId,
                haveChangedTestRun = pageProps.previousChosen.testRunId != chosenTestRunId;
            if (haveChangedTestRun) {
                editor.config.uploadUrl = pageProps.screenshotUploadUrl;
            }
        });

        editor.on('blur', function (e) {
            //ordering of the checkDirty and resetDirty in the function seems to be important. Unless they are up top in the function they would not always register, leading to too many false positives (ie server calls)

            // see if the ckeditor has changed before doing anything else 
            var isDirty = false;
            try {
                isDirty = editor.checkDirty();
            } catch (e) {
                //fail quietly
            }

            //reset the editor so that only future changes are marked as dirty to minimise server calls
            editor.resetDirty();

            //update the model - make sure to pick the right part of the model.
            if (isTestRun) {
                teViewModel.vm[fieldName]($(element).val());
            } else {
                teViewModel.vm.chosenTestStep()[fieldName]($(element).val());
            }
            teViewModel.vm.isEditorAndModelSynced(true);

            //if there are changes we want to send only the actual result field to the server
            if (isDirty && fieldName == "actualResult") {
                var chosenStep = teViewModel.vm.chosenTestStep();

                //update the model

                //update the server with the same
                pageCallbacks.UpdateTestRunActualResult(
                    chosenStep.testRunStepId,
                    chosenStep.actualResult()
                )
            }
        });

        //handle disposal (if KO removes by the template binding)
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            // pageCKE.destroy();
        });
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var val = ko.utils.unwrapObservable(valueAccessor());
        $(element).val(val);
    }
};


/*
 * Hopscotch product tour
 */
// Define the tour!
var teTour = {
    id: "testExecutionGuidedTour",
    showCloseButton: true,
    onStart: function() {
        teViewModel.vm.userSettingsGuidedTourSeen(true);   
    },
    i18n: {
        nextBtn: resx.Guide_Global_Next,
        prevBtn: resx.Guide_Global_Previous,
        doneBtn: resx.Guide_Global_Done,
        skipBtn: resx.Guide_Global_Skip,
        closeTooltip: resx.Guide_Global_Close,
    },
    steps: [
    {
        title: resx.Guide_TestExecution_intro_Title,
        content: resx.Guide_TestExecution_intro_Content,
        target: "te-header",
        placement: "bottom"
    },
    {
        title: resx.Guide_TestExecution_heading_Title,
        content: resx.Guide_TestExecution_heading_Content,
        target: "te-title-small",
        placement: "right",
        yOffset: "-20"
    },
    {
        title: resx.Guide_TestExecution_display_Title,
        content: resx.Guide_TestExecution_display_Content,
        target: "te-display-toolbar",
        placement: "left",
        onNext: function() {
            $('#lbl-display-tb_radio-splitView').click();
        }
    },
    {
        title: resx.Guide_TestExecution_split_Title,
        content: resx.Guide_TestExecution_split_Content,
        target: "display-tb_radio-splitView",
        placement: "left",
        yOffset: "-20",
        xOffset: "-20",
        onNext: function() {
            $('#lbl-display-tb_radio-gridView').click();
        }
        
    },
    {
        title: resx.Guide_TestExecution_table_Title,
        content: resx.Guide_TestExecution_table_Content,
        target: "display-tb_radio-gridView",
        placement: "left",
        yOffset: "-20",
        xOffset: "-20",
        showPrevButton: true,
        onNext: function() {
            $('#lbl-display-tb_radio-miniView').click();
        },
        onPrev: function() {
            $('#lbl-display-tb_radio-splitView').click();
        }
    },
    {
        title: resx.Guide_TestExecution_mini_Title,
        content: resx.Guide_TestExecution_mini_Content,
        target: "display-tb_radio-miniView",
        placement: "left",
        yOffset: "-20",
        xOffset: "-20",
        showPrevButton: true,
        showNextButton: true,
        showCTAButton: false,
        ctaLabel: resx.Guide_Global_Next,
        onNext: function() {
            $('#lbl-display-tb_radio-splitView').click();
        },
        onPrev: function() {
            $('#lbl-display-tb_radio-gridView').click();
        }
    },
    {
        title: resx.Guide_TestExecution_progress_Title,
        content: resx.Guide_TestExecution_progress_Content,
        target: "te-progress-bar",
        placement: "bottom",
        xOffset: "center",
        arrowOffset: "center"
    },
    {
        title: resx.Guide_TestExecution_moving_Title,
        content: resx.Guide_TestExecution_moving_Content,
        target: "pb-btn-previous",
        placement: "bottom"
    },
    {
        title: resx.Guide_TestExecution_pause_Title,
        content: resx.Guide_TestExecution_pause_Content,
        target: "pb-btn-play-pause",
        placement: "right",
        showNextButton: false,
        showCTAButton: true,
        ctaLabel: resx.Guide_Global_Next,
        onCTA: function() {
            $('#lbl-display-tb_radio-splitView').click();
            setTimeout(function() {
                hopscotch.nextStep();
            }, 300);
        }
    },
    {
        title: resx.Guide_TestExecution_status_Title,
        content: resx.Guide_TestExecution_status_Content,
        target: "te-inspector-execution-status-btn-grp",
        placement: "bottom",
        arrowOffset: "center",
        showNextButton: false,
        showCTAButton: true,
        ctaLabel: resx.Guide_Global_Next,
        onCTA: function() {
            $('#lbl-display-tb_radio-splitView').click();
            setTimeout(function() {
                hopscotch.nextStep();
            }, 300);
        }
    },
    {
        title: resx.Guide_TestExecution_screenshot_Title,
        content: resx.Guide_TestExecution_screenshot_Content,
        target: "te-inspector_tab_actual-results",
        placement: "top",
        arrowOffset: "center"
    },
    {
        title: resx.Guide_TestExecution_settings_Title,
        content: resx.Guide_TestExecution_settings_Content,
        target: "te-display-toolbar-options",
        placement: "left"
    },
    {
        title: resx.Guide_TestExecution_exit_Title,
        content: resx.Guide_TestExecution_exit_Content,
        target: "te-end-buttons-marker",
        placement: "left",

    }
    ]
};
