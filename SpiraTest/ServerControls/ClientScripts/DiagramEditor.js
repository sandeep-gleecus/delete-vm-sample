var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.DiagramEditor = function (element) {
	this._data = '';
	this._diagramType = "";
	this._enabled = true;
	this._controlsDefault = {
		autoLayout: false,
		apply: false,
		export: false,
		import: false
	};
	this._editor = null;

	//Load in all the passed parameters from the server-control
	Inflectra.SpiraTest.Web.ServerControls.DiagramEditor.initializeBase(this, [element]);
}

Inflectra.SpiraTest.Web.ServerControls.DiagramEditor.prototype =
{
	initialize: function () {
		Inflectra.SpiraTest.Web.ServerControls.DiagramEditor.callBaseMethod(this, 'initialize');

		//Display the editor
		this.display();
	},
	dispose: function () {
		Inflectra.SpiraTest.Web.ServerControls.DiagramEditor.callBaseMethod(this, 'dispose');
	},

	// -------- Properties -------- //
	get_data: function () {
		return this._data;
	},
	set_data: function (value) {
			this._data = value ? JSON.parse(JSON.stringify(value)) : "";
	},

	get_diagramType: function () {
		return this._diagramType;
	},
	set_diagramType: function (value) {
		this._diagramType = value;
	},

	get_enabled: function () {
		return this._enabled;
	},
	set_enabled: function (value) {
		this._enabled = value ? true : false;
	},

	setDiagramTypeFromMimeType: function (mimeType) {
		//This switch code is also used in Typescript/DocumentDetails.tsx
		//Only set a type when there is a matching mimeType so that we do not render a diagram if we do not actually have one
		var diagramType = "";
		switch (mimeType) {
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
		if (diagramType) {
			this.set_diagramType(diagramType);
		}
	},

	loadData: function () {
		if (this.get_data()) {
			this._editor.parse(this.get_data());
		}
	},

	// -------- Methods --------- //
	display: function () {
		//Clear out any existing content
		var editorWrapper = this.get_element();
		globalFunctions.clearContent(editorWrapper);
		editorWrapper.className = "h10 w-100 resize-v ov-auto";
		editorWrapper.onclick = () => false;

		var resx = Inflectra.SpiraTest.Web.GlobalResources;
		var locale = {
			applyAll: resx.DiagramEditor_ApplyAll,
			exportData: resx.DiagramEditor_ExportData,
			importData: resx.DiagramEditor_ImportData,
			resetChanges: resx.DiagramEditor_ResetChanges,
			autoLayout: resx.DiagramEditor_AutoLayout,
			gridStep: resx.DiagramEditor_GridStep,
			arrange: resx.DiagramEditor_Arrange,
			position: resx.DiagramEditor_Position,
			size: resx.DiagramEditor_Size,
			color: resx.DiagramEditor_Color,
			title: resx.DiagramEditor_Title,
			text: resx.DiagramEditor_Text,
			image: resx.DiagramEditor_Image,
			fill: resx.DiagramEditor_Fill,
			textProps: resx.DiagramEditor_TextProps,
			stroke: resx.DiagramEditor_Stroke,
			shapeSections: resx.DiagramEditor_ShapeSections,
			imageUpload: resx.DiagramEditor_ImageUpload,
			emptyState: resx.DiagramEditor_EmptyState
		};
		dhx.i18n.setLocale("diagram", locale);

		this._editor = new dhx.DiagramEditor(this.get_element().id, {
			type: this.get_diagramType(),
			controls: this._controlsDefault,
			editMode: this.get_enabled()
		});
	},

	reRender: function () {
		this.display();
		this.loadData();
	}
};

Inflectra.SpiraTest.Web.ServerControls.DiagramEditor.registerClass('Inflectra.SpiraTest.Web.ServerControls.DiagramEditor', Sys.UI.Control);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
