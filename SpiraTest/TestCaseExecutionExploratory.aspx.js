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
    associationWithoutPanel: true,
    isTestExecution: true,
    oneRem: 24,
    glyphs: {
        play: 'fa-play',
        pause: 'fa-pause',
        leave: 'fa-eject',
        end: 'fa-stop'
    },
    partClass: {
        grid: 'te-grid',
        inspector: 'te-inspector',
    },
    editableFields: {
        testRunName: 1,
        testRunDescription: 2,
        testStepDescription: 3,
        testStepExpectedResult: 4,
        testStepSampleData: 5,
        testStepActualResult: 6
    },
    controls: {
        $playPause: $('#pb-btn-play-pause')
    },
    caseNameId: 'txtName',
    caseDescriptionId: 'txtDescription',
    stepDescriptionId: 'testRunStepDescription',
    expectedResultId: 'testRunStepExpectedResult',
    sampleDataId: 'testRunStepSampleData',
    actualResultId: 'testRunStepActualResult',
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
    ajxService: Inflectra.SpiraTest.Web.Services.Ajax.TestExecutionExploratoryService,
    canViewTasks: _pageInfo.allowTasks
        && globalFunctions.isAuthorized(globalFunctions.permissionEnum.Create, globalFunctions.artifactTypeEnum.task) == globalFunctions.authorizationStateEnum.authorized
        && globalFunctions.isAuthorized(globalFunctions.permissionEnum.View, globalFunctions.artifactTypeEnum.task) == globalFunctions.authorizationStateEnum.authorized,
    canViewIncidents: globalFunctions.isAuthorized(globalFunctions.permissionEnum.View, globalFunctions.artifactTypeEnum.incident) == globalFunctions.authorizationStateEnum.authorized,
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
                if (ajxTestRunFormManager && ajxTestRunFormManager.get_dataItem())
                {
                    var dataItem = ajxTestRunFormManager.get_dataItem();
                    if (dataItem.Fields.BuildId)
                    {
                        var buildId = dataItem.Fields.BuildId.intValue;
                        if (buildId)
                        {
                            ddlBuild.set_selectedItem(buildId);
                        }
                        else
                        {
                            ddlBuild.set_selectedItem('');
                        }
                    }
                    else
                    {
                        ddlBuild.set_selectedItem('');
                    }
                }
                else
                {
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

    //Adjusts the width of label and field elements inside the inspector as it dynamically changes its width
    //Special account is taken of dataLabels/dataEntries from the incident form
    checkInspectorWidth: function () {
        $('#' + pageProps.partClass.inspector).on('transitionend webkitTransitionEnd oTransitionEnd otransitionend MSTransitionEnd',
            function (event) {
                if (event.target !== this) {
                    return;
                }
                // for unity wrappers with boxes
                uWrapper_onResize();
            }
        );
    },

    /*
     * Functions for the option buttons
     */
    //toggle which button is shown 
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
        $('#' + pageProps.partClass.grid + ' .border').css('max-height', 'calc(100vh - 20px - ' + parseInt($('#' + pageProps.partClass.grid).offset().top) + 'px');
    },
    //Setting scrolling on the grid (only if not in mobile phone view)
    scrollGridToCurrentStep: function () {
        if ($(window).width() > 501) {
            //scroll to the case if cases are shown in the progress bar, steps if steps are being displayed
            var scrollTarget = ' .js-grid-row.selected',
                scrollTo = $('#' + pageProps.partClass.grid + scrollTarget),
                container = $('#' + pageProps.partClass.grid + ' .border');

            container.animate({
                scrollTop: scrollTo.offset().top - container.offset().top + container.scrollTop()
            }, 200);
        }
    },

    //group function to manage clicking on the tab controls in the inspector to show the incident form
    handleIncidentTabClick: function() {
        pageCallbacks.getChosenIncidents();
        setTimeout(function () { uWrapper_onResize(); }, 0);
        
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
    addSubObject: function (receivingArray, sendingObject, childViewModel, parentId, passedOnField) { 
        receivingArray.push(new childViewModel(sendingObject));
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
    arrayIndexOfByField: function(array, field, fieldInArray, fieldIsObservable) {
        for (var i = 0, j = array.length; i < j; i++) {
            if(fieldIsObservable) {
                if(array[i][fieldInArray]() === field) {
                    return i;
                }
            } else {
                if(array[i][fieldInArray] === field) {
                    return i;
                }
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
        var glyph, status, status_bg, message, name;
        switch (input) {
            case pageProps.statusEnum.fail: 
                glyph = 'fas fa-times';
                status = 'ExecutionStatusFailed_color-pale';
                status_bg = 'ExecutionStatusFailed';
                message = resx.TestCaseExecution_ActualResultNeeded;
                name = resx.TestCaseExecution_Fail;
                break;
            case pageProps.statusEnum.passed: 
                glyph = 'fas fa-check';
                status = 'ExecutionStatusPassed_color-pale';
                status_bg = 'ExecutionStatusPassed';
                message = resx.TestCaseExecution_ActualResultNeeded;
                name = resx.TestCaseExecution_Pass;
                break;
            case pageProps.statusEnum.notRun: 
                glyph = '';
                status = '';
                break;
            case pageProps.statusEnum.notApplicable:
                glyph = 'fas fa-minus';
                status = 'ExecutionStatusNotApplicable_color-pale';
                status_bg = 'ExecutionStatusNotApplicable';
                message = resx.TestCaseExecution_ActualResultNeeded;
                name = resx.TestCaseExecution_NotApplicable;
                break;
            case pageProps.statusEnum.blocked:
                glyph = 'fas fa-ban';
                status = 'ExecutionStatusBlocked_color-pale';
                status_bg = 'ExecutionStatusBlocked';
                message = resx.TestCaseExecution_ActualResultNeeded;
                name = resx.TestCaseExecution_Blocked;
                break;
            case pageProps.statusEnum.caution: 
                glyph = 'fas fa-exclamation-triangle';
                status = 'ExecutionStatusCaution_color-pale';
                status_bg = 'ExecutionStatusCaution';
                message = resx.TestCaseExecution_ActualResultNeeded;
                name = resx.TestCaseExecution_Caution;
                break;
            default: 
                glyph = '';
                status = '';
                status_bg = '';
                message = '';
                name = '';
        }
        return {"glyph": glyph, "status": status, "status_bg": status_bg, "message": message, "name": name};
    }
};





var pageCKE = {
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
		disallowedContent: 'img{width,height}',
		disableNativeSpellChecker: false,
        height: 40
    },
    //the most reliable way to make sure all the text editors read only status is to do it editor by editor - can't do this in custom binding
    setReadOnlyOnTestRunSteps: function(shouldBeReadOnly) {
        var isReadOnly = shouldBeReadOnly || teViewModel.vm.chosenTestStep().isReadOnly();
        CKEDITOR.instances[pageProps.stepDescriptionId].setReadOnly(isReadOnly);
        CKEDITOR.instances[pageProps.expectedResultId].setReadOnly(isReadOnly);
        CKEDITOR.instances[pageProps.sampleDataId].setReadOnly(isReadOnly);
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
            testStepIndex = 0;
        
        //If we can turn user setting IDs into positions in the current array for navigation purposes
        if(self.userSettingsCurrentStepId()) {
            var testStepIndexOf = pageModelUtilities.arrayIndexOfByField(self.testRunStep(), self.userSettingsCurrentStepId(), 'testRunStepId'),
                testStepIndex = testStepIndexOf === -1 ? 0 : testStepIndexOf;
        }
        try {
            self.chosenTestStep(self.testRunStep()[testStepIndex]);
            //set the attachment panel's test run id so attachments can be added from all views
            pageManageAttachments.linkAttachmentsToChosenStep();
            pageManageAttachments.getChosenAttachments();

            //make sure the CK editors for the chosen test step are set to read only if needed - only once ckeditor is actually ready. This will likely be after the model is initiated
            //pass in true/false here to avoid any timing issues
            CKEDITOR.on( 'instanceReady', function( evt ) {
                pageCKE.setReadOnlyOnTestRunSteps(self.chosenTestStep().isReadOnly());
            } );

        } catch(e) {};
        //Handle timing issues
        pageTimeManagement.setLocalStart(function(){
            pageTimeManagement.setActualStart()
        });

        //make sure custom fields are returned to show in the UI
        pageManageCustomProperties.getTestCaseAndStep();
    },
    isMovingStepAllowed: function () {
        var result = true;
        pageModelStatusUpdate.syncViewModelAndCKEditors( function () {
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
                pageModelSetChosen.manageChosenStepChange(that.testRunStepId);
                pageModelMoveFocus.resetChosenStepSettingsOnMove();
            }
        } else {
            pageModelStatusUpdate.handleStatusUpdateAttempt(teViewModel.vm.chosenTestStep().executionStatusId());
        };
    },
    
    //The function that carries out the search and sets required observables
    setNewStep: function (newTestRunStepId) {
        var self = teViewModel.vm,
        stepToChoose = self.testRunStep().filter(function(step) {
            return step.testRunStepId === newTestRunStepId;
        })[0];
        //Set the new chosen step
        self.chosenTestStep(stepToChoose);
        
        //Set the chosen run / step ids as the latest position
        self.userSettingsCurrentStepId(self.chosenTestStep().testRunStepId);
		
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

        //make sure the CK editors for the chosen test step are set to read only if needed
        pageCKE.setReadOnlyOnTestRunSteps();

        //Update custom properties as required
        pageManageCustomProperties.getTestStep();

        //reset the associate incident panel
        pageManageIncidentForm.closeIncidentAssociationPanel();

        //and then get the latest list of incidents
        self.chosenLinkedIncidentsSuccess(false);
        pageCallbacks.getChosenIncidents();
    },
    //Wrapper function to manage the different functions and callbacks
    manageChosenStepChange: function (newTestRunStepId, callback) {
        
        //deal with time management
        if(!pageProps.timeManagement.skipTimeUpdate) {
            if(!pageProps.timeManagement.isPaused) {
                pageTimeManagement.setAllEndTimes();
            }
        }
        pageModelStatusUpdate.syncViewModelAndCKEditors( function() { 
            pageModelSetChosen.setNewStep(newTestRunStepId);
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
        pageModelStatusUpdate.syncViewModelAndCKEditors( function () {
            //make sure an actual result is entered if one is required 
            var self = teViewModel.vm,
                actualResultIsEmpty = globalFunctions.trim(self.chosenTestStep().actualResult()) == '' ? true : false,
                currentStatus = pageProps.temp.statusId,
                stepHasExecutionAttempt = currentStatus != 3,
                // actual result required if the project settings say so, or if the status is not pass
                actualResultRequired = stepHasExecutionAttempt && (_pageInfo.actualResultAlwaysRequired || currentStatus != 2);

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
    handlePassAll: function (moveForward) {
        pageProps.temp.statusId = pageProps.statusEnum.passed;
        pageProps.temp.isPassAll = true;
        pageProps.temp.shouldMoveForward = moveForward || false;
        
        //make sure the actual result has been synced to the model
        pageModelStatusUpdate.syncViewModelAndCKEditors(
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
    syncViewModelAndCKEditors: function (callback) {
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
            var checkSyncStatus = setInterval(function(){
                if (teViewModel.vm.isEditorAndModelSynced()) {
                    callback && callback();
                    clearInterval(checkSyncStatus);
                } else {
                    counter++;
                }

                if(counter === 5) {
                    clearInterval(checkSyncStatus);
                    //no callback to make sure wrong data is not sent anywhere
                    //display error
                    globalFunctions.display_error($get(_pageInfo.clientId), resx.Global_PleaseTryAgain);
                    console.warn("The ckeditor was delayed in synchronising with the model, as monitored by the function 'syncViewModelAndCKEditors'");
                }
            }, 100);
        }

    },
    openActualResult: function() {
        teViewModel.vm.actualResultNeedsFocus(true);
        //set focus on the actual result with a small delay to make sure the cursor gets positioned
        setTimeout(function() {
            CKEDITOR.instances[pageProps.actualResultId].focus();
        }, 100);
    }
};




//Move to the previous or next test step
var pageModelMoveFocus = {
    //Send the correct new array object as the selected test set step
    stepForward: function () {
        var self = teViewModel.vm;
        var currentStepIndex = self.itemArrayProps().stepCurrentIndex,
            newStepIndex = currentStepIndex + 1;
        if (pageModelSetChosen.isMovingStepAllowed()) {
            if (self.itemPositions().isVeryLastStep) {
                pageModelMoveFocus.resetChosenStepSettingsOnMove();
                return;
            } else {
                pageModelSetChosen.manageChosenStepChange(self.testRunStep()[newStepIndex].testRunStepId);
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
            newStepIndex = currentStepIndex == 0 ? 0 : currentStepIndex - 1;
        if (pageModelSetChosen.isMovingStepAllowed()) {
            if (self.itemPositions().isVeryFirstStep) {
                pageModelMoveFocus.resetChosenStepSettingsOnMove();
                return;
            } else {
                pageModelSetChosen.manageChosenStepChange(self.testRunStep()[newStepIndex].testRunStepId);
            }
            pageControls.scrollGridToCurrentStep();
            pageModelMoveFocus.resetChosenStepSettingsOnMove();
        } else {
            pageModelStatusUpdate.handleStatusUpdateAttempt(teViewModel.vm.chosenTestStep().executionStatusId());
        }
    },
    //set which type of action the progress bar should have (based on what is displayed in the progress bar)
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
        $('#te-inspector-tab-buttons_tasks').trigger('click');
        $('#te-inspector_tab_tasks').addClass('active');
        
        //Reset timing parameters
        pageProps.timeManagement.duration = 0;
        pageProps.timeManagement.skipTimeUpdate = false;

        
    }
};



//Moving, changing, creating, deleting, and cloning test run steps
var pageStepCRUD = {
    deleteStep: function(testRunStepId) {
        var stepsArray = teViewModel.vm.testRunStep,
            stepIndex = pageModelUtilities.arrayIndexOfByField(stepsArray(), testRunStepId, 'testRunStepId');
        
        //only let the user delete the test run step if it is not the only step left
        //knockout checks this anyway in the UI so this function should never run if this case is not met, but you never know
        if(stepsArray().length > 1) {

            //confirm that the user is content to delete
            if (window.confirm(resx.TreeView_ConfirmDelete.replace('{0}', resx.ArtifactType_TestStep))) {

                //remove the step for deletion (optimistically)
                stepsArray.splice(stepIndex, 1);

                //set the new chosen step
                var newStepIndex = teViewModel.vm.testRunStep().length == stepIndex ? stepIndex - 1 : stepIndex;
                stepToMoveTo = teViewModel.vm.testRunStep()[newStepIndex];
                pageModelSetChosen.selectTest(stepToMoveTo)


                //update the database on the server
                var projectId = _pageInfo.projectId,
                    testRunId = teViewModel.vm.testRunId,
                    testRunsPendingId = teViewModel.vm.testRunsPendingId;
                
                //make the actual call to the server
                pageProps.ajxService.DeleteExploratoryTestRunStep (
                    projectId, 
                    testRunsPendingId, 
                    testRunId, 
                    testRunStepId,
                    pageStepCRUD.deleteTestRunStep_success, 
                    pageStepCRUD.deleteTestRunStep_failure
                );

                pageStepCRUD.updateStepNumbers();
            }

        } else {
            alert(resx.TestExecutionService_ExploratoryDoNotDeleteLastStep);
        }
    },
    deleteTestRunStep_success: function (newExecutionStatusId) {
        //update the execution status using the value calculated and sent by the server
        teViewModel.vm.testRunExecutionStatusId(newExecutionStatusId)
    },
    deleteTestRunStep_failure: function() {
        //fail quietly
        console.log('deleteTestRunStep_failure')
    },

    //creating a new test run step
    createNewTestRunStep: function() {
        //only allow the creation of a new test step one at a time - ie not if one is in the process of being added
        if (!teViewModel.vm.isAdding() ) {
            var projectId = _pageInfo.projectId,
                testRunId = teViewModel.vm.testRunId,
                testCaseId = teViewModel.vm.testCaseId,
                testRunsPendingId = teViewModel.vm.testRunsPendingId;

            //update the state so the UI can show that an addition is in progress
            teViewModel.vm.isAdding(true);
    
            pageProps.ajxService.CreateNewExploratoryTestRunStep (
                projectId, 
                testRunsPendingId, 
                testRunId,
                testCaseId,
                pageStepCRUD.createNewTestRunStep_success, 
                pageStepCRUD.createNewTestRunStep_failure
            );
        }

    },
    createNewTestRunStep_success: function(data) {
        //create a generic test run step object
        var newStepObject = {
            TestCaseId: teViewModel.vm.testCaseId,
            TestRunStepId: data,
            ParentId: teViewModel.vm.testCaseId,
            Description: resx.Global_Description,
            ExecutionStatusId: pageProps.statusEnum.notRun
        };
        
        //add it to the model
        pageModelUtilities.addSubObject(
            teViewModel.vm.testRunStep, 
            newStepObject, 
            testRunStepViewModel, 
            self.testCaseId
            );

        //Reset the execution status of the entire test run - only required if the run execution status was PASSED - as adding a step means not all steps are passed
        if (teViewModel.vm.testRunExecutionStatusId() == pageProps.statusEnum.passed) {
            teViewModel.vm.testRunExecutionStatusId(pageProps.statusEnum.notRun)
        }
        
        //update the state so the UI can show addition is complete
        teViewModel.vm.isAdding(false);

        //refresh the step numbers - probably unecessary but just in case
        pageStepCRUD.updateStepNumbers();

        //set the new chosen step to the added one - ie the last one
        pageStepCRUD.moveToLastStepInList();
    },
    createNewTestRunStep_failure: function() {
        //update the state so the UI can show addition is complete
        teViewModel.vm.isAdding(false);

        //fail quietly
        console.log('createNewTestRunStep_failure')
    },



    cloneStep: function(stepId) {
        
        var self = teViewModel.vm,
            stepToClone = self.testRunStep().filter(function(step) {
                return step.testRunStepId === stepId
            })[0],
            indexOfClone = self.testRunStep().indexOf(stepToClone),
            clonedStep = {
                TestCaseId: stepToClone.testCaseId,
                ParentId: stepToClone.exploratoryTestCaseId,
                Description: stepToClone.description(),
                ExpectedResult: stepToClone.expectedResult(),
                SampleData: stepToClone.sampleData(),
                ExecutionStatusId: pageProps.statusEnum.notRun,
                indexOfClone: indexOfClone
            };
        
        //make sure people can't accidentally click clone twice while waiting on server
        self.testRunStep()[indexOfClone].isBeingCloned(true);
        
        pageProps.ajxService.CloneExploratoryTestRunStep (
            _pageInfo.projectId, 
            self.testRunsPendingId, 
            self.testRunId,
            clonedStep.TestCaseId,
            clonedStep.Description,
            clonedStep.ExpectedResult,
            clonedStep.SampleData,
            pageStepCRUD.cloneStep_success, 
            pageStepCRUD.cloneStep_failure,
            clonedStep
        );
    },
    cloneStep_success: function(data, clonedStep) {
        //reenable the clone button for the relevant test run step
        var self = teViewModel.vm;
        self.testRunStep()[clonedStep.indexOfClone].isBeingCloned(false);

        //update the cloned object with the correct ID from the server
        clonedStep.TestRunStepId =  data;
        
        //add it to the model
        pageModelUtilities.addSubObject(
            self.testRunStep, 
            clonedStep, 
            testRunStepViewModel, 
            self.testCaseId
            );

        //Reset the execution status of the entire test run - only required if the run execution status was PASSED - as adding a step means not all steps are passed
        if (teViewModel.vm.testRunExecutionStatusId() == pageProps.statusEnum.passed) {
            teViewModel.vm.testRunExecutionStatusId(pageProps.statusEnum.notRun)
        }

        //refresh the step numbers - probably unecessary but just in case
        pageStepCRUD.updateStepNumbers();

        //set the new chosen step to the added one - ie the last one
        pageStepCRUD.moveToLastStepInList();
    },
    cloneStep_failure: function(data, clonedStep) {
        //reenable the clone button for the relevant test run step
        var self = teViewModel.vm;
        self.testRunStep()[clonedStep.indexOfClone].isBeingCloned(false);

        //fail quietly
        console.log('cloneStep_failure');
    },

    moveStepOnePosition: function(param) {
        //check for a valid direction paramater passed in
        var direction = param.toLowerCase();
        if(direction !== "up" && direction !== "down") return;
        
        //get position of chosen step in array
        var self = teViewModel.vm,
            stepsLength = self.testRunStep().length,
            currentStep = self.chosenTestStep(),
            currentIndex = self.testRunStep.indexOf(currentStep)
        //return out if there is an error finding the current step
        if (currentIndex === -1 ) return;

        //set up boolean checks for making logic code simpler
        var goUp = direction == "up",
            goDown = !goUp,
            atEnd = currentIndex === stepsLength - 1,
            atStart = currentIndex === 0;
        
        //set new index - make sure to not go up/down beyond the end/start of the array
        var newIndex = currentIndex + ( 
            (goUp && !atEnd) ? 1 : 
            (goDown && !atStart) ?  -1 : 
            0 );

        self.testRunStep.remove(currentStep);
        self.testRunStep.splice(newIndex, 0, currentStep);
        pageStepCRUD.updateStepNumbers();
    },

    updateStepNumbers: function() {
        teViewModel.vm.testRunStep().map(function(step, index) {
            step.position(index + 1); 
            return step;
        });

        var projectId = _pageInfo.projectId,
            testRunId = teViewModel.vm.testRunId,
            testRunsPendingId = teViewModel.vm.testRunsPendingId,
            testRunStepPositions = teViewModel.vm.testRunStep().map(function(step) {
                return {
                    TestRunStepId: step.testRunStepId,
                    Position: step.position()
                }
            });

        pageProps.ajxService.UpdateTestRunStepPositions (
            projectId, 
            testRunsPendingId, 
            testRunId, 
            testRunStepPositions,
            pageStepCRUD.updateTestRunStepPositions_success, 
            pageStepCRUD.updateTestRunStepPositions_failure
        );
    },
    updateTestRunStepPositions_success: function() {
        //succeed quietly
    },
    updateTestRunStepPositions_failure: function() {
        //fail quietly
    },

    moveToLastStepInList: function () {
        var newStepIndex = teViewModel.vm.testRunStep().length - 1;
        stepToMoveTo = teViewModel.vm.testRunStep()[newStepIndex];
        pageModelSetChosen.selectTest(stepToMoveTo);
    }
};





//Attachment Panel
var pageManageAttachments = {
    //Set the attachment panel's test run id so it can upload new attachments
    linkAttachmentsToChosenStep: function() {
        if (tstucAttachmentPanel.set_artifactId) {
            if (teViewModel.vm.testerId === _pageInfo.userId) {
                tstucAttachmentPanel.set_artifactId(teViewModel.vm.testRunId);
            } else {
                tstucAttachmentPanel.set_artifactId(-1);
            }
        }
    },
    //Display the list of existing attachments for this test run, test case or test step
    getChosenAttachments: function () {
        var pnlAttachments = $get(_pageInfo.pnlTestExecution_Attachments),
            filters = {},
            chosenTestRunId = teViewModel.vm.testRunId,
            chosenTestRunTestCaseId = teViewModel.vm.testCaseId,
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
        if ( teViewModel.vm.appIsBusy() ) {
            return;
        }
        //set the callback data

        teViewModel.vm.appIsBusy(true);

        var self = teViewModel.vm,
            chosenStep = self.chosenTestStep(),
            data = {};
        data.projectId = _pageInfo.projectId;
        data.testRunsPendingId = self.testRunsPendingId;
        data.testRunId = self.testRunId;
        data.testRunStepId = chosenStep.testRunStepId;
        pageModelStatusUpdate.syncViewModelAndCKEditors( function () {
            data.actualResult = chosenStep.actualResult();
        });
        //Get any incident data (returns undefined if validation error)
        data.incidentDataItem = pageManageIncidentForm.captureIncidentData();
        
        //Make the callback
        if (data.incidentDataItem) {
            pageProps.ajxService.TestExecution_LogIncident(data.projectId, data.testRunsPendingId, data.testRunId, data.testRunStepId, data.actualResult, data.incidentDataItem, pageCallbacks.newIncidentAdd_success, pageCallbacks.newIncidentAdd_failure);
        } else {
            globalFunctions.display_error_message(
                //$get(_pageInfo.msgNewIncidentMessageInline), [kept the id in case we want to display this inline rather than as a modal popup in the future]
                null,
                resx.TestCaseExecution_IncidentEntryInvalid,
                true, //makes the popup modal
                "fas fa-exclamation-triangle mr3"
            );
        }
    },

    associateExistingIncident: function() {
        //we set the art id into spira context as this is the generic place where the association panel js looks for the id.
        SpiraContext.ArtifactId = teViewModel.vm.chosenTestStep().testRunStepId;
        SpiraContext.ArtifactTypeId = globalFunctions.artifactTypeEnum.testRunStep;

        //populate general data into the global panelAssociationAdd object, so it is accessible by React on render
        panelAssociationAdd.lnkAddBtnId = _pageInfo.btnLinkExistingIncident;
        panelAssociationAdd.addPanelId = 'associateIncidentPanel';
        panelAssociationAdd.displayType = globalFunctions.displayTypeEnum.TestRun_Incidents;
        panelAssociationAdd.messageBox = _pageInfo.divNewIncidentsMessage;
        panelAssociationAdd.customSaveSuccessFunction = pageManageIncidentForm.associateExistingIncident_success;
        panelAssociationAdd.listOfViewableArtifactTypeIds = '3';

        //now render the panel
        panelAssociationAdd.showPanel();
    },

    associateExistingIncident_success: function() {
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
        $('#te-inspector-tab-buttons_incidents').trigger('click');
        if (focusOnNameField) {
            document.getElementById("txtIncidentName").focus();
        }
    },

    closeIncidentAssociationPanel: function() {
        $("#" + _pageInfo.btnLinkExistingIncident).removeClass('disabled');
        //only unmount the panel if it is currently mounted
        var domId = panelAssociationAdd.addPanelId;
        if (typeof SpiraContext.uiState[domId] != "undefined" && SpiraContext.uiState[domId].isMounted) {
            SpiraContext.uiState[domId].isMounted = false;
            ReactDOM.unmountComponentAtNode(document.getElementById(domId));
        }
    }
};




var pageManageCustomProperties = {
    getTestCase: function() {
        var testCaseId = teViewModel.vm.testCaseId,
            show = teViewModel.vm.userSettingsShowCustomProperties(),
            ajxTestRunCaseFormManager = $find(_pageInfo.ajxTestRunCaseFormManager);
        if(show) {
            ajxTestRunCaseFormManager.set_projectId(_pageInfo.projectId);
            ajxTestRunCaseFormManager.set_primaryKey(testCaseId);
            ajxTestRunCaseFormManager.load_data(false,false,true);
            //Make sure any custom properties shown on page are disabled
            ajxTestRunCaseFormManager.add_loaded(function() {
                pageManageCustomProperties.disableAllInputs()
            });
        }
    },
    getTestStep: function() {
        var testStepId = teViewModel.vm.chosenTestStep().testStepId,
            testRunStepId = teViewModel.vm.chosenTestStep().testRunStepId,
            show = teViewModel.vm.userSettingsShowCustomProperties(),
            ajxTestRunStepFormManager = $find(_pageInfo.ajxTestRunStepFormManager);
        //make sure to only attempt the retrieve if the user wants to see custom properties and the chosen step has a test step id
        //a step added during execution will not yet have a test step id and sending blank to server causes an error
        if (show && testStepId) {
            ajxTestRunStepFormManager.set_projectId(_pageInfo.projectId);
            ajxTestRunStepFormManager.set_primaryKey(testStepId);
            ajxTestRunStepFormManager.load_data(false,false,true);
            //Make sure any custom properties shown on page are disabled
            ajxTestRunStepFormManager.add_loaded(function() {
                pageManageCustomProperties.disableAllInputs()
            });
        }

        //Once we have a step, we need to also get incidents if the project settings require an incident for every non pass step
        //We get the incidents here so that we have them ahead of an attempt to set an execution status to avoid async issues
        //here we check for the testRunStepId not the teststepid because the step may not have a teststepid but still have incidents (if the step was added during this testing session)
        if (_pageInfo.requireIncident && testRunStepId) {
            pageCallbacks.getChosenIncidents();
        }
    },
    getTestCaseAndStep: function() {
        this.getTestCase();
        this.getTestStep();
    },
    disableAllInputs: function() {
        if( teViewModel.vm.userSettingsShowCustomProperties() ) {
            $('.children-read-only input').prop('disabled', true);
        }
    }
};



var pageManageTasks = {
    //Overwrites any existing description in the task form with some combination of the description and actual result of the currently selected test step
    setDescriptionFromTestStep: function () {
        var chosenStep = teViewModel.vm.chosenTestStep(),
            stepDescription = chosenStep.description(),
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
    newTask_click: function() {
        if (teViewModel.vm.appIsBusy()) {
            return;
        }

        // make sure the model has the latest ckeditor values
        pageModelStatusUpdate.syncViewModelAndCKEditors(function() {

            if (teViewModel.vm.taskName() == "") {
                alert(resx.TaskDetails_SplitTaskNameRequired);
            } else {
                //set the callback data
                var self = teViewModel.vm,
                    chosenStep = self.chosenTestStep(),
                    projectId = _pageInfo.projectId,
                    testCaseId = chosenStep.testCaseId,
                    testRunsPendingId = self.testRunsPendingId,
                    testRunId = self.testRunId,
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
        });

    },
    newTask_success: function(data) {
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

    },
    newTask_failure: function(err) {
        teViewModel.vm.appIsBusy(false);
    },


    loadTasks: function(testRunId) {
        globalFunctions.display_spinner();
        Inflectra.SpiraTest.Web.Services.Ajax.TasksService.Task_RetrieveByTestRunId(
            _pageInfo.projectId, 
            testRunId, 
            pageManageTasks.loadTasksSuccess,
            pageManageTasks.loadTasksFailure
        );
    },
    loadTasksSuccess: function(data) {
        globalFunctions.hide_spinner();
        pageModelUtilities.addSubArray(teViewModel.vm.tasks, data.items, tasksViewModel);
    },
    loadTasksFailure: function(exception) {
        globalFunctions.hide_spinner();
        globalFunctions.display_error($get(_pageInfo.clientId), exception);
    }
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
    //this is a big function to get the main execution page all set up and ready to go
    TestExecution_RetrieveTestRunsPending_Success: function(data) {
        pageProps.data = data;
        //only set the model and show the main parts of the page if actual data was returned
        if (data != null && data.TestRun.length != 0) {
            teViewModel.vm = new testRunPendingViewModel();
            pageModelSetChosen.setInitialTestStepPosition();
            ko.applyBindings(teViewModel.vm);

            //start the tour on first time accessing the page
            teViewModel.vm.userSettingsGuidedTourSeen() ? void (null) : hopscotch.startTour(teTour);

            //resize any unity wrapper areas
            uWrapper_onResize();

            //Setup and scroll the grid if needed
            pageControls.setGridHeightOnSplit();
            pageControls.scrollGridToCurrentStep();

            //animate the panel transition - some delays added to ensure animation is smooth
            setTimeout(function () {
                //hide the wizard by sliding in the main page over the top
                $('#te-execution-panel').css('left', 0);
            }, 100);
            setTimeout(function () {
                //hide the wizard panel properly - just to be on the safe side
                $('#te-wizard-panel').hide();
            }, 700);
            
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

        // handle color mode on ckeditor
        setTimeout(function () { pageCKE.setColorMode() }, 500);
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
        }
    },
    loadIncidents: function(testRunStepId, testStepId) {
        globalFunctions.display_spinner();
        Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.RetrieveByTestRunStepId(
            _pageInfo.projectId, 
            testRunStepId, 
            testStepId || null,
            Function.createDelegate(this, pageCallbacks.loadIncidentsSuccess),
            Function.createDelegate(this, pageCallbacks.loadIncidentsFailure)
        );
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
            currentRun = teViewModel.vm.testRunId;
        pageProps.ajxService.TestExecution_LogCurrentPosition(projectId, currentStep, currentRun, pageCallbacks.sendBackPosition_success, pageCallbacks.sendBackPosition_failure);
    },
    sendBackPosition_success: function() {
        //succeed quietly
    },
    sendBackPosition_failure: function() {
        //fail quietly
    },

    //sending back current user display settings
    logDisplayOptions: function(newVal) {
        //we pass in the value from the subscription event - the event from knockout fires twice
        //one time returns a true/false, the other an undefined (order varies by browser). We only want to send the new value (true/false)
        if (typeof newVal !== "undefined") {
            var projectId = _pageInfo.projectId,
                showCaseDescription = teViewModel.vm.userSettingsShowCaseDescription(),
                showExpectedResult = teViewModel.vm.userSettingsShowExpectedResult(),
                showSampleData = teViewModel.vm.userSettingsShowSampleData(),
                showCustomProperties = teViewModel.vm.userSettingsShowCustomProperties(),
                showLastResult = teViewModel.vm.userSettingsShowLastResult(),
                guidedTourSeen = teViewModel.vm.userSettingsGuidedTourSeen();;
            Inflectra.SpiraTest.Web.Services.Ajax.TestExecutionExploratoryService.TestExecution_LogDisplaySettings(
                projectId, 
                showCaseDescription,
                showExpectedResult,
                showSampleData,
                showCustomProperties,
                showLastResult,
                guidedTourSeen, 
                pageCallbacks.logDisplayOptions_success, 
                pageCallbacks.logDisplayOptions_failure
                );
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
        var chosenStep = teViewModel.vm.chosenTestStep(),
            projectId = _pageInfo.projectId,
            testRunsPendingId = teViewModel.vm.testRunsPendingId,
            testRunStepId = chosenStep.testRunStepId,
            description = chosenStep.description(),
            expectedResult = chosenStep.expectedResult(),
            sampleData = chosenStep.sampleData(),
            actualResult = chosenStep.actualResult(),
            startDate = chosenStep.startDate(),
            endDate = chosenStep.endDate(),
            actualDuration = chosenStep.actualDuration(),
            testRunId = teViewModel.vm.testRunId,

            //Get any incident data (returns undefined if validation error)
            incidentDataItem = pageManageIncidentForm.captureIncidentData(),

            //set up a variable to assign based on the execution status.
            serviceEndPoint = "";

        if (typeof(incidentDataItem) != 'undefined') {
            //Call the webservice
            teViewModel.vm.appIsBusy(true);

            globalFunctions.display_spinner();
            if(pageProps.temp.isPassAll) {
                pageProps.ajxService.PassAllTestRunSteps(
                    projectId, 
                    testRunsPendingId, 
                    testRunId, 
                    actualResult, 
                    testRunStepId, 
                    startDate, 
                    endDate, 
                    actualDuration, 
                    pageCallbacks.recordStatusUpdateResult_success, 
                    pageCallbacks.recordStatusUpdateResult_failure
                );
            } else {
                switch (pageProps.temp.statusId) {
                    case 1: //fail
                        serviceEndPoint = "FailTestRunStep";
                        break;
                    case 2: //pass
                        serviceEndPoint = "PassTestRunStep";
                        break;
                    case 4: //not applicable
                        serviceEndPoint = "NotApplicableTestRunStep";
                        break;
                    case 5: //blocked
                        serviceEndPoint = "BlockTestRunStep";
                        break;
                    case 6: //caution
                        serviceEndPoint = "CautionTestRunStep";
                        break;
                }

                //make the AJAX call
                pageProps.ajxService[serviceEndPoint] (
                    projectId, 
                    testRunsPendingId, 
                    testRunId, 
                    testRunStepId, 
                    description,
                    expectedResult,
                    sampleData,
                    actualResult, 
                    startDate, 
                    endDate, 
                    actualDuration, 
                    incidentDataItem, 
                    pageCallbacks.recordStatusUpdateResult_success, 
                    pageCallbacks.recordStatusUpdateResult_failure
                );
            }
        }
    },

    recordStatusUpdateResult_success: function(data, e) {
        //manage success and record the requested new status into the model
        globalFunctions.hide_spinner();
        teViewModel.vm.appIsBusy(false);
        //update test run status
        if (teViewModel.vm.testRunId === Number(data.kTestRunId)) {
            var newStatus = Number(data.kTestRunExecutionStatusId);
            isNaN(newStatus) ? void(null) : teViewModel.vm.testRunExecutionStatusId(newStatus);
        }
        //update test run step status(es)
        if (pageProps.temp.isPassAll) {
            //assign status update to every step in the run
            var parent = teViewModel.vm.testRunStep(),
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



    updateTestRunSingleField: function (testRunStepId, fieldToUpdate, textField) {
        var projectId = _pageInfo.projectId,
            testRunId = teViewModel.vm.testRunId,
            testRunsPendingId = teViewModel.vm.testRunsPendingId;

        pageProps.ajxService.UpdateTestRunSingleField (
            projectId, 
            testRunsPendingId, 
            testRunId, 
            testRunStepId || null, 
            fieldToUpdate,
            textField,
            pageCallbacks.updateTestRunSingleField_success, 
            pageCallbacks.updateTestRunSingleField_failure
        );
    },
    updateTestRunSingleField_success: function() {
        //succeed quietly
    },
    updateTestRunSingleField_failure: function() {
        //fail quietly
    },


    updateTestRunStepPositions: function () {
        var projectId = _pageInfo.projectId,
            testRunId = teViewModel.vm.testRunId,
            testRunsPendingId = teViewModel.vm.testRunsPendingId,
            testRunStepPositions = teViewModel.vm.testRunStep().map(function(step) {
                return {
                    TestRunStepId: step.testRunStepId,
                    Position: step.position()
                }
            });

        pageProps.ajxService.UpdateTestRunStepPositions (
            projectId, 
            testRunsPendingId, 
            testRunId, 
            testRunStepPositions,
            pageCallbacks.updateTestRunStepPositions_success, 
            pageCallbacks.updateTestRunStepPositions_failure
        );
    },
    updateTestRunStepPositions_success: function() {
        //succeed quietly
    },
    updateTestRunStepPositions_failure: function() {
        //fail quietly
    },


    newIncidentAdd_success: function() {
        globalFunctions.hide_spinner();
        teViewModel.vm.appIsBusy(false);
        pageManageIncidentForm.clearIncidentFields();

        //get the incidents so that you can see the new incident straight away
		var chosenStep = teViewModel.vm.chosenTestStep();
		//Check permissions before showing existing incidents
		if (globalFunctions.isAuthorized(globalFunctions.permissionEnum.View, globalFunctions.artifactTypeEnum.incident) == globalFunctions.authorizationStateEnum.authorized) {
			pageCallbacks.loadIncidents(chosenStep.testRunStepId, chosenStep.testStepId);
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
    logChosenStepTiming: function() {
        pageTimeManagement.setActualStart();
        pageTimeManagement.setAllEndTimes(function callback() {
            var projectId = _pageInfo.projectId,
                testRunsPendingId = teViewModel.vm.testRunsPendingId,
                testRunId = teViewModel.vm.testRunId,
                testRunStepId = teViewModel.vm.chosenTestStep().testRunStepId
                startDate = teViewModel.vm.chosenTestStep().startDate(),
                endDate =  teViewModel.vm.chosenTestStep().endDate(),
                actualDuration = teViewModel.vm.chosenTestStep().actualDuration();

                pageProps.ajxService.TestExecution_LogStepTiming(projectId, testRunsPendingId, testRunId, testRunStepId, startDate, endDate, actualDuration, function () { pageCallbacks.logChosenStepTiming_success() }, function () { pageCallbacks.logChosenStepTiming_failure() });
        });
    },
    logChosenStepTiming_success: function () {
        pageProps.temp.shouldLeave ? window.location.href = _pageInfo.fullReferrerUrl : void(null);
    },
    logChosenStepTiming_failure: function (err) {
        pageProps.temp.shouldLeave = false;
        var divNewIncidentsMessage = $get(_pageInfo.divNewIncidentsMessage);
        globalFunctions.display_error(divNewIncidentsMessage, err);
    },
};

/// DOM triggered jquery events
//On load
$(document).ready(function () {
    $('#te-wizard-panel').show(); //just in case
    $('#global-nav-keyboard-shortcuts #shortcuts-test-execution-exploratory').removeClass('dn'); //make sure the correct keyboard shortcuts are visible

    //Hide new incident logging if we don't have permissions
    if (globalFunctions.isAuthorized(globalFunctions.permissionEnum.Create, globalFunctions.artifactTypeEnum.incident) != globalFunctions.authorizationStateEnum.authorized) {
        $('#' + _pageInfo.addIncidentArea).addClass('hidden');
    }
});


//CLICK EVENTS
//options menu
$('#te-menu-show-guided-tour').on("click", function() {
    hopscotch.startTour(teTour); 
});

//play button
pageProps.controls.$playPause.on("click", function (event) {
    event.preventDefault();
    pageControls.playPauseChange();
});

//leave button - opens popup to let user make choice as to exit type
$('#' + _pageInfo.pbBtnLeave).on("click", function (event) {
    event.preventDefault();
    pageControls.leave();
});

//add new tasks
$('#btnNewTask').on("click", function (event) {
    event.preventDefault();
    pageManageTasks.newTask_click();
});



//Switch inspector tabs for results, attachments, incidents
Mousetrap.bind('d t', function() {
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
Mousetrap.bind('j', function() {
    pageModelMoveFocus.stepForward();
});
Mousetrap.bind('k', function() {
    pageModelMoveFocus.stepBack();
});

// SPECIFIC FOR EXPLORATORY
//delete current step
Mousetrap.bind('a d', function() {
    var currentStepId = teViewModel.vm.chosenTestStep().testRunStepId;
    pageStepCRUD.deleteStep(currentStepId);
});

//clone current step
Mousetrap.bind('a c', function() {
    var currentStepId = teViewModel.vm.chosenTestStep().testRunStepId;
    pageStepCRUD.cloneStep(currentStepId);
});

//add new step
Mousetrap.bind('a a', function() {
    pageStepCRUD.createNewTestRunStep();
});

//move step up / down
Mousetrap.bind('m j', function() {
    pageStepCRUD.moveStepOnePosition("up");
});
Mousetrap.bind('m k', function() {
    pageStepCRUD.moveStepOnePosition("down");
});

//For quickly editing all the different fields
//Checks are made against those that can be hidden so that no text is accidentally entered into them when they are hidden
Mousetrap.bind('e c n', function() {
    //set timeout to make sure last letter of shortcut doesn't get included in the field
    setTimeout(function() {document.getElementById(pageProps.caseNameId).focus()}, 100);
});
Mousetrap.bind('e c d', function() {
    if(teViewModel.vm.userSettingsShowCaseDescription()) {
        CKEDITOR.instances[pageProps.caseDescriptionId].focus();
    }
});
Mousetrap.bind('e d', function() {
    CKEDITOR.instances[pageProps.stepDescriptionId].focus();
});
Mousetrap.bind('e e', function() {
    if(teViewModel.vm.userSettingsShowExpectedResult()) {
        CKEDITOR.instances[pageProps.expectedResultId].focus();
    }
});
Mousetrap.bind('e s', function() {
    if(teViewModel.vm.userSettingsShowSampleData()) {
        CKEDITOR.instances[pageProps.sampleDataId].focus();
    }
});
Mousetrap.bind('e a', function() {
    CKEDITOR.instances[pageProps.actualResultId].focus();
});


//showing or hiding different elements - all are options in main options menu
Mousetrap.bind('d d', function() {
    teViewModel.vm.userSettingsShowCaseDescription( !teViewModel.vm.userSettingsShowCaseDescription() );
    pageCallbacks.logDisplayOptions(true);
});
Mousetrap.bind('d e', function() {
    teViewModel.vm.userSettingsShowExpectedResult( !teViewModel.vm.userSettingsShowExpectedResult() );
    pageCallbacks.logDisplayOptions(true);
});
Mousetrap.bind('d s', function() {
    teViewModel.vm.userSettingsShowSampleData( !teViewModel.vm.userSettingsShowSampleData() );
    pageCallbacks.logDisplayOptions(true);
});
Mousetrap.bind('d c', function() {
    teViewModel.vm.userSettingsShowCustomProperties( !teViewModel.vm.userSettingsShowCustomProperties() );
    pageCallbacks.logDisplayOptions(true);
});
Mousetrap.bind('d l', function() {
    teViewModel.vm.userSettingsShowLastResult( !teViewModel.vm.userSettingsShowLastResult() );
    pageCallbacks.logDisplayOptions(true);
});



//Knockout view model
function testRunPendingViewModel() {
    var data = pageProps.data;
    var self = this;

    /*
     * CORE MODEL CREATION
     */

    //test runs pending top level fields
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
    
    //test run specific fields (ie of the single test case)
    self.testCaseId = data.TestRun[0].TestCaseId || '';
    self.testRunId = data.TestRun[0].TestRunId || '';
    self.releaseId = data.TestRun[0].ReleaseId || '';
    self.testerId = data.TestRun[0].TesterId || '';
    self.testRunName = ko.observable(data.TestRun[0].Name || '');
    self.testRunDescription = ko.observable(data.TestRun[0].Description || '');
    self.testRunExecutionStatusId = ko.observable(data.TestRun[0].ExecutionStatusId || '');
    self.testRunStartDate = ko.observable(globalFunctions.parseJsonDate(data.TestRun[0].StartDate) || '');
    self.testRunEndDate = ko.observable(globalFunctions.parseJsonDate(data.TestRun[0].EndDate) || '');
    self.testRunActualDuration = ko.observable(data.TestRun[0].ActualDuration || '');

    //information about the last time the test case was run
    self.lastTestRunId = data.TestRun[0].LastTestRunId || '';
    self.lastReleaseId = data.TestRun[0].LastReleaseId || '';
    self.lastReleaseVersion = data.TestRun[0].LastReleaseVersion || '';
        
    //Create empty KO array, loop over all test steps in the run / case and push them to the array
    self.testRunStep = ko.observableArray();
    pageModelUtilities.addSubArray(
        self.testRunStep, 
        data.TestRun[0].TestRunStep, 
        testRunStepViewModel, 
        self.testCaseId
        );


    /*
     * FOR MANAGING EXECUTION STATUSES
     */

    //Computed functions that set display classes based on current execution status
    self.testRunExecutionStatusClass = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.testRunExecutionStatusId()).status;
    });
    self.testRunExecutionStatusGlyph = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.testRunExecutionStatusId()).status;
    });
    self.testRunExecutionStatusIcon = ko.computed(function () {
        return self.testRunExecutionStatusClass() ? ('fas fa-sticky-note ' + self.testRunExecutionStatusClass()) : 'far fa-sticky-note';
    });

    //execution counts and information for all of the test run steps
    self.countSteps = ko.computed(function () {
        return self.testRunStep().length;
    });
    self.liveCountFail = ko.computed(function () {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.fail);
    });
    self.liveCountPassed = ko.computed(function () {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.passed);
    });
    self.liveCountNotRun = ko.computed(function () {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.notRun);
    });
    self.liveCountNotApplicable = ko.computed(function () {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.notApplicable);
    });
    self.liveCountBlocked = ko.computed(function () {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.blocked);
    });
    self.liveCountCaution = ko.computed(function () {
        return pageModelUtilities.countInArray(self.testRunStep(), 'executionStatusId', pageProps.statusEnum.caution);
    });

    //execution summaries for the test run
    self.liveTotalRun = ko.computed(function () {
        return self.liveCountFail() + self.liveCountPassed() + self.liveCountNotApplicable() + self.liveCountBlocked() + self.liveCountCaution();
    });
    self.allRunsHaveBeenRun = ko.computed(function () {
        return self.testRunExecutionStatusId() !== pageProps.statusEnum.notRun;
    });

    //info for the screenshot upload
    self.screenshotUploadUrl = pageProps.screenshotUploadUrl = _pageInfo.screenshotUploadUrl
        .replace("{art}", self.testRunId)
        .replace("{artifactType}", _pageInfo.artifactTypeFilter_TestRun);

    


    /*
     * TASK FIELDS
     */
    self.taskName = ko.observable("");
    self.taskDescription = ko.observable("");
    self.hasTaskName = ko.computed(function() {
        return self.taskName() != "";
    });

    //Create empty KO array for existing tasks / newly created tasks
    self.tasks = ko.observableArray();




    /*
     * UI MANAGEMENT
     */
    //Set the user settings
    self.userSettingsGuidedTourSeen = ko.observable(data.SettingsExploratory ? data.SettingsExploratory[0].GuidedTourSeen : false);
    self.userSettingsCurrentStepId = ko.observable(data.SettingsExploratory ? data.SettingsExploratory[0].CurrentTestRunStepId : '' );

    self.userSettingsShowCaseDescription = ko.observable(data.SettingsExploratory ? data.SettingsExploratory[0].ShowCaseDescription : true);
    self.userSettingsShowExpectedResult = ko.observable(data.SettingsExploratory ? data.SettingsExploratory[0].ShowExpectedResult : true);
    self.userSettingsShowSampleData = ko.observable(data.SettingsExploratory ? data.SettingsExploratory[0].ShowSampleData : true);
    self.userSettingsShowCustomProperties = ko.observable(data.SettingsExploratory ? data.SettingsExploratory[0].ShowCustomProperties : false);
    self.userSettingsShowLastResult = ko.observable(data.SettingsExploratory ? data.SettingsExploratory[0].ShowLastResult : false);



    self.userSettingsShowCustomProperties.subscribe(function(newVal) {
        pageCallbacks.logDisplayOptions(newVal); 
        pageManageCustomProperties.getTestCaseAndStep();
    });
    self.userSettingsShowCaseDescription.subscribe( function(newVal) { pageCallbacks.logDisplayOptions(newVal)} );
    self.userSettingsShowExpectedResult.subscribe( function(newVal) { pageCallbacks.logDisplayOptions(newVal)} );
    self.userSettingsShowSampleData.subscribe( function(newVal) { pageCallbacks.logDisplayOptions(newVal)} );
    self.userSettingsShowLastResult.subscribe(function (newVal) { pageCallbacks.logDisplayOptions(newVal) });

    self.appIsBusy = ko.observable(false);

    //Allow knockout to make a control have focus
    self.chosenPendingStatusUpdate = ko.observable();
    self.chosenPendingStatusClass = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.chosenPendingStatusUpdate()).status;
    });
    self.chosenPendingStatusBgClass = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.chosenPendingStatusUpdate()).status_bg;
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

    self.enableActualResultInInspector = true;

    self.isEditorAndModelSynced = ko.observable(true);

    self.showAddIncidentMessage = ko.observable(false);
    
    //To select a specific test run step
    self.chosenTestStep = ko.observable();
    self.chosenTestStepIncidents = ko.observableArray();

    ////Computeds to ascertain behaviour for moving to the previous or next test step
    //First get information about the arrays and current position of the chosen test set within them
    self.itemArrayProps = ko.computed(function () {
        if (self.chosenTestStep()) {
            return {
                stepArrayLength: self.testRunStep().length,
                stepCurrentIndex: ko.utils.arrayIndexOf(self.testRunStep(), self.chosenTestStep())
            };
        }
    });
    
    //Then determine whether the current step and run are the first or last in the respective arrays
    self.itemPositions = ko.computed(function () {
        if (self.chosenTestStep()) {
            return {
                isVeryFirstStep: self.itemArrayProps().stepCurrentIndex === 0 ? true : false,
                isVeryLastStep: self.itemArrayProps().stepCurrentIndex === (self.itemArrayProps().stepArrayLength - 1) ? true : false
            };
        }
    });
    
    
    //Calculate the total number of test runs and test steps in the test set
    self.totalTestRuns = 1;
    self.totalTestSteps = ko.computed(function () {
        return self.testRunStep().length;
    });
    


    /*
     * FOR TRACKING CHANGES TO TEST RUN NAME FIELD
     */
    self.testRunNamePrevious = ko.observable(data.Name || "");

    self.testRunNameLosesFocus = function(newValue) {
        if (self.testRunNamePrevious() !== newValue.testRunName()) {
            pageCallbacks.updateTestRunSingleField(
                false, //states that a test run step is NOT being sent
                pageProps.editableFields.testRunName, 
                newValue.testRunName()
            )
            self.testRunNamePrevious(newValue.testRunName());
        }
    };

    //tracking if the add button has been pressed
    self.isAdding = ko.observable(false);


    /*
     *Manage Callbacks
     */
    self.chosenLinkedIncidentsSuccess = ko.observable(false);



    /*
     * FOR MANAGING DRAG AND DROP (USED FOR TEST RUN STEPS)
     */
    self.dragStart = function (item) {
        item.dragging(true);
    };

    self.dragEnd = function (item) {
        item.dragging(false);

        pageStepCRUD.updateStepNumbers();
    };

    self.reorder = function (event, dragData, zoneData) {
        if (dragData !== zoneData.testRunStep) {
            var zoneDataIndex = zoneData.items.indexOf(zoneData.item);
            zoneData.items.remove(dragData);
            zoneData.items.splice(zoneDataIndex, 0, dragData);
        }
    };
};


function testRunStepViewModel(testRunStepObj) {
    var self = this;

    /*
     * CORE TEST RUN STEP MODEL CREATION
     */
    self.testRunStepId = testRunStepObj.TestRunStepId || '';
    self.testStepId = testRunStepObj.TestStepId || '';
    self.testCaseId = testRunStepObj.TestCaseId || '';
    self.exploratoryTestCaseId = testRunStepObj.ParentId || '';
    self.description = ko.observable(testRunStepObj.Description || '');
    self.position = ko.observable(testRunStepObj.Position || '');
    self.expectedResult = ko.observable(testRunStepObj.ExpectedResult || '');
    self.sampleData = ko.observable(testRunStepObj.SampleData || '');
    self.actualResult = ko.observable(testRunStepObj.ActualResult || '');
    self.executionStatusId = ko.observable(testRunStepObj.ExecutionStatusId || '');
    self.startDate = ko.observable(globalFunctions.parseJsonDate(testRunStepObj.StartDate) || '');
    self.endDate = ko.observable(globalFunctions.parseJsonDate(testRunStepObj.EndDate) || '');
    self.actualDuration = ko.observable(testRunStepObj.ActualDuration || '');

    //information about the last time the test step was run
    self.lastTestRunId = testRunStepObj.LastTestRunId || '';
    self.lastActualResult = testRunStepObj.LastActualResult || '--';
    self.lastExecutionStatusId = testRunStepObj.LastExecutionStatusId || '';
    self.lastEndDate = testRunStepObj.LastEndDate ? globalFunctions.parseJsonDate(testRunStepObj.LastEndDate).toLocaleDateString() : '';


    self.positionTitle = ko.computed(function () {
        return resx.TestStep_Step + ' ' + self.position();
    });

    self.isBeingCloned = ko.observable(false);

    self.isReadOnly = ko.computed(function () {
        return self.testCaseId !== self.exploratoryTestCaseId;
    });

    /*
     * FOR MANAGING EXECUTION STATUSES
     */
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

    //for the last execution status
    self.lastExecutionStatusClass = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.lastExecutionStatusId).status;
    });
    self.lastExecutionStatusGlyph = ko.computed(function () {
        return pageModelUtilities.executionStatusProps(self.lastExecutionStatusId).glyph;
    });
    self.lastExecutionStatusIcon = ko.computed(function () {
        return self.lastExecutionStatusClass() ? ('fas fa-circle ' + self.lastExecutionStatusClass()) : 'far fa-circle';
    });

    /*
     * FOR MANAGING DRAG AND DROP REORDERING
     */
    self.dragging = ko.observable(false);
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
        
        //optional height param to set height for a specific a specific CKEditor instance
        //this needs to be an integer as will be converted into pixels
        //var height = allBindingsAccessor.get('cke_height') || null;

        //the value of the actual field passed in on the page's data-bind
        var fieldName = allBindings.get("field"),
            isTestRun = allBindings.get("isTestRun");

        //first, as this is a custom binding, we need to manually set the dom val of the element to the value of the model
        //use ko.unwrap to make sure to get the plain, not observable, value - ie the actual html
        $(element).val(ko.unwrap( valueAccessor() ));

        //now create the ckeditor using the general options for all editors on the apge
        $(element).ckeditor(pageCKE.options);

        //with the element created we can get the specific editor object
        var txtBoxID = $(element).attr("id");
        var editor = CKEDITOR.instances[txtBoxID];
        
        // update the url - all fields are attached to the test run
        editor.config.uploadUrl = teViewModel.vm.screenshotUploadUrl;

        //this let's us create event listeners so that every time the editor gains/loses focus the knockout model is updated
        //this uses the CKEDitor built in events
        editor.on('focus', function (e) {
            //reset here so that we are sure that only changes made in the UI (not from navigating steps) set the dirty flag
            editor.resetDirty();

            teViewModel.vm.isEditorAndModelSynced(false);
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

            
            //if there are changes we want to send only that field to the server
            if (isDirty) {

                var chosenStep = bindingContext.chosenTestStep(),
                    isTestRunDescription = false,
                    textField = "",
                    fieldToUpdate = 0;
                
                if ( isTestRun ) {
                    if (fieldName == "testRunDescription" ) {
                        fieldToUpdate = pageProps.editableFields.testRunDescription;
                        textField = bindingContext.testRunDescription();
                    }
                } else if (fieldName == "actualResult") {
                    fieldToUpdate = pageProps.editableFields.testStepActualResult;
                    textField = chosenStep.actualResult();
                } else if ( fieldName == "description" ) {
                    fieldToUpdate = pageProps.editableFields.testStepDescription;
                    textField = chosenStep.description();
                } else if ( fieldName == "expectedResult" ) {
                    fieldToUpdate = pageProps.editableFields.testStepExpectedResult;
                    textField = chosenStep.expectedResult();
                } else if ( fieldName == "sampleData" ) {
                    fieldToUpdate = pageProps.editableFields.testStepSampleData;
                    textField = chosenStep.sampleData();
                }

                //attempt to update the field if we know which field to update (where we don't it could be a different CK Editor - eg task description)
                if (fieldToUpdate > 0) {
                    if (isTestRunDescription) {
                        pageCallbacks.updateTestRunSingleField (
                            false, //states that a test run step is NOT being sent
                            fieldToUpdate,
                            textField
                        ) 
                    } else {
                        pageCallbacks.updateTestRunSingleField (
                            chosenStep.testRunStepId,
                            fieldToUpdate,
                            textField
                        )
                    }                
                }
                
            }
        });
        
        //handle disposal (if KO removes by the template binding)
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
           // not currently used;
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
    id: "testExecutionExploratoryGuidedTour",
    showCloseButton: true,
    onStart: function() {
        teViewModel.vm.userSettingsGuidedTourSeen(true);
        // we need to log the display options here to make sure the database stores that the tour has been seen
        pageCallbacks.logDisplayOptions(true);
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
            title: resx.Guide_TestExecutionExploratory_intro_Title,
            content: resx.Guide_TestExecutionExploratory_intro_Content,
            target: "txtName",
            placement: "bottom"
        },
        {
            title: resx.Guide_TestExecutionExploratory_editing_Title,
            content: resx.Guide_TestExecutionExploratory_editing_Content,
            target: "txtName",
            placement: "bottom",
            showPrevButton: true
        },
        {
            title: resx.Guide_TestExecutionExploratory_grid_Title,
            content: resx.Guide_TestExecutionExploratory_grid_Content,
            target: "te-grid",
            placement: "right",
            showPrevButton: true
        },
        {
            title: resx.Guide_TestExecutionExploratory_gridStep_Title,
            content: resx.Guide_TestExecutionExploratory_gridStep_Content,
            target: "te-grid",
            placement: "right",
            showPrevButton: true
        },
        {
            title: resx.Guide_TestExecutionExploratory_gridStepMoving_Title,
            content: resx.Guide_TestExecutionExploratory_gridStepMoving_Content,
            target: "te-grid",
            placement: "right",
            showPrevButton: true
        },
        {
            title: resx.Guide_TestExecutionExploratory_gridAdd_Title,
            content: resx.Guide_TestExecutionExploratory_gridAdd_Content,
            target: _pageInfo.gridAddBtn,
            placement: "top",
            showPrevButton: true,
            showNextButton: false,
            showCTAButton: true,
            ctaLabel: resx.Guide_Global_Next,
            onCTA: function() {
                // take different action based on whether tasks are present or not - eg if using Team or Test
                if (SpiraContext.ProductType == 'SpiraTest') {
                    var currentStep = hopscotch.getCurrStepNum();
                    hopscotch.showStep(currentStep + 2);
                } else {
                    $('#te-inspector-tab-buttons_tasks').click();
                    setTimeout(function() {
                        hopscotch.nextStep();
                    }, 300);
                }
            }
        },
        {
            title: resx.Guide_TestExecutionExploratory_tasks_Title,
            content: resx.Guide_TestExecutionExploratory_tasks_Content,
            target: "te-inspector_tab_tasks",
            placement: "top",
            showPrevButton: true
        },
        {
            title: resx.Guide_TestExecutionExploratory_settings_Title,
            content: resx.Guide_TestExecutionExploratory_settings_Content,
            target: "toolbar-options-btn-settings",
            placement: "left",
            showPrevButton: true
        },
        {
            title: resx.Guide_TestExecutionExploratory_exit_Title,
            content: resx.Guide_TestExecutionExploratory_exit_Content,
            target: "te-display-toolbar-options",
            placement: "left",
            showPrevButton: true
        }
    ]
};



//drag and drop knockout custom bindings
//https://github.com/One-com/knockout-dragdrop
//http://one-com.github.io/knockout-dragdrop/examples/
!function(t){"function"==typeof define&&define.amd?define(["knockout"],t):"object"==typeof module&&module.exports?module.exports=t(require("knockout")):t(ko)}(function(t){function e(t){t=t||{};for(var e=1;e<arguments.length;e++)if(arguments[e])for(var n in arguments[e])arguments[e].hasOwnProperty(n)&&(t[n]=arguments[e][n]);return t}function n(t,e,n){var i=t.className.split(" "),o=i.indexOf(e);o>=0&&!n&&i.splice(o,1),o<0&&n&&i.push(e),t.className=i.join(" ")}function i(t,e){if(!t.tagName)return null;var n=document.documentElement;return(n.matches||n.matchesSelector||n.webkitMatchesSelector||n.mozMatchesSelector||n.msMatchesSelector||n.oMatchesSelector).call(t,e)}function o(t,e){do{if(i(t,e))return t;t=t.parentNode}while(t);return null}function r(t){this.init(t)}function a(t,e){return t.type.indexOf("touch")>=0?t.changedTouches[0][e]:t[e]}function s(t){this.init(t),this.drop=function(e){t.drop(e,t.data)}}function d(t){this.element=t,n(this.element,"drag-element",!0),this.element.style.position="fixed",this.element.style.zIndex=9998,this.element.addEventListener("selectstart",function(){return!1},!0)}function l(t){this.element=t.element,this.name=t.name,this.dragStart=t.dragStart,this.dragEnd=t.dragEnd,this.data=t.data}function c(t,e){var n=t.getBoundingClientRect();this.element=t,this.scrollMarginHeight=Math.floor(n.height/10),this.scrollMarginWidth=Math.floor(n.width/10),this.offset={top:n.top+window.pageYOffset-document.documentElement.clientTop,left:n.left+window.pageXOffset-document.documentElement.clientLeft},this.innerHeight=n.height,this.innerWidth=n.width,this.scrollDeltaMin=5,this.scrollDeltaMax=30,this.delay=e||0,this.inZone="center",this.scrolling=!1}function u(t){if(t.accepts)return[].concat(t.accepts);if(t.name)return[t.name];throw new Error("A drop zone must specify the drag zones it accepts")}var h={},p={},m=t.utils.arrayForEach,f=t.utils.arrayFirst,g=t.utils.arrayFilter,v="ontouchstart"in document.documentElement,y=!1;r.prototype.init=function(t){this.element=t.element,this.data=t.data,this.dragEnter=t.dragEnter,this.dragOver=t.dragOver,this.dragLeave=t.dragLeave,this.active=!1,this.inside=!1,this.dirty=!1},r.prototype.refreshDomInfo=function(){if(this.hidden="none"===this.element.style.display,!this.hidden){var t=this.element.getBoundingClientRect();this.top=t.top+window.pageYOffset-document.documentElement.clientTop,this.left=t.left+window.pageXOffset-document.documentElement.clientLeft,this.width=t.width,this.height=t.height}},r.prototype.isInside=function(t,e){return!this.hidden&&(!(t<this.left||e<this.top)&&(!(this.left+this.width<t)&&!(this.top+this.height<e)))},r.prototype.update=function(t,e){this.isInside(a(t,"pageX"),a(t,"pageY"))?(this.inside||this.enter(t,e),this.dragOver&&this.dragOver(t,e,this.data)):this.leave(t)},r.prototype.enter=function(t,e){this.inside=!0,this.dragEnter?this.active=!1!==this.dragEnter(t,e,this.data):this.active=!0,this.dirty=!0},r.prototype.leave=function(t){t&&(t.target=this.element),this.inside&&this.dragLeave&&this.dragLeave(t,this.data),this.active=!1,this.inside=!1,this.dirty=!0},s.prototype=r.prototype,s.prototype.updateStyling=function(){this.dirty&&(n(this.element,"drag-over",this.active),n(this.element,"drop-rejected",this.inside&&!this.active)),this.dirty=!1},d.prototype.updatePosition=function(t){this.element.style.top=t.pageY-window.pageYOffset+"px",this.element.style.left=t.pageX-window.pageXOffset+"px",this.element.style.top=a(t,"pageY")-window.pageYOffset+"px",this.element.style.left=a(t,"pageX")-window.pageXOffset+"px"},d.prototype.remove=function(){this.element.parentNode.removeChild(this.element)},l.prototype.startDrag=function(t){if(this.dragStart&&!1===this.dragStart(this.data,t))return!1},l.prototype.drag=function(t){var e=this,n=this.name,i=h[n].concat(p[n]);m(i,function(t){t.refreshDomInfo()}),m(i,function(n){t.target=n.element,n.update(t,e.data)}),m(h[n],function(t){t.updateStyling()})},l.prototype.dropRejected=function(){var t=this.name;return!f(h[t],function(t){return t.inside})||!f(h[t],function(t){return t.active})},l.prototype.cancelDrag=function(t){this.dragEnd&&this.dragEnd(this.data,t)},l.prototype.drop=function(t){var e=this.name,n=o(t.target,".drop-zone"),i=g(h[e],function(t){return t.active}),r=g(i,function(t){return t.element===n})[0];m(h[e].concat(p[e]),function(e){e.leave(t)}),m(h[e],function(t){t.updateStyling()}),r&&r.drop&&r.drop(this.data),this.dragEnd&&this.dragEnd(this.data,t)},c.prototype.scroll=function(t,e){this.x=t,this.y=e,this.topLimit=this.scrollMarginHeight+this.offset.top,this.bottomLimit=this.offset.top+this.innerHeight-this.scrollMarginHeight,this.leftLimit=this.scrollMarginWidth+this.offset.left,this.rightLimit=this.offset.left+this.innerWidth-this.scrollMarginWidth;var n="";e<this.topLimit?n+="top":e>this.bottomLimit&&(n+="bottom"),t<this.leftLimit?n+="left":t>this.rightLimit&&(n+="right"),""===n&&(n="center"),this.updateZone(n)},c.prototype.enter=function(t){var e=this;this.delayTimer=setTimeout(function(){e.scrolling=!0},this.delay)},c.prototype.leave=function(t){this.scrolling=!1,clearTimeout(this.delayTimer)},c.prototype.over=function(t){var e,n=function(t){return t*(this.scrollDeltaMax-this.scrollDeltaMin)+this.scrollDeltaMin}.bind(this);this.scrolling&&(-1!==t.indexOf("top")?(e=(this.topLimit-this.y)/this.scrollMarginHeight,this.element.scrollTop-=n(e)):-1!==t.indexOf("bottom")&&(e=(this.y-this.bottomLimit)/this.scrollMarginHeight,this.element.scrollTop+=n(e)),-1!==t.indexOf("left")?(e=(this.leftLimit-this.x)/this.scrollMarginWidth,this.element.scrollLeft-=n(e)):-1!==t.indexOf("right")&&(e=(this.x-this.rightLimit)/this.scrollMarginWidth,this.element.scrollLeft+=n(e)))},c.prototype.updateZone=function(t){this.zone!==t&&(this.leave(this.zone),this.enter(t)),this.zone=t,this.over(t)},t.utils.extend(t.bindingHandlers,{dropZone:{init:function(e,i,o,r,a){var d=t.utils.unwrapObservable(i()),l=u(d);n(e,"drop-zone",!0);var c=new s({element:e,data:d.data||a&&a.$data,drop:d.drop,dragEnter:d.dragEnter,dragOver:d.dragOver,dragLeave:d.dragLeave});l.forEach(function(t){h[t]=h[t]||[],h[t].push(c)}),t.utils.domNodeDisposal.addDisposeCallback(e,function(){c.leave(),l.forEach(function(t){h[t].splice(h[t].indexOf(c),1)})})}},dragEvents:{init:function(e,n,i,o,a){var s=t.utils.unwrapObservable(n()),d=u(s),l=new r({element:e,data:s.data||a&&a.$data,dragEnter:s.dragEnter,dragOver:s.dragOver,dragLeave:s.dragLeave});d.forEach(function(t){p[t]=p[t]||[],p[t].push(l)}),t.utils.domNodeDisposal.addDisposeCallback(e,function(){l.leave(),d.forEach(function(t){p[t].splice(p[t].indexOf(l),1)})})}},dragZone:{init:function(i,o,r,s,c){function u(){var t=i.cloneNode(!0);i.parentNode.appendChild(t);var e=window.getComputedStyle(i,null);return t.style.height=e.getPropertyValue("height"),t.style.width=e.getPropertyValue("width"),t.style.opacity=.7,t.style.filter="alpha(opacity=70)",t}function m(){var e=document.createElement("div");document.body.appendChild(e);var n="data"in E?c.createChildContext(E.data):c;return t.renderTemplate(E.element,n,{},e),e}function f(t){t.preventDefault(),t.stopPropagation()}function g(o){function r(t){function i(t){O.drag(t);var e=O.dropRejected();e!==D&&(n(w,"drop-rejected",e),w.style.cursor=e?"no-drop":"move",D=e),b=setTimeout(function(){i(t)},100)}function r(t){return y=!1,clearTimeout(h),clearTimeout(b),L&&L.remove(),w.parentNode.removeChild(w),O.cancelDrag(t),document.removeEventListener("touchmove",p),document.removeEventListener("touchend",g),t.preventDefault(),t.stopPropagation(),!0}function p(t){return t.button>0?r(t):(clearTimeout(b),L&&L.updatePosition(t),i(t),t.preventDefault(),t.stopPropagation(),!1)}function g(t){y=!1,clearTimeout(h),clearTimeout(b),L&&L.remove(),w.parentNode.removeChild(w);var n=e({},t);return n.target=document.elementFromPoint(a(t,"clientX"),a(t,"clientY")),O.drop(n),document.removeEventListener("selectstart",f,!0),document.removeEventListener("touchmove",p),document.removeEventListener("touchend",g),t.preventDefault(),t.stopPropagation(),!1}if(document.removeEventListener("mouseup",s,!0),document.removeEventListener("touchend",s,!0),document.removeEventListener("click",s,!0),document.removeEventListener("mousemove",l,!0),document.removeEventListener("touchmove",c,!0),y)return!0;if(!1===O.startDrag(o))return t.preventDefault(),t.stopPropagation(),!1;y=!0;var L=null;void 0===E.element&&(L=new d(u()));var w=document.createElement("div");w.className="drag-overlay",w.setAttribute("unselectable","on"),w.style.zIndex=9999,w.style.position="fixed",w.style.top=0,w.style.left=0,w.style.right=0,w.style.bottom=0,w.style.cursor="move",w.style.backgroundColor="white",w.style.opacity=0,w.style.filter="alpha(opacity=0)",w.style.userSelect="none",w.style.webkitUserSelect="none",w.style.MozUserSelect="none",w.style.msUserSelect="none",w.style.OUserSelect="none",w.addEventListener("selectstart",f),document.body.appendChild(w),E.element&&(L=new d(m())),L&&L.updatePosition(o);var b=null,D=!1;v||w.addEventListener("mousedown",r),w.addEventListener("mousemove",p),document.addEventListener("touchmove",p),w.addEventListener("mouseup",g),document.addEventListener("touchend",g)}function s(t){return document.removeEventListener("mouseup",s,!0),document.removeEventListener("touchend",s,!0),document.removeEventListener("click",s,!0),document.removeEventListener("mousemove",l,!0),document.removeEventListener("touchmove",c,!0),document.removeEventListener("selectstart",f,!0),!0}function l(t){if(t.target.tagName&&"input"===t.target.tagName.toLowerCase())return!0;var e=a(o,"pageX")-a(t,"pageX"),n=a(o,"pageY")-a(t,"pageY");return Math.sqrt(Math.pow(e,2)+Math.pow(n,2))>w&&r(t),t.preventDefault(),t.stopPropagation(),!1}function c(t){if(t.target.tagName&&"input"===t.target.tagName.toLowerCase())return!0;var e=a(o,"pageX")-a(t,"pageX"),n=a(o,"pageY")-a(t,"pageY");Math.sqrt(Math.pow(e,2)+Math.pow(n,2))>w&&clearTimeout(h)}if(o.button>0)return!0;document.addEventListener("selectstart",f,!0),document.addEventListener("mouseup",s,!0),document.addEventListener("touchend",s,!0),document.addEventListener("click",s,!0);var h;return"touchstart"===o.type&&(h=setTimeout(function(){r(o)},t.unwrap(E.longTapDelay)),i.addEventListener("touchend",function(){clearTimeout(h)})),document.addEventListener("mousemove",l,!0),document.addEventListener("touchmove",c,!0),!0}var E=t.utils.unwrapObservable(o()),L=E.name,w="number"==typeof E.dragDistance?E.dragDistance:10;if(h[L]=h[L]||[],p[L]=p[L]||[],!L||"string"!=typeof L)throw new Error("A drag zone must specify a name");if(!E.disabled){var b=E.data||c&&c.$data,O=new l({element:i,name:L,data:b,dragStart:E.dragStart,dragEnd:E.dragEnd});void 0===E.longTapDelay&&(E.longTapDelay=500),i.addEventListener("selectstart",function(t){return!(!t.target.tagName||"input"!==t.target.tagName.toLowerCase()&&"textarea"!==t.target.tagName.toLowerCase())||(t.preventDefault(),t.stopPropagation(),!1)},!0),n(i,"draggable",!0),i.addEventListener("mousedown",g),i.addEventListener("touchstart",g),t.utils.domNodeDisposal.addDisposeCallback(i,function(){document.removeEventListener("selectstart",f,!0)})}}},scrollableOnDragOver:{init:function(e,n,i){function o(){g.scroll(h,p)}function r(t){g=new c(e,l.delay),m=setInterval(o,100)}function s(t){h=a(t,"pageX"),p=a(t,"pageY")}function d(t){clearInterval(m)}var l=t.utils.unwrapObservable(n());("string"==typeof l||Array.isArray(l))&&(l={accepts:l}),l.delay=l.delay||0;var h,p,m,f=u(l),g=null;t.bindingHandlers.dragEvents.init(e,function(){return{accepts:f,dragEnter:r,dragOver:s,dragLeave:d}})}}})});
