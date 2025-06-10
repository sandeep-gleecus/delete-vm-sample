//TypeScript React file that supports DocumentDetails.aspx

//external dependendencies (js libraries)
declare var React: any;
declare var ReactDOM: any;
declare var $: any;
declare var $find: any;
declare var $create: any;
declare var accessReact: any;
declare var $get: any;
declare var AspNetAjax$Function: any;
declare var filterXSS: any;
declare var filterXssInlineStyleOptions: any;
declare var dhx: any;

//inflectra services
declare var globalFunctions: any;
declare var SpiraContext: any;
declare var Inflectra: any;
declare var resx: any;
declare var syntaxHighlighting: any; // the client side code that uses the SyntaxHighlighter library to create the highlighted output

//inflectra objects
declare var ajxFormManager_id: string;

//Page controls
declare var lnkDocumentName_id: string;
declare var msgUploadMessage_id: string;

//URL templates
declare var urlTemplate_attachmentOpenUrl: string;
declare var urlTemplate_attachmentVersionOpenUrl: string;


//global object accessible by the aspx page
var documentDetails = {
    versionGridId: '' as string,
    versionHeaders: [] as Array<string>,
    uploadButton: '' as string,
    messageBoxId: '' as string,
    makeActiveButton: '' as string,
    deleteButton: '' as string,
    uploadClickHandler: null as Function,
    uploadFile_details: null as any,
    dlgUploadNewVersion_id: '' as string,
	attachmentTypeId: -1 as number,
	isAuthorizedToModify: false as boolean,
	mimeType: '' as string,

    check_hasData: function ()
    {
        //See if we have data
        Inflectra.SpiraTest.Web.Services.Ajax.DocumentVersionService.DocumentVersion_CountVersions(
            SpiraContext.ProjectId,
            SpiraContext.ArtifactId,
            AspNetAjax$Function.createDelegate(this, this.check_hasData_success),
            this.check_hasData_failure
        );
    },
    check_hasData_success: function (count)
    {
        //Update the tab 'has data'
        let hasData: boolean = (count > 0);
        $find(tabControl_id).updateHasData('tabVersions', hasData);
    },
    check_hasData_failure: function (ex)
    {
        //Fail quietly
    },

    updateUrl: function ():void
    {
        //Get the id and type of document (file vs url)
        let ajxFormManager = $find(ajxFormManager_id);
        let dataItem = ajxFormManager.get_dataItem();
        if (dataItem)
        {
            let url: string;
            this.attachmentTypeId = dataItem.Fields._AttachmentTypeId.intValue;
            if (dataItem.Fields._AttachmentTypeId.intValue == globalFunctions.attachmentTypeEnum.file)
            {
                //Use the Attachment template
                url = urlTemplate_attachmentOpenUrl.replace(globalFunctions.artifactIdToken, dataItem.primaryKey);
            }
            else
            {
                //Convert into URL
                url = globalFunctions.formNavigatableUrl(dataItem.Fields.Filename.textValue);
            }
            $get(lnkDocumentName_id).href = url;
        }
    },
    updatePreview: function (): void
    {
		//See if we can display a preview
        var previewAvailable:boolean = false;
        //Get the id and type of document (file vs url)
        let ajxFormManager = $find(ajxFormManager_id);
        let dataItem = ajxFormManager.get_dataItem();
        if (dataItem && dataItem.Fields._AttachmentTypeId.intValue == globalFunctions.attachmentTypeEnum.file)
        {
            //See if we have a known code or image type
            this.mimeType = dataItem.Fields._MimeType.textValue;

			//handle rich text preview
			if (this.mimeType == 'text/markdown') {
                //Load the preview
                Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.Documents_OpenMarkdown(SpiraContext.ProjectId, SpiraContext.ArtifactId, AspNetAjax$Function.createDelegate(this, this.updatePreviewMarkdown_success), AspNetAjax$Function.createDelegate(this, this.updatePreview_failure), dataItem);
                previewAvailable = true;
            }
			else if (this.mimeType == 'text/html') {
                //Load the preview
                Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.Documents_OpenText(SpiraContext.ProjectId, SpiraContext.ArtifactId, AspNetAjax$Function.createDelegate(this, this.updatePreviewMarkdown_success), AspNetAjax$Function.createDelegate(this, this.updatePreview_failure), dataItem);
                previewAvailable = true;
            }
			else {
				//Hide the preview and stop the setinterval in case it is running
				$('#htmlPreview').hide();
				$('#htmlPreviewIframe').hide()
            }

			//handle code / plain text preview
			if (this.mimeType != 'text/markdown' && this.mimeType != 'text/html' && this.mimeType.substr(0, 'text'.length) == 'text' || this.mimeType == 'application/x-rapise' || this.mimeType == 'application/json' || this.mimeType == 'application/xml' || this.mimeType == 'application/x-bat')
            {
				//Load the preview
                Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.Documents_OpenText(SpiraContext.ProjectId, SpiraContext.ArtifactId, AspNetAjax$Function.createDelegate(this, this.updatePreview_success), AspNetAjax$Function.createDelegate(this, this.updatePreview_failure), dataItem);
                previewAvailable = true;
                $('#codePreview').show();
            }
            else
            {
                $('#codePreview').hide();
			}

			//handle diagram preview preview
			if (this.mimeType == 'application/x-diagram' || this.mimeType == 'application/x-orgchart' || this.mimeType == 'application/x-mindmap' ) {
				//Load the preview
				Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.Documents_OpenText(SpiraContext.ProjectId, SpiraContext.ArtifactId, AspNetAjax$Function.createDelegate(this, this.updatePreviewDiagram_success), AspNetAjax$Function.createDelegate(this, this.updatePreview_failure), dataItem);
				previewAvailable = true;
				$('#diagramPreview').show();
			}
			else {
				$('#diagramPreview').hide();
			}

			//handle image preview
			if (this.mimeType.substr(0, 'image'.length) == 'image') 
            {
                let url: string = urlTemplate_attachmentOpenUrl.replace(globalFunctions.artifactIdToken, dataItem.primaryKey);
                previewAvailable = true;
                $('#imagePreview').show();
                $('#imgPreviewHyperLink').attr('href', url);
                $('#imgPreview').attr('src', url);
            }
            else
            {
                $('#imagePreview').hide();
            }
        }

        if (previewAvailable)
        {
            //Hide no preview
            $('#noPreview').hide();
        }
        else
        {
            //Display no preview
            $('#noPreview').show();
            $('#codePreview').hide();
			$('#imagePreview').hide();
			//Hide the preview and stop the setinterval in case it is running
			$('#htmlPreview').hide();
			$('#htmlPreviewIframe').hide()
        }

        //Update the tab
        $find(tabControl_id).updateHasData('tabPreview', previewAvailable);
    },

    updatePreview_success: function (preview: string, dataItem:any)
    {
        //run syntax highlighting
        let codePreview = $get('codePreview');
        let extension: string = dataItem.Fields.Filename.textValue.split('.').pop();
        syntaxHighlighting.highlightElement(preview, extension, codePreview, true);
    },
    updatePreview_failure: function (ex:any)
    {
        //Display no preview
        $('#noPreview').show();
        $('#codePreview').hide();
		$('#imagePreview').hide();
		//Hide the preview and stop the setinterval in case it is running
		$('#htmlPreview').hide();
		$('#htmlPreviewIframe').hide();
		$('#diagramPreview').hide();
		document.getElementById("diagramPreview-preview").innerHTML = "";
    },
	updatePreviewMarkdown_success: function (htmlString: string, dataItem: any) {
		const thisRef = this;

		//Check the preview string to see if it contains a style tag - we need to treat this differently to stop it polluting the app styling
		const hasStyleTag = htmlString.match(/<style[^>]*>[.\S\s\n]*?<\/style>/gm);
		//Display in an iframe if the string has a style tag
		if (hasStyleTag) {
			$('#htmlPreview').hide();
			$('#htmlPreviewIframe').show();
			//set the source of the iframe to the html to preview
			let htmlPreviewIframe = $get('htmlPreviewIframe');
			htmlPreviewIframe.srcdoc = htmlString;
			globalFunctions.cleanHtml(htmlPreviewIframe);

			//update the iframe's height - if it has changed
			var newHeight = (htmlPreviewIframe.contentWindow.document.body.scrollHeight + 50) + 'px';
			if (htmlPreviewIframe.style.height != newHeight) {
				htmlPreviewIframe.style.height = newHeight;
			}
			//otherwise render in a normal div - to give a more in-app look
		} else {
			$('#htmlPreview').show();
			$('#htmlPreviewIframe').hide();
			//set the preview string into the preview div
			let htmlPreview = $get('htmlPreview');
			globalFunctions.clearContent(htmlPreview);
			htmlPreview.innerHTML = htmlString;
			globalFunctions.cleanHtml(htmlPreview);
		}
	},

	updatePreviewDiagram_success: function (preview: string, dataItem: any) {
		//This switch code is also used in ServerControls/DiagramEditor.js
		//Specify a default so that the diagram will definitely load - diagram type is the most complete
		var diagramType = "default";
		switch (this.mimeType) {
			case 'application/x-diagram':
				diagramType = "default";
				break;
			case 'application/x-orgchart':
				diagramType = "org";
				break;
			case 'application/x-mindmap':
				diagramType = "mindmap";
				break;
		}

		document.getElementById("diagramPreview-preview").innerHTML = "";
		var diagram = new dhx.Diagram("diagramPreview-preview", {
			type: diagramType
		});
		if (preview) {
			diagram.data.parse(JSON.parse(JSON.stringify(preview)));
			document.getElementById('diagramPreviewExportPng').addEventListener("click", () => diagram.export.png());
			document.getElementById('diagramPreviewExportPdf').addEventListener("click", () => diagram.export.pdf());
		}
		//set the click events on the export buttons
	},

    //Renders the Versions ReactJS grid
	displayVersionsGrid: function (attachmentId: number, isAuthorizedToModify: boolean): void
    {
        ReactDOM.render(
            <DocumentVersionGrid
                domId={this.versionGridId}
                projectId={SpiraContext.ProjectId}
                attachmentId={attachmentId}
                headers={this.versionHeaders}
                uploadButtonLegend={this.uploadButton}
                uploadClickHandler={this.uploadClickHandler}
                messageBoxId={this.messageBoxId}
                makeActiveLegend={this.makeActiveButton}
                deleteLegend={this.deleteButton}
                attachmentTypeId={this.attachmentTypeId}
                ref={(rct_comp_documentVersionGrid) => { (window as any).rct_comp_documentVersionGrid = rct_comp_documentVersionGrid }}
                />,
            document.getElementById(this.versionGridId)
        );
    },

    uploadFile: function (file: any, attachmentId: number, description: string, version: string, makeActive:boolean)
    {
        if (file.size <= 0)
        {
            alert(resx.FileUpload_AttachmentEmpty);
            return;
        }
        if (file.size > SpiraContext.MaxAllowedContentLength)
        {
            alert(resx.FileUpload_AttachmentTooLarge.replace('{0}', (SpiraContext.MaxAllowedContentLength/1024)));
            return;
        }
        this.uploadFile_details = {
            filename: file.name,
            attachmentId: attachmentId,
            description: description,
            version: version,
            makeActive: makeActive,
        };
        globalFunctions.display_spinner();
        var reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = AspNetAjax$Function.createDelegate(this, this.uploadFile_shipOff);
    },

    uploadFile_shipOff: function(event)
    {
        var result:any = event.target.result;
        Inflectra.SpiraTest.Web.Services.Ajax.DocumentVersionService.DocumentVersion_UploadFile(
            SpiraContext.ProjectId,
            this.uploadFile_details.attachmentId,
            this.uploadFile_details.filename,
            this.uploadFile_details.description,
            this.uploadFile_details.version,
            result,
            this.uploadFile_details.makeActive,
            AspNetAjax$Function.createDelegate(this, this.uploadFile_success),
            AspNetAjax$Function.createDelegate(this, this.uploadFile_failure)
            );
    },
    uploadFile_success: function (newId)
    {
        //Close the dialog box and reload the grid
        globalFunctions.hide_spinner();
        $find(this.dlgUploadNewVersion_id).close();
        accessReact('rct_comp_documentVersionGrid', 'reload');
    },
    uploadFile_failure: function (exception)
    {
        globalFunctions.hide_spinner();
        globalFunctions.display_error($get(msgUploadMessage_id), exception);
    }
};

//the parent react component that handles all state
class DocumentVersionGrid extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.state = {
            items: [] as Array<any>,
            isAuthorizedToModify: false as boolean
        }

        this.loadData = this.loadData.bind(this);
        this.reload = this.reload.bind(this);
        this.uploadVersion = this.uploadVersion.bind(this);
        this.loadData_success = this.loadData_success.bind(this);
    }

    componentDidMount ()
    {
		this.loadData(this.props.attachmentId);
    }

    componentWillReceiveProps(nextProps)
    {
        if (this.props.attachmentId != nextProps.attachmentId)
        {
			this.loadData(nextProps.attachmentId);
        }
    }

    reload()
    {
        this.loadData(this.props.attachmentId);
    }

    loadData(attachmentId: number)
    {
        globalFunctions.display_spinner();
        var self = this;
        Inflectra.SpiraTest.Web.Services.Ajax.DocumentVersionService.DocumentVersion_RetrieveVersions(
            this.props.projectId,
            attachmentId,
            self.loadData_success,
            self.loadData_failure);
    }
    loadData_success(items)
    {
		globalFunctions.hide_spinner();
		const ajxFormManager = $find(ajxFormManager_id);

        //Update items
		this.setState({
			items: items,
			isAuthorizedToModify: globalFunctions.isAuthorizedToModifyCurrentArtifact(globalFunctions.artifactTypeEnum.document, ajxFormManager)
		});

		//Get the maximum version number of returned data
		SpiraContext.uiState.documentMaxVersionNumber = items.length ? Math.max(...items.map(x => x.Fields.VersionNumber.textValue)) : 0;

		//Also refresh the form manager
		$find(ajxFormManager_id).load_data();
    }
    loadData_failure (exception)
    {
        globalFunctions.hide_spinner();
        globalFunctions.display_error($get(this.props.messageBoxId), exception);
    }

    set_disabled()
    {
        this.setState({ isAuthorizedToModify: false })
    }
    set_enabled() {
        this.setState({ isAuthorizedToModify: true })
    }

    uploadVersion() {
        //Call the external handler
        this.props.uploadClickHandler();
    }

    render() {
        return (
            <React.Fragment>
                <div class="TabControlHeader">
                    <div className="btn-group mb3" role="group">
                        <button
                            type="button"
                            id="DocumentVersionGrid_Refresh"
                            class="btn btn-default"
                            onClick={this.reload}
                            >
                            <span class="fas fa-sync mr3"></span>
                            {resx.Global_Refresh}
                        </button>
                    </div>

                    {this.props.attachmentTypeId == globalFunctions.attachmentTypeEnum.file && this.state.isAuthorizedToModify ?
                        <div className="btn-group mb3" role="group">
                            <button
                                type="button"
                                id="DocumentVersionGrid_UploadVersion"
                                class="btn btn-default mr3"
                                onClick={this.uploadVersion}
                            >
                                <span class="fas fa-upload mr3"></span>
                                {this.props.uploadButtonLegend}
                            </button>
                        </div>
                        : null
                    }
                </div>

                <table className="DataGrid w-100">
                    <DocumentVersionGridHead
                        headers={this.props.headers}
                        />
                    <DocumentVersionGridBody
                        isAuthorizedToModify={this.state.isAuthorizedToModify}
                        loadData={this.loadData}
                        items={this.state.items}
                        projectId={this.props.projectId}
                        attachmentId={this.props.attachmentId}
                        messageBoxId={this.props.messageBoxId}
                        makeActiveLegend={this.props.makeActiveLegend}
                        deleteLegend={this.props.deleteLegend}
                        />
                </table>
            </React.Fragment>
        );
    }
}

function DocumentVersionGridHead(props) {
    return (
        <thead>
            <tr className="Header">
                <th className="priority2">{props.headers[0]}</th>
                <th className="priority1">{props.headers[1]}</th>
                <th className="priority1">{props.headers[2]}</th>
                <th className="priority2">{props.headers[3]}</th>
                <th className="priority4">{props.headers[4]}</th>
                <th className="priority4">{props.headers[5]}</th>
                <th className="priority3 ws-nowrap">
                    {props.headers[6]}
                    <span className="fas fa-sort-down" style={{paddingLeft: '3px'}} />
                </th>
                <th>{props.headers[7]}</th>
            </tr>
        </thead>
    )
}

function DocumentVersionGridBody(props) {
    return (
        <tbody>
            {props.items.map(function (item, i)
            {
                return <DocumentVersionGridRow
                    key={i}
                    item={item}
                    loadData={props.loadData}
                    messageBoxId={props.messageBoxId}
                    projectId = {props.projectId}
                    attachmentId = {props.attachmentId}
                    isAuthorizedToModify={props.isAuthorizedToModify}
                    makeActiveLegend={props.makeActiveLegend}
                    deleteLegend={props.deleteLegend}
                    />
            }) }
        </tbody>
    );
}
class DocumentVersionGridRow extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        //Event handling bindings
        this.makeActive = this.makeActive.bind(this);
        this.makeActive_success = this.makeActive_success.bind(this);
        this.deleteVersion = this.deleteVersion.bind(this);
        this.deleteVersion_success = this.deleteVersion_success.bind(this);
    }

    componentDidMount ()
    {
        //Clean the HTML to avoid XSS
        globalFunctions.cleanHtml(this.refs.versionDescription);
    }

    /* Event Handlers */

    makeActive () {
        globalFunctions.display_spinner();
        var self = this;
        Inflectra.SpiraTest.Web.Services.Ajax.DocumentVersionService.DocumentVersion_MakeActive(
            this.props.projectId,
            this.props.item.primaryKey,
            self.makeActive_success,
            self.makeActive_failure);
    }
    makeActive_success () {
        globalFunctions.hide_spinner();
        //Reload the grid
        this.props.loadData(this.props.attachmentId);

        //Also refresh the form manager
        $find(ajxFormManager_id).load_data();
    }
    makeActive_failure (exception) {
        globalFunctions.hide_spinner();
        globalFunctions.display_error($get(this.props.messageBoxId), exception);
    }

    deleteVersion () {
        //Make sure they actually want to delete the version
        if (confirm(resx.DocumentDetails_DeleteVersionConfirm)) {
            globalFunctions.display_spinner();
            var self = this;
            Inflectra.SpiraTest.Web.Services.Ajax.DocumentVersionService.DocumentVersion_Delete(
                this.props.projectId,
                this.props.item.primaryKey,
                self.deleteVersion_success,
                self.deleteVersion_failure);
        }
    }
    deleteVersion_success () {
        globalFunctions.hide_spinner();
        //Reload the grid
        this.props.loadData(this.props.attachmentId);
    }
    deleteVersion_failure (exception) {
        globalFunctions.hide_spinner();
        globalFunctions.display_error($get(this.props.messageBoxId), exception);
    }

    render() {
        var that = this;
        return (
            <tr>
                <td className="text-center priority2">
                    <span className="fas fa-check" style={{ display: (this.props.item.Fields.IsCurrent.textValue == 'Y') ? 'inline' : 'none' }} />
                </td>
                <td className="priority1 ws-nowrap">
                    <img
                        className="w4 h4"
                        src={SpiraContext.BaseThemeUrl + 'Images/' + this.props.item.Fields.Filetype.textValue}
                        title={this.props.item.Fields.Filetype.tooltip}
                        />
                    <span className="pr2"></span>
                    <a className="external-link" href={urlTemplate_attachmentVersionOpenUrl.replace(globalFunctions.artifactIdToken, this.props.item.primaryKey)} target="_blank">
                        {this.props.item.Fields.Filename.textValue}
                    </a>
                </td>
                <td className="priority1 ws-nowrap">{this.props.item.Fields.VersionNumber.textValue}</td>

                <td className="priority2" ref="versionDescription" dangerouslySetInnerHTML={{ __html: filterXSS(this.props.item.Fields.Description.textValue, filterXssInlineStyleOptions) }}></td>
                <td className="priority4 ws-nowrap">{this.props.item.Fields.Size.textValue}</td>
                <td className="priority4 ws-nowrap">{this.props.item.Fields.AuthorId.textValue}</td>
                <td className="priority3 ws-nowrap">
                    <span className="has-tooltip">
						{this.props.item.Fields.UploadDate.textValue}
						<span className="is-tooltip">
							{globalFunctions.parseJsonDate(this.props.item.Fields.UploadDate.dateValue).toString()}
						</span>
                    </span>
                </td>
                <td>
                    {this.props.item.Fields.IsCurrent.textValue == 'N' && this.props.isAuthorizedToModify ?
                        <div className="btn-group dif" role="group">
                            <button type="button" className="btn btn-default" onClick={this.makeActive}>
                                {this.props.makeActiveLegend}
                            </button>
                            <button type="button" className="btn btn-default" onClick={this.deleteVersion}>
                                {this.props.deleteLegend}
                            </button>
                        </div>
                        : null
                    }
                </td>
            </tr>
        );
    }
}
