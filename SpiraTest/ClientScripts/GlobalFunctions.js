// Contains any global functions/constants that are not control/page specific
Type.registerNamespace('Inflectra.SpiraTest.Web');
Inflectra.SpiraTest.Web.GlobalFunctions = function ()
{
    //Internal variables
    this._spinnerCount = 0;
    this._activitySpinner = null;
    this._suppressErrors = false;
    this._windowOnBeforeUnloadHandler = null;
    this._audioContext = null;

    //Global constants
    this.keyPrefix = 'k';   //Used in JSON dictionaries
    this.dataType_DataItem = 'DataItem:tst.dataObjects';
    this.dataType_DataItemField = 'DataItemField:tst.dataObjects';
    this.artifactIdToken = '{art}';   //Used in passed-in URLs to controls
    this.projectIdToken = '{proj}';
    this.reportWindowOptions = 'height=600, width=800,status=yes, resizable=yes, scrollbars=yes, toolbar=yes,location=yes,menubar=yes';
    this.authenticationMessage = 'Authentication failed';
    this.authorizationMessage = 'Authorization failed';
    this.holdDelay = 200; // Milliseconds to wait before recognizing a hold

    //ckEditor standard configuration
    this.ckEditor_toolbarNormal = [{ name: 'styles', items: ['Format', 'Font', 'FontSize'] }, { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', '-', 'RemoveFormat'] }, { name: 'colors', items: ['TextColor', 'BGColor'] }, { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight'] }, { name: 'insert', items: ['Link', 'Unlink', '-', 'Image', 'CodeSnippet', 'Table', 'HorizontalRule', '-', 'PasteFromWord', '-', 'CreateToken'] }, { name: 'tools', items: ['Maximize', '-', 'Source', '-', 'Templates', 'UIColor', 'ShowBlocks'] }];

    //ArtifactFieldType constants
    this._fieldType_text = 1;
    this._fieldType_lookup = 2;
    this._fieldType_dateTime = 3;   /* Actually Date Only */
    this._fieldType_identifier = 4;
    this._fieldType_equalizer = 5;
    this._fieldType_nameDescription = 6;
    this._fieldType_customPropertyLookup = 7;
    this._fieldType_integer = 8;
    this._fieldType_timeInterval = 9;
    this._fieldType_flag = 10;
    this._fieldType_hierarchyLookup = 11;
    this._fieldType_html = 12;
    this._fieldType_decimal = 13;
    this._fieldType_customPropertyMultiList = 14;
    this._fieldType_customPropertyDate = 15;
    this._fieldType_multiList = 16;

    //Parameters
    this.parameterThemeName = 'themeName';

    //Report formats
    this.reportFormatEnum = {
        Html: 1,
        MsWord2003: 2,
        MsExcel2003: 3,
        MsProj2003: 4,
        Xml: 5,
        MsWord2007: 6,
        MsExcel2007: 7,
        Pdf: 8
    };

    Inflectra.SpiraTest.Web.GlobalFunctions.initializeBase(this);
}

Inflectra.SpiraTest.Web.GlobalFunctions.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.GlobalFunctions.callBaseMethod(this, 'initialize');
    },
    dispose: function ()
    {
        Inflectra.SpiraTest.Web.GlobalFunctions.callBaseMethod(this, 'dispose');
    },

    // Properties
    get_suppressErrors: function ()
    {
        return this._suppressErrors;
    },
    set_suppressErrors: function (value)
    {
        this._suppressErrors = value;
    },

    display_spinner: function ()
    {
        // check if the global nav react component exists - ie has fully mounted
        if (window && window.rct_comp_globalNav && window.rct_comp_globalNav.spinnerShow) {
            window.rct_comp_globalNav.spinnerShow();
        }
    },
    hide_spinner: function ()
    {
        // check if the global nav react component exists - ie has fully mounted
        if (window && window.rct_comp_globalNav && window.rct_comp_globalNav.spinnerHide) {
            window.rct_comp_globalNav.spinnerHide();
        }
    },

    clearContent: function (element)
    {
        if (element.firstChild)
        {
            while (element.firstChild)
            {
                element.removeChild(element.firstChild);
            }
        }
    },

    //Makes a simple beeping sound
    //http://stackoverflow.com/questions/24151121/how-to-play-wav-audio-byte-array-via-javascript-html5
    beep: function (duration, url, finishedCallback)
    {
        try
        {
            if (window.AudioContext || window.webkitAudioContext)
            {
                var context;
                if (this._audioContext)
                {
                    context = this._audioContext;
                }
                else
                {
                    context = new (window.AudioContext || window.webkitAudioContext);
                    this._audioContext = context;
                }
                if (context)
                {
                    /*var osc = ctx.createOscillator();
                    osc.frequency.value = 400;

                    if (!duration)
                    {
                        duration = 100;
                    }*/

                    if (url)
                    {
                        var buffer = null;
                        var request = new XMLHttpRequest();
                        request.open('GET', url, true);
                        request.responseType = 'arraybuffer';

                        // Decode asynchronously
                        request.onload = function ()
                        {
                            context.decodeAudioData(request.response, function (theBuffer)
                            {
                                buffer = theBuffer;
                                var source = context.createBufferSource(); // creates a sound source
                                source.buffer = buffer;                    // tell the source which sound to play
                                source.connect(context.destination);       // connect the source to the context's destination (the speakers)
                                source.start(0);
                            }, function (error)
                            {
                            });
                        }
                        request.send();
                        buffer.start(0);

                    }
                    /*else
                    {
                        osc.type = 3;
                    }

                    osc.connect(ctx.destination);
                    osc.start(0);

                    setTimeout(function ()
                    {
                        osc.stop(0);
                        if (finishedCallback)
                        {
                            finishedCallback();
                        }
                    }, duration);*/
                }
            }
        }
        catch (e)
        {
            //Audio API not supported
        }
    },

    //Formats an artifact id as [XX:000000]
    formatArtifactId: function (prefix, artifactId)
    {
        var idString = artifactId.toString();
        if (idString.length < 6)
        {
            idString = ('0000000000' + idString).slice(-6);
        }
        return '[' + prefix + ':' + idString + ']';
    },

    //Sets the images on a hierarchical dropdown based on artifact type
    getHierarchyLookupImages: function (dropdown, artifact)
    {
        if (dropdown)
        {
            if (artifact == 'Release' || artifact == 'ReleaseId' || artifact == 'DetectedReleaseId' || artifact == 'ResolvedReleaseId' || artifact == 'VerifiedReleaseId')
            {
                dropdown.set_itemImage('Images/artifact-Release.svg');
                dropdown.set_summaryItemImage('Images/artifact-Release.svg');
                dropdown.set_alternateItemImage('Images/artifact-Iteration.svg');
            }
            if (artifact == 'TestSet' || artifact == 'TestSetId')
            {
                dropdown.set_itemImage('Images/artifact-TestSet.svg');
                dropdown.set_summaryItemImage('Images/FolderOpen.svg');
            }
            if (artifact == 'Requirement' || artifact == 'RequirementId')
            {
                dropdown.set_itemImage('Images/artifact-Requirement.svg');
                dropdown.set_summaryItemImage('Images/artifact-RequirementSummary.svg');
            }
        }
    },

    //Attachment types
	//PCS
    productEnum: {
        "spiraTest": "SpiraTest",
        "spiraTeam": "SpiraTeam",
		"spiraPlan": "SpiraPlan",
		"validationMaster": "ValidationMaster"
    },

    artifactTypeEnum: {
        "administration": -6,
        "automationEngine": 10,
        "automationHost": 9,
        "build": -27,
        "document": 13,
        "errorPage": -7,
        "incident": 3,
        "login": -2,
        "message": -2,
        "myPage": -3,
        "myProfile": -8,
        "myTimecard": -24,
        "none": 0,
        "placeholder": 11,
        "project": -1,
        "programHome": -12,
        "programIncidents": -35,
        "programPlanningBoard": -36,
        "programReleases": -34,
        "projectHome": -4,
        "release": 4,
        "reports": -5,
        "requirement": 1,
        "requirementStep": 12,
        "resource": -11,
        "risk": 14,
        "riskMitigation": 15,
        "sourceCode": -13,
        "sourceCodeRevisions": -28,
        "pullRequests": -38,
        "task": 6,
        "testCase": 2,
        "testConfigurationSet": -33,
        "testRun": 5,
        "testRunStep": -4,
        "testSet": 8,
        "testStep": 7,
        "user": -3,
        "portfolioHome": -401, /* neg as not artifact proper, 4 for the enum of the workspace type, 01 as homepage is the first page we made for portfolios */
        "enterpriseHome": -501 /* neg as not artifact proper, 5 for the enum of the workspace type, 01 as home is the first page we made for enterprise */
    },

    // display type enums used for association tabs on details pages
    displayTypeEnum: {
        None: 0,
        ArtifactLink: 1,
        Requirement_TestCases: 2,
        TestStep_Requirements: 3,
        TestCase_Releases: 4,
        TestCase_Requirements: 5,
        Release_TestCases: 6,
        Attachments: 10,
        Requirement_Tasks: 11,
        TestCase_Runs: 12,
        TestSet_Runs: 13,
        TestCase_Incidents: 14,
        TestSet_Incidents: 15,
        TestRun_Incidents: 16,
        TestCase_TestSets: 17,
        TestSet_TestCases: 18,
        TestStep_Incidents: 19,
        Build_Incidents: 20,
        Build_Associations: 21,
        Risk_Tasks: 22,
        SourceCodeRevision_Associations: 23,
        SourceCodeFile_Associations: 24
    },

    //Attachment types
    attachmentTypeEnum: {
        "file": 1,
        "url": 2,
        "sourceCode": -2
    },

    /// <summary>The different possible authorization states</summary>
    authorizationStateEnum: {
        'prohibited': 1, // Not authorized
        'authorized': 2, // Authorized for all instances of an artifact type
        'limited': 3     // Authorized for just the items created/assigned to the user
    },

    //The permissions
    permissionEnum: {
        'None': -1,
        'ProjectAdmin': -2,
        'SystemAdmin': -3,
		'ProjectGroupAdmin': -4,
		'ProjectTemplateAdmin': -5,
		'ReportAdmin': -6,
		'ResourceAdmin': -7,
		'PortfolioAdmin': -8,
        'Create': 1,
        'Modify': 2,
        'Delete': 3,
        'View': 4,
        'LimitedModify': 5,
        'BulkEdit': 6
    },

    //simple check for artifact types
    requirementTypeEnum: {
        "package": -1,
        "need": 1,
        "feature": 2,
        "useCase": 3,
        "userStory": 4,
        "quality": 5,
        "designElement": 6
    },

    //simple check for artifact types
    testCaseTypeEnum: {
        "acceptance": 1,
        "compatibility": 2,
        "functional": 3,
        "integration": 4,
        "loadPerformance": 5,
        "network": 6,
        "regression": 7,
        "scenario": 8,
        "security": 9,
        "unit": 10,
        "usability": 11,
        "exploratory": 12
    },

    //test run types
    testRunTypeEnum: {
        manual:  1,
        automated: 2
    },

    //returns an array of all artifact type names, ids, glyphs
    getArtifactTypes: function (typeToFilterBy, subTypeToFilterBy) {
        var resx = Inflectra.SpiraTest.Web.GlobalResources;
        var allTypes = [
            {
                id: 1,
                val: 'requirement',
                token: 'RQ',
                name: resx.ArtifactType_Requirement,
                image: 'Images/artifact-Requirement.svg',
                hasFolders: true,
                types: [
                    {
                        id: "summary",
                        image: "Images/artifact-RequirementSummary.svg"
                    },
                    {
                        id: "useCase",
                        image: "Images/artifact-UseCase.svg"
                    }
                ]
            },
            {
                id: 2,
                val: 'testcase',
                token: 'TC',
                name: resx.ArtifactType_TestCase,
                image: 'Images/artifact-TestCase.svg',
                hasFolders: true
            },
            {
                id: 3,
                val: 'incident',
                token: 'IN',
                name: resx.ArtifactType_Incident,
                image: 'Images/artifact-Incident.svg',
                hasFolders: false
            },
            {
                id: 4,
                val: 'release',
                token: 'RL',
                name: resx.ArtifactType_Release,
                image: 'Images/artifact-Release.svg',
                hasFolders: false,
                types: [
                    {
                        id: "iterationOrPhase",
                        image: "Images/artifact-Iteration.svg"
                    }
                ]
            },
            {
                id: 5,
                val: 'testrun',
                token: 'TR',
                name: resx.ArtifactType_TestRun,
                image: 'Images/artifact-TestRun.svg',
                hasFolders: false
            },
            {
                id: 6,
                val: 'task',
                token: 'TK',
                name: resx.ArtifactType_Task,
                image: 'Images/artifact-Task.svg',
                hasFolders: true
            },
            {
                id: 7,
                val: 'teststep',
                token: 'TS',
                name: resx.ArtifactType_TestStep,
                image: 'Images/artifact-TestStep.svg',
                hasFolders: true,
                foldersAreDifferentArtifactType: true
            },
            {
                id: 8,
                val: 'testset',
                token: 'TX',
                name: resx.ArtifactType_TestSet,
                image: 'Images/artifact-TestSet.svg',
                hasFolders: true
            },
            {
                id: 9,
                val: 'automationhost',
                token: 'AH',
                name: resx.ArtifactType_AutomationHost,
                image: 'Images/artifact-AutomationHost.svg',
                hasFolders: false
            },
            {
                id: 10,
                val: 'automationengine',
                token: 'AE',
                name: resx.ArtifactType_AutomationEngine,
                image: 'Images/artifact-AutomationHost.svg',
                hasFolders: false
            },
            {
                id: 12,
                val: 'requirementstep',
                token: 'RS',
                name: resx.ArtifactType_RequirementStep,
                image: 'Images/artifact-Requirement.svg',
                hasFolders: false
            },
            {
                id: 13,
                val: 'document',
                token: 'DC',
                name: resx.ArtifactType_Document,
                image: 'Images/artifact-Document.svg',
                hasFolders: true
            },
            {
                id: -1,
                val: 'project',
                token: 'PR',
                name: resx.Global_Project,
                image: 'Images/org-Project.svg',
                hasFolders: false
            },
            {
                id: -2,
                val: 'message',
                token: 'MS',
                name: resx.ArtifactType_Message,
                image: '',
                hasFolders: false
            },
            {
                id: -3,
                val: 'user',
                token: 'US',
                name: resx.ArtifactType_User,
                image: 'Images/artifact-Resource.svg',
                hasFolders: false
            },
            {
                id: -4,
                val: 'testrunstep',
                token: 'RS',
                name: resx.ArtifactType_TestRunStep,
                image: '',
                hasFolders: false
            },
            {
                id: 14,
                val: 'risk',
                token: 'RK',
                name: resx.ArtifactType_Risk,
                image: 'Images/artifact-Risk.svg',
                hasFolders: false
            },
            {
                id: 15,
                val: 'riskmitigation',
                token: 'RM',
                name: resx.ArtifactType_RiskMitigation,
                image: 'Images/artifact-RiskMitigation.svg',
                hasFolders: false
            },
            {
                id: -11,
                val: 'resources',
                token: 'US',
                name: resx.ArtifactType_User,
                image: 'Images/artifact-Resource.svg',
                hasFolders: false
            },
            {
                id: -13,
                val: 'sourcecode',
                token: 'SC',
                name: resx.ArtifactType_SourceCode,
                image: 'Images/artifact-SourceCode.svg',
                hasFolders: false
            },
            {
                id: -28,
                val: 'sourcecoderevision',
                token: 'SR',
                name: resx.ArtifactType_SourceCodeRevision,
                image: 'Images/artifact-Revision.svg',
                hasFolders: false
            },
            {
                id: -38,
                val: 'pullrequest',
                token: 'TK',
                name: resx.ArtifactType_PullRequest,
                image: 'Images/artifact-PullRequest.svg',
                hasFolders: false
            },
            {
                id: -23,
                val: 'planningboard',
                token: '',
                name: resx.ArtifactType_PlanningBoard,
                image: 'Images/artifact-PlanningBoard.svg',
                hasFolders: false
            },
            {
                id: -33,
                val: 'testconfiguration',
                token: '',
                name: resx.ArtifactType_TestConfigurationSet,
                image: 'Images/artifact-TestConfigurationSet.svg',
                hasFolders: false
            },
            {
                id: -36,
                val: 'programplanningboard',
                token: '',
                name: resx.ArtifactType_PlanningBoard,
                image: 'Images/artifact-PlanningBoard.svg',
                hasFolders: false
            },
            {
                id: -35,
                val: 'programincident',
                token: 'IN',
                name: resx.ArtifactType_Incident,
                image: 'Images/artifact-Incident.svg',
                hasFolders: false
            },
            {
                id: -34,
                val: 'programrelease',
                token: 'RL',
                name: resx.ArtifactType_Release,
                image: 'Images/artifact-Release.svg',
                hasFolders: false,
                types: [
                    {
                        id: "iterationOrPhase",
                        image: "Images/artifact-Iteration.svg"
                    }
                ]
            }
        ]
        //filter to a single type if requested
        var filteredTypes = function () {
            var singleArtifact;
            if (typeToFilterBy) {
                singleArtifact = allTypes.filter(function (item) {
                    return item.id == typeToFilterBy;
                });
                //additionally filter by subtype to get image token if one is requested
                if (subTypeToFilterBy && singleArtifact && singleArtifact[0].types) {
                    var subType = singleArtifact[0].types.filter(function (sub) {
                        return sub.id == subTypeToFilterBy;
                    });
                    singleArtifact[0].image = subType[0].image;
                }
            }
            return singleArtifact;
        };

        return typeToFilterBy ? filteredTypes() : allTypes;
    },



    //Is the current product authorized to access the artifact
    //Based on the method in Business > ArtifactManager.cs > IsSupportedByLicense
    //returns true if supported
    isSupportedByLicense: function(productType, artifactType)
    {
        var isSupported = true;
        //SpiraTeam and SpiraPlan handle all artifacts, SpiraTest does not
		//PCS:Comment
        //if (productType == this.productEnum.spiraTest)
        //{
        //    switch (artifactType) {
        //        case this.artifactTypeEnum.task:
        //        case this.artifactTypeEnum.message:
        //        case this.artifactTypeEnum.user:
        //        case this.artifactTypeEnum.myTimecard:
        //            isSupported = false;
        //            break;
        //    }
        //}
        ////SpiraPlan alone support group level artifacts
        //if (productType == this.productEnum.spiraTest || productType == this.productEnum.spiraTeam) {
        //    switch (artifactType) {
        //        case this.artifactTypeEnum.risk:
        //        case this.artifactTypeEnum.groupIncidents:
        //        case this.artifactTypeEnum.groupPlanningBoard:
        //        case this.artifactTypeEnum.groupReleases:
        //            isSupported = false;
        //            break;
        //    }
        //}

		//PCS:Comment
        return isSupported;
    },

    //Is the current user authorized to perform the permission on the artifact
    //returns: authorizationStateEnum value (1,2,3)
    isAuthorized: function(permissionId, artifactType)
    {
        //First check to make sure the product has access to the artifact
        if (!this.isSupportedByLicense(SpiraContext.ProductType, artifactType)) {
            return this.authorizationStateEnum.prohibited;
        }

        //Next handle the special case that we require a system administrator
        //This takes affect regardless of the artifact type or role id
        if (permissionId == this.permissionEnum.SystemAdmin)
        {
            if (SpiraContext.IsSystemAdmin)
                return this.authorizationStateEnum.authorized;
            else
                return this.authorizationStateEnum.prohibited;
        }

        //Now if the user is a system administrator, this automatically authorizes him/her for all other permissions
        if (SpiraContext.IsSystemAdmin)
        {
            return this.authorizationStateEnum.authorized;
        }

        //Handle the special case of the 'None' permission
        if (permissionId == this.permissionEnum.None)
        {
            return this.authorizationStateEnum.authorized;
        }

        //Now handle the case of a Project Group Admin
        if (permissionId == this.permissionEnum.ProjectGroupAdmin)
        {
            //See if the user is either a system admin or a project group admin
            if (SpiraContext.IsSystemAdmin || SpiraContext.IsGroupAdmin)
            {
                return this.authorizationStateEnum.authorized;
            }
            else
            {
                return this.authorizationStateEnum.prohibited;
            }
        }

        //Handle the special case of the project administrator permission
        if (permissionId == this.permissionEnum.ProjectAdmin)
        {
            //See if the user is either a system admin or a project group admin
            if (SpiraContext.IsSystemAdmin || SpiraContext.IsProjectAdmin)
            {
                return this.authorizationStateEnum.authorized;
            }
            else
            {
                return this.authorizationStateEnum.prohibited;
            }
        }

        //For the other cases we need to access the project role permissions
        var authorized = this.authorizationStateEnum.prohibited;

        //If we have a placeholder artifact, the permission is taken from the Incident artifact type
        if (artifactType == this.artifactTypeEnum.placeholder)
        {
            artifactType = this.artifactTypeEnum.incident;
        }
        //Test configurations use the permissions associated with test sets
        if (artifactType == this.artifactTypeEnum.testConfigurationSet) {
            artifactType = this.artifactTypeEnum.testSet;
        }

        //If the artifact type is source code file/revision and permission = view, allow if Edit Source Code
        if (permissionId == this.permissionEnum.View && (artifactType == this.artifactTypeEnum.sourceCode || artifactType == this.artifactTypeEnum.sourceCodeRevisions)) {
            if (SpiraContext.ProjectRole && SpiraContext.ProjectRole.IsSourceCodeView) {
                return this.authorizationStateEnum.authorized;
            }
            return this.authorizationStateEnum.prohibited;
        }
        //If the artifact type is source code file/revision and permission = modify, allow if Edit Source Code
        if (permissionId == this.permissionEnum.Modify && (artifactType == this.artifactTypeEnum.sourceCode || artifactType == this.artifactTypeEnum.sourceCodeRevisions))
        {
            if (SpiraContext.ProjectRole && SpiraContext.ProjectRole.IsSourceCodeEdit) {
                return this.authorizationStateEnum.authorized;
            }
            return this.authorizationStateEnum.prohibited;
        }

        //If the artifact type is set to None, but the permission is set to View,
        //need to determine if the user only has limited permissions
        if (permissionId == this.permissionEnum.View && artifactType == this.artifactTypeEnum.none)
        {
            if (SpiraContext.ProjectRole)
            {
                return (SpiraContext.ProjectRole.IsLimitedView) ? this.authorizationStateEnum.limited : this.authorizationStateEnum.authorized;
            }
            return this.authorizationStateEnum.prohibited;
        }

        //Next see if the user has the appropriate standard permissions from his/her role
        if (SpiraContext.ProjectRole && SpiraContext.Permissions)
        {
            var matchFound = false;
            for (var i = 0; i < SpiraContext.Permissions.length; i++)
            {
                var projectRolePermission = SpiraContext.Permissions[i];
                if (projectRolePermission.ArtifactTypeId == artifactType && projectRolePermission.PermissionId == permissionId)
                {
                    //See if we have the special case of a user who can only view their own items
                    matchFound = true;
                    if (permissionId == this.permissionEnum.View && SpiraContext.ProjectRole.IsLimitedView)
                    {
                        authorized = this.authorizationStateEnum.limited;
                    }
                    else
                    {
                        authorized = this.authorizationStateEnum.authorized;
                    }
                    break;
                }
            }
            if (!matchFound)
            {
                //See if we have the special case of a user who can only edit their own items
                if (permissionId == this.permissionEnum.Modify)
                {
                    var limitedModifyFound = false;
                    for (var i = 0; i < SpiraContext.Permissions.length; i++)
                    {
                        var projectRolePermission = SpiraContext.Permissions[i];
                        if (projectRolePermission.ArtifactTypeId == artifactType && projectRolePermission.PermissionId == this.permissionEnum.LimitedModify)
                        {
                            limitedModifyFound = true;
                            break;
                        }
                    }
                    if (limitedModifyFound)
                    {
                        authorized = this.authorizationStateEnum.limited;
                    }
                }
            }
        }

        return authorized;
    },

	//Is the current user authorized to modify the currently loaded artifact (ie a specific artifact with a specific id) - handles limited view permissions
    //returns: bool true if the user can modify 
	isAuthorizedToModifyCurrentArtifact: function (artifactType, ajxFormManager) {
		var canModify = false;
		var authorizedState = this.isAuthorized(this.permissionEnum.Modify, artifactType);
		if (authorizedState == this.authorizationStateEnum.authorized) {
			canModify = true;
		} else if (authorizedState == this.authorizationStateEnum.limited) {
			var isCreatorOrOwner = ajxFormManager.get_isArtifactCreatorOrOwner();
			if (isCreatorOrOwner) {
				canModify = true;
			}
		}
		return canModify;
	},

	//Check if current user can access particular admin functions
	//Param: array of ints that are specific enum permission - should match this.permissionEnum
	//Param: object of permission enumbers - should usually / always be globalFunctions.permissionEnum
	//returns: bool true if user can access those admin functions, and false if not or if no permissions are provided
	isAuthorizedAdminAccess: function (adminPermissions, permissionEnums) {
		var isAuthorized = false;

		// if no permissions provided do not allow access
		if (!adminPermissions || !adminPermissions.length) {
			return isAuthorized;
		}

		// go through each admin permission provided and check if the user has that permission
		for (var i = 0; i < adminPermissions.length; i++) {
			// only check a permission if the user is not already authorized
			if (isAuthorized) {
				break;
			}

			switch (adminPermissions[i]) {
				case permissionEnums.ProjectAdmin:
					isAuthorized = SpiraContext.IsProjectAdmin || false;
					break;
				case permissionEnums.SystemAdmin:
					isAuthorized = SpiraContext.IsSystemAdmin || false;
					break;
				case permissionEnums.ProjectGroupAdmin:
					isAuthorized = SpiraContext.IsGroupAdmin || false;
					break;
				case permissionEnums.ProjectTemplateAdmin:
					isAuthorized = SpiraContext.IsTemplateAdmin || false;
					break;
				case permissionEnums.ReportAdmin:
					isAuthorized = SpiraContext.IsReportAdmin || false;
					break;
				case permissionEnums.PortfolioAdmin:
					isAuthorized = SpiraContext.IsPortfolioAdmin || false;
					break;
				case permissionEnums.ResourceAdmin:
					isAuthorized = SpiraContext.IsResourceAdmin || false;
					break;
				}
		};
		return isAuthorized;
	},

    //Gets the default url for an artifact
    getArtifactDefaultUrl: function (appRoot, projectId, artifactField, artifactId)
    {
        //Handle the special case of no vdir
        if (appRoot.slice(-1) == '/')
        {
            appRoot = appRoot.slice(0, -1);
        }
        if (artifactField == "ReleaseId" || artifactField == "DetectedReleaseId" || artifactField == "ResolvedReleaseId" || artifactField == "VerifiedReleaseId")
        {
            return appRoot + '/' + projectId + '/Release/' + artifactId + '.aspx';
        }
        if (artifactField == "TestSetId")
        {
            return appRoot + '/' + projectId + '/TestSet/' + artifactId + '.aspx';
        }
        if (artifactField == "RequirementId")
        {
            return appRoot + '/' + projectId + '/Requirement/' + artifactId + '.aspx';
        }
        if (artifactField == "TestExecute") {
            return appRoot + '/' + projectId + '/TestExecute/' + artifactId + '.aspx';
        }
        return '';
    },

    //Gets the default url for a workspace
    getWorkspaceDefaultUrl: function (appRoot, workspaceEnum, workspaceId, productId) {
        //Handle the special case of no vdir
        if (appRoot.slice(-1) == '/') {
            appRoot = appRoot.slice(0, -1);;
        }

        if (workspaceEnum != undefined && workspaceId) {
            switch (workspaceEnum) {
                case SpiraContext.WorkspaceEnums.enterprise:
                    return appRoot + '/Enterprise/Default.aspx';
                    break;
                case SpiraContext.WorkspaceEnums.portfolio:
                    return appRoot + '/pf/' + workspaceId + '.aspx';
                    break;
                case SpiraContext.WorkspaceEnums.program:
                    return appRoot + '/pg/' + workspaceId + '.aspx';
                    break;
                case SpiraContext.WorkspaceEnums.product:
                    return appRoot + '/' + workspaceId + '.aspx';
                    break;
                case SpiraContext.WorkspaceEnums.product:
                    return appRoot + '/' + workspaceId + '.aspx';
                    break;
                case SpiraContext.WorkspaceEnums.template:
                    return appRoot + '/pt/' + workspaceId + 'Administration/Default.aspx';
                    break;
            }

            if (productId) {
                switch (workspaceEnum) {
                    case SpiraContext.WorkspaceEnums.release:
                    case SpiraContext.WorkspaceEnums.sprint:
                        return appRoot + '/' + productId + '/Release/' + workspaceId + '.aspx';
                        break;
                    case SpiraContext.WorkspaceEnums.releaseTask:
                    case SpiraContext.WorkspaceEnums.sprintTask:
                        return appRoot + '/' + productId + '/Task/' + workspaceId + '.aspx';
                        break;
                }
            }
        }

        return "";
    },

    //Encodes an values as a .NET typecode + string representation
    serializeValueInt: function (input)
    {
        return '0009' + input;
    },
    serializeValueString: function (input)
    {
        return '0018' + input;
    },
    serializeValueBool: function (input)
    {
        return '0003' + input;
    },

    //Decodes a .NET typecode + string representation back into its original form
    deserializeValueInt: function (input) {
        return input.replace(/^(0009)/, "");
    },
    deserializeValueString: function (input) {
        return input.replace(/^(0018)/, "");
    },
    deserializeValueBool: function (input) {
        return input.replace(/^(0003)/, "");
    },

    //Encodes the custom types with an x-prefix so that we know they're not .NET typecodes
    serializeValueDateRange: function (input)
    {
        return 'x-dr' + input;
    },

    //Parses a JSON date in format /Date(1245398693390[-0500])/ to a JS date object
    parseJsonDate: function (jsonDate)
    {
        var reDate = /^\/Date\((\d+)([\+\-]\d*)\)\//;
        if (reDate.test(jsonDate))
        {
            var m = reDate.exec(jsonDate);
            var d = new Date();
            //We need to adjust the date to factor out the browser timezone
            var userOffset = d.getTimezoneOffset() * 60000;
            var serverTicks = parseInt(m[1]);
            var serverOffset = 0;
            if (m.length >= 3)
            {
                var offset = Number(m[2]);
                var hoursOffset = Math.floor(offset / 100);
                var minsOffset = offset % 100;
                serverOffset = ((hoursOffset * 60) + minsOffset) * 60000;
            }
            d = new Date(serverTicks + userOffset + serverOffset);
            return d;
        }
        else
        {
            //WCF bug where UTC dates come back already parsed!
            return jsonDate;
        }
    },

    //Creates a JSON date in format /Date(1245398693390)/ from a JS date object - no time zone required
    createJsonDate: function (/**Date*/dateObj)
    {
        if (dateObj) {
            var dateInMsSince1970 = dateObj.getTime();
            return "/Date(" + dateInMsSince1970 + ")/";
        }
        return null;
    },

    //Creates a WCF friendly JSON date with time zone removed
    createJsonDateFromMoment: function (moment)
    {
        if (moment)
        {
            //We need to adjust the date to factor out the browser timezone
            var dateInMsSince1970 = moment.utc().valueOf();
            return "/Date(" + dateInMsSince1970 + ")/";
        }
        return null;
    },

    //Trims a string
    trim: function (str)
    {
        while (str.substring(0, 1) == ' ')
        {
            str = str.substring(1, str.length);
        }
        while (str.substring(str.length - 1, str.length) == ' ')
        {
            str = str.substring(0, str.length - 1);
        }
        return str;
    },
    isInteger: function (str)
    {
        var n = this.trim(str);
        return n.length > 0 && !(/[^0-9]/).test(str);
    },
    isNullOrUndefined: function (val)
    {
        if (typeof (val) == 'undefined' || val == null)
        {
            return true;
        }
        return false;
    },
    displayIfDefined: function (str)
    {
        if (typeof (str) == 'undefined' || str == null)
        {
            return '';
        }
        return str;
	},


	htmlDecode: function (value) {
		return $('<textarea/>').html(value).text();
	},
	htmlEncode: function (value) {
		return $('<textarea/>').text(value).html(); 
	},

    cleanHtml: function (parent)
	{
		// do not attempt to clean html if the parent does not exist or have any childNodes
		if (!parent || !parent.childNodes || !parent.childNodes.length > 0) {
			return;
		}
		var nodesToRemove = new Array();
        for (var i = 0; i < parent.childNodes.length; i++)
        {
            var childNode = parent.childNodes[i];
            if (childNode.hasChildNodes)
            {
                this.cleanHtml(childNode);
            }
            //Remove the event attributes (anything beginning with on)
            if (childNode.attributes)
            {
                for (var j = 0; j < childNode.attributes.length; j++)
                {
                    var name = childNode.attributes[j].nodeName;
                    if (name.length > 2 && name.substr(0, 2) == 'on' && childNode.attributes[j].nodeValue)
                    {
                        childNode.attributes[j].nodeValue = '';
                    }
                }
			}

			//Remove any IFRAMEs
			if (childNode.tagName == 'IFRAME') {
				nodesToRemove.push(childNode);
			}

            //Clear any handlers added through AJAX
            $clearHandlers(childNode);
		}

		//Remove any dangerous nodes
		for (var j = 0; j < nodesToRemove.length; j++) {
			parent.removeChild(nodesToRemove[j]);
		}
    },

    convertIntArrayToString: function (array)
    {
        var str = '';
        for (var i = 0; i < array.length; i++)
        {
            if (str == '')
            {
                str += array[i];
            }
            else
            {
                str += ',' + array[i];
            }
        }
        return str;
    },

    getposOffset: function (obj, offsettype)
    {
        var position = $(obj).position();
        var totaloffset = (offsettype == "left") ? position.left : position.top;
        return totaloffset;
    },

    getScrollTop: function ()
    {
        return (document.documentElement && document.documentElement.scrollTop) || document.body.scrollTop;
    },
    getScrollLeft: function ()
    {
        return (document.documentElement && document.documentElement.scrollLeft) || document.body.scrollLeft;
	},

	//takes an HTML element
	//returns an object specifying ints for pixels top and left that the element is from the document as a whole
	//note: this is different to offsetLeft or left which is specific to the parent container of the element
	getElementWindowPosition(elem) { 
		var box = elem.getBoundingClientRect();

		var body = document.body;
		var docEl = document.documentElement;

		var scrollTop = window.pageYOffset || docEl.scrollTop || body.scrollTop;
		var scrollLeft = window.pageXOffset || docEl.scrollLeft || body.scrollLeft;

		var clientTop = docEl.clientTop || body.clientTop || 0;
		var clientLeft = docEl.clientLeft || body.clientLeft || 0;

		var top = box.top + scrollTop - clientTop;
		var left = box.left + scrollLeft - clientLeft;

		return { top: Math.round(top), left: Math.round(left) };
	},

    clearbrowseredge: function (obj, whichedge)
    {
        var ie4 = document.all
        var ns6 = document.getElementById && !document.all

        var edgeoffset = 0
        if (whichedge == "rightedge")
        {
            var windowedge = ie4 && !window.opera ? this.ietruebody().scrollLeft + this.ietruebody().clientWidth - 15 : window.pageXOffset + window.innerWidth - 15
            obj.contentmeasure = obj.offsetWidth
            if (windowedge - obj.x < obj.contentmeasure)
            {
                edgeoffset = obj.contentmeasure - (windowedge - obj.x);
            }
        }
        else
        {
            var topedge = ie4 && !window.opera ? this.ietruebody().scrollTop : window.pageYOffset
            var windowedge = ie4 && !window.opera ? this.ietruebody().scrollTop + this.ietruebody().clientHeight - 15 : window.pageYOffset + window.innerHeight - 18
            obj.contentmeasure = obj.offsetHeight
            if (windowedge - obj.y < obj.contentmeasure)
            {
                //move up?
                edgeoffset = obj.contentmeasure + obj.offsetHeight
                if ((obj.y - topedge) < obj.contentmeasure) //up no good either?
                    edgeoffset = obj.y + obj.offsetHeight - topedge
            }
        }
        return edgeoffset
    },

    onDOMReady: function (fn, ctx)
    {
        var ready, timer;
        var onChange = function (e)
        {
            if (e && e.type == "DOMContentLoaded")
            {
                fireDOMReady();
            } else if (e && e.type == "load")
            {
                fireDOMReady();
            } else if (document.readyState)
            {
                if ((/loaded|complete/).test(document.readyState))
                {
                    fireDOMReady();
                } else if (!!document.documentElement.doScroll)
                {
                    try
                    {
                        ready || document.documentElement.doScroll('left');
                    } catch (e)
                    {
                        return;
                    }
                    fireDOMReady();
                }
            }
        };

        var fireDOMReady = function ()
        {
            if (!ready)
            {
                ready = true;
                fn.call(ctx || window);
                if (document.removeEventListener)
                    document.removeEventListener("DOMContentLoaded", onChange, false);
                document.onreadystatechange = null;
                window.onload = null;
                clearInterval(timer);
                timer = null;
            }
        };

        if (document.addEventListener)
            document.addEventListener("DOMContentLoaded", onChange, false);
        document.onreadystatechange = onChange;
        timer = setInterval(onChange, 5);
        window.onload = onChange;
    },

    contains: function (node, child)
    {
        //Handle nulls safely
        if (!child)
        {
            return false;
        }

        //See if the nodes are the same
        if (node == child)
        {
            return true;
        }

        //See if we have the built-in contains function already
        if (node.contains)
        {
            return node.contains(child);
        }
        var b = child;
        while (b.parentNode)
        {
            if ((b = b.parentNode) == node)
            {
                return true;
            }
        }
        return false;
    },

    ietruebody: function ()
    {
        return (document.compatMode && document.compatMode != "BackCompat") ? document.documentElement : document.body;
    },

    clear_errors: function (messageBox)
    {
        if (messageBox)
        {
            messageBox.className = 'alert alert-hidden';
            $get(messageBox.id + '_text').innerHTML = '';
        }
    },
    display_error_message: function (messageBox, message, isModal, glyphClasses)
    {
        //Make sure these errors are not due to ajax calls being terminated during a page unload
        if (!this._suppressErrors)
        {
            var glyph = !glyphClasses ? "" : glyph = '<span class="' + glyphClasses + '"></span>';

            if (messageBox)
            {
                messageBox.className = 'alert alert-danger';
                $get(messageBox.id + '_text').innerHTML = glyph + message;
            }
            else
            {
                this.globalAlert(message, "danger", isModal, null, glyphClasses);
            }
        }
    },
    display_info_message: function (messageBox, message)
    {
        if (messageBox)
        {
            messageBox.className = 'alert alert-info';
            $get(messageBox.id + '_text').innerHTML = message;
        }
        else
        {
            this.globalAlert(message, "info");
        }
    },
    display_error: function (messageBox, exception)
    {
        //Make sure these errors are not due to ajax calls being terminated during a page unload
        if (!this._suppressErrors)
        {
            var errorMessage = Inflectra.SpiraTest.Web.GlobalResources.GlobalFunctions_DefaultErrorMessage;
            var errorType = Inflectra.SpiraTest.Web.GlobalResources.GlobalFunctions_DefaultErrorType;
            if (exception.get_exceptionType && exception.get_exceptionType())
            {
                errorType = exception.get_exceptionType();
                if (errorType == 'System.InvalidOperationException')
                {
                    errorType = Inflectra.SpiraTest.Web.GlobalResources.GlobalFunctions_InvalidOperation;
                }
            }
            if (exception.get_message && exception.get_message())
            {
                errorMessage = exception.get_message();
            }
            else
            {
                errorMessage = exception;
            }
            var errorDisplay = errorType + ': ' + errorMessage;

            //If we have a data validation, only display the message (friendly error)
            if (errorType == 'DataValidationException')
            {
                errorDisplay = errorMessage;
            }
            if (errorType == 'DataValidationExceptionEx')
            {
                var messages = eval("(" + errorMessage + ")");
                if (messages.length > 0)
                {
                    errorDisplay = messages[0].Message;
                }
            }
            //Uncomment this for more detailed error reporting
            //errorDisplay = errorType + ': ' + errorMessage + ' - ' + exception.get_stackTrace();
            if (messageBox)
            {
                messageBox.className = 'alert alert-danger';
                $get(messageBox.id + '_text').innerHTML = errorDisplay;
            }
            else
            {
                alert(errorDisplay);
            }
        }
    },

    // unmounts and removes any react stuff inside of the global dialog box
    dlgGlobalDynamicClear: function () {
        ReactDOM.unmountComponentAtNode(document.getElementById('dlgGlobalDynamic'));
    },

    // displays an alert box with information 
    // param: message (string) to display in the alert
    // param: type (string) to specify the styling of the alert box
    // param: isModal (bool) if true the alert will be displayed as a modal
    // param: childComponent (object) - a react component to display as part of the control
    // param: messageGlyph (string) - classes to add to show a glyph
    globalAlert: function (message, type, isModal, childComponent, glyphClasses) {
        if (message || childComponent) {
            globalFunctions.dlgGlobalDynamicClear();
            ReactDOM.render(
                React.createElement(RctMessagePopup, {
                    childComponent: childComponent,
                    isModal: isModal,
                    message: message,
                    type: type,
                    glyphClasses: glyphClasses
                }, null),
                document.getElementById('dlgGlobalDynamic')
            );
        }
    },

    // displays a custom confirm box with a specific message
    // param: message (string) to display in the confirm
    // param: type (string) to specify the styling of the alert box
    // param: confirmFunction (function) js function to call as a result of the confirm dialog
    // param: childComponent (object) - a react component to display as part of the control
    globalConfirm: function (message, type, confirmFunction, refs, childComponent) {
        if ((message || childComponent) && confirmFunction) {
            globalFunctions.dlgGlobalDynamicClear();
            ReactDOM.render(
                React.createElement(RctMessagePopup, {
                    childComponent: childComponent,
                    confirm: confirmFunction,
                    isModal: true,
                    message: message,
                    type: type,
                    refs: refs
                }, null),
                document.getElementById('dlgGlobalDynamic')
            );
        }
    },


    // Creates a navigatable URL. If we pass in http://www.x.com then it does nothing
    // however if we pass www.x.com then it prepends http:// (by default)
    formNavigatableUrl: function (url)
    {
        if (url.indexOf("://") == -1)
        {
            return "http://" + url;
        }
        else
        {
            return url;
        }
    },

    launchStandardReport: function (reportToken, format, filter, artifactId)
    {
        //Check the final URL will not be invalid by exceeding 2048 characters
        var MAX_ARTIFACT_LIST_LENGTH = 1800; // we need to leave enough space for the rest of the url so this must be substantially less than 2048
        if (artifactId.length > MAX_ARTIFACT_LIST_LENGTH)
        {
            //Be helpful and suggest how many artifacts they can select: take the string, cut it at the max length, then count how many items it contains
            var artifactsMax = artifactId.slice(0, MAX_ARTIFACT_LIST_LENGTH).split(",").length;
            var artifactsRecommended = Math.floor(artifactsMax / 10) * 10;
            //Form the message
            var resx = Inflectra.SpiraTest.Web.GlobalResources;
            var message = resx.ListPage_ExportToReport_MaxLengthExceeded.replace('{0}', artifactsRecommended);
            alert(message);
        }
        else 
        {
            //Get the report type from the format
            var reportFormatId;
            if (format == 'word')
            {
                reportFormatId = this.reportFormatEnum.MsWord2007;
            }
            else if (format == 'excel')
            {
                reportFormatId = this.reportFormatEnum.MsExcel2007;
            }
            else if (format == 'pdf')
            {
                reportFormatId = this.reportFormatEnum.Pdf;
            }
            else
            {
                //Default to HTML
                reportFormatId = this.reportFormatEnum.Html;            
            }

            //Get the standard report URL
            Inflectra.SpiraTest.Web.Services.Ajax.GlobalService.Global_GetStandardReportUrl(
                SpiraContext.ProjectId,
                reportToken,
                filter,
                reportFormatId,
                Function.createDelegate(this, this.launchStandardReport_success),
                Function.createDelegate(this, this.launchStandardReport_failure),
                artifactId
            );
        }

    },
    launchStandardReport_success: function (url, artifactId)
    {
        window.open(url + artifactId + '&' + this.parameterThemeName + '=' + SpiraContext.ThemeName, 'SpiraTestReportViewer', this.reportWindowOptions);
    },
    launchStandardReport_failure: function (ex, artifactId)
    {
        //Display error message
        this.globalFunctions.display_error(null, ex);
    },

    //Clears any fired .NET client-side validators on the page
    clearValidators: function ()
    {
        //clear out the existing text from all our summaries
        for (var vi in Page_ValidationSummaries)
        {
            var vs = Page_ValidationSummaries[vi];
            vs.style.display = "none";
            vs.innerHTML = "";
        }
        //Clear any text in the validators
        for (var v in Page_Validators)
        {
            var vo = Page_Validators[v];
            vo.style.visibility = 'hidden';
        }
    },

    insertAtCaret: function (areaId, text)
    {
        var txtarea = $get(areaId);
        var scrollPos = txtarea.scrollTop;
        var strPos = 0;
        var br = ((txtarea.selectionStart || txtarea.selectionStart == '0') ?
        "ff" : (document.selection ? "ie" : false));
        if (br == "ie")
        {
            txtarea.focus();
            var range = document.selection.createRange();
            range.moveStart('character', -txtarea.value.length);
            strPos = range.text.length;
        }
        else if (br == "ff") strPos = txtarea.selectionStart;

        var front = (txtarea.value).substring(0, strPos);
        var back = (txtarea.value).substring(strPos, txtarea.value.length);
        txtarea.value = front + text + back;
        strPos = strPos + text.length;
        if (br == "ie")
        {
            txtarea.focus();
            var range = document.selection.createRange();
            range.moveStart('character', -txtarea.value.length);
            range.moveStart('character', strPos);
            range.moveEnd('character', 0);
            range.select();
        }
        else if (br == "ff")
        {
            txtarea.selectionStart = strPos;
            txtarea.selectionEnd = strPos;
            txtarea.focus();
        }
        txtarea.scrollTop = scrollPos;
    },

    _onBeforeUnload: function ()
    {
        //Page is loading, which will cause spurious ajax errors
        this.set_suppressErrors(true);
    },




    // converts the standard server side url with ~ into a client side readable url
    // param: string url
    // return: new url
    replaceBaseUrl: function (url) {
        return SpiraContext && SpiraContext.BaseUrl && url ? url.replace(/~\//i, SpiraContext.BaseUrl) : url;
    },

    // resolves a url to add the base url to it
    // param: string url
    // return: new url
    // TODO: shb 2020-10 I think the first if check should ONLY check for the final character - not that the baseUrl is only a slash
    addBaseUrl: function (url) {
        if (SpiraContext.BaseUrl == '/') {
            return SpiraContext.BaseUrl + url;
        }
        else {
            return SpiraContext.BaseUrl + '/' + url;
        }
    },




    //Array functions
    //takes an array and returns a flat simple array of all unique values of the specified property
    listUniquesInArray: function (array, field) {
        return array
            .map(function (item) { return field ? item[field] : item; })
  			.reduce(function (p, c) { if (p.indexOf(c) < 0) p.push(c); return p; }, [])
    },

    //takes an array and returns a filtered array based on matches between one of its fields (optional) and a flat array list of values
    filterArrayByList: function (array, list, field) {
        return array
            .filter(function (item) {
                return field ? list.indexOf(item[field]) > -1 : list.indexOf(item) > -1; /*using indexOf instead of includes due to IE*/
            })
    },
    findAnyInArray: function (haystack, arr) {
        return arr.some(function (v) {
            return haystack.indexOf(v) >= 0;
        });
    },
    findItemInArray: function (haystack, val) {
        if (haystack && haystack.length && val) return haystack.indexOf(val) > -1;
    },

    // Object functions
    objectIsEmpty: function(obj) {
        for(var key in obj) {
            if(obj.hasOwnProperty(key))
                return false;
        }
        return true;
    }
};

Inflectra.SpiraTest.Web.GlobalFunctions.registerClass('Inflectra.SpiraTest.Web.GlobalFunctions', Sys.Component);


if (typeof (Sys) != 'undefined')
{
    Sys.Application.notifyScriptLoaded();
}

//Now instantiate one instance of this class and store as a global variable that all components can use
var globalFunctions = $create(Inflectra.SpiraTest.Web.GlobalFunctions);

//Register the handler for preventing page navigations throwing ajax errors
var globalFunctions_windowOnBeforeUnloadHandler = Function.createDelegate(globalFunctions, globalFunctions._onBeforeUnload);
$addHandler(window, 'beforeunload', globalFunctions_windowOnBeforeUnloadHandler);

//React helper command for finding ReactJS component associated with DOM node (equivalent to $find in ASP.NET AJAX)
// NOTE AS OF V6.0 of Spira this code does not appear to work. This could be because of upgrades to React
window.$react = function (domId)
{
    var dom = document.getElementById(domId);
    if (dom && dom.firstChild)
    {
        var reactDom = dom.firstChild;
        for (var key in reactDom)
        {
            if (key.startsWith("__reactInternalInstance$"))
            {
                var compInternals = reactDom[key]._currentElement;
                var compWrapper = compInternals._owner;
                var comp = compWrapper._instance;
                return comp;
            }
        }
    }
    return null;
};

// alternate react helper command - using new ref syntax - which requires explicit adding on the component
// On successful return it executes the method - but assumes no parameters are required
window.accessReact = function (component, method) {
    if (component && method && window[component] && window[component][method]) {
        return window[component][method]();
    } else {
        return null;
    }
}



// EVENT LISTENERS
window.addEventListener("load", function () {
    // handles copying textcontent of marked elements to the clipboard
    function handleCopyToClipbard() {
        var elementsWatchForCopy = document.querySelectorAll("[data-copytoclipboard");
        if (elementsWatchForCopy.length) {
            var textArea = document.createElement("textarea");
            textArea.className = "fixed top0 left0 w0 h0";
            textArea.visibility = "hidden";
            textArea.id = "copyToClipboardHiddenTextarea";
            document.body.appendChild(textArea);

            Array.prototype.forEach.call(elementsWatchForCopy, function (element) {
                element.addEventListener("click", function () {
                    var textContent = element.textContent;
                    var copyTextarea = document.getElementById("copyToClipboardHiddenTextarea");
                    copyTextarea.value = textContent;
                    copyTextarea.focus();
                    copyTextarea.select();

                    try {
                        var success = document.execCommand("copy");
                        //handle animation on success
                        element.classList.add("u-mini-bounceIn");
                        setTimeout(function () { element.classList.remove("u-mini-bounceIn") }, 1000);

                    } catch (err) {
                        console.log("could not copy the text as requested")
                    }
                })

            })
        }
    }
    handleCopyToClipbard();
});

// function for XSS.js management
//filterXSS options - display some styling for span tags only
var filterXssInlineStyleWhitelist = filterXSS.getDefaultWhiteList();
filterXssInlineStyleWhitelist.span.push('style');
filterXssInlineStyleWhitelist.span.push('class');
filterXssInlineStyleWhitelist.p.push('class');
filterXssInlineStyleWhitelist.ul.push('class');
filterXssInlineStyleWhitelist.mark = ['class'];
filterXssInlineStyleWhitelist.figure = ['class'];
filterXssInlineStyleWhitelist.figcaption = ['class'];
filterXssInlineStyleWhitelist.label = ['class'];
filterXssInlineStyleWhitelist.input = ['class', 'type', 'disabled', 'checked'];
var filterXssInlineStyleOptions = {
    whiteList: filterXssInlineStyleWhitelist
};
