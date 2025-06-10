var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.TreeView = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.TreeView.initializeBase(this, [element]);
    this._webServiceClass = null;
    this._loadingImageUrl = '';
    this._cssClass = '';
    this._containerId = -1;
    this._themeFolder = '';
    this._errorMessageControlId = '';
    this._nodeLegendControlId = '';
    this._nodeLegendFormat = '';
    this._rootNodeID = '';
    this._selectedNode = '';
    this._count = 0;
    this._nodesToExpand = new Array();
    this._controlBaseId = '';
    this._itemName = '';
    this._clickElements = new Array();
    this._clickHandlers = new Array();
    this._tooltips = new Array();
    this._tooltipMouseOverHandlers = new Array();
    this._tooltipMouseOutHandlers = new Array();
    this._isOverNode = false;
    this._autoLoad = true;
    this._loaded = false;
    this._allowEdit = false;
    this._editDescriptions = false;
    this._addDialog = null;
    this._addDialog_ddl = null;
    this._addDialog_name = null;
    this._editDialog = null;
    this._editDialog_ddl = null;
    this._editDialog_name = null;
    this._editButton = null;
    this._editPanel = null;
    this._inEditMode = false;
    this._currentlyEditedNodeId = null;
    this._innerDiv = null;
    this._applicationPath = '';
    this._pageUrlTemplate = '';

    //Dragging variables
    this._allowDragging = false;
    this._draggingCompleteCallback = null;
}

Inflectra.SpiraTest.Web.ServerControls.TreeView.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.TreeView.callBaseMethod(this, 'initialize');

        //Create the inner DIV that contains the tree
        this._innerDiv = d.ce('div');
        this.get_element().appendChild(this._innerDiv);

        this._controlBaseId = this.get_element().id + '_';  //Used to prefix all element IDs
        this.prepareTree();
        if (this._allowEdit)
        {
            this.createEditButtons();
        }
        if (this._autoLoad)
        {
            this.onLeafClick(this._rootNodeID, 'ATV_tree', false, Function.createDelegate(this, this._initialize_callback));
        }
        else
        {
            //Let other controls know we've initialized
            this.raise_init();

            //See if we have a selected node, expand all the nodes to it
            if (this._selectedNode && this._selectedNode != '')
            {
                this.refresh();
            }
            else
            {
                //We're done loading
                this.raise_loaded();
            }
        }
        //Add any event listeners
        //Need to add event listener for handling browsing back on load - so that listener is always running
        if (this._pageUrlTemplate)
        {
            var _this = this;
            window.addEventListener("popstate", function (event) {
                _this._handlePopState(event, _this);
            });
        }
    },
    _initialize_callback: function ()
    {
        //Let other controls know we've initialized
        this.raise_init();

        //See if we have a selected node, expand all the nodes to it
        if (this._selectedNode && this._selectedNode != '')
        {
            this.refresh();
        }
        else
        {
            //We're done loading
            this.raise_loaded();
        }
    },
    dispose: function ()
    {
        //Add custom dispose actions here
        for (var i = 0; i < this._tooltips.length; i++)
        {
            $removeHandler(this._tooltips[i], 'mouseover', this._tooltipMouseOverHandlers[i]);
            $removeHandler(this._tooltips[i], 'mouseout', this._tooltipMouseOutHandlers[i]);
            delete this._tooltips[i];
            delete this._tooltipMouseOverHandlers[i];
            delete this._tooltipMouseOutHandlers[i];
        }
        for (var i = 0; i < this._clickElements.length; i++)
        {
            $removeHandler(this._clickElements[i], 'click', this._clickHandlers[i]);
            delete this._clickElements[i];
            delete this._clickHandlers[i];
        }

        if (this._addDialog)
        {
            delete this._addDialog;
            delete this._addDialog_ddl;
            delete this._addDialog_name;
        }
        if (this._editDialog)
        {
            delete this._editDialog;
            delete this._editButton;
            delete this._editDialog_name;
        }
        if (this._editPanel)
        {
            delete this._editPanel;
        }

        delete this._innerDiv;
        delete this._tooltips;
        delete this._tooltipMouseOverHandlers;
        delete this._tooltipMouseOutHandlers;
        delete this._clickHandlers;
        delete this._clickElements;
        delete this._nodesToExpand;
        delete this._draggingCompleteCallback;

        Inflectra.SpiraTest.Web.ServerControls.TreeView.callBaseMethod(this, 'dispose');
    },
    /*  =========================================================
    The properties
    =========================================================  */
    get_cssClass: function ()
    {
        return this._cssClass;
    },
    set_cssClass: function (value)
    {
        this._cssClass = value;
    },

    get_allowDragging: function ()
    {
        return this._allowDragging;
    },
    set_allowDragging: function (value)
    {
        this._allowDragging = value;
    },

    get_allowEdit: function ()
    {
        return this._allowEdit;
    },
    set_allowEdit: function (value)
    {
        this._allowEdit = value;
    },
    
    get_editDescriptions: function ()
    {
        return this._editDescriptions;
    },
    set_editDescriptions: function (value)
    {
        this._editDescriptions = value;
    },

    get_applicationPath: function ()
    {
        return this._applicationPath;
    },
    set_applicationPath: function (value)
    {
        this._applicationPath = value;
    },

    get_autoLoad: function ()
    {
        return this._autoLoad;
    },
    set_autoLoad: function (value)
    {
        this._autoLoad = value;
    },

    get_loaded: function ()
    {
        return this._loaded;
    },
    set_loaded: function (value)
    {
        this._loaded = value;
    },

    get_itemName: function ()
    {
        return this._itemName;
    },
    set_itemName: function (value)
    {
        this._itemName = value;
    },

    get_containerId: function ()
    {
        return this._containerId;
    },
    set_containerId: function (value)
    {
        this._containerId = value;
    },

    get_themeFolder: function ()
    {
        return this._themeFolder;
    },
    set_themeFolder: function (value)
    {
        this._themeFolder = value;
    },

    get_errorMessageControlId: function ()
    {
        return this._errorMessageControlId;
    },
    set_errorMessageControlId: function (value)
    {
        this._errorMessageControlId = value;
    },

    get_nodeLegendControlId: function ()
    {
        return this._nodeLegendControlId;
    },
    set_nodeLegendControlId: function (value)
    {
        this._nodeLegendControlId = value;
    },

    get_nodeLegendFormat: function ()
    {
        return this._nodeLegendFormat;
    },
    set_nodeLegendFormat: function (value)
    {
        this._nodeLegendFormat = value;
    },

    get_loadingImageUrl: function ()
    {
        /// <value type="String" mayBeNull="false">
        /// The image to display during loading new nodes
        /// </value> 
        return this._loadingImageUrl;
    },
    set_loadingImageUrl: function (value)
    {
        /// <value type="String" mayBeNull="false">
        /// The image to display during loading new nodes
        /// </value> 
        this._loadingImageUrl = value;
    },

    get_webServiceClass: function ()
    {
        return this._webServiceClass;
    },
    set_webServiceClass: function (value)
    {
        this._webServiceClass = value;
    },

    get_rootNodeID: function ()
    {
        /// <value type="String" mayBeNull="false">
        /// Which node id to begin the tree with
        /// </value> 
        return this._rootNodeID;
    },
    set_rootNodeID: function (value)
    {
        /// <value type="String" mayBeNull="false">
        /// Which node id to begin the tree with
        /// </value> 
        this._rootNodeID = value;
    },
    
    get_pageUrlTemplate: function ()
    {
        return this._pageUrlTemplate;
    },
    set_pageUrlTemplate: function (value)
    {
        this._pageUrlTemplate = value;
    },

    /*    =========================================================
    selectedNode()
    stores the Node number which should be marked as selected
    =========================================================  */
    set_selectedNode: function (value)
    {
        this._selectedNode = value;
    },
    get_selectedNode: function ()
    {
        return this._selectedNode;
    },

    /*  =========================================================
    The methods
    =========================================================  */

    //Updates the status icons (if populated)
    updateStatus: function (nodeId, imageUrl)
    {
        //Find the status icon
        var statusDiv = $get(this._controlBaseId + 'status_' + nodeId);
        if (statusDiv)
        {
            statusDiv.style.backgroundImage = 'url(' + this._themeFolder + 'Images/' + imageUrl + ')';
        }
    },
    //Forces the tree to reload
    refresh: function ()
    {
        //Call the webservice to get the list of nodes we need to expand (based on the currently selected node)
        var webService = this.get_webServiceClass();
        webService.TreeView_GetExpandedNodes(this._containerId, Function.createDelegate(this, this.refresh_success), Function.createDelegate(this, this.operation_failure));
    },
    refresh_success: function (ids)
    {
        //Call the methods to expand the nodes
        this.setNodeExpansionQueue(ids);
        this.expandAllInQueue();
    },

    display_add_dialog: function (evt)
    {
        if (this._addDialog)
        {
            //Display the dialog and populate the parent dropdown list
            //Set the parent to the current folder (if one selected)
            this._addDialog.display(evt);
            this._addDialog_name.value = '';
            this._addDialog_ddl.clearItems();
            this._addDialog_ddl.addItem('', '0', '', '', 'Y', resx.Global_NoneDropDown);
            if (this._selectedNode && this._selectedNode != '' && this._selectedNode != '0')
            {
                this._addDialog_ddl.set_selectedItem(this._selectedNode);
            }
            else
            {
                this._addDialog_ddl.set_selectedItem('');
            }
            if (this._editDescriptions && CKEDITOR)
            {
                var controlId = this.get_element().id + '_addDialog_desc';
                var rte = CKEDITOR.instances[controlId];
                if (rte)
                {
                    rte.setData('');
                }
            }
            var webService = this.get_webServiceClass();
            webService.TreeView_GetAllNodes(this.get_containerId(), Function.createDelegate(this, this.display_add_dialog_success), Function.createDelegate(this, this.operation_failure));
        }
    },
    display_add_dialog_success: function (data)
    {
        this._addDialog_ddl.set_dataSource(data);
        this._addDialog_ddl.dataBind();
    },
    display_edit_dialog: function (evt, nodeId, name, desc)
    {
        if (this._editDialog)
        {
            var context = {};
            context.nodeId = nodeId;
            this._currentlyEditedNodeId = nodeId;

            //Display the dialog and populate the parent dropdown list
            this._editDialog.display(evt);
            this._editDialog_name.value = name;
            if (this._editDescriptions && CKEDITOR)
            {
                var controlId = this.get_element().id + '_editDialog_desc';
                var rte = CKEDITOR.instances[controlId];
                if (rte)
                {
                    rte.setData(desc);
                }
            }
            this._editDialog_ddl.clearItems();
            this._editDialog_ddl.addItem('', '0', '', '', 'Y', resx.Global_NoneDropDown);
            this._editDialog_ddl.set_selectedItem('');
            var webService = this.get_webServiceClass();
            webService.TreeView_GetAllNodes(this.get_containerId(), Function.createDelegate(this, this.display_edit_dialog_success), Function.createDelegate(this, this.operation_failure), context);
        }
    },
    display_edit_dialog_success: function (data, context)
    {
        //Remove the current node (if present)
        this._editDialog_ddl.set_dataSource(data);
        this._editDialog_ddl.dataBind();
        this._editDialog_ddl.removeItem(context.nodeId);

        //Get the parent id of the current node
        var webService = this.get_webServiceClass();
        webService.TreeView_GetParentNode(this.get_containerId(), context.nodeId, Function.createDelegate(this, this.display_edit_dialog_success2), Function.createDelegate(this, this.operation_failure), context);
    },
    display_edit_dialog_success2: function (parentNodeId, context)
    {
        if (parentNodeId && parentNodeId != '')
        {
            this._editDialog_ddl.set_selectedItem(parentNodeId);
        }
        else
        {
            this._editDialog_ddl.set_selectedItem('');
        }
    },

    //Called when an item is dropped onto a node
    node_item_drop: function (artifactIds, nodeId)
    {
        var context = {};
        var webService = this.get_webServiceClass();
        webService.TreeView_DragDestination(this.get_containerId(), artifactIds, nodeId, Function.createDelegate(this, this.node_item_drop_success), Function.createDelegate(this, this.operation_failure), context);
    },
    node_item_drop_success: function (data, context)
    {
        //Execute the callback method if appropriate
        this.raise_itemDropped();
    },

    prepareTree: function ()
    {
        /// <summary>
        /// Sets up the tree before it can be used
        /// </summary>
        /// <returns />
        var treeContainerEl = this._innerDiv;
        treeContainerEl.innerHTML = '';
        var div = d.ce('div');
        div.id = this._controlBaseId + 'cat_' + this._rootNodeID;
        treeContainerEl.appendChild(div);
    },
    createEditButtons: function ()
    {
        this._editPanel = d.ce('div');
        this._editPanel.className = "ATV_AdminOptions";
        this.get_element().appendChild(this._editPanel);

        //Create the buttons for adding/editing nodes
        //Edit/Done
        this._editButton = d.ce('button');
        this._editButton.className = 'btn btn-default';
        this._editButton.type = 'button';
        this._editButton.value = resx.Global_Edit;
        this._editButton.textContent = resx.Global_Edit;
        this._editPanel.appendChild(this._editButton);
        var editClickHandler = Function.createDelegate(this, this._onEditClick);
        $addHandler(this._editButton, 'click', editClickHandler);
        this._clickElements.push(this._editButton);
        this._clickHandlers.push(editClickHandler);

        //Add
        var add = d.ce('button');
        add.className = 'btn btn-default';
        add.type = 'button';
        add.value = resx.Global_Add;
        add.textContent = resx.Global_Add;
        this._editPanel.appendChild(add);
        var addClickHandler = Function.createDelegate(this, this._onAddClick);
        $addHandler(add, 'click', addClickHandler);
        this._clickElements.push(add);
        this._clickHandlers.push(addClickHandler);

        //Also create the two dialog boxes
        //Add
        var div = d.ce('div');
        div.className = 'PopupPanel rich-text-mini';
        div.style.position = 'absolute';
        div.style.display = 'none';
        div.style.width = '400px';
        div.style.height = '250px';
        this.get_element().appendChild(div);
        var title = resx.TreeView_AddItem.replace('{0}', this._itemName);
        this._addDialog = $create(Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel, { themeFolder: this.get_themeFolder(), errorMessageControlId: this.get_errorMessageControlId(), title: title, persistent: true, modal: true }, null, null, div);
        var p = d.ce('p');
        div.appendChild(p);
        p.appendChild(d.createTextNode(resx.TreeView_ChooseParentItem.replace('{0}', this._itemName)));
        //Parent dropdown list
        var ddl = d.ce('div');
        ddl.style.width = '100%';
        ddl.style.marginBottom = '1rem';
        div.appendChild(ddl);
        this._addDialog_ddl = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy, { themeFolder: this._themeFolder, itemImage: 'Images/FolderOpen.svg', enabledCssClass: "u-dropdown is-active", disabledCssClass: "u-dropdown disabled" }, null, null, ddl);
        p = d.ce('p');
        div.appendChild(p);
        p.appendChild(d.createTextNode(resx.TreeView_EnterName.replace('{0}', this._itemName)));
        //The name of the new folder
        p = d.ce('p');
        div.appendChild(p);
        this._addDialog_name = d.ce('input');
        this._addDialog_name.style.width = '100%';
        this._addDialog_name.type = 'text';
        this._addDialog_name.maxLength = 255;
        this._addDialog_name.className = 'u-input is-active mb4';
        p.appendChild(this._addDialog_name);
        //The description (optional)
        if (this._editDescriptions)
        {
            div.style.minHeight = '430px'; //Make the dialog taller
            div.style.height = '';
            p = d.ce('p');
            div.appendChild(p);
            p.appendChild(d.createTextNode(resx.TreeView_EnterDescription.replace('{0}', this._itemName)));
            var textarea = d.ce('textarea');
            textarea.id = this.get_element().id + '_addDialog_desc';
            textarea.style.width = '100%';
            textarea.style.height = '80px';
            div.appendChild(textarea);

            //Convert to ckEditor
            CKEDITOR.replace(textarea.id, {
                customConfig: '',
                height: '80px',
                uploadUrl: '',
                toolbar: globalFunctions.ckEditor_toolbarNormal
            });
        }
        p = d.ce('p');
        div.appendChild(p);
        var btnGrp = d.ce('div');
        btnGrp.className = "btn-group pt3";
        p.appendChild(btnGrp);
        var add = d.ce('input');
        add.type = 'button';
        add.className = 'btn btn-primary';
        add.type = 'button';
        add.value = resx.Global_Add;
        add.textContent = resx.Global_Add;
        btnGrp.appendChild(add);
        var cancel = d.ce('input');
        cancel.type = 'button';
        cancel.className = 'btn btn-default';
        cancel.type = 'button';
        cancel.value = resx.Global_Cancel;
        cancel.textContent = resx.Global_Cancel;
        btnGrp.appendChild(cancel);
        //Hook up the buttons
        var addConfirmClickHandler = Function.createDelegate(this, this._onAddConfirmClick);
        $addHandler(add, 'click', addConfirmClickHandler);
        this._clickElements.push(add);
        this._clickHandlers.push(addConfirmClickHandler);
        var addCancelClickHandler = Function.createDelegate(this, this._onAddCancelClick);
        $addHandler(cancel, 'click', addCancelClickHandler);
        this._clickElements.push(cancel);
        this._clickHandlers.push(addCancelClickHandler);

        //Edit
        div = d.ce('div');
        div.className = 'PopupPanel rich-text-mini';
        div.style.position = 'absolute';
        div.style.display = 'none';
        div.style.width = '400px';
        div.style.height = '250px';
        this.get_element().appendChild(div);
        title = resx.TreeView_EditItem.replace('{0}', this._itemName);
        this._editDialog = $create(Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel, { themeFolder: this.get_themeFolder(), errorMessageControlId: this.get_errorMessageControlId(), title: title, persistent: true, modal: true }, null, null, div);
        var p = d.ce('p');
        div.appendChild(p);
        p.appendChild(d.createTextNode(resx.TreeView_EditParentItem.replace('{0}', this._itemName)));
        //Parent dropdown list
        var ddl = d.ce('div');
        ddl.style.width = '100%';
        ddl.style.marginBottom = '1rem';
        div.appendChild(ddl);
        this._editDialog_ddl = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy, { themeFolder: this._themeFolder, itemImage: 'Images/FolderOpen.svg', enabledCssClass: "u-dropdown is-active", disabledCssClass: "u-dropdown disabled" }, null, null, ddl);
        p = d.ce('p');
        div.appendChild(p);
        p.appendChild(d.createTextNode(resx.TreeView_EditName.replace('{0}', this._itemName)));
        //The name of the new folder
        p = d.ce('p');
        div.appendChild(p);
        this._editDialog_name = d.ce('input');
        this._editDialog_name.style.width = '100%';
        this._editDialog_name.type = 'text';
        this._editDialog_name.maxLength = 255;
        this._editDialog_name.className = 'u-input is-active mb4';
        p.appendChild(this._editDialog_name);
        //The description (optional)
        if (this._editDescriptions)
        {
            div.style.minHeight = '430px'; //Make the dialog taller
            div.style.height = '';
            p = d.ce('p');
            div.appendChild(p);
            p.appendChild(d.createTextNode(resx.TreeView_EditDescription.replace('{0}', this._itemName)));
            var textarea = d.ce('textarea');
            textarea.id = this.get_element().id + '_editDialog_desc';
            textarea.style.width = '100%';
            textarea.style.height = '80px';
            div.appendChild(textarea);

            //Convert to ckEditor
            CKEDITOR.replace(textarea.id, {
                customConfig: '',
                height: '80px',
                uploadUrl: '',
                toolbar: globalFunctions.ckEditor_toolbarNormal
            });
        }
        p = d.ce('p');
        div.appendChild(p);
        var btnGrp = d.ce('div');
        btnGrp.className = "btn-group pt3";
        p.appendChild(btnGrp);
        var edit = d.ce('input');
        edit.type = 'button';
        edit.className = 'btn btn-primary';
        edit.type = 'button';
        edit.value = resx.Global_Update;
        edit.textContent = resx.Global_Update;
        btnGrp.appendChild(edit);
        var deleteButton = d.ce('button');
        deleteButton.className = 'btn btn-default';
        deleteButton.type = 'button';
        deleteButton.value = resx.Global_Delete;
        deleteButton.textContent = resx.Global_Delete;
        btnGrp.appendChild(deleteButton);
        var cancel = d.ce('input');
        cancel.type = 'button';
        cancel.className = 'btn btn-default';
        cancel.type = 'button';
        cancel.value = resx.Global_Cancel;
        cancel.textContent = resx.Global_Cancel;
        btnGrp.appendChild(cancel);
        //Hook up the buttons
        var editConfirmClickHandler = Function.createDelegate(this, this._onEditConfirmClick);
        $addHandler(edit, 'click', editConfirmClickHandler);
        this._clickElements.push(edit);
        this._clickHandlers.push(editConfirmClickHandler);
        var editCancelClickHandler = Function.createDelegate(this, this._onEditCancelClick);
        $addHandler(cancel, 'click', editCancelClickHandler);
        this._clickElements.push(cancel);
        this._clickHandlers.push(editCancelClickHandler);
        var editDeleteClickHandler = Function.createDelegate(this, this._onEditDeleteClick);
        $addHandler(deleteButton, 'click', editDeleteClickHandler);
        this._clickElements.push(deleteButton);
        this._clickHandlers.push(editDeleteClickHandler);
    },
    toggle_edit_state: function ()
    {
        //Toggle the state
        this._inEditMode = !this._inEditMode;
        //Update the items
        var editButtons = d.getElementsByName(this._controlBaseId + 'edit');
        if (editButtons)
        {
            for (var i = 0; i < editButtons.length; i++)
            {
                editButtons[i].style.display = (this._inEditMode) ? 'inline' : 'none';
            }
        }
        //Set the label appropriately
        this._editButton.value = (this._inEditMode) ? resx.Global_Done : resx.Global_Edit;
        this._editButton.textContent = (this._inEditMode) ? resx.Global_Done : resx.Global_Edit;
    },

    _onToggleClick: function (sender, e)
    {
        e.thisRef.expandClick(e.nodeId);
    },
    _onNodeClick: function (sender, e)
    {
        e.thisRef.nodeClick(e.nodeId, null, e.nodeName, sender, false);
    },
    _onNodeEditClick: function (evt, e)
    {
        e.thisRef.display_edit_dialog(evt, e.nodeId, e.name, e.desc);
    },

    _onEditClick: function (evt)
    {
        this.toggle_edit_state();
    },
    _onAddClick: function (evt)
    {
        this.display_add_dialog(evt);
    },
    _onAddConfirmClick: function (evt)
    {
        if (globalFunctions.trim(this._addDialog_name.value) == '')
        {
            alert(resx.TreeView_NeedToProvideItemName.replace('{0}', this._itemName));
            return;
        }

        var parentFolderId = null;
        if (this._addDialog_ddl.get_selectedItem())
        {
            parentFolderId = this._addDialog_ddl.get_selectedItem().get_value();
        }

        //Call the webservice to add the new item
        var webServiceClass = this._webServiceClass;
        var desc = null;
        if (this._editDescriptions && CKEDITOR)
        {
            var controlId = this.get_element().id + '_addDialog_desc';
            var rte = CKEDITOR.instances[controlId];
            if (rte)
            {
                desc = rte.getData();
            }
        }
        webServiceClass.TreeView_AddNode(this._containerId, this._addDialog_name.value, parentFolderId, desc, Function.createDelegate(this, this._onAddConfirmClick_success), Function.createDelegate(this, this.dialog_operation_failure));
    },
    _onAddConfirmClick_success: function (newNodeId)
    {
        //The new node was added
        this._addDialog.close();
        this.load_data(true);
    },
    _onAddCancelClick: function (evt)
    {
        this._addDialog.close();
    },

    _onEditConfirmClick: function (evt)
    {
        if (!this._currentlyEditedNodeId)
        {
            return;
        }

        if (globalFunctions.trim(this._editDialog_name.value) == '')
        {
            alert(resx.TreeView_NeedToProvideItemName.replace('{0}', this._itemName));
            return;
        }

        var parentFolderId = null;
        if (this._editDialog_ddl.get_selectedItem())
        {
            parentFolderId = this._editDialog_ddl.get_selectedItem().get_value();
        }

        //Call the webservice to update the item
        var webServiceClass = this._webServiceClass;
        var desc = null;
        if (this._editDescriptions && CKEDITOR)
        {
            var controlId = this.get_element().id + '_editDialog_desc';
            var rte = CKEDITOR.instances[controlId];
            if (rte)
            {
                desc = rte.getData();
            }
        }
        webServiceClass.TreeView_UpdateNode(this._containerId, this._currentlyEditedNodeId, this._editDialog_name.value, parentFolderId, desc, Function.createDelegate(this, this._onEditConfirmClick_success), Function.createDelegate(this, this.dialog_operation_failure));
    },
    _onEditConfirmClick_success: function ()
    {
        //The node was updated
        this._editDialog.close();
        this.toggle_edit_state();
        this.load_data(true);
    },
    _onEditDeleteClick: function (evt)
    {
        if (!this._currentlyEditedNodeId)
        {
            return;
        }
        //Verify they want to delete
        if (!confirm(resx.TreeView_ConfirmDelete.replace('{0}', this._itemName)))
        {
            return;
        }

        //Call the webservice to delete the item
        var webServiceClass = this._webServiceClass;
        webServiceClass.TreeView_DeleteNode(this._containerId, this._currentlyEditedNodeId, Function.createDelegate(this, this._onEditDeleteClick_success), Function.createDelegate(this, this.operation_failure));

    },
    _onEditDeleteClick_success: function ()
    {
        //The node was deleted
        this._editDialog.close();
        this.toggle_edit_state();
        this.load_data(true);
    },
    _onEditCancelClick: function (evt)
    {
        this._editDialog.close();
    },

    _onNodeMouseOver: function (sender, e)
    {
        //Don't display if in drag
        if (e.thisRef._isInDrag)
        {
            return;
        }

        //If the tooltip is null, need to get it through a web service
        //If we allow descriptions to be edited, always use what was provided
        if (e.thisRef._editDescriptions)
        {
            //In this case the 'tooltip' field only has the description
            if (e.tooltip)
            {
				ddrivetip('<u>' + globalFunctions.htmlEncode(e.name) + '</u><br/>' + globalFunctions.htmlEncode(e.tooltip));
            }
            else
            {
				ddrivetip(globalFunctions.htmlEncode(e.name));
            }
        }
        else
        {
            if (e.tooltip)
            {
				ddrivetip(globalFunctions.htmlEncode(e.tooltip));
            }
            else
            {
                //Display the loading message
                ddrivetip(resx.Global_Loading);
                e.thisRef._isOverNode = true;   //Set the flag since asynchronous
                //Now get the real tooltip via Ajax web-service call
                var nodeId = e.nodeId;
                var webServiceClass = e.thisRef._webServiceClass;
                webServiceClass.TreeView_GetNodeTooltip(nodeId, Function.createDelegate(e.thisRef, e.thisRef.getNodeTooltip_success), Function.createDelegate(e.thisRef, e.thisRef.getNodeTooltip_failure));
            }
        }
    },
    getNodeTooltip_success: function (tooltip)
    {
        if (this._isOverNode)
        {
            ddrivetip(tooltip);
        }
    },
    getNodeTooltip_failure: function (exception)
    {
        //Do nothing
    },

    _onNodeMouseOut: function (sender, e)
    {
        hideddrivetip();
        e.thisRef._isOverNode = false;
    },

    onLeafClick: function (nodeId, className, forceOpen, callback)
    {
        /// <summary>
        /// triggered when a leaf or branch is clicked. It will either expand or collapse a branch
        /// </summary>
        /// <param name="nodeId" type="number">
        /// the id of the node which is being clicked
        /// </param>
        /// <returns />
        var li = $get(this._controlBaseId + 'cat_' + nodeId);

        if (!li)
        {
            if (callback != undefined)
            {
                callback();
            }
            return;
        }

        // no sub cats means do not expand
        if (li.className.indexOf('ATV_bullet') != -1)
        {
            if (callback != undefined)
            {
                callback();
            }
            return;
        }

        // check if the node has been expanded
        if (li.className.indexOf('ATV_opened') != -1 && !forceOpen)
        {
            // update the LI
            li.className = 'ATV_closed';

            // possibly expand another node
            this.expandNextInQueue();

            // possibly update the selected node
            this.refreshSelectedNodes();
            if (callback != undefined)
            {
                callback();
            }
            return;
        }

        // if the subnodes are collapsed, they might still be loaded
        if (li.getAttribute('tst:loaded') == 'yes')
        {
            // get the sub nodes container
            li.className = 'ATV_opened';

            // possibly expand another node
            this.expandNextInQueue();

            // possibly update the selected node
            this.refreshSelectedNodes();

            if (callback != undefined)
            {
                callback();
            }
            return;
        }

        // Possibly add an UL with temporary content (spinner)
        var ul = $get(this._controlBaseId + 'branch_' + nodeId);
        if (ul)
        {
            var li2 = d.ce('li');
            li2.setAttribute('role', 'treeitem');
            ul.appendChild(li2);
            var img = d.ce('img');
            img.className = "w4 h4";
            img.src = this._themeFolder + this._loadingImageUrl;
            li2.appendChild(img);
        }
        else
        {
            var ul2 = d.ce('ul');
            ul2.setAttribute('role', 'group');
            ul2.id = this._controlBaseId + 'branch_' + nodeId;
            ul2.className = className;
            var li2 = d.ce('li');
            li2.setAttribute('role', 'treeitem');
            ul2.appendChild(li2);
            var img = d.ce('img');
            img.className = "w4 h4";
            img.src = this._themeFolder + this._loadingImageUrl;
            li2.appendChild(img);
            li.appendChild(ul2);
        }

        // expand the clicked node to display the placeholder
        li.className = 'ATV_opened';

        // Send the AJAX request
        var context = {};
        context.nodeId = nodeId;
        context.callback = callback;
        var webService = this._webServiceClass;
        webService.TreeView_GetNodes(this._containerId, nodeId, Function.createDelegate(this, this.get_nodes_success), Function.createDelegate(this, this.operation_failure), context);
    },
    expandBranch: function (parentId, nodes)
    {
        /// <summary>
        /// expands a tree node and fills it with the supplied list of nodes
        /// </summary>
        /// <param name="parentId" type="number">
        /// the id of the node which is being expanded
        /// </param>
        /// <param name="nodes" type="array">
        /// Array of TreeViewNode serialized objects
        /// </param>
        /// <returns />
        // populate the UL with LI nodes
        var ul = $get(this._controlBaseId + 'branch_' + parentId);

        //Clear any existing content
        ul.innerHTML = '';

        //Create the child elements for each returned node
        for (var i = 0; i < nodes.length; i++)
        {
            var treeViewNode = nodes[i];

            //Create the LI
            var li = d.ce('li');
            li.setAttribute('role', 'treeitem');
            li.className = 'ATV_closed';
            li.id = this._controlBaseId + 'cat_' + treeViewNode.nodeId;
            li.setAttribute('tst:nodeid', treeViewNode.nodeId);
            ul.appendChild(li);

            //Create the DIVs

            //See if we need to display an expand/collapse
            var div1 = d.ce('div');
            //HACK: Need to add a dummy image for IE8 to work (known issue in IE8)
            if (document.all)
            {
                var dummyImage = d.ce('img');
                dummyImage.style.width = '9px';
                dummyImage.style.height = '12px';
                dummyImage.src = 'data:image/gif;base64,R0lGODlhAQABAJH/AP///wAAAMDAwAAAACH5BAEAAAIALAAAAAABAAEAQAICVAEAOw==';
                div1.appendChild(dummyImage);
            }
            if (treeViewNode.hasChildren)
            {
                div1.className = 'Toggle';
                div1.title = Inflectra.SpiraTest.Web.GlobalResources.TreeView_ExpandCollapse;
            }
            else
            {
                div1.className = 'Leaf';
            }
            div1.style.cssFloat = 'left';
            li.appendChild(div1);
            //Add the click handler
            if (treeViewNode.hasChildren)
            {
                var _onDivHandler = Function.createCallback(this._onToggleClick, { thisRef: this, nodeId: treeViewNode.nodeId });
                $addHandler(div1, 'click', _onDivHandler);
                this._clickElements.push(div1);
                this._clickHandlers.push(_onDivHandler);
            }

            //See if we have a status icon to display
            if (treeViewNode.statusImageUrl)
            {
                var div4 = d.ce('div');
                div4.id = this._controlBaseId + 'status_' + treeViewNode.nodeId;
                div4.style.cssFloat = 'left';
                div4.className = 'Node';
                div4.style.backgroundImage = 'url(' + this._themeFolder + 'Images/' + treeViewNode.statusImageUrl + ')';
                li.appendChild(div4);
            }

            var div2 = d.ce('div');
            //See if we have an explicit image to use
            if (treeViewNode.nodeImageUrl)
            {
                div2.className = 'Node';
                div2.style.backgroundImage = 'url(' + this._themeFolder + 'Images/' + treeViewNode.nodeImageUrl + ')';
            }
            else
            {
                div2.className = 'folder-tree-folder w4 h4 mr2 fl';
                div2.title = Inflectra.SpiraTest.Web.GlobalResources.TreeView_Folder;
            }
            li.appendChild(div2);

            var div3 = d.ce('div');
            div3.className = 'folder-name';
            li.appendChild(div3);

            //Add the drag target behavior
            if (this.get_allowDragging())
            {
                $create(Inflectra.SpiraTest.Web.ServerControls.TreeViewNodeBehavior, { 'primaryKey': treeViewNode.nodeId, 'parent': this }, null, null, div3);
            }

            //The hyperlink (if applicable)
            if (treeViewNode.clickable)
            {
                //See if we support URL rewriting
                var a = d.ce('a');
                if (this._pageUrlTemplate && this._pageUrlTemplate != '')
                {
                    a.href = this._pageUrlTemplate.replace(globalFunctions.artifactIdToken, '' + treeViewNode.nodeId);
                }
                else
                {
                    a.href = 'javascript:void(0)';
                }
                a.className = "nowrap";
                a.appendChild(d.createTextNode(treeViewNode.name));
                div3.appendChild(a);
                //Add the click handler
                var _onLinkHandler = Function.createCallback(this._onNodeClick, { thisRef: this, nodeId: treeViewNode.nodeId, nodeName: treeViewNode.name });
                $addHandler(a, 'click', _onLinkHandler);
                this._clickElements.push(a);
                this._clickHandlers.push(_onLinkHandler);
            }
            else
            {
                var span = d.ce('span');
                span.style.fontWeight = 'bold';
                span.appendChild(d.createTextNode(treeViewNode.name));
                div3.appendChild(span);
            }

            //Add the tooltips
            var _tooltipMouseOverHandler = Function.createCallback(this._onNodeMouseOver, { thisRef: this, nodeId: treeViewNode.nodeId, tooltip: treeViewNode.tooltip, name: treeViewNode.name });
            var _tooltipMouseOutHandler = Function.createCallback(this._onNodeMouseOut, { thisRef: this, nodeId: treeViewNode.nodeId });
            $addHandler(div3, 'mouseover', _tooltipMouseOverHandler);
            $addHandler(div3, 'mouseout', _tooltipMouseOutHandler);
            this._tooltips.push(div3);
            this._tooltipMouseOverHandlers.push(_tooltipMouseOverHandler);
            this._tooltipMouseOutHandlers.push(_tooltipMouseOutHandler);

            //If this item is editable, add the edit button
            if (this._allowEdit && treeViewNode.nodeId > 0)
            {
                var edit = d.ce('button');
                var thisRef = this;
                var name = treeViewNode.name;
                edit.className = 'btn btn-default btn-inline';
                edit.type = 'button';
                edit.id = this._controlBaseId + 'edit_' + treeViewNode.nodeId;
                edit.name = this._controlBaseId + 'edit';
                edit.value = resx.Global_Edit;
                edit.textContent = resx.Global_Edit;
                edit.style.display = 'none';
                var nodeEditClickHandler = Function.createCallback(this._onNodeEditClick, { thisRef: this, nodeId: treeViewNode.nodeId, name: treeViewNode.name, desc: treeViewNode.tooltip });
                $addHandler(edit, 'click', nodeEditClickHandler);
                this._clickElements.push(edit);
                this._clickHandlers.push(nodeEditClickHandler);
                div3.appendChild(edit);
            }

            //Final DIV
            var div4 = d.ce('div');
            div4.style.clear = 'both';
            li.appendChild(div4);
        }

        // update the parent node (LI)
        var parentNode = $get(this._controlBaseId + 'cat_' + parentId);
        parentNode.setAttribute('tst:loaded', 'yes');
        parentNode.className = 'ATV_opened';

        // possibly update the selected node
        this.refreshSelectedNodes();
    },

    load_data: function (ignoreExisting)
    {
        //If we're in edit mode, cancel
        if (this._inEditMode)
        {
            this.toggle_edit_state();
        }

        //See if we need to clear all the existing content (used when refreshing treeview)
        if (ignoreExisting)
        {
            this.prepareTree();
            this.onLeafClick(this._rootNodeID, 'ATV_tree', false, Function.createDelegate(this, this.load_data_completed));
        }
        else
        {
            this.onLeafClick(this._rootNodeID, 'ATV_tree', false);
        }
    },
    load_data_completed: function ()
    {
        this._loaded = false;
        this.refresh();
    },

    operation_failure: function (exception, context)
    {
        //Populate the error message control if we have one (if not use alert instead)
        //Display validation exceptions in a friendly manner
        if (exception.get_message() == 'Authorization failed')
        {
            //Simply don't load the nodes, and add a message instead
            var ul = $get(this._controlBaseId + 'branch_');
            if (ul)
            {
                ul.innerHTML = '<li>' + resx.TreeView_NotAuthorizedToView + '</li>';
            }
            var callback = context.callback;
            if (callback != undefined)
            {
                callback();
            }
        }
        else
        {
            globalFunctions.display_error($get(this.get_errorMessageControlId()), exception);
        }
    },
    dialog_operation_failure: function (exception)
    {
        //Display as an alert
        globalFunctions.display_error(null, exception);
    },

    expandClick: function (nodeId, className, forceOpen, callback)
    {
        this.onLeafClick(nodeId, className, forceOpen, callback);
    },
    nodeClick: function (nodeId, className, nodeName, evt, doNotUpdateFolderUrl)
    {
        //If we have a CTRL/SHIFT click and a URL is available, just let the browser open the new tab
        var newTab = false;
        if (evt && this._pageUrlTemplate && this._pageUrlTemplate != '')
        {
            if (evt.shiftKey || evt.ctrlKey || evt.metaKey)
            {
                newTab = true;
            }
        }
        if (!newTab)
        {
            //Stop the normal URL firing if it might do
            if (evt)
            {
                evt.stopPropagation();
                evt.preventDefault();
            }

            //First make sure the node is open
            this.onLeafClick(nodeId, className, true);

            //Now we need to set the style of this node to selected
            this.set_selectedNode(nodeId);
            this.refreshSelectedNodes();

            //See if we have an external legend to set
            if (nodeName)
            {
                this.updateNodeLegend(nodeName);
            }

            //Do not attempt to update the folder url when bool passed in when handling pop state of history
            if (nodeId && !doNotUpdateFolderUrl)
            {
                //Update the URL if that is enabled
                this.updateFolderUrl(nodeId, nodeName);
            }

            //Finally persist the change
            var webService = this._webServiceClass;
            webService.TreeView_SetSelectedNode(this._containerId, nodeId, Function.createDelegate(this, this.nodeClick_success), Function.createDelegate(this, this.operation_failure));
        }
    },
    nodeClick_success: function ()
    {
        //Now raise an event that can be listened for
        this.raise_nodeSelected();
    },

    updateFolderUrl: function (nodeId, nodeName)
    {
        //Update the URL to reflect the change in folder, if the page supports it
        if (this._pageUrlTemplate && this._pageUrlTemplate != '' && history && history.pushState)
        {
            var href = this._pageUrlTemplate.replace(globalFunctions.artifactIdToken, '' + nodeId);
            history.pushState({ nodeId: nodeId, nodeName: nodeName }, null, href);
        }
    },

    //Handles browser pop state - eg when hitting browser back button
    _handlePopState: function (event, thisRef) {
        //check if a data value for the back/forward page was stored using pushState above
        var nodeIdPrevious = event.state && event.state.nodeId ? event.state.nodeId : null;
        var nodeIdToLoad = nodeIdPrevious;

        // live reload the data if we are actually moving to a different artifact id
        if (nodeIdToLoad) {
            thisRef.nodeClick(nodeIdToLoad, null, event.state.nodeName || null, null, true);
        }
        else
        {
            window.location = location;
        }
    },

    updateNodeLegend: function (nodeName)
    {
        if (nodeName && nodeName != null)
        {
            var nodeLegendText = this._nodeLegendFormat.replace('{0}', nodeName);
            var legendSpan = $get(this._nodeLegendControlId);
            if (legendSpan)
            {
                globalFunctions.clearContent(legendSpan);
                legendSpan.appendChild(d.createTextNode(nodeLegendText));
            }
        }
        else
        {
            var legendSpan = $get(this._nodeLegendControlId);
            if (legendSpan)
            {
                globalFunctions.clearContent(legendSpan);
            }
        }
    },

    /* Event handlers managers */
    add_nodeSelected: function (handler)
    {
        this.get_events().addHandler('nodeSelected', handler);
    },
    remove_nodeSelected: function (handler)
    {
        this.get_events().removeHandler('nodeSelected', handler);
    },
    raise_nodeSelected: function ()
    {
        var h = this.get_events().getHandler('nodeSelected');
        if (h) h(this._selectedNode);
    },
    add_loaded: function (handler)
    {
        this.get_events().addHandler('loaded', handler);
    },
    remove_loaded: function (handler)
    {
        this.get_events().removeHandler('loaded', handler);
    },
    raise_loaded: function ()
    {
        this._loaded = true;
        var h = this.get_events().getHandler('loaded');
        if (h) h();
    },
    add_init: function (handler)
    {
        this.get_events().addHandler('init', handler);
    },
    remove_init: function (handler)
    {
        this.get_events().removeHandler('init', handler);
    },
    raise_init: function ()
    {
        var h = this.get_events().getHandler('init');
        if (h) h();
    },

    add_itemDropped: function (handler)
    {
        this.get_events().addHandler('itemDropped', handler);
    },
    remove_itemDropped: function (handler)
    {
        this.get_events().removeHandler('itemDropped', handler);
    },
    raise_itemDropped: function ()
    {
        var h = this.get_events().getHandler('itemDropped');
        if (h) h();
    },

    /* Other Functions */

    get_nodes_success: function (nodes, context)
    {
        var parentId = context.nodeId;
        var callback = context.callback;
        this.expandBranch(parentId, nodes);
        //Finally call the callback function
        if (callback != undefined)
        {
            callback();
        }
    },

    /*    =========================================================
    refreshSelectedNodes()
    verifies and updates the class names of the selected node 
    to ensure the right node is marked as selected
    =========================================================  */
    refreshSelectedNodes: function ()
    {
        //Iterate through all the LI nodes and see if we have a match
        //If so then select it, otherwise de-select
        var rootNode = $get(this._controlBaseId + 'branch_' + this._rootNodeID);
        this.refreshNode(rootNode);
    },
    refreshNode: function (rootNode)
    {
        var i;
        for (i = 0; i < rootNode.childNodes.length; i++)
        {
            var node = rootNode.childNodes[i];
            //We only want to examine element nodes (type 1)
            if (node.nodeType == 1 && (node.tagName == 'LI' || node.tagName == 'UL'))
            {
                //See if this the selected node or not
                if (node.id == this._controlBaseId + 'cat_' + this._selectedNode)
                {
                    if (node.className.indexOf('ATV_selected') == -1)
                    {
                        // apply the ATV_selected class to the correct LI
                        node.className += " ATV_selected";
                    }
                }
                else
                {
                    node.className = node.className.replace('ATV_selected', '');
                }
                //See if we have any child nodes to recurse
                if (node.childNodes.length > 0)
                {
                    this.refreshNode(node);
                }
            }
        }
    },

    /*    =========================================================
    this.setNodeExpansionQueue()
    Stores an array of nodes to open progressively.
    Use this method to "sync" the tree with the current page,
    in other words, if someone follows a direct link to a page
    you will want to open the tree accordingly. Send in the
    parent ids to be opened and the tree will expand them in
    turn until all of them are expanded.
    =========================================================  */
    setNodeExpansionQueue: function (ids)
    {
        this._nodesToExpand = ids;
    },
    appendToNodeExpansionQueue: function (ids)
    {
        this._nodesToExpand = this._nodesToExpand.concat(ids);
    },

    /*    =========================================================
    this.expandNextInQueue()
    pos the last Node in queue and expands it
    =========================================================  */
    expandNextInQueue: function ()
    {
        if (this._nodesToExpand.length < 1)
            return false;

        // get and remove the first Node number
        var id = this._nodesToExpand.shift();

        // Expand the Node
        this.expandClick(id, '', true);
    },
    expandAllInQueue: function ()
    {
        if (this._nodesToExpand.length < 1)
        {
            //Have a slight to delay to allow all the ajax calls to complete
            var thisRef = this;
            setTimeout(function () { thisRef.expandAllInQueue_completed(); }, 1000);
            //this.expandAllInQueue_completed();
            return false;
        }

        // get and remove the first Node number
        var id = this._nodesToExpand.shift();

        // Expand the Node and callback recursively to expand successive nodes
        this.expandClick(id, '', true, Function.createDelegate(this, this.expandAllInQueue));
    },
    expandAllInQueue_completed: function ()
    {
        //Mark the currently selected node (if there is one)
        if (this._selectedNode && this._selectedNode != '' && !this._loaded)
        {
            //Find this node and mark as selected
            var li = $get(this._controlBaseId + 'cat_' + this._selectedNode);
            if (li)
            {
                if (li.className.indexOf('ATV_selected') == -1)
                {
                    // apply the ATV_selected class to the correct LI
                    li.className += " ATV_selected";
                }
                if (li.childNodes.length > 2 && li.childNodes[2].childNodes.length > 0 && li.childNodes[2].childNodes[0].tagName == 'A')
                {
                    var a = li.childNodes[2].childNodes[0];
                    this.updateNodeLegend(a.innerText);
                }
            }
        }

        //Raise the loaded event
        this.raise_loaded();
    }
}
Inflectra.SpiraTest.Web.ServerControls.TreeView.registerClass('Inflectra.SpiraTest.Web.ServerControls.TreeView', Sys.UI.Control);

/* TreeViewNode */
Inflectra.SpiraTest.Web.ServerControls.TreeViewNodeBehavior = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.TreeViewNodeBehavior.initializeBase(this, [element]);
    this._element = element;
    this._parent = null;
    this._primaryKey = null;
}
Inflectra.SpiraTest.Web.ServerControls.TreeViewNodeBehavior.prototype =
{
    /* Constructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.TreeViewNodeBehavior.callBaseMethod(this, 'initialize');

        // register this drop target in DragDropManager
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.registerDropTarget(this);
    },
    dispose: function ()
    {
        // unregister this drop target in DragDropManager
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.unregisterDropTarget(this);

        Inflectra.SpiraTest.Web.ServerControls.TreeViewNodeBehavior.callBaseMethod(this, 'initialize');
    },

    /* Properties */
    get_element: function ()
    {
        return this._element;
    },

    get_parent: function ()
    {
        return this._parent;
    },
    set_parent: function (value)
    {
        this._parent = value;
    },

    get_primaryKey: function ()
    {
        return this._primaryKey;
    },
    set_primaryKey: function (value)
    {
        this._primaryKey = value;
    },

    /* IDragSource Interface */

    //Uncomment this section if we decide to allow treeviews to self-drag
    //get_dragDataType: function ()
    //{
    //    return 'TreeViewNode';
    //},

    //getDragData: function (context)
    //{
    //    return this;
    //},

    //get_dragMode: function ()
    //{
    //    return Inflectra.SpiraTest.Web.ServerControls.DragMode.Move;
    //},

    //onDragStart: function () { },

    //onDrag: function ()
    //{
    //    var evt = window._event;
    //    var visual = this._parent.get_visual();
    //    var ddm = Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance();
    //    if (evt.changedTouches)
    //    {
    //        //touch
    //        var touchPosition = { x: evt.changedTouches[0].pageX, y: evt.changedTouches[0].pageY };
    //        var position = ddm.subtractPoints(touchPosition, visual.startingPoint);
    //        Sys.UI.DomElement.setLocation(visual, position.x, position.y);
    //    }
    //    else
    //    {
    //        //mouse
    //        var mousePosition = { x: evt.clientX, y: evt.clientY };
    //        var scrollOffset = ddm.getScrollOffset(visual, /* recursive */true);
    //        var position = ddm.addPoints(ddm.subtractPoints(mousePosition, visual.startingPoint), scrollOffset);
    //        Sys.UI.DomElement.setLocation(visual, position.x, position.y);
    //    }
    //},

    //onDragEnd: function (canceled)
    //{
    //    this._parent._isInDrag = false;
    //    var visual = this._parent.get_visual();
    //    this._touchDragging = false;
    //    //Clear the visual
    //    if (visual)
    //    {
    //        visual.parentNode.removeChild(visual);
    //        this._parent.set_visual(null);
    //    }
    //    //Make sure all targets returned to normal
    //    $(this._parent.get_element()).find('tr.drag-target').removeClass('drag-target');
    //},

    /* IDropTarget Interface */

    // return a DOM element represents the container
    get_dropTargetElement: function ()
    {
        return this.get_element();
    },

    // if this draggable item can be dropped into this drop target
    canDrop: function (dragMode, dataType, data)
    {
        return (dataType == "SortedGridRow" && data);
    },

    // drop this draggable item into this drop target
    drop: function (dragMode, dataType, data)
    {
        if (dataType == "SortedGridRow" && data)
        {
            $(this.get_element()).removeClass('drag-target');
            var sourcePrimaryKey = data.get_primaryKey();
            var destPrimaryKey = this._primaryKey;
            //See if we need to include any selected items (checkbox)
            if (data.get_selectedItems && data.get_selectedItems() && data.get_selectedItems().length > 0)
            {
                //Call the webservice with the source and destination ids
                var sourcePrimaryKeys = data.get_selectedItems();
                if (sourcePrimaryKeys.indexOf(sourcePrimaryKey) == -1)
                {
                    sourcePrimaryKeys.push(sourcePrimaryKey);
                }
                this._parent.node_item_drop(sourcePrimaryKeys, destPrimaryKey);
            }
            else
            {
                //Call the webservice with the source and destination ids
                this._parent.node_item_drop([sourcePrimaryKey], destPrimaryKey);
            }
        }
    },

    // this method will be called when a draggable item is entering this drop target
    onDragEnterTarget: function (dragMode, dataType, data)
    {
        if (dataType == "SortedGridRow" && data)
        {
            $(this.get_element()).addClass('drag-target');
        }
    },

    // this method will be called when a draggable item is leaving this drop target
    onDragLeaveTarget: function (dragMode, dataType, data)
    {
        if (dataType == "SortedGridRow" && data)
        {
            $(this.get_element()).removeClass('drag-target');
        }
    },

    // this method will be called when a draggable item is hovering on this drop target
    onDragInTarget: function (dragMode, dataType, data)
    {
        if (dataType == "SortedGridRow" && data)
        {
        }
    }
}
Inflectra.SpiraTest.Web.ServerControls.TreeViewNodeBehavior.registerClass('Inflectra.SpiraTest.Web.ServerControls.TreeViewNodeBehavior', Sys.UI.Behavior, Inflectra.SpiraTest.Web.ServerControls.IDropTarget);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();

// attaches a handler that is fired first after web service is returned, before specific handler is called   
// specifically, if the web service errored because of authentication failure (auth cookie expired)   
// then redirect the page to the login page.  
Sys.Net.WebRequestManager.add_completedRequest(On_WebRequestCompleted);

function On_WebRequestCompleted(sender, eventArgs)
{
    if (sender.get_statusCode() === 500)
    {
        if (sender.get_object().Message === "Authentication failed.")
        {
            window.location.href = window.location.protocol + "//" + window.location.host + window.location.pathname.substring(0, window.location.pathname.indexOf('/', 1)) + '/Login.aspx?ReturnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
        }
        if (sender.get_object().Message === "Reload Page.")
        {
            //Force a reload
            window.location.href = window.location.href;
        }
    }
}
