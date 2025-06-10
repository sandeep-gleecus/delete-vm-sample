//TypeScript React file that supports SourceCodeFileDetails.aspx

//external dependendencies (js libraries)
declare var React: any;
declare var ReactDOM: any;
declare var $: any;
declare var $find: any;
declare var $create: any;
declare var accessReact: any;
declare var $get: any;
declare var AspNetAjax$Function: any;

//inflectra services
declare var globalFunctions: any;
declare var SpiraContext: any;
declare var Inflectra: any;
declare var resx: any;
declare var syntaxHighlighting: any; // the client side code that uses the SyntaxHighlighter library to create the highlighted output

//inflectra objects
declare var ajxFormManager_id: string;

//url templates
declare var sourceCodeViewer_urlTemplate: string;

//global object accessible by the aspx page
var sourceCodeFileDetails = {
    messageBoxId: '' as string,

    updatePreview: function (branchKey, fileKey): void {
        //See if we can display a preview
        var previewAvailable: boolean = false;
        //Get the id and type of document (file vs url)
        let ajxFormManager = $find(ajxFormManager_id);
        let dataItem = ajxFormManager.get_dataItem();

        //See if we have a known code or image type
        let mimeType: string = dataItem.Fields._MimeType.textValue;

        if (mimeType == 'text/markdown')
        {
            //Load the preview
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService.SourceCodeFile_OpenMarkdown(SpiraContext.ProjectId, branchKey, fileKey, AspNetAjax$Function.createDelegate(this, this.updatePreviewMarkdown_success), AspNetAjax$Function.createDelegate(this, this.updatePreview_failure), dataItem);
            $('#markdownPreview').show();
            previewAvailable = true;
        }
        else
        {
            $('#markdownPreview').hide();
        }

        if (mimeType != 'text/markdown' && mimeType.substr(0, 'text'.length) == 'text' || mimeType == 'application/x-rapise' || mimeType == 'application/json' || mimeType == 'application/xml' || mimeType == 'application/x-bat') {
            //Load the preview
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService.SourceCodeFile_OpenText(SpiraContext.ProjectId, branchKey, fileKey, AspNetAjax$Function.createDelegate(this, this.updatePreview_success), AspNetAjax$Function.createDelegate(this, this.updatePreview_failure), dataItem);
            previewAvailable = true;
            $('#codePreview').show();
        }
        else {
            $('#codePreview').hide();
        }

        if (mimeType.substr(0, 'image'.length) == 'image') {
            let url: string = sourceCodeViewer_urlTemplate.replace('{0}', fileKey).replace('{1}', branchKey);
            previewAvailable = true;
            $('#imagePreview').show();
            $('#imgPreviewHyperLink').attr('href', url);
            $('#imgPreview').attr('src', url);
        }
        else {
            $('#imagePreview').hide();
        }

        if (previewAvailable) {
            //Hide no preview
            $('#noPreview').hide();
        }
        else {
            //Display no preview
            $('#noPreview').show();
            $('#codePreview').hide();
            $('#imagePreview').hide();
            $('#markdownPreview').hide();
        }

        //Update the tab
        $find(tabControl_id).updateHasData('tabPreview', previewAvailable);
    },

    updatePreview_success: function (preview: string, dataItem: any) {
        //run syntax highlighting
        let codePreview = $get('codePreview');
        let extension: string = dataItem.Fields.Name.textValue.split('.').pop();
        syntaxHighlighting.highlightElement(preview, extension, codePreview, true);
    },
    updatePreview_failure: function (ex: any) {
        //Display no preview
        $('#noPreview').show();
        $('#codePreview').hide();
        $('#imagePreview').hide();
        $('#markdownPreview').hide();
    },

    updatePreviewMarkdown_success: function (preview: string, dataItem: any) {
        let markdownPreview = $get('markdownPreview');
        globalFunctions.clearContent(markdownPreview);
        markdownPreview.innerHTML = preview;
        globalFunctions.cleanHtml(markdownPreview);
    }
};
