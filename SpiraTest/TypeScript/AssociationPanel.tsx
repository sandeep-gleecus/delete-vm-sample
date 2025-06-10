///// <reference path="typings/react.d.ts"/>
///// <reference path="typings/react-dom.d.ts"/>
//import * as React from 'react';
//import * as ReactDOM from 'react-dom';


//external dependendencies (js libraries)
declare var React: any;
declare var ReactDOM: any;
declare var $: any;
declare var $find: any;
declare var Mousetrap: any;
declare var filterXSS: any;

//inflectra services
declare var globalFunctions: any;
declare var SpiraContext: any;
declare var Inflectra: any;
declare var pageCallbacks: any;

var resx = Inflectra.SpiraTest.Web.GlobalResources;

//global object accessible by the aspx page
//and for storing general information about associations
const panelAssociationAdd = {
    addPanelId: '' as string,
    addPanelObj: '' as any,
    lnkAddBtnId: '' as string,
    messageBox: '' as string,
    sortedGridId: '' as string,
    displayType: 0 as number,
    listOfViewableArtifactTypeIds: '' as string,
    displayTypeHasComment: [
        globalFunctions.displayTypeEnum.ArtifactLink,
        globalFunctions.displayTypeEnum.SourceCodeRevision_Associations,
        globalFunctions.displayTypeEnum.SourceCodeFile_Associations
    ],
    blob: resx.ArtifactLinkType_Related,
    artifactLinkList: [
        {
            name: resx.ArtifactLinkType_Related,
            id: 1
        }, {
            name: resx.ArtifactLinkType_Depends,
            id: 2
        }
    ],
    customSaveSuccessFunction: null, //optional - e.g. for use on test execution where main panel and sorted grid are not used

    //which artifacts are shared with different artifact types and their relevant association panels
    artifactsIncluded: function (art, dis) {
        var res;

        //STANDARD ASSOCIATION PANEL
        if (dis === globalFunctions.displayTypeEnum.ArtifactLink) {
            //RQ details - add RQ, IN
            if (art === globalFunctions.artifactTypeEnum.requirement) {
                res = [
                    globalFunctions.artifactTypeEnum.requirement,
                    globalFunctions.artifactTypeEnum.incident,
                    globalFunctions.artifactTypeEnum.risk
                ];

            //IN details - add RQ, TS, TK, IN
            } else if (art === globalFunctions.artifactTypeEnum.incident) {
                res = [
                    globalFunctions.artifactTypeEnum.requirement,
                    globalFunctions.artifactTypeEnum.testStep,
                    globalFunctions.artifactTypeEnum.task,
                    globalFunctions.artifactTypeEnum.incident,
                    globalFunctions.artifactTypeEnum.risk
                ];

            //TK details - add TK, IN
            } else if (art === globalFunctions.artifactTypeEnum.task) {
                res = [
                    globalFunctions.artifactTypeEnum.task,
                    globalFunctions.artifactTypeEnum.incident
                ];

                //DC Details - add relevant artifacts
            } else if (art === globalFunctions.artifactTypeEnum.document) {
                res = [
                    globalFunctions.artifactTypeEnum.requirement,
                    globalFunctions.artifactTypeEnum.release,
                    globalFunctions.artifactTypeEnum.testCase,
                    globalFunctions.artifactTypeEnum.testSet,
                    globalFunctions.artifactTypeEnum.testRun,
                    globalFunctions.artifactTypeEnum.testStep,
                    globalFunctions.artifactTypeEnum.automationHost,
                    globalFunctions.artifactTypeEnum.task,
                    globalFunctions.artifactTypeEnum.incident,
                    globalFunctions.artifactTypeEnum.risk
                ];

            // risk details - add relevant artifacts (tasks are handled by another control)
            } else if (art === globalFunctions.artifactTypeEnum.risk) {
                res = [
                    globalFunctions.artifactTypeEnum.requirement,
                    globalFunctions.artifactTypeEnum.risk,
                    globalFunctions.artifactTypeEnum.testCase,
                    globalFunctions.artifactTypeEnum.incident
                ];

            // test case details - general associations - ie tasks and risks
            } else if (art === globalFunctions.artifactTypeEnum.testCase) {
                res = [
                    globalFunctions.artifactTypeEnum.task,
                    globalFunctions.artifactTypeEnum.risk
                ];
            }

        //TEST COVERAGE PANELS
        //RQ, RL, TX DETAILS: add TCs
        } else if (art === globalFunctions.artifactTypeEnum.requirement && dis === globalFunctions.displayTypeEnum.Requirement_TestCases) {
            res = [globalFunctions.artifactTypeEnum.testCase];

        } else if (art === globalFunctions.artifactTypeEnum.release && dis === globalFunctions.displayTypeEnum.Release_TestCases) {
            res = [globalFunctions.artifactTypeEnum.testCase];

        } else if (art === globalFunctions.artifactTypeEnum.testSet && dis === globalFunctions.displayTypeEnum.Release_TestCases) {
            res = [globalFunctions.artifactTypeEnum.testCase];

        //REQUIREMENT COVERAGE
        //TC Details: add RQs
        } else if (art === globalFunctions.artifactTypeEnum.testCase && (dis === globalFunctions.displayTypeEnum.TestCase_Requirements || dis === globalFunctions.displayTypeEnum.TestStep_Requirements )) {
            res = [globalFunctions.artifactTypeEnum.requirement];

        //RELEASE COVERAGE
        //TC details: add RLs
        } else if (art === globalFunctions.artifactTypeEnum.testCase && dis === globalFunctions.displayTypeEnum.TestCase_Releases) {
            res = [globalFunctions.artifactTypeEnum.release];

        //TEST STEP INCIDENTS
        //TS details: add INs
        } else if (art === globalFunctions.artifactTypeEnum.testStep && dis === globalFunctions.displayTypeEnum.TestStep_Incidents)
        {
            res = [globalFunctions.artifactTypeEnum.incident];

        //TEST SET COVERAGE
        //TC details: add TXs
        } else if (art === globalFunctions.artifactTypeEnum.testCase && dis === globalFunctions.displayTypeEnum.TestCase_TestSets) {
            res = [globalFunctions.artifactTypeEnum.testSet];

        //add TCs to TC list on TX pages
        } else if (art === globalFunctions.artifactTypeEnum.testSet && dis === globalFunctions.displayTypeEnum.TestSet_TestCases) {
            res = [globalFunctions.artifactTypeEnum.testCase];

        //add RQs to RQ coverage on TC and TS pages
        } else if ((art === globalFunctions.artifactTypeEnum.testCase ||
            art === globalFunctions.artifactTypeEnum.testStep) &&
            (dis === globalFunctions.displayTypeEnum.TestCase_Requirements || dis === globalFunctions.displayTypeEnum.TestStep_Requirements) ) {
            res = [globalFunctions.artifactTypeEnum.requirement];

        //TEST EXECUTION
        //test execution pages: add IN to IN panel
        } else if (dis === globalFunctions.displayTypeEnum.TestRun_Incidents) {
            res = [globalFunctions.artifactTypeEnum.incident];

        //ATTACHMENTS
        //add attachments to on all attachment panels
        } else if (dis === globalFunctions.displayTypeEnum.Attachments)
        {
            //See if we're coming FROM or TO an attachment
            if (art === globalFunctions.artifactTypeEnum.document)
            {
                res = [
                    globalFunctions.artifactTypeEnum.requirement,
                    globalFunctions.artifactTypeEnum.release,
                    globalFunctions.artifactTypeEnum.testCase,
                    globalFunctions.artifactTypeEnum.testSet,
                    globalFunctions.artifactTypeEnum.testRun,
                    globalFunctions.artifactTypeEnum.testStep,
                    globalFunctions.artifactTypeEnum.automationHost,
                    globalFunctions.artifactTypeEnum.task,
                    globalFunctions.artifactTypeEnum.incident,
                    globalFunctions.artifactTypeEnum.risk
                ];
            }
            else
            {
                res = [globalFunctions.artifactTypeEnum.document];
            }
        }
        //SOURCE CODE REVISIONS
        //add association from Revision to all Spira artifacts
        else if (dis === globalFunctions.displayTypeEnum.SourceCodeRevision_Associations) {
            //See if we're coming FROM a source code revision
            if (art === globalFunctions.artifactTypeEnum.sourceCodeRevisions) {
                res = [
                    globalFunctions.artifactTypeEnum.requirement,
                    globalFunctions.artifactTypeEnum.release,
                    globalFunctions.artifactTypeEnum.testCase,
                    globalFunctions.artifactTypeEnum.testSet,
                    globalFunctions.artifactTypeEnum.testRun,
                    globalFunctions.artifactTypeEnum.testStep,
                    globalFunctions.artifactTypeEnum.automationHost,
                    globalFunctions.artifactTypeEnum.task,
                    globalFunctions.artifactTypeEnum.incident,
                    globalFunctions.artifactTypeEnum.risk
                ];
            }
            else {
                res = [globalFunctions.artifactTypeEnum.sourceCodeRevisions];
            }
        }
        //SOURCE CODE FILES
        //add association from Files to all Spira artifacts
        else if (dis === globalFunctions.displayTypeEnum.SourceCodeFile_Associations) {
            //See if we're coming FROM a source code file
            if (art === globalFunctions.artifactTypeEnum.sourceCode) {
                res = [
                    globalFunctions.artifactTypeEnum.requirement,
                    globalFunctions.artifactTypeEnum.release,
                    globalFunctions.artifactTypeEnum.testCase,
                    globalFunctions.artifactTypeEnum.testSet,
                    globalFunctions.artifactTypeEnum.testRun,
                    globalFunctions.artifactTypeEnum.testStep,
                    globalFunctions.artifactTypeEnum.automationHost,
                    globalFunctions.artifactTypeEnum.task,
                    globalFunctions.artifactTypeEnum.incident,
                    globalFunctions.artifactTypeEnum.risk
                ];
            }
            else {
                res = [globalFunctions.artifactTypeEnum.sourceCodeRevisions];
            }
        }

        // we have the full list of artifacts that the panel can associate with
        // filter this list to only those artifacts that the user can view
        const authorizedArtifacts = res.filter(x => globalFunctions.isAuthorized(globalFunctions.permissionEnum.View, x) === globalFunctions.authorizationStateEnum.authorized);
        return authorizedArtifacts;
    },
    parseArtifactArray: function (arr) {
        if (!arr || !arr.length) { return; }

        var res = arr.map(function (item) {
            var newItem = globalFunctions.getArtifactTypes(item)[0];
            return newItem;
        })
        return res;
    },

    artifactsList: function (artifactType, displayType) {
        var arr = panelAssociationAdd.artifactsIncluded(artifactType, displayType);
        if (!arr)
        {
            alert(resx.AssociationPanel_NoArtifactTypesCanBeAdded);
        }
        return panelAssociationAdd.parseArtifactArray(arr);
    },

    //show the add association panel and render in the react component
    showPanel: function () {
        $("#" + panelAssociationAdd.lnkAddBtnId).addClass('disabled');
        const hasArtifactsToAssociate = panelAssociationAdd.artifactsList(SpiraContext.ArtifactTypeId, panelAssociationAdd.displayType) != undefined;
        if (hasArtifactsToAssociate) {
            ReactDOM.render(
                <AssocationBox
                    displayType={panelAssociationAdd.displayType}
                    domId={panelAssociationAdd.addPanelId}
                    lnkAddBtnId={panelAssociationAdd.lnkAddBtnId}
                    messageBox={panelAssociationAdd.messageBox}
                    sortedGridId={panelAssociationAdd.sortedGridId}
                    projectId={SpiraContext.ProjectId}
                    customSaveSuccessFunction={panelAssociationAdd.customSaveSuccessFunction} //optional - e.g. for use on test execution where main panel and sorted grid are not used
                    />,
                document.getElementById(panelAssociationAdd.addPanelId)
            );
        }
        
    }
};

//the parent react component that handles all state
class AssocationBox extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);

        const artifactsIncluded = panelAssociationAdd.artifactsIncluded(SpiraContext.ArtifactTypeId, this.props.displayType),
            filterAllObj = {
                artifact: { name: resx.AssociationPanel_SelectArtifact, id: 0 },
                folder: { name: this.setAllFoldersName(null), id: 0, indentLevel: "" },
                project: { name: resx.Global_CurrentProject, id: 0, childIds: artifactsIncluded }
            },
            artifacts = panelAssociationAdd.artifactsList(SpiraContext.ArtifactTypeId, this.props.displayType),
            artifactTokens = artifacts.map(function (item) { return item.token; }),

            //convert the string of numbers from the code behind into an array of numbers (each as string)
            listOfViewableArtifactTypeIds = panelAssociationAdd.listOfViewableArtifactTypeIds.split(","),
            folders = [],
            projects = [];

        //put request to choose item at top of each array for the dropdowns
        artifacts.unshift(filterAllObj.artifact);
        folders.unshift(filterAllObj.folder);
        projects.unshift(filterAllObj.project);

        //convert array of numbers as strings to pure numbers
        var viewableArtifactTypes = listOfViewableArtifactTypeIds.map(function (item) { return parseInt(item, 10) });

        this.state = {
            searchTerm: '' as string,
            searchTermLastSent: '' as string,
            searchTermIsValidToken: false as boolean,
            searchCompleted: false,

            isTestCoverage: (this.props.displayType === globalFunctions.displayTypeEnum.Requirement_TestCases || this.props.displayType === globalFunctions.displayTypeEnum.Release_TestCases),
            isCreateFromCurrentArtifact: this.canCreateFromCurrentArtifact(this.props.displayType),
            newArtifactTypeToCreate:
                this.props.displayType === globalFunctions.displayTypeEnum.Requirement_TestCases ? globalFunctions.artifactTypeEnum.testCase :
                this.props.displayType === globalFunctions.displayTypeEnum.Release_TestCases ? globalFunctions.artifactTypeEnum.testSet :
                this.props.displayType === globalFunctions.displayTypeEnum.TestCase_Requirements ? globalFunctions.artifactTypeEnum.requirement :
                (SpiraContext.ArtifactTypeId === globalFunctions.artifactTypeEnum.incident && this.props.displayType === globalFunctions.displayTypeEnum.ArtifactLink) ? globalFunctions.artifactTypeEnum.requirement :
                (SpiraContext.ArtifactTypeId === globalFunctions.artifactTypeEnum.task && this.props.displayType === globalFunctions.displayTypeEnum.ArtifactLink) ? globalFunctions.artifactTypeEnum.incident :
                null,

            artifactId: SpiraContext.ArtifactId,
            artifactTypeId: SpiraContext.ArtifactTypeId,

            artifacts: artifacts,
            artifactsEnabled: artifactsIncluded as Array<Number>,
            artifactsPermissionToView: viewableArtifactTypes,
            artifactFilter: this.setInitialArtifactFilter(this.props.displayType) as number,
            artifactDropDownNotRequired: artifacts.length < 3,
            artifactTokens: artifactTokens,
            artifactTokenRegexPattern: '^\[?(' + artifactTokens.join('|') + '):?[0-9]+\]?' as string,

            folders: folders,
            folderFilter: 0 as number,
            onlyShowFolders: false, //by default show folders and their children

            projects: projects,
            projectFilter: 0 as number,

            results: [],
            resultsNotEmpty: false,

            browseFields: {
                project: 1,
                artifact: 2,
                folder: 3
            },

            selectionAssociations: [],
            selectionLength: 0 as number,

            showComment: globalFunctions.findItemInArray(panelAssociationAdd.displayTypeHasComment, this.props.displayType),
            linkType: 1 as number,
            linkTypeList: panelAssociationAdd.artifactLinkList,
            commentTerm: "" as string
        }

        // register functions
        this.getProject_success = this.getProject_success.bind(this);
        this.getFolderList = this.getFolderList.bind(this);
        this.getFolderList_success = this.getFolderList_success.bind(this);
        this.getFolderList_failure = this.getFolderList_failure.bind(this);
        this.setAllFoldersName = this.setAllFoldersName.bind(this);
        this.setEnabledArtifacts = this.setEnabledArtifacts.bind(this);
        this.resetArtifactSelection = this.resetArtifactSelection.bind(this);
        this.setInitialArtifactFilter = this.setInitialArtifactFilter.bind(this);
        this.browseProjectChange = this.browseProjectChange.bind(this);
        this.browseArtifactChange = this.browseArtifactChange.bind(this);
        this.browseFolderChange = this.browseFolderChange.bind(this);
        this.folderExpandCollapseClick = this.folderExpandCollapseClick.bind(this);
        this.searchTermChange = this.searchTermChange.bind(this);
        this.searchEnterPress = this.searchEnterPress.bind(this);
        this.searchClick = this.searchClick.bind(this);
        this.searchClick_success = this.searchClick_success.bind(this);
        this.searchClick_failure = this.searchClick_failure.bind(this);
        this.onHeaderClick = this.onHeaderClick.bind(this);
        this.onRowClick = this.onRowClick.bind(this);
        this.onExpandCollapseGridClick = this.onExpandCollapseGridClick.bind(this);
        this.onGridRowNameMouseEnter = this.onGridRowNameMouseEnter.bind(this);
        this.populateNameTooltip = this.populateNameTooltip.bind(this);
        this.selectAssociations = this.selectAssociations.bind(this);
        this.setLinkType = this.setLinkType.bind(this);
        this.commentChange = this.commentChange.bind(this);
        this.saveClick = this.saveClick.bind(this);
        this.saveClick_success = this.saveClick_success.bind(this);
        this.closeClick = this.closeClick.bind(this);
        this.createFromCurrentArtifact = this.createFromCurrentArtifact.bind(this);
        this.createFromCurrentArtifact_success = this.createFromCurrentArtifact_success.bind(this);

    }

    componentDidMount() {
        //set a flag in the global namespace that the component has been mounted
        SpiraContext.uiState[this.props.domId] = { isMounted: true };

        //create information to send to server to get projects
        var artifactId = this.state.artifactId,
            artifactTypeIds = panelAssociationAdd.artifactsIncluded(this.state.artifactTypeId, this.props.displayType),
            projectId = SpiraContext.ProjectId;

        //make call to server to retrieve projects
        Inflectra.SpiraTest.Web.Services.Ajax.AssociationService.Association_RetrieveForDestProjectAndArtifact(
            projectId,
            artifactTypeIds,
            this.props.displayType,
            this.getProject_success,
            this.getProject_failure);

        //get folders if display type is coverage - ie one with only one type of artifact
        if (this.props.displayType === globalFunctions.displayTypeEnum.Requirement_TestCases ||
            this.props.displayType === globalFunctions.displayTypeEnum.Release_TestCases ||
            this.props.displayType === globalFunctions.displayTypeEnum.TestStep_Requirements) {
            var projectId = this.state.projectFilter || this.props.projectId;
            this.getFolderList(globalFunctions.artifactTypeEnum.testCase, projectId)
        };

        //init bootstrap tooltips
        $('[data-toggle="tooltip"]').tooltip()
    }

    //handle getting list of projects to show in dropdown
    getProject_success(data) {
        if (data && data.length > 0) {
            // append the new data to the existing list of projects
            this.setState({ projects: [...this.state.projects, ...data] },
                () => {
                    //set initial list of artifact types user has permission to view
                    this.setEnabledArtifacts(this.state.projects, this.state.projectFilter, this.state.artifactsPermissionToView);
                }
            );
        } else {
            //set initial list of artifact types user has permission to view
            this.setEnabledArtifacts(this.state.projects, this.state.projectFilter, this.state.artifactsPermissionToView);
        }
    }
    getProject_failure () {
        //fail quietly
    }

    //handle getting list of folders (if relevant) to show in dropdown
    getFolderList (artifactTypeId, projectId) {
        var self = this,
            activeProjectId = projectId || this.props.projectId;
        if (artifactTypeId > 0 && activeProjectId > 0) {

            //Send information to server
            Inflectra.SpiraTest.Web.Services.Ajax.AssociationService.Association_RetrieveArtifactFolders(
                activeProjectId,
                artifactTypeId,
                self.getFolderList_success,
                self.getFolderList_failure);
        }
    }
    getFolderList_success (data) {
        var folderTitle = [];
        folderTitle.push(this.state.folders[0]);
        //set the 'all folder' item name to specific phrases where relevant
        var allFoldersName = this.setAllFoldersName(this.state.artifactFilter);
        folderTitle[0].name = allFoldersName;

        // get info about any hierarchically nested items
        var newData = this.setCollapsibleProperties(data);

        //set the state
        var newFolders = folderTitle.concat(newData);
        this.setState({
            folders: newFolders,
            folderFilter: 0
        }, this.searchClick);
    }
    getFolderList_failure () {
        //fail quietly
        this.setState({
            folders: [],
            folderFilter: 0
        });
    }

    //make sure the all folder name is customized to the artifact type
    setAllFoldersName (artifact) {
        var newName;
        switch (artifact) {
            case globalFunctions.artifactTypeEnum.testCase:
            case globalFunctions.artifactTypeEnum.testSet:
            case globalFunctions.artifactTypeEnum.testStep:
                newName = resx.Folders_Root;
                break;
            case globalFunctions.artifactTypeEnum.requirement:
                newName = resx.Folders_AllPackages;
                break;
            default:
                newName = resx.Global_AllFolders;
        }
        return newName;
    }

    //returns the input array with any hierarchies / indentation used to set the children contained by each parent
    setCollapsibleProperties (data) {
        if (!data) {
            return
        };
        var newData = data,
            l = data.length,
            i = 0;
        for (i; i < l; i++) {
            //only check for hierarchy if indents are present
            if (newData[i].indentLevel && newData[i].indentLevel.length > 0) {
                //if there is a proceeding item, check to see if it is a child of current item
                var nextItemIsChild = indentLevelIsGreater(newData, i, i + 1);

                if (nextItemIsChild) {
                    //set a new array on the item to track the children
                    newData[i].children = new Array;

                    //add a bool for checking the current expand/collapse state
                    //by default this is false, but this gets set once the collapsible list is sent to its next function
                    newData[i].hideChildren = false;

                    //then get the list of all children of the parent item
                    for (var j = i + 1; j < l; j++) {
                        if (indentLevelIsGreater(newData, i, j)) {
                            newData[i].children.push(newData[j].id);
                        } else {
                            break;
                        }
                    }
                }
            }
        }
        return newData;

        function indentLevelIsGreater(array, currentPosition, positionToCheck) {
            var currentIemExists = currentPosition < array.length ? true : false,
                currentItemIndentLevel = currentIemExists ? array[currentPosition].indentLevel.length : null,
                nextItemExists = positionToCheck < array.length ? true : false,
                nextItemIndentLevel = nextItemExists ? array[positionToCheck].indentLevel.length : null;
            return (currentIemExists && nextItemExists) ? nextItemIndentLevel > currentItemIndentLevel : false;
        }
    }

    //sets the correct show / hide state for all children of an item.
    toggleShowHideChildren (array, children, newHideChildrenStatus) {
        var newArray = array.map(function (item) {
            if (children.indexOf(item.id) >= 0) {
                item.hide = newHideChildrenStatus;
                if (item.children) {
                    item.hideChildren = newHideChildrenStatus;
                }
            }
            return item;
        });
        return newArray;
    }

    // artifacts are enabled based on whether they are shared from the currently selected project to the active project, and if user has permission to view (in active project)
    setEnabledArtifacts (projects, projectFilter, artifactsPermissionToView) {
        var sharedBySelectedProject = projects.filter(function (item) {
            return item.id === projectFilter;
        })[0],
            sharedArtifactTypeIds = sharedBySelectedProject.childIds;
        var sharedAndViewable = globalFunctions.filterArrayByList(sharedArtifactTypeIds, artifactsPermissionToView);

        this.setState({ artifactsEnabled: sharedAndViewable });

        //if the project does not have currently selected artifact enabled
        if (!globalFunctions.findItemInArray(sharedAndViewable, this.state.artifactFilter)) {
            //reset the artifactFilter and reset the search grid
            if(sharedAndViewable.length > 1) {
                this.resetArtifactSelection();
            } else {
                this.setState({ artifactFilter: sharedAndViewable[0] }, function () {
                    this.setArtifactFoldersAndResults(projectFilter)
                });
            }
        } else {
            this.setArtifactFoldersAndResults(projectFilter);
        }
    }

    //reset the artifacts - so no incorrect ones are shown (eg when switching projects) 
    resetArtifactSelection () {
        var cleanResults = [];
        this.setState({ 
            artifactFilter: 0,
            results: cleanResults
            } );
    }

    //set folders to right ones for the artifact and project, and refresh results
    setArtifactFoldersAndResults (projectFilter) {
        if (globalFunctions.getArtifactTypes(this.state.artifactFilter)[0].hasFolders) {
            this.getFolderList(this.state.artifactFilter, projectFilter);
        } else {
            this.searchClick(null);
        }
    }


    setInitialArtifactFilter (displayType) {
        var filter = 0;
        switch (displayType) {
            case globalFunctions.displayTypeEnum.ArtifactLink:
            case globalFunctions.displayTypeEnum.Attachments:
            case globalFunctions.displayTypeEnum.SourceCodeRevision_Associations:
            case globalFunctions.displayTypeEnum.SourceCodeFile_Associations:
                filter = 0;
                break;
            case globalFunctions.displayTypeEnum.Requirement_TestCases: 
            case globalFunctions.displayTypeEnum.Release_TestCases:
            case globalFunctions.displayTypeEnum.TestSet_TestCases:
                filter = globalFunctions.artifactTypeEnum.testCase;
                break;
            case globalFunctions.displayTypeEnum.TestCase_Requirements:
            case globalFunctions.displayTypeEnum.TestStep_Requirements:
                filter = globalFunctions.artifactTypeEnum.requirement;
                break;
            case globalFunctions.displayTypeEnum.TestCase_Releases:
                filter = globalFunctions.artifactTypeEnum.release;
                break;
            case globalFunctions.displayTypeEnum.TestRun_Incidents:
                filter = globalFunctions.artifactTypeEnum.incident;
                break;
            default:
                filter = 0;
                break;
        }
        return filter;
    }

    //handle changes in selection of project/artifact/folder (if relevant) in the top dropdowns
    browseProjectChange (projectFilter, e) {
        if (projectFilter >= 0) {
            //update the project, update disabled status of artifacts for project including based on user permissions
            this.setState({ projectFilter: projectFilter }, this.setEnabledArtifacts(this.state.projects, projectFilter, this.state.artifactsPermissionToView) );
        }
    }
    browseArtifactChange (filter, enabled, e) {
        //state for artifact filter is set in each parts of IF to minimize search click calls
        if (enabled && filter >= 0) {
            var projectId = this.state.projectFilter || this.props.projectId;
            if (globalFunctions.getArtifactTypes(filter)[0].hasFolders) {
                this.setState({
                    //close the dropdown, set the filter and then get the folder list
                    artifactDropdownOpen: false,
                    artifactFilter: filter,
                    onlyShowFolders: globalFunctions.getArtifactTypes(filter)[0].foldersAreDifferentArtifactType ? true : false
                }, this.getFolderList(filter, projectId));
            } else {
                this.setState({
                    //set the filter and initiate the search
                    artifactFilter: filter
                }, this.searchClick);
            };
        };
    }
    browseFolderChange (filter, e) {
        if (filter >= 0) {
            this.setState({ folderFilter: filter }, this.searchClick);
        } else {
            this.searchClick(null);
        }
    }

    //handle a click on any expand/collapse buttons in the folder dropdown
    folderExpandCollapseClick (children, newData, newHideChildrenStatus) {
        
        //cycle through all the children of the parent and toggle their show/hide status
        var toggleShowHideData = this.toggleShowHideChildren(newData, children, newHideChildrenStatus);

        this.setState({
            //set the children 
            folders: toggleShowHideData
        });
    }

    //manage the search box and button
    searchTermChange (event) {
        var searchTerm = event.target.value;

        //check term against regex to see if it is a valid token
        var regexMatch = false;
        var regExPattern = new RegExp (this.state.artifactTokenRegexPattern, "gi");
        if( searchTerm.match(regExPattern) ) {
            regexMatch = true;
        };

        //update state
        this.setState({ 
            searchTerm: searchTerm,
            searchTermIsValidToken: regexMatch
         });

    }
    searchEnterPress (event) {

        if (event.keyCode == 13) {
            var isEnter = 13;
            this.searchClick(isEnter);
        }
    }
    searchClick (isEnter) {

        //first check if artifact is active - and therefore search should be carried out
        var isSearchEnabled = this.state.artifactDropDownNotRequired || 
                this.state.artifactFilter > 0 || 
                this.state.searchTermIsValidToken;

        if (isSearchEnabled) {
            //then prepare the data for server.
            var self = this,
                artifactTypeId = this.state.artifactTypeId,
                artifactId = this.state.artifactId,
                projectId = this.props.projectId,
                searchArtifactTypeId = this.state.artifactFilter,
                searchFolderId = this.state.folderFilter ? this.state.folderFilter : null,
                searchProjectId = this.state.projectFilter || this.props.projectId,
                searchTerm = this.state.searchTerm ? this.state.searchTerm : null;

            //Send information to server to get results
            Inflectra.SpiraTest.Web.Services.Ajax.AssociationService.Association_SearchByProjectAndArtifact(
                projectId,
                artifactTypeId,
                artifactId,
                searchArtifactTypeId,
                searchFolderId,
                searchProjectId,
                searchTerm,
                self.searchClick_success,
                self.searchClick_failure );
        
        //13 is passed through when enter key is pressed purely to give it a specific value that can be tested 
        } else if (isEnter === 13) {
            alert(resx.AssociationPanel_SelectArtifact);
        }
    }
    searchClick_success(data) {
        var self = this,
            newData = this.setCollapsibleProperties(data).map(function (item) {
                if (item.children) item.hideChildren = self.state.onlyShowFolders;
                item.hide = item.children ? false : self.state.onlyShowFolders;
                return item;
            }),
            cleanResults = [];

        // when we perform a search we could be searching without setting an artifact, so update the artifact we are using
        // if we have no data then keep the artifact type filter we already have in state
        var artifactTypeId = data.length ? data[0].artifactTypeId : this.state.artifactFilter;
        //if the first search result is a test case we could be searching for test STEPS so check that
        if (artifactTypeId == globalFunctions.artifactTypeEnum.testCase && data.length > 1) {
            artifactTypeId = data[1].artifactTypeId;
        }

        //if we are switching artifact then make sure to get the folders we need as well - do this before set state so we don't have to make an extra network call if the state will not change
        if (this.state.artifactFilter !== artifactTypeId) {
            this.getFolderList(artifactTypeId, this.props.projectId)
        }

        //set state
        this.setState({
            results: cleanResults.concat(newData),
            resultsNotEmpty: data.length ? true : false,
            artifactFilter: artifactTypeId,
            searchCompleted: true,
            searchTermLastSent: this.state.searchTerm
        });
    }
    searchClick_failure (error) {
        var messageBox = document.getElementById(this.props.messageBox);
        globalFunctions.display_error(messageBox, error);
    }

    //handle events on the grid
    //...reverse the selection state of all displayed items (eg if none are selected, select all)
    onHeaderClick (event) {
        var newResults = this.state.results.map(function (item) {
            if (item.id > 0) {
                item.selected = !item.selected;
            }
            return item;
        });
        this.setState({
            results: newResults,
        });

        this.selectAssociations(newResults);
    }
    //...reverse the selection state of the specific row
    onRowClick (index, event) {
        //only take action if the row is enabled (should not be needed but for belts and braces)
        if (this.state.results[index].id < 0) return;

        var newSelectedValue = !this.state.results[index].selected;
        var newResults = this.state.results;
        newResults[index].selected = newSelectedValue;

        this.setState({
            results: newResults
        });
        
        this.selectAssociations(newResults);
    }

    //handle a click on any expand/collapse buttons in the grid
    onExpandCollapseGridClick (index, children, event) {
        event.stopPropagation();
        //toggle the hide children status on the parent item
        var newHideChildrenStatus = !this.state.results[index].hideChildren;
        var newData = this.state.results;
        newData[index].hideChildren = newHideChildrenStatus;

        //now cycle through all the children of the parent and toggle their show/hide status
        var toggleShowHideData = this.toggleShowHideChildren(newData, children, newHideChildrenStatus);

        this.setState({
            results: toggleShowHideData
        });
    }

    //on hovering of name of association on the grid, get tooltip information from service
    onGridRowNameMouseEnter (id, index, event) {
        //only make the service call if we haven't already for this particular row
        if (!this.state.results[index].nameTooltip) {
            Inflectra.SpiraTest.Web.Services.Ajax.AssociationService.Association_RetrieveTooltip(
                this.props.projectId,
                this.state.artifactFilter,
                id,
                this.populateNameTooltip,
                ajaxCall_failure,
                index
            );
        }
        function ajaxCall_failure(err) {
            //fail quietly
        };
    }

    // adds tooltip html to specific result row, takes deep copy to make sure update is handled properly
    // this is not the most performant way of handling this (deep copy is slow), but it does work fine
    populateNameTooltip (data, index) {
         var resultsDeepCopy = JSON.parse(JSON.stringify(this.state.results));
         resultsDeepCopy[index].nameTooltip = data;

         this.setState({ results: resultsDeepCopy });
    }

    //general function to set the selection from the results
    selectAssociations (results) {
        var selectedResults = results.filter(function (item) {
            return item.selected;
        })
        .map(function (item) {
            var obj = {
                projectId: item.projectId,
                artifactId: item.id,
                artifactTypeId: item.artifactTypeId
            };
            return obj;
        });
        this.setState({ selectionAssociations: selectedResults });
        this.setState({ selectionLength: selectedResults.length });
 
    }

    //manage the user selecting a link type and comment
    setLinkType (newType, e) {
        this.setState({ linkType: newType });
    }
    commentChange (event) {
        this.setState({ commentTerm: event.target.value });
    }

    //handling save of adding new associations
    saveClick (isActive, e) {
        if (isActive) {
            var self = this,
                projectId = this.props.projectId as number,
                artifactTypeId = SpiraContext.ArtifactTypeId as number,
                artifactId = this.state.artifactId,
                displayType = this.props.displayType as number,
                artifactLinkTypeId = this.state.linkType,
                comment = this.state.commentTerm,
                selectionAssociations = this.state.selectionAssociations;

            //See if we have any selected items to insert before (only used in ordered/hierarchical grids)
            var existingItemId: number = null;
            if (this.props.sortedGridId)
            {
                var grid: any = $find(this.props.sortedGridId);
                if (grid.get_selected_items && grid.get_selected_items() && grid.get_selected_items().length == 1)
                {
                    existingItemId = parseInt(grid.get_selected_items()[0]);
                }
            }

            if (!pageProps.isTestExecution) {
                //normally we just use the association service
                Inflectra.SpiraTest.Web.Services.Ajax.AssociationService.Association_Add(
                    projectId,
                    artifactTypeId,
                    artifactId,
                    displayType,
                    artifactLinkTypeId,
                    comment,
                    selectionAssociations,
                    existingItemId,
                    self.saveClick_success,
                    self.saveClick_failure);

            } else {
                //but for test execution we hook into a different service
                var incidentIds = selectionAssociations.map(function (item) {
                        return item.artifactId;
                    }),
                    testRunStepId = artifactId;

                //service varies depending on which type of test execution we are on
                var service = Inflectra.SpiraTest.Web.Services.Ajax.TestExecutionService || Inflectra.SpiraTest.Web.Services.Ajax.TestExecutionExploratoryService;

                //send selected incident IDs back to server
                service.TestExecution_AddIncidentAssociation(
                    projectId,
                    testRunStepId,
                    incidentIds,
                    self.saveClick_success,
                    self.saveClick_failure);
            }

        }
    }
    saveClick_success (data) {
        if (pageProps.associationWithoutPanel) {
            //run custom save function if there is one
            if (this.props.customSaveSuccessFunction) {
                this.props.customSaveSuccessFunction();
            }
        } else {
            //make sure the grid is reloaded
            $find(this.props.sortedGridId).load_data();
        }

        //and make sure to close the panel
        $("#" + this.props.lnkAddBtnId).removeClass('disabled');
        SpiraContext.uiState[this.props.domId].isMounted = false;
        ReactDOM.unmountComponentAtNode(document.getElementById(this.props.domId));

    }
    saveClick_failure (error) {
        var messageBox = document.getElementById(this.props.messageBox);
        globalFunctions.display_error(messageBox, error);
    }

    //close out the panel and clear react
    closeClick (e) {
        e.stopPropagation();

        //and make sure to close the panel
        $("#" + this.props.lnkAddBtnId).removeClass('disabled');
        SpiraContext.uiState[this.props.domId].isMounted = false;
        ReactDOM.unmountComponentAtNode(document.getElementById(this.props.domId));
    }




    /*
     * ===============
     * manage the creation of 'new X from artifact' buttons
     * ===============
     */

    canCreateFromCurrentArtifact(displayType) {
        let canCreate = false;
        const currentArtifact = SpiraContext.ArtifactTypeId,
            artifactEnums = globalFunctions.artifactTypeEnum,
            createPermission = globalFunctions.permissionEnum.Create,
            isAuthorizedEnum = globalFunctions.authorizationStateEnum.authorized,
            displayEnums = globalFunctions.displayTypeEnum;

        // return back false if no display type was passed in
        if (!displayType) {
            return canCreate;
        }

        // check we are on an artifact and display type that let's you create an artifact
        // then check if the user has create permissions for that new artifact
        if (currentArtifact === artifactEnums.requirement && displayType === displayEnums.Requirement_TestCases) {
            canCreate = globalFunctions.isAuthorized(createPermission, artifactEnums.testCase) === isAuthorizedEnum;
        } else if (currentArtifact === artifactEnums.release && displayType === displayEnums.Release_TestCases) {
            canCreate = globalFunctions.isAuthorized(createPermission, artifactEnums.testCase) === isAuthorizedEnum && globalFunctions.isAuthorized(createPermission, artifactEnums.testSet) === isAuthorizedEnum;
        } else if (currentArtifact === artifactEnums.testCase && displayType === displayEnums.TestCase_Requirements) {
            canCreate = globalFunctions.isAuthorized(createPermission, artifactEnums.requirement) === isAuthorizedEnum;
        } else if (currentArtifact === artifactEnums.incident && displayType === displayEnums.ArtifactLink) {
            canCreate = globalFunctions.isAuthorized(createPermission, artifactEnums.requirement) === isAuthorizedEnum;
        } else if (currentArtifact === artifactEnums.task && displayType === displayEnums.ArtifactLink) {
            canCreate = globalFunctions.isAuthorized(createPermission, artifactEnums.incident) === isAuthorizedEnum;
        }

        return canCreate;
    }

    createFromCurrentArtifact () {
        //only call service if project filter is set to current project (ie is 0)
        if (this.state.projectFilter === 0) {
            var self = this,
                serviceToUse: any = "";

            switch(this.state.newArtifactTypeToCreate) {
                case globalFunctions.artifactTypeEnum.testCase:
                    serviceToUse = Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService;
                    break;
                case globalFunctions.artifactTypeEnum.requirement:
                    serviceToUse = Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService;
                    break;
                case globalFunctions.artifactTypeEnum.incident:
                    serviceToUse = Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService;
                    break;
                case globalFunctions.artifactTypeEnum.testSet:
                    //Uses the test case service
                    serviceToUse = Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService;
                    break;
                default:
                    //no action
            }

            globalFunctions.display_spinner();
            //Call the appropriate web-service method - if one was identified in above switch statement
            serviceToUse && serviceToUse.AssociationPanel_CreateNewLinkedItem (
                self.props.projectId,
                self.state.artifactId,
                self.state.artifactTypeId,
                null,
                self.state.folderFilter || null,
                self.createFromCurrentArtifact_success,
                self.createFromCurrentArtifact_failure
            );
        }
    }
    createFromCurrentArtifact_success (errorMessage, context) {
        globalFunctions.hide_spinner();
        if (errorMessage == '') {
            var newArtifactName = globalFunctions.getArtifactTypes(this.state.newArtifactTypeToCreate)[0].name,
                currentArtifactName = globalFunctions.getArtifactTypes(this.state.artifactTypeId)[0].name,
                message = resx.AssociationPanel_ArtifactXCreatedFromArtifactY.replace('{0}', newArtifactName).replace('{1}', currentArtifactName);

            //Reload the data and display success
            $find(this.props.sortedGridId).load_data();
            var messageBox = document.getElementById(this.props.messageBox);
            globalFunctions.display_info_message(messageBox, message);
        }
        else {
            globalFunctions.display_error_message(messageBox, errorMessage);
        }
    }
    createFromCurrentArtifact_failure (exception) {
        //Populate the error message if we have one
        globalFunctions.hide_spinner();
        var messageBox = document.getElementById(this.props.messageBox);
        globalFunctions.display_error(messageBox, exception);
    }





    /*
     * ===============
     * rendering the dom
     * ===============
     */
    render () {
        //functions or vars used in display etc based on state/props
        var isProjectActive = this.state.projects.length > 1,
            isArtifactActive = (this.props.displayType === globalFunctions.displayTypeEnum.ArtifactLink || this.props.displayType === globalFunctions.displayTypeEnum.Attachments || this.props.displayType === globalFunctions.displayTypeEnum.SourceCodeRevision_Associations || this.props.displayType === globalFunctions.displayTypeEnum.SourceCodeFile_Associations),
            isFolderActive = this.state.folders.length > 0 &&
                this.state.artifactFilter > 0 &&
                globalFunctions.getArtifactTypes(this.state.artifactFilter)[0].hasFolders,
            isSearchEnabled = this.state.artifactDropDownNotRequired || 
                this.state.artifactFilter > 0 || 
                this.state.searchTermIsValidToken,
            firstAvailableArtifactToken = this.state.artifacts[1].token;

        var searchButtonClasses = isSearchEnabled ? "btn btn-default" : "btn btn-default disabled",
            saveButtonClasses = this.state.selectionLength ? "btn btn-primary" : "btn btn-primary disabled"; //only enable the save button if 1+ artifacts are selected
        //create the actual panel
        return (
            <div className="well clearfix">
                <div className="mb3">
                    <div className="mln3 dib">
                        {isProjectActive ?
                            <Assn_DropButton
                                currentFilter={this.state.projectFilter}
                                itemClickExternalFunction={this.browseProjectChange}
                                items={this.state.projects}
                                />
                            : null }
                        {isArtifactActive ?
                            <Assn_DropButton
                                enabled={this.state.artifactsEnabled}
                                currentFilter={this.state.artifactFilter}
                                itemClickExternalFunction={this.browseArtifactChange}
                                items={this.state.artifacts}
                                />
                            : null }
                        {isFolderActive ?
                            <Assn_DropButton
                                currentFilter={this.state.folderFilter}
                                itemClickExternalFunction={this.browseFolderChange}
                                items={this.state.folders}
                                expandCollapseExternalFunction={this.folderExpandCollapseClick}
                                />
                            : null }
                    </div>
                    <div className="dib">
                        <input
                            type="text"
                            className="text-box mx3 w9 xxs-w8"
                            placeholder={resx.AssociationPanel_FilterPlaceholder + firstAvailableArtifactToken + ":4)"}
                            value={this.state.searchTerm}
                            onChange={this.searchTermChange}
                            onKeyDown={this.searchEnterPress}
                            />
                        <div href="#"
                            className={searchButtonClasses}
                            onClick={this.searchClick} >
                            <span className="fas fa-search mr3"></span>
                            {resx.Global_Search}
                        </div>
                    </div>
                </div>
                <div onClick={this.closeTopDropdowns}>
                    {this.state.searchCompleted ?
                        <Assn_Grid
                            onExpandCollapseGridClick={this.onExpandCollapseGridClick}
                            onGridRowNameMouseEnter={this.onGridRowNameMouseEnter}
                            onHeaderClick={this.onHeaderClick}
                            onRowClick={this.onRowClick}
                            project={this.state.projectFilter}
                            results={this.state.results}
                            resultsNotEmpty={this.state.resultsNotEmpty}
                            searchCompleted={this.state.searchCompleted}
                            searchTerm={this.state.searchTerm}
                            searchTermLastSent={this.state.searchTermLastSent}
                            />
                        : null
                    }
                    {this.state.selectionLength ?
                        <p className="alert alert-warning alert-narrow mt3 mb0">
                            {this.state.selectionLength} {resx.Global_RowsSelected}
                        </p>
                        : null
                    }
                    {this.state.showComment && this.state.results.length ?
                        <div className="my3">
                            <div className="mb2">
                                <span className="v-mid mr3">{resx.Global_Type}: </span>
                                <Assn_DropButton
                                    currentFilter={this.state.linkType}
                                    itemClickExternalFunction={this.setLinkType}
                                    items={this.state.linkTypeList}
                                    />
                            </div>
                            <span className="mr4">{resx.Global_Comment}: </span>
                            <input
                                type="text"
                                className="text-box mr3 w9 xxs-w8"
                                value={this.state.commentTerm}
                                onChange={this.commentChange}
                                />
                        </div>
                        : null
                    }
                    <div className="display-inline-block mt3">
                        <div className="btn-group">
                            <div
                                className={saveButtonClasses}
                                onClick={this.saveClick.bind(null, this.state.selectionLength) }
                                >
                                {resx.Global_Save}
                            </div>
                            <div
                                className="btn btn-default"
                                onClick={this.closeClick}
                                >
                                {resx.Global_Cancel}
                            </div>
                        </div>
                    </div>
                    {this.state.isCreateFromCurrentArtifact ?
                        <Assn_NewArtifactFromThisItem
                            artifactId={this.state.artifactId}
                            artifactTypeId={this.state.artifactTypeId}
                            newArtifactTypeToCreate={this.state.newArtifactTypeToCreate}
                            createFromCurrentArtifact={this.createFromCurrentArtifact}
                            isCurrentProject={this.state.projectFilter === 0}
                            />
                        : null
                    }
                </div>
            </div>
            )
    }
}



//Drop Button component
interface Assn_DropButtonProps {
    currentFilter: string; //used to set the current selected item by filtering on the items in the dropdown list
    enabled: boolean;
    itemClickFunction: any; //function to call when an item in the dropdown list is clicked
    items: Array<any>; //the list of values to show in the dropdown along with their associated IDs
    onExpandCollapseFolderBtnItemClick: any; //if the dropdown items are hierarchically nested, this is the function to call on one of the expand/collapse buttons being clicked
}
class Assn_DropButton extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.state = {
            dropdownIsOpen: false
        }

        this.toggleExpandCollapse = this.toggleExpandCollapse.bind(this);
        this.collapseDropdown = this.collapseDropdown.bind(this);
        this.onDropDownBlur = this.onDropDownBlur.bind(this);
        this.onItemClick = this.onItemClick.bind(this);
        this.onItemExpandCollapseClick = this.onItemExpandCollapseClick.bind(this);
    }

    bindKeyboardShortcuts() {
        Mousetrap.bind('down', function downKeyInDropDown() {
            return false;
        });
        Mousetrap.bind('up', function upKeyInDropDown() {
            return false;
        });
    }
    unbindKeyboardShortcuts() {
        Mousetrap.unbind('down');
        Mousetrap.unbind('up');
    }

    toggleExpandCollapse(e) {
        e.stopPropagation();
        this.state.dropdownIsOpen ? this.unbindKeyboardShortcuts() : this.bindKeyboardShortcuts();
        this.setState({ dropdownIsOpen: !this.state.dropdownIsOpen })
    }
    collapseDropdown(e) {
        this.setState({ dropdownIsOpen: false })
    }
    onDropDownBlur(e) {
        const currentTarget = e.currentTarget;
        this.unbindKeyboardShortcuts();

        //setTimeout used so that as the event happens the correct element will be focused to make sure close event occurs only when it should
        setTimeout(() => {
            //check to see if the clicked item is contained within the element being closed on blur
            if (!currentTarget.contains(document.activeElement)) {
                this.collapseDropdown(null);
            }
        }, 0);
    }

    //manage the clicking of an item in the dropdown - logic and functionality effectively passed to parent component to manage main state
    onItemClick(filterId, enabled, e) {
        //call external function to change main state
        this.props.itemClickExternalFunction(filterId, enabled, e)

        //close the menu
        this.collapseDropdown(e);
    }

    //handle a click on any expand/collapse buttons in the dropdown
    onItemExpandCollapseClick (index, children, event) {
        event.preventDefault();
        event.stopPropagation();
        //toggle the hide children status on the parent item
        var newHideChildrenStatus = !this.props.items[index].hideChildren,
            newData = this.props.items;
        newData[index].hideChildren = newHideChildrenStatus;

        //pass in the information about the changes to the parent component to manage all required state changes
        this.props.expandCollapseExternalFunction(children, newData, newHideChildrenStatus);
    }

    render() {
        var self = this,
            dropdownClasses = this.state.dropdownIsOpen ? "dropdown-menu mh9 ov-y-auto db" : "dropdown-menu dn",
            ariaExpanded = this.state.dropdownIsOpen ? "false" : "true",
            hasItems = this.props.items.length > 1 && this.props.items[1] != undefined, //should always have one item - the "All Folders" entry
            activeItem = hasItems ?
                this.props.items.filter(function (item) {
                    return item.id === self.props.currentFilter;
                })[0]
                ||
                this.props.items.filter(function (item) {
                    return item.id === self.props.items[0].id;
                })[0]
                : this.props.items[0],
            itemNodes = hasItems ?
                this.props.items.map(function (item, index) {
                    var enabled = self.props.enabled ? globalFunctions.findItemInArray(self.props.enabled, item.id) : true;
                    return (
                        <li>
                            <Assn_BtnItem
                                children={item.children}
                                clickFunction={self.onItemClick}
                                enabled={enabled}
                                filter={self.props.currentFilter}
                                hide={item.hide}
                                hideChildren={item.hideChildren}
                                expandCollapseClick={self.onItemExpandCollapseClick}
                                id={item.id}
                                indent={item.indentLevel}
                                index={index}
                                key={item.id}
                                name={item.name} />
                        </li>
                    )
                })
            : null;
        return (
            <div
                className="btn-group"
                onBlur={this.onDropDownBlur}
                >
                <button
                    aria-expanded={ariaExpanded}
                    aria-haspopup="true"
                    className="btn btn-flat dropdown-toggle"
                    onClick={this.toggleExpandCollapse}
                    type="button"
                    >
                    {activeItem.name}
                    {hasItems ?
                        <span className="caret"></span>
                    : null }
                </button>
                {hasItems ?
                    <ul
                        className={dropdownClasses}
                        >
                        {itemNodes}
                    </ul>
                : null}
            </div>
        )
    }
}



//Browse Button Item component
interface Assn_BtnItemProps {
    clickFunction: any;
    enabled: boolean;
    expandCollapseClick: any;
    filter: number;
    id: number;
    name: string;
}

function Assn_BtnItem(props) {
    var indentPx,
        indentStyle;
    var itemClasses = props.hide ? "dn" : props.enabled ? (props.filter === props.id ? "active" : null) : "disabled",
        expandCollapseClasses = props.children ? "w4 mln4 pointer fas fa-fw orange-hover" + (props.hideChildren ? " fa-caret-right" : " fa-caret-down") : "";
    if (props.indent) {
        indentPx = ((props.indent.length - 1) / 2);
        indentStyle = {
            marginLeft: indentPx + "em",
            marginBottom: 0
        };
    };

    return (
        <a
            className={itemClasses}
            style={indentStyle}
            href="javascript:void(0)"
            onClick={() => props.clickFunction( props.id, props.enabled) }
            >
            {props.children ?
                <span
                    className={expandCollapseClasses}
                    onClick={props.expandCollapseClick.bind(null, props.index, props.children) }
                    />
                : null
            }
            {props.name}
        </a>
    )
}

function Assn_Grid(props) {
    var self = this;
    var displayMessage = !props.resultsNotEmpty && props.searchCompleted;
        
    var rowNodes = props.results.map(function (item, index) {
        var mainIcon = globalFunctions.getArtifactTypes(item.artifactTypeId)[0].image,
            subIcon = item.artifactSubType ? globalFunctions.getArtifactTypes(item.artifactTypeId, item.artifactSubType)[0].image : null,
            iconToUse = subIcon || mainIcon;
        return (
            <Assn_GridRow
                children={item.children}
                id={item.id}
                icon={SpiraContext.BaseThemeUrl + iconToUse}
                indent ={item.indentLevel}
                index={index}
                isDisabled={item.id < 0}
                hide={item.hide}
                hideChildren={item.hideChildren}
                key={index}
                name={item.name}
                nameTooltip={item.nameTooltip}
                onGridRowNameMouseEnter={props.onGridRowNameMouseEnter}
                onRowClick={props.onRowClick}
                projectId={item.projectId}
                projectName={item.projectName}
                selected={item.selected}
                token={globalFunctions.getArtifactTypes(item.artifactTypeId)[0].token}
                onExpandCollapseGridClick={props.onExpandCollapseGridClick}
                type={item.artifactTypeId}
                />
        )
    });
    var rowMessage = <Assn_GridRow_Message text={resx.AssociationPanel_NoUnassociatedResults + ": " + props.searchTermLastSent} />;

    return (
        <div className="table-responsive mh9 min-h7 resize-v scrollbox">
            <table className="table DataGrid DataGrid-no-bands always-visible">
                {!displayMessage ? 
                    <thead>
                        <tr className="Header">
                            <th className="Checkbox" onClick={props.onHeaderClick}>
                                <span className="fas fa-check pointer" data-toggle="tooltip" data-placement="bottom" title={resx.Global_SelectAll}></span>
                            </th>
                            <th>{resx.Global_ID}</th>
                            <th>{resx.Global_Name}</th>
                            <th className="hidden-xs">{resx.Global_Project}</th>
                        </tr>
                    </thead>
                    : null
                }
                <tbody>
                    {displayMessage ? rowMessage : rowNodes}
                </tbody>
            </table>
        </div>
    )
}

function Assn_GridRow(props) {
    var indentPx,
        indentStyle;
    if (props.indent) {
        indentPx = ((props.indent.length - 1) / 2);
        indentStyle = {
            marginLeft: indentPx + "em"
        };
    };
    var rowClasses = props.hide ? "dn" : props.isDisabled ? "is-disabled silver" : props.selected ? "Highlighted" : "",
        expandCollapseClasses = props.children ? "w4 mln4 pointer fas fa-fw" + (props.hideChildren ? " fa-caret-right" : " fa-caret-down") : "";
    return (
        <tr
            className={rowClasses}
            onClick={props.isDisabled ? null : props.onRowClick.bind(null, props.index) } >
            <td>
                <input
                    type="checkbox"
                    checked={props.selected}
                    disabled={props.isDisabled}
                    defaultChecked={props.selected} />
            </td>
            <td className="w6">
                {props.token}:{Math.abs(props.id)}
            </td>
            <td>
                <span
                    className="has-tooltip db"
                    onMouseEnter={ props.isDisabled ? null : props.onGridRowNameMouseEnter.bind(null, props.id, props.index) }
                    style={indentStyle}
                    >
                    {props.children ?
                        <span
                            className={expandCollapseClasses}
                            onClick={props.onExpandCollapseGridClick.bind(null, props.index, props.children) }
                            />
                        : null }
                    <img
                        src={props.icon}
                        className="mr3 w4 h4"/>
                    {props.name || resx.Global_None2}

                    { // only render the tooltip element if it is populated. display inner html as html
                        props.nameTooltip ?
                        <div className="is-tooltip" dangerouslySetInnerHTML={{ __html: filterXSS(props.nameTooltip) }} />
                        : null
                    }
                </span>
            </td>
            <td className="hidden-xs">
                <span data-toggle="tooltip" data-placement="bottom" title={"PR:" + props.projectId}>{props.projectName}</span>
            </td>
        </tr>
    )
}

function Assn_GridRow_Message(props) {
    return (
        <tr>
            <td>
                <div className="font-125 py3 px4">~ {props.text} ~</div>
            </td>
        </tr>
    )
}

function Assn_NewArtifactFromThisItem(props){
    var currentArtifactType = globalFunctions.getArtifactTypes(props.artifactTypeId)[0],
        currentArtifactIcon = SpiraContext.BaseThemeUrl + currentArtifactType.image,
        currentArtifactName = currentArtifactType.name,
        newArtifactName = globalFunctions.getArtifactTypes(props.newArtifactTypeToCreate)[0].name,
        newArtifactIcon = SpiraContext.BaseThemeUrl + globalFunctions.getArtifactTypes(props.newArtifactTypeToCreate)[0].image,
        classes = props.isCurrentProject ? "btn btn-default pull-right mt3" : "btn btn-default pull-right disabled mt3",

        tooltip = resx.AssociationPanel_CreateArtifactXFromArtifactYTooltip.replace('{0}', newArtifactName).replace('{1}', currentArtifactName),
        text = resx.AssociationPanel_CreateArtifactXFromArtifactY.replace('{0}', newArtifactName).replace('{1}', currentArtifactName);

    return (
        <div
            className={classes}
            data-placement="bottom" 
            data-toggle="tooltip" 
            onClick={props.createFromCurrentArtifact}
            title={tooltip} >
                <span className="fas fa-plus pr3"></span>
                <span className="pr3">{text}</span>
            <img
                src={newArtifactIcon}
                alt={newArtifactName}
                className="w4 h4"
                />
        </div>
    )
}
