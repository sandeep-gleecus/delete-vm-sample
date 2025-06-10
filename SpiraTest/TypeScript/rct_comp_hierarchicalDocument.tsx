//TypeScript React file that displays the HierarchicalDocument React component

//external dependendencies (js libraries)
declare var React: any;
declare var ReactDOM: any;
declare var $: any;
declare var $find: any;
declare var $create: any;
declare var $get: any;
declare var render: any;
declare var g: any;
declare var graphlibDot: any;
declare var d3: any;
declare var filterXSS: any;
declare var filterXssInlineStyleOptions: any;

//external components
declare var RctCkeditor: any;
declare var CKEDITOR: any;

//inflectra services
declare var globalFunctions: any;
declare var SpiraContext: any;
declare var Inflectra: any;
declare var resx: any;

//Generates new ids
let lastId = 0;
function newId(prefix) {
    lastId++;
    return `${prefix}_${lastId}`;
}

//global object accessible by the aspx page
var react_HierarchicalDocument = {
    documentDomId: '' as string,
    messageBoxId: '' as string,
    webServiceClass: '' as Object,
    artifactToken: '' as string,
    artifactType: 0 as number,
    itemImage: '' as string,
    alternateItemImage: '' as string,
    summaryItemImage: '' as string,
    urlTemplate: '' as string,
	urlUploadTemplate: '' as string,

    //Renders the Versions ReactJS grid
    loadData: function (): void {
        ReactDOM.render(
            <HierarchicalDocument
                domId={this.documentDomId}
                projectId={SpiraContext.ProjectId}
                messageBoxId={this.messageBoxId}
                webServiceClass={this.webServiceClass}
                artifactToken={this.artifactToken}
                artifactType={this.artifactType}
                itemImage={this.itemImage}
				alternateItemImage={this.alternateItemImage}
				// ref lets us access the whole react component from the global namespace!
                ref={(rct_comp_hierarchicalDocument) => { (window as any).rct_comp_hierarchicalDocument = rct_comp_hierarchicalDocument }}
                showOutlineCode={this.showOutlineCode}
                parentRequirementId={this.parentRequirementId || null}
                summaryItemImage={this.summaryItemImage}
                urlTemplate={this.urlTemplate}
                urlUploadTemplate={this.urlUploadTemplate}
            />,
            document.getElementById(this.documentDomId)
        );
    }
};

//the parent react component that handles all state
class HierarchicalDocument extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.state = {
            // from calls to service
            itemsAll: null as Array<any>,
            itemsIdsAll: null as Array<number>,
            itemsVisible: null as Array<any>,
            headerFields: ["RequirementStatusId", "RequirementTypeId", "ImportanceId", "OwnerId"] as Array<string>,
            visibleCount: null as number,
            totalCount: null as number,
            parentRequirementId: this.props.parentRequirementId as number,

            // set on page
            visibleStartRow: null as number,
            visibleEndRow: null as number,
            visiblePage: 1 as number,
            visiblePageInput: 1 as number,

            // to handle editing
            idsInEditMode: [] as Array<number>,
            changesCount: 0 as number,
        }

        this.reload = this.reload.bind(this);
        this.loadData = this.loadData.bind(this);
        this.loadData_success = this.loadData_success.bind(this);
		this.focusOnRequirement = this.focusOnRequirement.bind(this);
		this.focusOnRequirementConfirmed = this.focusOnRequirementConfirmed.bind(this);
        this.pageMove = this.pageMove.bind(this);
        this.pageMoveConfirmed = this.pageMoveConfirmed.bind(this);
        this.handlePageInputChange = this.handlePageInputChange.bind(this);
        this.handlePageInputKeyDown = this.handlePageInputKeyDown.bind(this);

        this.startRow = 1; //The starting row
        this.retrieveSize = 50; //Number of rows to retrieve each call - use 50 in production
        this.pageSize = 50; // how many rows to display on each page - use 50 in production
        this.maxRowsToRetrieve = 5000; // the absolute max data to retrieve from the server - use 5000 in production

        // to handle editing
        this.userCanBulkEdit = globalFunctions.isAuthorized(globalFunctions.permissionEnum.BulkEdit, this.props.artifactType) == globalFunctions.authorizationStateEnum.authorized;
        this.editModeToggle = this.editModeToggle.bind(this);
        this.updateChangesCount = this.updateChangesCount.bind(this);
		this.pageMoveTemp = null as Array<any>;
		this.focusChangeIdTemp = null as number;
    }

	componentDidMount() {
        this.loadData();
    }

    reload() {
        // reset the page to the start of the document
        this.setState({
            itemsAll: null,
            itemsIdsAll: null,
            itemsVisible: null,
            visibleStartRow: 1,
            visibleEndRow: null,
            visiblePage: 1,
            visiblePageInput: 1
        }, () => {
            this.startRow = 1;
            this.loadData();
        })
    }

    loadData() {
        globalFunctions.display_spinner();

        var self = this;
        this.props.webServiceClass.HierarchicalDocument_Retrieve(
            this.props.projectId,
            this.startRow,
            this.retrieveSize,
            this.state.parentRequirementId,
            self.loadData_success.bind(self),
            self.loadData_failure.bind(self));
    }
    loadDataLazy() {
        // this function is used for successive rounds of loading when on the same page - so we increment the start row to the correct position
        this.startRow += this.retrieveSize;
        var self = this;
        this.props.webServiceClass.HierarchicalDocument_Retrieve(
            this.props.projectId,
            this.startRow,
            this.retrieveSize,
            this.state.parentRequirementId,
            self.loadData_success.bind(self),
            self.loadData_failure.bind(self)
        );
    }
    // access the actual dom to scroll the specific element into view
    focusOnRequirement(parentRequirementId, forceMove) {
		// allow page move if we have no unsaved changes - or if we force the move (following confirmation from the user via focusOnRequirementConfirmed)
		if (this.state.changesCount === 0 || forceMove) {
			//de-highlight the currently selected requirement
			let sidebarRequirement = null;
			// if a parent requirement is specified try and find its matching sidebar element
			if (this.state.parentRequirementId) {
				sidebarRequirement = document.getElementById("lstPackages_artifactId-" + this.state.parentRequirementId);
			}
			// if there's no parent specified, or there's no matching sidebar element, select the root element
			// note: we can have a parent requirement but no dom element in cases where that parent requirement was selected but since deleted or is no longer an epic
			if (!this.state.parentRequirementId || !sidebarRequirement) {
				sidebarRequirement = document.getElementById("lstPackages_artifactRoot");
			}
			if (sidebarRequirement) {
				sidebarRequirement.classList.remove("list-item-selected");
			}

			// set state to update the parent requirement id
			this.setState({ parentRequirementId: parentRequirementId || null }, () => {
				// reload data
				this.reload();
			});
			//if we have unsaved changes
		} else {
			//store the arguments passed in so we can call them if needed
			this.focusChangeIdTemp = parentRequirementId;
			//get confirmation from user
			globalFunctions.globalConfirm(resx.AjxFormManager_UnsavedChanges, "info", this.focusOnRequirementConfirmed);
		}
	}
	focusOnRequirementConfirmed(shouldMove) {
		if (shouldMove) {
			this.focusOnRequirement(this.focusChangeIdTemp, true)
		} else {
			//reset the tempe  
			this.focusChangeIdTemp = null;
		}
	}


    loadData_success(newData) {
        globalFunctions.hide_spinner();
        // keep an array of just the ids to make searching for a matching ID more performant in other functions
        const newDataIds = newData.items.map(x => x.primaryKey);
        const fullItemsArray = this.state.itemsAll ? [...this.state.itemsAll, ...newData.items] : newData.items;
        const fullIdsArray = this.state.itemsIdsAll ? [...this.state.itemsIdsAll, ...newDataIds] : newDataIds;
        // if this is the first data load...
        if (!this.state.itemsAll) {
            // highlight the relevant requirement in the sidebar
			let sidebarRequirement = null;
			// if a parent requirement is specified try and find its matching sidebar element
			if (this.state.parentRequirementId) {
				sidebarRequirement = document.getElementById("lstPackages_artifactId-" + this.state.parentRequirementId);
			}
			// if there's no parent specified, or there's no matching sidebar element, select the root element
			// note: we can have a parent requirement but no dom element in cases where that parent requirement was selected but since deleted or is no longer an epic
			if (!this.state.parentRequirementId || !sidebarRequirement) {
				sidebarRequirement = document.getElementById("lstPackages_artifactRoot");
			}
			// if the element exists add the class
			if (sidebarRequirement) {
				sidebarRequirement.classList.add("list-item-selected");
			}
            // set some general state
            this.setState({
                totalCount: newData.totalCount,
                visibleCount: newData.visibleCount,
            });
        }
        // after every load data bind the items data
        this.setState({
            itemsAll: fullItemsArray,
            itemsIdsAll: fullIdsArray
        }, () => {
            // once we have at least one page of data then set the visible rows and render the data
            if (!this.state.itemsVisible && (this.state.itemsAll.length >= this.pageSize || this.state.itemsAll.length == newData.totalCount)) {
                this.setVisibleData(this.state.itemsAll, this.pageSize, this.state.visiblePage, newData.totalCount, null, null);
            }
        });

        // If there is more data to load then get the next amount now with the spinner not displayed
        const moreDataAvailable = (this.startRow + this.retrieveSize - 1) < this.state.totalCount;
        const shouldGetMoreData = (this.startRow + this.retrieveSize) < this.maxRowsToRetrieve;
        if (moreDataAvailable && shouldGetMoreData) {
            setTimeout(function () { this.loadDataLazy() }.bind(this), 100);
        }

    }
    loadData_failure(exception) {
        globalFunctions.hide_spinner();
        globalFunctions.display_error($get(this.props.messageBoxId), exception);
    }

    setVisibleData(items, pageSize, visiblePage, totalCount, callback, callbackParam) {
        const startRow = 1 + (pageSize * (visiblePage - 1)),
            endOfPage = pageSize * visiblePage,
            endRow = totalCount < (endOfPage) ? totalCount : endOfPage;
        this.pageMoveTemp = null;
        this.setState({
            itemsVisible: items.slice(startRow - 1, endRow),
            visibleEndRow: endRow > this.maxRowsToRetrieve ? this.maxRowsToRetrieve : endRow,
            visiblePage: visiblePage,
            visiblePageInput: visiblePage,
            visibleStartRow: startRow,
            changesCount: 0,
            idsInEditMode: [],
        }, () => { callback && callback(callbackParam) });

        //reset page editing state
        window.onbeforeunload = null;
        SpiraContext.uiState.hierarchicalDocumentHasChanges = false;
    }

    // changes the page back or forward (default) one - reloading all data for new page
	pageMove(newPage, callback, callbackParam, forceMove) {
        // make sure we are not going to move beyond the start or end of the pages available
        const canMove = newPage > 0 && newPage <= Math.ceil(this.state.totalCount / this.pageSize);
        // set the new start row eg page 2, with a page size of 10, will start at 11, page 3 at 21
        if (canMove) {
            // allow page move if we have no unsaved changes - or if we force the move (following confirmation from the user via pageMoveConfirmed)
            if (this.state.changesCount === 0 || forceMove) {
                this.startRow = (this.pageSize * (newPage - 1)) + 1;

                // now set state
                // check that we have the data yet for this page - ie that it has been lazily loaded
                if (this.state.itemsAll.length >= this.startRow) {
                    this.setVisibleData(this.state.itemsAll, this.pageSize, newPage, this.state.totalCount, callback, callbackParam);
                }
            //if we have unsaved changes
            } else {
                //store the arguments passed in so we can call them if needed
                this.pageMoveTemp = [newPage, callback, callbackParam];
                //get confirmation from user
                globalFunctions.globalConfirm(resx.AjxFormManager_UnsavedChanges, "info", this.pageMoveConfirmed);
            }
        }
    }
    pageMoveConfirmed(shouldMove) {
        if (shouldMove) {
            this.pageMove(this.pageMoveTemp[0], this.pageMoveTemp[1], this.pageMoveTemp[2], true)
        } else {
            //reset the page number on the input 
            this.pageMoveTemp = null;
            this.setState({
                visiblePageInput: this.state.visiblePage,
            })
        }
    }

    // manage the input that lets you set the page number to go to
    handlePageInputChange(event) {
        if (event.target.value <= Math.ceil(this.state.totalCount / this.pageSize)) {
            this.setState({ visiblePageInput: event.target.value });
        }
    }

    // change page based off the input when you click enter
    handlePageInputKeyDown(event) {
        if (event.key === 'Enter') {
            if (event.target.value > 0 && event.target.value <= Math.ceil(this.state.totalCount / this.pageSize)) {
                this.pageMove(parseInt(event.target.value), null, null, false);
            }
        }
    }


    // FUNCTIONS USED TO GENERATE A SPECIFIC ITEM
    artifactTokenGenerate(item) {
        // create the proper image for the specific artifact
        var artifactImage = SpiraContext.BaseThemeUrl + 'Images/';
        if (item.summary) {
            artifactImage += this.props.summaryItemImage;
        }
        else if (item.alternate) {
            artifactImage += this.props.alternateItemImage;
        }
        else {
            artifactImage += this.props.itemImage;
        }
        return artifactImage;
    }

    // converts an indent string in the form of "aaaaab" into a code in the form of "1.2"
    getOutlineCodeFromIndent(indent) {
        // split the string into triplet groups of 3 letters - make it lowercase to ensure the character codes are all consistent
        const indentSplit = indent.toLowerCase().match(/[A-Za-z]{1,3}/g);
        // iterate over each triplet and turn that into a number
        var indentsAsNumbers = indentSplit.map(x => {
            // split the triplet into an array of single letters and get the character code for each letter (this ascends from a-z)
            const lettersToNumbers = x.split("").map(y => y.charCodeAt(0) - 97);
            // start with an index of 1 this represents the index of "aaa"
            var index = 1;
            // do the required math for each letter in the triplet
            if (lettersToNumbers[2] > 0) {
                index = index + lettersToNumbers[2];
            }
            if (lettersToNumbers[1] > 0) {
                index = index + (lettersToNumbers[1] * 26);
            }
            if (lettersToNumbers[0] > 0) {
                index = index + (lettersToNumbers[0] * 26 * 26);
            }
            return index;
        });
        // create and return the final code - use a period between individual indexes if we have more than one triplet
        if (indentSplit.length > 1) {
            return indentsAsNumbers.join(".");
        } else {
            return indentsAsNumbers[0];
        }
    }

    getArtifactUrlFromTemplate(url, primaryKey) {
        return url.replace('{0}', primaryKey).replace('~/', SpiraContext.BaseUrl)
    }
    getUploadUrl(url, primaryKey, artifactType) {
        return url.replace('{0}', primaryKey + "/" + artifactType).replace('~/', SpiraContext.BaseUrl);
    }

    //header fields are all those that are not the rich text or the name field
    //param: fields - array of field objects
    getHeaderFields(fields) {
        if (!fields) return;
        let headerArray = [];
        for (const prop in fields) {
            if (fields.hasOwnProperty(prop) && fields[prop].fieldType > 0 && fields[prop].fieldType != globalFunctions._fieldType_html && fields[prop].fieldName != "Name") {
                headerArray.push(fields[prop]);
            }
        }
        headerArray.sort((a,b) => a.Name - b.Name)
        return headerArray;
    }

    //get all rich text fields - these can be edited
    //param: fields - array of field objects
    getRichTextFields(fields) {
        if (!fields) return;
        let richTextArray = [];
        for (const prop in fields) {
            if (fields.hasOwnProperty(prop) && fields[prop].fieldType == globalFunctions._fieldType_html) {
                richTextArray.push(fields[prop]);
            }
        }
        return richTextArray;
    }


    // FUNCTIONS TO HANDLE EDITING
    editModeToggle(id) {
        if (id && this.state.idsInEditMode) {
            const index = this.state.idsInEditMode.length ? this.state.idsInEditMode.indexOf(id) : -1;
            if (index > -1) {
                let splicedArray = [...this.state.idsInEditMode]
                splicedArray.splice(index, 1);
                this.setState({
                    idsInEditMode: splicedArray
                });
            } else {
                this.setState({
                    idsInEditMode: [...this.state.idsInEditMode, id]
                });
            }
        }
    }
    //param: int increment
    updateChangesCount(increment) {
        if (this.state.changesCount + increment > 0) {
            window.onbeforeunload = function (event) {
                event.preventDefault();
                event.returnValue = "";
            };
            SpiraContext.uiState.hierarchicalDocumentHasChanges = true;
        } else {
            window.onbeforeunload = null;
            SpiraContext.uiState.hierarchicalDocumentHasChanges = false;
        }
        this.setState({ changesCount: this.state.changesCount + increment });
    }

    render() {
        const isAtEnd = this.state.visibleEndRow == this.state.totalCount || (this.state.totalCount > this.maxRowsToRetrieve && this.state.visibleEndRow == this.maxRowsToRetrieve);
        const pageCount = Math.ceil(this.state.totalCount / this.pageSize);
        return (
            <React.Fragment>
                
                <div
                    className="Header sticky df items-center justify-between h5 w-100 top0 z-3 bg-white bb b-near-white bw2"
                    id={this.props.domId + "-hierarchicalDoc-header"}
                    role="header"
                    >

                    {this.state.totalCount > 0 ?
                        <span className="badge">
                            {this.state.visibleStartRow}-{this.state.visibleEndRow} / {this.state.totalCount}
                        </span>
                        :
                        <p className="fw-b fs-110">{resx.RequirementDocument_NoRequirements}</p>
                    }

                    {this.state.idsInEditMode.length > 0 &&
                        <span>
                            {resx.HierarchicalDocument_EditingTitle.replace("{0}", this.state.idsInEditMode.length).replace("{1}", this.state.changesCount)}    
                        </span>
                    }

                    {pageCount > 1 ?
						<div
							className="mx3"
							id={this.props.domId + "-hierarchicalDoc-header-page-selector"}
							>
                            <div class="mx3 dib">
                                <input
									className="u-input w6 tr"
                                    id="txt-document-page"
                                    onChange={this.handlePageInputChange}
                                    onKeyDown={this.handlePageInputKeyDown}
                                    value={this.state.visiblePageInput}
                                /> / {pageCount}
                            </div>
                            <div className="btn-group">
                                <button
                                    className="btn btn-default lh1 py2 px3"
                                    disabled={this.state.visiblePage == 1 ? "true" : ""}
                                    onClick={this.pageMove.bind(null, this.state.visiblePage - 1, false, false, false)}
                                    title={resx.Pagination_Previous}
                                    type="button"
                                    >
                                    <i className="fas fa-caret-left"></i>
                                </button>
                                <button
                                    className="btn btn-default lh1 py2 px3"
                                    disabled={isAtEnd ? "true" : ""}
                                    onClick={this.pageMove.bind(null, this.state.visiblePage + 1, false, false, false)}
                                    title={resx.Pagination_Next}
                                    type="button"
                                    >
                                    <i className="fas fa-caret-right"></i>
                                </button>
                            </div>
						</div>
						:
						<div id={this.props.domId + "-hierarchicalDoc-header-page-selector"}></div>
                    }
                </div>
                

                <div
                    className="Body mb4 pr4"
                    id={this.props.domId + "-hierarchicalDoc-body"}
                    role="body"
                    >
                    {this.state.itemsVisible && this.state.itemsVisible.map((item, i) => 
                        <HierarchicalDocumentBodyItem
                            key={item.primaryKey}
                            index={i}
                            item={item}

                            headerFields={this.getHeaderFields(item.Fields)}
                            richTextFields={this.getRichTextFields(item.Fields)}
                            artifactToken={this.props.artifactToken}
                            artifactImage={this.artifactTokenGenerate(item)}
                            artifactUrl={this.getArtifactUrlFromTemplate(this.props.urlTemplate, item.primaryKey)}
                            imageUploadUrl={this.getUploadUrl(this.props.urlUploadTemplate, item.primaryKey, this.props.artifactType)}
                            outlineCode={this.props.showOutlineCode ? this.getOutlineCodeFromIndent(item.indent) : null}
                            canEdit={this.userCanBulkEdit}
                            editModeToggle={this.editModeToggle}
                            updateChangesCount={this.updateChangesCount}

                            webServiceClass={this.props.webServiceClass}
                            domId={this.props.domId}
                            messageBoxId={this.props.messageBoxId}
                            reloadAll={this.reload}
                        />
                    )}
                </div>

                {(isAtEnd && this.state.totalCount > this.maxRowsToRetrieve) ?
                    <div className="ma4 alert alert-inf">
                        {resx.MoreArtifactsThanCanDisplay}
                    </div>
                    : null
                }
            </React.Fragment>
        )
    }
}

//Single document entry
class HierarchicalDocumentBodyItem extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.state = {
            original_Name: this.props.item.Fields.Name.textValue,
            current_Name: this.props.item.Fields.Name.textValue,
            concurrencyValue: this.props.item.concurrencyValue,
            isEditing: false,
            hasChanges: false,
            errorFields: [],
        }

        this.editor = null;
        this.cancelEdit = this.cancelEdit.bind(this);
        this.cancelEditConfirmed = this.cancelEditConfirmed.bind(this);
        this.enableEdit = this.enableEdit.bind(this);
        this.saveChanges = this.saveChanges.bind(this);
        this.saveChanges_success = this.saveChanges_success.bind(this);
        this.saveChanges_failure = this.saveChanges_failure.bind(this);
		this.handleNameChange = this.handleNameChange.bind(this);

		this.ref = React.createRef();
    }

    componentDidMount() {
        this.props.richTextFields.forEach(field => {
            this.setState({
                ["original_" + field.fieldName]: field.textValue,
                ["current_" + field.fieldName]: field.textValue,
            });
        })
    }

    enableEdit() {
        if (this.props.canEdit) {
            this.props.editModeToggle(this.props.item.primaryKey);
            this.setState({ isEditing: true });
        }
	}

	// if the description field is taller than the window, switching to edit can scroll the user so that the description is not in view at all
	// to get round that we force the just clicked editor to scroll properly into view
	scrollIntoView() {
		if (this.ref.current) {
			this.ref.current.scrollIntoView();
		}
	}

    //on clicking cancel
    cancelEdit() {
        //if the user changed the requirement display a confirmation asking if they want to undo those changes 
        //we do this on a list page because people may have made extensive changes to a description field
        if (this.state.hasChanges) {
            globalFunctions.globalConfirm(resx.AjxFormManager_UnsavedChanges, "info", this.cancelEditConfirmed);
        } else {
            this.cancelEditConfirmed(true);
        }

    }
    // param: bool shouldCancel - passed from the global confirm dialog
    cancelEditConfirmed(shouldCancel) {
        if (shouldCancel) {
            //pass up to parent that this item is no longer being edited
            this.props.editModeToggle(this.props.item.primaryKey);
            if (this.state.hasChanges) {
                this.props.updateChangesCount(-1);
            }

            //update rich text data and read only status via state
            this.props.richTextFields.forEach(field => {
                //const editor = this.state["editor_" + field.fieldName];
                //editor.isReadOnly = true;
                //editor.setData(this.state["original_" + field.fieldName]);
                this.setState({
                    ["current_" + field.fieldName]: this.state["original_" + field.fieldName],
                });
            });

            //update state
            this.setState({
                current_Name: this.state.original_Name,
                hasChanges: false,
                isEditing: false,
            });
        }
    }

    //Call to server to save changes - if we have any
    saveChanges() {
        if (this.state.hasChanges) {
            //check that we don't have a blank name field - this is always required
            if (!this.state.current_Name.trim()) {
                globalFunctions.globalAlert(resx.FormValidation_FillInRequiredFields, "error", true, "fas fa-exclamation-triangle");
                this.setState({ errorFields: [this.props.item.Fields.Name.fieldName] })
            } else {
                globalFunctions.display_spinner();

                //prepare the object to return to the server
                let dataItem = JSON.parse(JSON.stringify(this.props.item));
                dataItem.Fields.Name.textValue = this.state.current_Name;
                dataItem.concurrencyValue = this.state.concurrencyValue;

                //set rich text data fields
                //update rich text data and read only status via state
                this.props.richTextFields.forEach(field => {
                    dataItem.Fields[field.fieldName].textValue = this.state["current_" + field.fieldName]
                });

                const self = this;
                this.props.webServiceClass.HierarchicalDocument_Save(
                    SpiraContext.ProjectId,
                    dataItem,
                    self.saveChanges_success,
                    self.saveChanges_failure
                );
            }
        }
    }
    saveChanges_success(messages) {
        globalFunctions.hide_spinner();
        //See if we have any validation messages
        const hasMessages = messages && messages.length ? true : false;
		const errorMessages = hasMessages && messages.filter(message => message.FieldName === null || (message.FieldName && message.FieldName.indexOf("$") !== 0));
		const newConcurrency = hasMessages && messages.filter(message => message.FieldName && message.FieldName.indexOf("$newConcurrency") == 0);

        //see if we only have messages that are not errors
        if (errorMessages.length) {
            //highlight fields with errors
			//get just the fields that are available in this document
			let documentFields = [];
			for (const prop in this.props.item.Fields) {
				documentFields.push(this.props.item.Fields[prop].fieldName);
			}
			const errorFields = errorMessages.filter(message => message.FieldName !== null).map(message => message.FieldName);
			const documentErrorFields = errorFields.length ? errorFields.filter(field => documentFields.indexOf(field) >= 0) : [];
			this.setState({ errorFields: documentErrorFields });
            //show alert of messages
			let alertMessage = errorMessages.map(message => message.Message).join("\n");
			//if any of the fields are not on this page (eg custom properties that are empty but empty is not allowed) add a helpful message about that
			if (errorFields.length && documentErrorFields.length < errorFields.length) {
				alertMessage += " " + resx.HierarchicalDocument_ErrorsInFieldsNotVisible;
			}
            globalFunctions.globalAlert(alertMessage, "error", true, "fas fa-exclamation-triangle");

        } else if (newConcurrency) {
            //pass up to parent that this item is no longer being edited
            this.props.editModeToggle(this.props.item.primaryKey);
            this.props.updateChangesCount(-1);

            //update rich text data and read only status via state
            this.props.richTextFields.forEach(field => {
                const editor = this.state["editor_" + field.fieldName];
                editor.isReadOnly = true;
                this.setState({
                    ["original_" + field.fieldName]: this.state["current_" + field.fieldName],
                });
            });


            //update state for the requirement as a whole
            this.setState({
                original_Name: this.state.current_Name,
                concurrencyValue: newConcurrency[0].Message,
                hasChanges: false,
                errorFields: [],
                isEditing: false,
            });

        } else {
            // If we got no messages, we don't know if the item was saved or not so need to refresh the whole list
            this.props.reloadAll();
        }
    }
    saveChanges_failure(exception) {
        globalFunctions.hide_spinner();
        const messages = exception.map(x => x.FieldName + ": " + x.Message).join("\n");
        globalFunctions.globalAlert(messages, "error", true, "fas fa-exclamation-triangle");
    }

    handleNameChange(event) {
        // the name field can't be blank. Handle live validation of the field
		let errorFields = [...this.state.errorFields]
		const index = errorFields.length ? errorFields.indexOf(this.props.item.Fields.Name.fieldName) : -1;
		if (event.target.value && event.target.value.trim()) {
            if (index > -1) {
                errorFields.splice(index, 1);
			}
		// if the field is blank / only contains spaces add it to the error fields - only if it is not already there
        } else if (index === -1) {
            errorFields.push(this.props.item.Fields.Name.fieldName);
        }
        //update state
        this.setState({
            current_Name: event.target.value,
            errorFields: errorFields
        }, this.setHasChanges);
    }
	handleRichTextFieldChange(event, editor, fieldName) {
		const data = this.state["editor_" + fieldName].getData();
		//Handle validation - only required for custom fields that are not allowed to be empty by the custom property definition and are currently in error.
		//This code only therefore removes the fields from the list of errorfields, never adds them
		const index = this.state.errorFields.length ? this.state.errorFields.indexOf(fieldName) : -1;
		if (index > -1) {
			let errorFields = [...this.state.errorFields]
			if (data.length) {
				errorFields.splice(index, 1);
				this.setState({
					["current_" + fieldName]: data,
					errorFields: errorFields
				}, this.setHasChanges);
			}
		} else {
			this.setState({ ["current_" + fieldName]: data }, this.setHasChanges);
		}

    }

    setHasChanges() {
        const nameHasChanges = this.state.original_Name != this.state.current_Name;
        const richTextHasChanges = this.props.richTextFields.some(field => {
            return this.state["original_" + field.fieldName] != this.state["current_" + field.fieldName];
        })

        const hasChanges = nameHasChanges || richTextHasChanges;
        if (this.state.hasChanges !== hasChanges) {
            //if we had changes, but now don't we decrement, otherwise we increment
            const newChangesIncrement = this.state.hasChanges ? -1 : 1;
            this.props.updateChangesCount(newChangesIncrement);
        }

        this.setState({ hasChanges: hasChanges });

    }

    render() {
        return (
            <div
                className={"hierarchical-body-item mb1" + (this.props.index > 0 ? " mt5" : "")}
				style={{ marginLeft: ((this.props.item.indent.length - 3) * 10) + 'px' }}
				ref={this.ref}
                role="treeitem"
                aria-label={this.props.artifactToken + ':' + this.props.item.primaryKey}
                data-primarykey={this.props.item.primaryKey}
                >
                <div className={"df justify-between items-center mt4 mb2 py3 br3 sticky top2 z-2" + (this.state.isEditing ? " bg-peach-light" : " bg-white")}>
					<h2 className={"dif items-center wb-word grow-1 fs-h3 my0" + (this.props.item.summary ? " fw-b" : "")} role="heading">
                        <img src={this.props.artifactImage} className="w5 h5 mr3" />
                        {this.props.outlineCode &&
                            <span className="mr3 fs-h6 fw-normal">{this.props.outlineCode}</span>
                        }
                        <span className="fs-h5 light-silver mr3">
                            [{this.props.artifactToken}:{this.props.item.primaryKey}]
                        </span> 
                        
                        {!this.state.isEditing ?
                            <a className="tdn transition-all" href={this.props.artifactUrl}>
                                {this.state.current_Name}
                            </a>
                            :
                            <input
                                className={"u-input pa0 mbn1 grow-1" + (this.state.errorFields.includes(this.props.item.Fields.Name.fieldName) ? " validation-error" : "")}
                                value={this.state.current_Name}
                                onChange={this.handleNameChange}
                                />
                        }
                    </h2>
                    {this.props.canEdit ?
                        !this.state.isEditing ?
                            <button
                                type="button"
                                className="df pa3 o-50 o-100-hover u-btn-minimal transition-all"
                                onClick={this.enableEdit}
                                >
                                <i class="hierarchical-body-item-edit fas fa-edit"></i>
                            </button>
                            :
                            <div role="group" className="dif">
                                <button
                                    type="button"
                                    className={"df pa3 u-btn-minimal transition-all" + (this.state.hasChanges ? " orange scale-150" : " o-50")}
                                    onClick={this.saveChanges}
                                    disabled={!this.state.hasChanges && !this.state.errorFields.length}
                                    >
                                    <i class="fas fa-save"></i>
                                </button>
                                <button
                                    type="button"
                                    className="df pa3 u-btn-minimal"
                                    onClick={this.cancelEdit}
                                    >
                                    <i class="fas fa-times"></i>
                                </button>
                            </div>
                        : null
                    }
                </div>
                <div className="hierarchical-body-item-fields ml5 ml0-xs bg-near-white df flex-wrap br3 pa3">
                    {this.props.headerFields && this.props.headerFields.map((field, i) =>
                        <div className="Field mr5 my2 dif items-center" aria-label={field.fieldName} key={i}>
                            <span className="Caption fw-b mr3">{field.caption}:</span>
                            <span
                                className={"Value df" + (field.cssClass ? (field.cssClass.charAt(0) == "#" ? " px3 br2 black-always" : " " + field.cssClass) : "") + (field.tooltip ? " has-tooltip" : "")}
                                style={(field.cssClass && field.cssClass.charAt(0) == "#") ? { backgroundColor: field.cssClass } : null}
                                >
                                {field.tooltip &&
                                    <div class="is-tooltip">{field.tooltip}</div>
                                }
                                {field.textValue ?
                                    field.textValue
                                    :  
                                    field.equalizerGray >= 0 ?
                                        <EqualizerMiniChart
                                            field={field}
                                            />
                                        :
                                        resx.Global_None2
                                }
                            </span>
                        </div>
                    )}
                </div>

                {this.props.richTextFields.map((field, i) =>
                    <React.Fragment key={i}>
						{this.props.richTextFields.length > 1 &&
							<label
								className={"ml5 ml0-xs pl3 mt3 py2 db bb b-light-gray fw-b fs-90 bg-off-white br2-top" + (field.tooltip ? " has-tooltip cursor-default" : "")}
								>
								{field.tooltip &&
									<div class="is-tooltip">{field.tooltip}</div>
								}
                                {field.caption}:
                            </label>
                        }

                        {!this.state.isEditing ?
                            <div
								className="ml5 ml0-xs pl3 mt3 br3 wb-word responsive-images cf">
								<div dangerouslySetInnerHTML={{ __html: filterXSS(this.state["current_" + field.fieldName], filterXssInlineStyleOptions) }}/>
                            </div>
                            :
                            <div
								className={"ml5 ml0-xs pl3 mt0 br3 wb-word responsive-images" + (this.state.errorFields.includes(field.fieldName) ? " validation-error" : "")}
                                >
                                <RctCkeditor.CKEditor
									editor={CKEDITOR.InlineEditor}
                                    data={this.state["current_" + field.fieldName]}
                                    config={{
                                        simpleUpload: { uploadUrl: this.props.imageUploadUrl }
                                    }}
                                    onReady={editor => {
                                        // store the editor so can be accessed by in the parent component.
										this.state["editor_" + field.fieldName] = editor;
										this.scrollIntoView();
                                    }}
                                    onChange={(event, editor) => {
                                        this.handleRichTextFieldChange(event, editor, field.fieldName);
                                    }}
                                    onBlur={(event, editor) => {
                                    }}
                                    onFocus={(event, editor) => {
                                    }}
                                />
                            </div>
                        }
                    </React.Fragment>
                )}


                {(this.props.item.Fields._Diagram && this.props.item.Fields._Diagram.textValue) &&
                    <HierarchicalDocumentBodyItemDiagram
                        item={this.props.item}
                        domId={this.props.domId}
                        messageBoxId={this.props.messageBoxId}
                        />
                }
            </div>
        );
    }
}



//Single item's diagram
class HierarchicalDocumentBodyItemDiagram extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        //Event handling bindings
        //None
    }

    componentWillMount() {
        this.dotNotation = null;
        if (this.props.item.Fields._Diagram && this.props.item.Fields._Diagram.textValue)
        {
            this.svgDomId = newId(this.props.domId);
            this.dotNotation = this.props.item.Fields._Diagram.textValue;
        }
    }

    componentDidMount() {
        if (this.dotNotation && this.svgDomId) {
            //Render the diagram, unless IE11 (!Array.prototype.find)
            if (Array.prototype['find']) {
                this.renderDiagram(this.dotNotation, this.svgDomId);
            }
            else
            {
                $get(this.svgDomId).style.height = '0px';
            }
        }
    }

    renderDiagram(dotNotation, svgDomId) {
        try {
            //Clear the initial content
            var inner = $("#" + svgDomId + " g")[0];
            globalFunctions.clearContent(inner);

            if (dotNotation) {
                //create the graph from the notation
                g = graphlibDot.read(dotNotation);

                // Set margins, if not present
                if (!g.graph().hasOwnProperty("marginx") &&
                    !g.graph().hasOwnProperty("marginy")) {
                    g.graph().marginx = 20;
                    g.graph().marginy = 20;
                }

                g.graph().transition = function (selection) {
                    return selection.transition().duration(500);
                };

                // Render the graph into svg g
                d3.select("#" + svgDomId + " g").call(render, g);

                //Next set the width of the SVG so that the scrollbars in the container DIV appear
                var svgUseCaseDiagram = $get(svgDomId);
                var bBox = svgUseCaseDiagram.getBBox();
                var unscaled_width = parseInt(bBox.width + 100);
                var unscaled_height = parseInt(bBox.height + 80);
                svgUseCaseDiagram.style.width = unscaled_width + 'px';
                if (unscaled_height > 80) {
                    svgUseCaseDiagram.style.height = unscaled_height + 'px';
                }
                else
                {
                    svgUseCaseDiagram.style.height = '0px';
                }
            }
        }
        catch (err) {
            globalFunctions.display_error_message($get(this.props.messageBoxId), err);
        }
    }

    render() {
        var that = this;
        if (this.dotNotation && this.svgDomId) {
            return (
                <div className="Diagram ml5 ml0-xs dn-xs">
                    <svg id={this.svgDomId} class="graphviz-hierarchical graphviz-large-text">
                        <g />
                    </svg>
                </div>
            );
        }
        else
        {
            return null;
        }
    }
}

//Equalizer
const EqualizerMiniChart = (props) => {
    const WIDTH_DEFAULT = 110;
    return (
        <div
            className={"equalizer df br2 ov-hidden " + (props.widthClass && !props.widthPixels ? " " + props.widthClass : "")}
            style={{ width: props.widthPixels || WIDTH_DEFAULT + "px" }}
            >
            {props.field.equalizerGreen ?
                <span
                    className="EqualizerGreen"
                    style={{ width: props.field.equalizerGreen + "%" }}
                    >
                </span>
                : null
            }
            {props.field.equalizerRed ?
                <span
                    className="EqualizerRed"
                    style={{ width: props.field.equalizerRed + "%" }}
                    >
                </span>
                : null
            }
            {props.field.equalizerOrange ?
                <span
                    className="EqualizerOrange"
                    style={{ width: props.field.equalizerOrange + "%" }}
                    >
                </span>
                : null
            }
            {props.field.equalizerYellow ?
                <span
                    className="EqualizerYellow"
                    style={{ width: props.field.equalizerYellow + "%" }}
                    >
                </span>
                : null
            }
            {props.field.equalizerGray ?
                <span
                    className="EqualizerGray"
                    style={{ width: props.field.equalizerGray + "%" }}
                    >
                </span>
                : null
            }
        </div>
    );
}
