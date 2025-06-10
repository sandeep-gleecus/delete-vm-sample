var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.DropDownList = function (element)
{
    this._element = element;

    if (!element)
    {
        this._element = d.ce('div');
//        this._element.style.width = '100px';    //Default width
    }
    //ARIA
    this._element.setAttribute('role', 'combobox');
    this._element.setAttribute('aria-owns', this._element.id + '_list,' + this._element.id + '_text');

    Inflectra.SpiraTest.Web.ServerControls.DropDownList.initializeBase(this, [this._element]);

    /* Enumerators */
    this.ClassNamesEnum =
    {
        valuePanelClassName: 'value',
        itemsPanelClassName: 'list dropdown-popup DropDownList-menu',
        itemClassName: 'item'
    };

    /* Init internal items */
    this._valueTextBox = d.ce('input');
    this._valueTextBox.id = this._element.id + '_text';
    this._valueTextBox.type = 'text';
    this._valueTextBox.length = 255;    //Max length
    this._valueTextBox.autocomplete = 'off';
    
    this._menuDown = d.ce('div');
    this._menuDown.id = this._element.id + '_menuDown';
    this._menuDown.className = 'fas fa-chevron-down menu-down';
       
    //ARIA
    this._valueTextBox.setAttribute('role', 'textbox');
    this._valueTextBox.setAttribute('aria-autocomplete', 'list');
    this._valueTextBox.setAttribute('aria-owns', this._element.id + '_list');
    this._valueTextDiv = d.ce('div');
    this._valueHidden = d.ce('input');
    this._valueHidden.id = this._element.id + '_Value';
    this._valueHidden.type = 'hidden';
    this._listDiv = d.ce('div');
    this._listDiv.id = this._element.id + '_list'
    //ARIA
    this._listDiv.setAttribute('role', 'listbox');
    this._textNode = d.createTextNode('');

    /* Internal variables*/
    this._controlClicked = false;
    this._expanded = false;
    this._multiSelectable = false;
    this._displayMobileAsDesktop = false; //can specify, if set to true, to NOT render control differently on mobile devices
    this._selectedItem = null;
    this._itemKeys = null;
    this._itemValues = null;
    this._dataBindIndex = 0;
    this._renderedFullList = false;
    this._oldText = '';
    this._enabled = true;
    this._displayAsHyperLinks = false;
    this._hyperlinkBaseUrl = '';
    this._enabledCssClass = 'DropDown DropDownList';
    this._disabledCssClass = "DropDown Disabled DropDownList";
    this._hidden = false;
    this._scrollableParent = null;
    this._raiseChangeOnClosed = false;
    this._isMobileDevice = false; //this is not linked to the server control

    //Datasource
    this._dataSource = null;
};

Inflectra.SpiraTest.Web.ServerControls.DropDownList.prototype =
{
    /* Init / Dispose */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.DropDownList.callBaseMethod(this, 'initialize');

        /* Event delegates */
        this._onClickHandler = Function.createDelegate(this, this._onClick);
        this._onTextKeyPressHandler = Function.createDelegate(this, this._onTextKeyPress);
        this._onItemClickHandler = Function.createDelegate(this, this._onItemClick);
        this._onDocumentClickHandler = Function.createDelegate(this, this._onDocumentClick);

        /* Public method delegates */
        this.dropdown = Function.createDelegate(this, this._onClick);
        this.getItem = Function.createDelegate(this, this._onGetItem);
        this.addItem = Function.createDelegate(this, this._onAddItem);
        this.removeItem = Function.createDelegate(this, this._onRemoveItem);
        this.clearItems = Function.createDelegate(this, this._onClearItems);

        this._isMobileDevice = SpiraContext.IsMobile;

        // Append elements to the parent div
        this._valueTextDiv.appendChild(this._valueTextBox);
        this._valueTextDiv.appendChild(this._menuDown);
        this._element.appendChild(this._valueTextDiv);
        this._element.appendChild(this._listDiv);
        this._element.appendChild(this._valueHidden);

        /* Assign controls event handlers */
        $addHandlers(this._element, { 'click': this._onClickHandler }, this);
        $addHandlers(document, { 'click': this._onDocumentClickHandler }, this);
        $addHandler(this._valueTextBox, 'keyup', this._onTextKeyPressHandler);

        /* Set default values */
        if (this._enabled)
        {
            this._element.className = this._enabledCssClass;
            this._valueTextBox.disabled = '';
        }
        else
        {
            this._element.className = this._disabledCssClass;
            this._valueTextBox.disabled = 'disabled';
        }
        this._valueTextBox.className = this.ClassNamesEnum.valuePanelClassName;
        this._listDiv.className = this.ClassNamesEnum.itemsPanelClassName;
        
        $(this._listDiv).addClass('dn').removeClass('dib');
        if (this._isMobileDevice && !this._displayMobileAsDesktop) {
            $(this._listDiv).removeClass('u-popup u-popup_full_keep-nav is-open ov-y-auto');
        }

        if (this._listWidth)
        {
            if (this._listWidth && this._listWidth.substr(this._listWidth.length - 2, 2) == "px")
            {
                this._listDiv.style.width = this._listWidth;
            }
            else
            {
                this._listDiv.style.width = 'auto';
            }
        }
        else
        {
            //Use the width of the main control unless it's a %
            var elementWidth = this.get_element().style.width;
            if (elementWidth && elementWidth.substr(elementWidth.length - 2, 2) == "px")
            {
                this._listDiv.style.width = elementWidth;
            }
            else
            {
                this._listDiv.style.width = 'auto';
            }
        }

        //If we have a datasource then databind
        if (this._dataSource)
        {
            this.dataBind();
        }
    },
    dispose: function ()
    {
        $clearHandlers(this.get_element());
        $clearHandlers(this._valueTextBox);
        $clearHandlers(this._listDiv);

        if (this._onClickHandler) delete this._onClickHandler;
        if (this._onTextKeyPressHandler) delete this._onTextKeyPressHandler;
        if (this._onItemClickHandler) delete this._onItemClickHandler;
        if (this._onDocumentClickHandler) delete this._onDocumentClickHandler;

        if (this.dropdown) delete this.dropdown;
        if (this.getItem) delete this.getItem;
        if (this.removeItem) delete this.removeItem;
        if (this.clearItems) delete this.clearItems;
        delete this._valueTextBox;
        delete this._valueHidden;
        delete this._listDiv;
        delete this._menuDown;

        Inflectra.SpiraTest.Web.ServerControls.DropDownList.callBaseMethod(this, 'dispose');
    },

    /* Event Handlers */
    _onTextKeyPress: function (evt)
    {
        //Check that it's not certain ignored keys (TAB, Down Arrow)
        var keynum = evt.keyCode | evt.which;
        //on mobile the dropdown should render inline (ie not fullscreen) on key presses
        var renderInlineDropdownOnMobile = this._isMobileDevice;

        if (keynum == 40)
        {
            //Render all items (down-arrow)
            this.render_items(false);
            this._expanded = true;

            this.display_popup(renderInlineDropdownOnMobile);
        }
        else if (keynum != 9 && keynum != 38 && keynum != 37 && keynum != 39)
        {
            //Render just the matching items and display
            this.render_items(true);
            this._expanded = true;
            this.display_popup(renderInlineDropdownOnMobile);
        }
    },
    _onClick: function (e)
    {
        //See what was clicked
        if (e.target)
        {
            targ = e.target;
        }
        else if (e.srcElement)
        {
            targ = e.srcElement;
        }
        if (targ.nodeType == 3) // defeat Safari bug
        {
            targ = targ.parentNode;
        }
        var tname;
        tname = targ.tagName;

        //Make sure enabled
        if (!this._enabled)
        {
            return;
        }

        //Call the method to display the dropdownlist (if not already displayed and not on the input)
        if (tname == 'INPUT')
        {
            //Need to select all the text
            if (targ.type == 'text')
            {
                this._valueTextBox.select();
            }
        }

        //If not rendered, render now
        //on mobile make sure to render the dropdown not full screen (ie inline) if the input element was clicked
        var renderInlineDropdownOnMobile = tname == 'INPUT' && this._isMobileDevice;
        if (!this._renderedFullList)
        {
            this.render_items(false);
        }
        this._expanded = !this._expanded;
        this.display_popup(renderInlineDropdownOnMobile);
        this._controlClicked = true;
    },
    _onItemClick: function (itm, e)
    {
        e.stopPropagation();

        //See what was clicked
        if (e.target)
        {
            targ = e.target;
        }
        else if (e.srcElement)
        {
            targ = e.srcElement;
        }
        if (targ.nodeType == 3) // defeat Safari bug
        {
            targ = targ.parentNode;
        }
        var tname;
        tname = targ.tagName;

        //Handle the case of the checkbox separately from the item itself
        if (tname == 'INPUT')
        {
            if (targ.checked)
            {
                this.add_selectedItem(itm.get_value());
            }
            else
            {
                this.remove_selectedItem(itm.get_value());
            }
        }
        else
        {
            this.set_selectedItem(itm.get_value());

            //Close the menu when the item itself is selected
            $(this._listDiv).addClass('dn').removeClass('dib');
            $(this._element).addClass('is-closed').removeClass('is-open');
            this._menuDown.className = 'fas fa-chevron-down menu-down';
            this._expanded = false;
        }
    },
    _onDocumentClick: function (args)
    {
        if (!this._controlClicked && this._expanded)
        {
            this._expanded = false;
            //close the list and reset the main element controls
            $(this._listDiv).addClass('dn').removeClass('dib');
            $(this._element).addClass('is-closed').removeClass('is-open');
            this._menuDown.className = 'fas fa-chevron-down menu-down';

            //and make it full screen on mobile
            if (this._isMobileDevice) {
                $(this._listDiv).removeClass('u-popup u-popup_full_keep-nav ov-y-auto is-open');
            }

            this._valueTextBox.value = this._oldText;
            this._valueTextBox.title = this._oldText;
            if (this._raiseChangeOnClosed)
            {
                this.raiseSelectedItemChanged();
            }
        }
        this._controlClicked = false;
    },

    /* Public methods */
    get_items: function ()
    {
        if (this._listDiv)
        {
            return this._listDiv._items;
        }
        return null;
    },
    activate: function (evt)
    {
        //Called when an associated label is clicked
        this._valueTextBox.select();
    },
    update_state: function (state)
    {
        var element = this.get_element();
        if (!state || state == 'hidden')
        {
            if (this.get_hidden())
            {
                element.style.display = 'none';
            }
            else
            {
                element.style.display = 'inline-block';
            }
        }
    },
    _iecompattest: function ()
    {
        return (document.compatMode && document.compatMode != "BackCompat") ? document.documentElement : document.body;
    },
    _clearbrowseredge: function (obj, whichedge)
    {
        var ie4 = document.all
        var ns6 = document.getElementById && !document.all

        var edgeoffset = 0
        if (whichedge == "rightedge")
        {
            var windowedge = ie4 && !window.opera ? this._iecompattest().scrollLeft + this._iecompattest().clientWidth - 15 : window.pageXOffset + window.innerWidth - 15
            this._listDiv.contentmeasure = this._listDiv.offsetWidth
            if (windowedge - this._listDiv.x < this._listDiv.contentmeasure)
                edgeoffset = this._listDiv.contentmeasure - obj.offsetWidth
        }
        else
        {
            var topedge = ie4 && !window.opera ? this._iecompattest().scrollTop : window.pageYOffset
            var windowedge = ie4 && !window.opera ? this._iecompattest().scrollTop + this._iecompattest().clientHeight - 15 : window.pageYOffset + window.innerHeight - 18
            this._listDiv.contentmeasure = this._listDiv.offsetHeight
            if (windowedge - this._listDiv.y < this._listDiv.contentmeasure)
            {
                //move up?
                edgeoffset = this._listDiv.contentmeasure + obj.offsetHeight
                if ((this._listDiv.y - topedge) < this._listDiv.contentmeasure) //up no good either?
                    edgeoffset = this._listDiv.y + obj.offsetHeight - topedge
            }
        }
        return edgeoffset
    },
    _onGetItem: function (item)
    {
        if (!this._listDiv._items) return null;

        var v = typeof (item) == 'object' ? item.get_value() : item;
        var items = this._listDiv._items;

        for (var i = 0, j = items.length; i < j; i++)
        {
            if (items[i].get_value() == v)
            {
                return items[i];
            }
        }
        return null;
    },
    _onAddItem: function (value, text, active)
    {
        //Ignore any AJAX JSON type attributes
        if (value == '_type')
        {
            return;
        }
        var item = new Inflectra.SpiraTest.Web.ServerControls.DropDownListItem();
        if (this._multiSelectable && value != '' && value > 0)
        {
            item.set_multiSelectable(true);
        }
        else
        {
            //Specify if it's to be displayed as a link
            if (this.get_displayAsHyperLinks() && value != '' && value > 0)
            {
                item.set_displayAsHyperLink(true);
            }
        }
        item.initialize();
        item.set_parent(this);
        item.set_value(value);
        if (typeof (active) == 'undefined')
        {
            item.set_active('Y');
        }
        else
        {
            item.set_active(active);
        }
        item.set_text(text);

        //Set the CSS class
        var e = item.get_element();
        e.className = this.ClassNamesEnum.itemClassName;

        if (this.get_isEmpty())
        {
            //fail quietly if there is a timing error caused by the while loop in _dataBindRow (which results in this being undefined)
            if (this._listDiv)
            {
                this._listDiv._items = new Array(item);
            }
        }
        else
        {
            this._listDiv._items[this._listDiv._items.length] = item;
        }
        item.add_click(this._onItemClickHandler);

        //Finally if this is the selected value, set as such (if not already set)
        if (this._valueHidden.value && this._valueHidden.value.indexOf(',') != -1)
        {
            //Multiselect case
            var selectedValues = this._valueHidden.value.split(',');
            for (var j = 0; j < selectedValues.length; j++)
            {
                var selectedValue = selectedValues[j];
                if (value == selectedValue)
                {
                    item.get_checkbox().checked = true;
                }
            }
        }
        else
        {
            if (value == this._valueHidden.value && this._selectedItem != item)
            {
                this._selectedItem = item;
                this._valueTextBox.value = text;
                this._valueTextBox.title = text;
                this._oldText = text;
            }
        }
    },
    _onRemoveItem: function (value)
    {
        if (this.get_isEmpty()) return;

        var item = this.getItem(value);

        if (!item) return;

        if (item == this._selectedItem)
        {
            this._selectedItem = null;
            this._valueTextBox.value = '';
            this._valueTextBox.title = '';
            this._valueHidden.value = '';
            this._oldText = '';
        }

        item.remove_click(this._onItemClickHandler);

        Array.remove(this._listDiv._items, item);
        if (this._renderedFullList && item.get_active() == 'Y')
        {
            this._listDiv.removeChild(item.get_element());
        }
        item.dispose();
    },
    _onClearItems: function ()
    {
        if (this.get_isEmpty()) return;

        var items = this._listDiv._items;
        for (var i = 1, j = items.length; i < j; i++)
        {
            items[i].get_events().removeHandler('click', this._onItemClickHandler);
            items[i].dispose();
        }

        this._listDiv.innerHTML = '';
        this._listDiv._items = new Array();
    },

    /* public properties */
    get_isEmpty: function ()
    {
        return !this._listDiv || !this._listDiv._items || this._listDiv._items.length == 0;
    },
    get_element: function ()
    {
        return this._element;
    },
    get_name: function ()
    {
        return this._valueHidden.name;
    },
    set_name: function (name)
    {
        if (this._valueHidden.name != name)
        {
            this._valueHidden.name = name;
            this.raisePropertyChanged('name');
        }
    },

    //Used to tell the dropdown to account for a scrollable object that contains the dropdown (e.g. DIV)
    get_scrollableParent: function ()
    {
        return this._scrollableParent;
    },
    set_scrollableParent: function (value)
    {
        this._scrollableParent = value;
    },

    get_multiSelectable: function ()
    {
        return this._multiSelectable;
    },
    set_multiSelectable: function (value)
    {
        this._multiSelectable = value;
    },

    get_displayMobileAsDesktop: function ()
    {
        return this._displayMobileAsDesktop;
    },
    set_displayMobileAsDesktop: function (value)
    {
        this._displayMobileAsDesktop = value;
    },

    get_displayAsHyperLinks: function ()
    {
        return this._displayAsHyperLinks;
    },
    set_displayAsHyperLinks: function (value)
    {
        this._displayAsHyperLinks = value;
    },

    get_hyperlinkBaseUrl: function ()
    {
        return this._hyperlinkBaseUrl;
    },
    set_hyperlinkBaseUrl: function (value)
    {
        this._hyperlinkBaseUrl = value;
    },

    get_enabledCssClass: function ()
    {
        return this._enabledCssClass;
    },
    set_enabledCssClass: function (value)
    {
        this._enabledCssClass = value;
    },
    get_disabledCssClass: function ()
    {
        return this._disabledCssClass;
    },
    set_disabledCssClass: function (value)
    {
        this._disabledCssClass = value;
    },
    get_enabled: function ()
    {
        return this._enabled;
    },
    set_enabled: function (value)
    {
        this._enabled = value;
        if (this._enabled)
        {
            this._valueTextBox.disabled = '';
            this.get_element().className = this._enabledCssClass;
        }
        else
        {
            this._valueTextBox.disabled = 'disabled';
            this.get_element().className = this._disabledCssClass;
        }
    },

    get_hidden: function ()
    {
        return this._hidden;
    },
    set_hidden: function (value)
    {
        this._hidden = value;
        this.update_state('hidden');
    },


    get_valueElement: function ()
    {
        return this._valueHidden;
    },
    get_valueTextBox: function ()
    {
        return this._valueTextBox;
    },
    set_valueTextBox: function (value)
    {
        if (this._valueTextBox != value)
        {
            this._valueTextBox = value;
            this.raisePropertyChanged('valueTextBox');
        }
    },
    get_listDiv: function ()
    {
        return this._listDiv;
    },
    set_listDiv: function (value)
    {
        if (this._listDiv != value)
        {
            this._listDiv = value;
            this.raisePropertyChanged('listDiv');
        }
    },
    get_listWidth: function ()
    {
        return this._listWidth;
    },
    set_listWidth: function (value)
    {
        if (this._listWidth != value)
        {
            this._listWidth = value;
            this.raisePropertyChanged('listWidth');
        }
    },
    get_dataSource: function ()
    {
        return this._dataSource;
    },
    set_dataSource: function (value)
    {
        if (this._dataSource != value)
        {
            this._dataSource = value;
            this.raisePropertyChanged('dataSource');
        }
    },
    get_expanded: function ()
    {
        return this._expanded;
    },

    get_selectedItem: function ()
    {
        return this._selectedItem;
    },
    add_selectedItem: function (value)
    {
        //Adds an item in the case of a multivalued list
        if (this._valueHidden.value == '')
        {
            //quiet set to true so that box is not closed on a multi dropdown filter on a grid
            this.set_selectedItem(value, true);
            this._raiseChangeOnClosed = true;
        }
        else
        {
            var existingValues = this._valueHidden.value.split(',');
            var matchFound = false;
            for (var i = 0; i < existingValues.length; i++)
            {
                if (existingValues[i] == value)
                {
                    matchFound = true;
                }
            }
            if (!matchFound)
            {
                this._valueHidden.value += ',' + value;
                this._valueTextBox.value = resx.DropDownList_Multiple;
                this._valueTextBox.title = resx.DropDownList_Multiple;
                this._oldText = this._valueTextBox.value;
                this._raiseChangeOnClosed = true;
            }
        }
    },
    remove_selectedItem: function (value)
    {
        //Removes an item in the case of a multivalued list
        if (this._valueHidden.value != '')
        {
            var newValues = '';
            var existingValues = this._valueHidden.value.split(',');
            var itemCount = 0;
            for (var i = 0; i < existingValues.length; i++)
            {
                if (existingValues[i] != value)
                {
                    if (newValues == '')
                    {
                        newValues = existingValues[i];
                    }
                    else
                    {
                        newValues += ',' + existingValues[i];
                    }
                    itemCount++;
                }
            }
            this._valueHidden.value = newValues;
            if (itemCount == 0)
            {
                var itm = this._onGetItem('');
                if (itm)
                {
                    this._valueTextBox.value = itm.get_text();
                    this._valueTextBox.title = itm.get_text();
                    this._oldText = this._valueTextBox.value;
                }
            }
            else if (itemCount == 1)
            {
                var itm = this._onGetItem(newValues);
                if (itm)
                {
                    this._valueTextBox.value = itm.get_text();
                    this._valueTextBox.title = itm.get_text();
                    this._oldText = this._valueTextBox.value;
                }
            }
            else
            {
                this._valueTextBox.value = resx.DropDownList_Multiple;
                this._valueTextBox.title = resx.DropDownList_Multiple;
                this._oldText = this._valueTextBox.value;
            }
            if (itemCount < existingValues.length)
            {
                this._raiseChangeOnClosed = true;
            }
        }
    },
    //Pass quiet=true if you don't want to fire an event onchange
    set_selectedItem: function (value, quiet)
    {
        if (!value)
        {
            value = '';
        }

        //First load the value, so that the correct data is stored right from the start
        if (this._valueHidden.value != value)
        {
            this._valueHidden.value = value;
        }

        //If we have multiple values, just set the text to 'multiple'
        var isMultiple = false;
        if (value.indexOf)
        {
            if (value.indexOf(',') != -1)
            {
                isMultiple = true;
                var newValues = value.split(',');
                this._valueTextBox.value = resx.DropDownList_Multiple;
                this._valueTextBox.title = resx.DropDownList_Multiple;
                this._oldText = this._valueTextBox.value;
                var items = this._listDiv._items;
                for (var i = 0; i < items.length; i++)
                {
                    if (items[i].get_checkbox())
                    {
                        var match = false;
                        for (var j = 0; j < newValues.length; j++)
                        {
                            if (items[i].get_value() == newValues[j])
                            {
                                match = true;
                            }
                        }
                        items[i].get_checkbox().checked = match;
                    }
                }
            }
        }

        //If items loaded, set the text otherwise it will get set during the databind
        var itm = this._onGetItem(value);

        //See if the selected item changed or if we switched from multi-select to single
        if (itm && (this._selectedItem != itm || (!isMultiple && this._valueTextBox.value == resx.DropDownList_Multiple)))
        {
            if (this._multiSelectable)
            {
                this.update_checkboxes(itm);
            }
            this._selectedItem = itm;
            this._valueTextBox.value = itm.get_text();
            this._valueTextBox.title = itm.get_text();
            this._oldText = this._valueTextBox.value;
            if (!quiet)
            {
                this.raisePropertyChanged('selectedItem');
                this.raiseSelectedItemChanged();
            }
        }
        //Handle the case of resetting a multi-select to None
        if (itm && itm.get_value() == '' && this._selectedItem && this._multiSelectable)
        {
            this.update_checkboxes(itm);
            this._selectedItem = itm;
            this._valueTextBox.value = itm.get_text();
            this._valueTextBox.title = itm.get_text();
        }
    },

    /* public methods */
    display_popup: function (onlyShowMatches)
    {
        if (this._expanded) {
            $(this._listDiv).addClass('dib').removeClass('dn');
            $(this._element).addClass('is-open').removeClass('is-closed');
            this._menuDown.className = 'fas fa-chevron-up menu-down';

            //specific classes for showing the full dropdown on a mobile device
            //only to show when mobile device ui is set to be different to that on desktop devices (which is the default)
            if (!onlyShowMatches && !this._displayMobileAsDesktop && this._isMobileDevice) {
                $(this._listDiv).addClass('u-popup u-popup_full_keep-nav ov-y-auto is-open');
            }
        } else {
            $(this._listDiv).addClass('dn').removeClass('dib');
            $(this._element).addClass('is-closed').removeClass('is-open');
            this._menuDown.className = 'fas fa-chevron-down menu-down';

            if (this._isMobileDevice && !this._displayMobileAsDesktop) {
                $(this._listDiv).removeClass('u-popup u-popup_full_keep-nav ov-y-auto is-open');
            }
        }

        this.raise_clicked();
    },
    render_items: function (onlyMatchingItems)
    {
        //Remove any items already in the list
        var listDiv = this._listDiv;
        while (listDiv.childNodes.length > 0)
        {
            listDiv.removeChild(listDiv.firstChild);
        }

        //We add the active items to the dom and set rendered flag
        for (var i = 0; i < listDiv._items.length; i++)
        {
            var item = listDiv._items[i];
            if (item.get_active() == 'Y')
            {
                //See if we need to match the name
                var addItem = true;
                if (onlyMatchingItems && this._valueTextBox.value != '')
                {
                    var len = this._valueTextBox.value.length;
                    var text = item.get_text();
                    if (len > text.length)
                    {
                        addItem = false;
                    }
                    else
                    {
                        if (text.toLowerCase().indexOf(this._valueTextBox.value.toLowerCase()) == -1)
                        {
                            addItem = false;
                        }
                    }
                }
                if (addItem)
                {
                    var element = item.get_element();
                    listDiv.appendChild(element);
                }
            }
        }
        //If we're rendering the full-list set the flag otherwise unset
        this._renderedFullList = !onlyMatchingItems;
    },
    update_checkboxes: function (itm)
    {
        //Reset all checkboxes and set the current one
        var items = this._listDiv._items;
        for (var i = 0; i < items.length; i++)
        {
            if (items[i].get_checkbox())
            {
                items[i].get_checkbox().checked = (itm == items[i]);
            }
        }
    },

    /* Event handlers managers */
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
        var h = this.get_events().getHandler('loaded');
        if (h) h();
    },

    add_clicked: function (handler)
    {
        this.get_events().addHandler('clicked', handler);
    },
    remove_clicked: function (handler)
    {
        this.get_events().removeHandler('clicked', handler);
    },
    raise_clicked: function ()
    {
        var h = this.get_events().getHandler('clicked');
        if (h) h();
    },

    add_selectedItemChanged: function (handler)
    {
        this.get_events().addHandler('selectedItemChanged', handler);
    },
    remove_selectedItemChanged: function (handler)
    {
        this.get_events().removeHandler('selectedItemChanged', handler);
    },
    raiseSelectedItemChanged: function ()
    {
        this._raiseChangeOnClosed = false;  //reset
        var h = this.get_events().getHandler('selectedItemChanged');
        if (h) h(this._selectedItem);
    },

    dataBind: function ()
    {
        //Load the items from the datasource, need to use timeout to avoid locking browser thread
        //First copy into an array so that we can split up the rendering into batches
        this._renderedFullList = false;
        var itemKeys = [];
        var itemValues = [];
        var i = 0;
        for (var itemKey in this._dataSource)
        {
            //Need to remove the prefix added by the serializer
            var item = itemKey.substring(1);
            itemKeys[i] = item;
            itemValues[i] = this._dataSource[itemKey];
            i++;
        }
        this._itemKeys = itemKeys;
        this._itemValues = itemValues;

        //Now actually add the items to the DOM
        this._dataBindIndex = 0;
        this._dataBindRow();
    },
    _dataBindRow: function ()
    {
        var localIndex = 0; //We add them 50 at a time then use timeout to avoid locking main thread with many dom manipulations in a row
        if (this._dataBindIndex < this._itemKeys.length)
        {
            while (localIndex < 50 && this._dataBindIndex < this._itemKeys.length)
            {
                //Split up the item into value|active
                var itemKey = this._itemKeys[this._dataBindIndex];
                if (itemKey != '_type')
                {
                    var items = itemKey.split('_');
                    var value = items[0];
                    var active;
                    if (items.length > 1)
                    {
                        if (items[1] == 'Y' || items[1] == 'N')
                        {
                            active = items[1];
                        }
                        else
                        {
                            //The field itself has an underscore (unusual case)
                            value = itemKey;
                            active = 'Y';
                        }
                    }
                    else
                    {
                        active = 'Y';
                    }
                    this._onAddItem(value, this._itemValues[this._dataBindIndex], active);
                }
                this._dataBindIndex++;
                localIndex++;

            }
            setTimeout(Function.createDelegate(this, this._dataBindRow), 0);
        }
        else
        {
            //Indicate data is loaded
            this.raise_loaded();
        }
    }
};

Inflectra.SpiraTest.Web.ServerControls.DropDownList.registerClass('Inflectra.SpiraTest.Web.ServerControls.DropDownList', Sys.UI.Control);

/* Dropdown list item definition */
Inflectra.SpiraTest.Web.ServerControls.DropDownListItem = function (element)
{
    this._element = element;
    if (!element) this._element = d.ce('div');

    Inflectra.SpiraTest.Web.ServerControls.DropDownListItem.initializeBase(this, [this._element]);

    /* Internal variables*/
    this._value;
    this._text;
    this._parent;
    this._multiSelectable = false;
    this._displayAsHyperLink = false;

    // Element references
    this._link = null;
    this._checkbox = null;
    this._textNode = null;
};

Inflectra.SpiraTest.Web.ServerControls.DropDownListItem.prototype =
{
    /* Init / Dispose */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.DropDownListItem.callBaseMethod(this, 'initialize');

        //ARIA
        this._element.setAttribute('role', 'option');
        //Add the checkbox (if necessary) and text-node
        if (this._multiSelectable)
        {
            this._checkbox = d.ce('input');
            this._checkbox.type = 'checkbox';
            this._checkbox.style.verticalAlign = 'middle';
            this._element.appendChild(this._checkbox);
        }
        if (this._displayAsHyperLink)
        {
            this._link = d.ce('a');
            this._element.appendChild(this._link);
            this._textNode = d.createTextNode('');
            this._link.appendChild(this._textNode);
        }
        else
        {
            this._textNode = d.createTextNode('');
            this._element.appendChild(this._textNode);

            //Add item click event
            this._onClickHandler = Function.createDelegate(this, this._onClick);
            $addHandlers(this._element, { 'click': this._onClickHandler }, this);
        }
    },
    dispose: function ()
    {
        $clearHandlers(this.get_element());
        if (this._onClickHandler)
        {
            delete this._onClickHandler;
        }

        delete this._link;
        delete this._checkbox;
        delete this._textNode;

        Inflectra.SpiraTest.Web.ServerControls.DropDownListItem.callBaseMethod(this, 'dispose');
    },

    /* Event Handlers */
    _onClick: function (evt)
    {
        this.raiseClick(evt);
    },

    /* Public properties */
    get_value: function ()
    {
        return this._value;
    },
    set_value: function (value)
    {
        if (this._value != value)
        {
            this._value = value;
            this.get_element()._value = value;
            if (this._displayAsHyperLink && this._link)
            {
                var url = this._parent.get_hyperlinkBaseUrl().replace('{0}', value);
                this._link.href = url;
            }
            this.raisePropertyChanged('value');
        }
    },
    get_parent: function ()
    {
        return this._parent;
    },
    set_parent: function (parent)
    {
        if (this._parent != parent)
        {
            this._parent = parent;
            this.get_element()._parent = parent;
            this.raisePropertyChanged('parent');
        }
    },
    get_active: function ()
    {
        return this._active;
    },
    set_active: function (active)
    {
        if (this._active != active)
        {
            this._active = active;
            this.raisePropertyChanged('active');
        }
    },
    get_text: function ()
    {
        return this._text;
    },
    set_text: function (value)
    {
        if (this._text != value)
        {
            this._text = value;
            this._textNode.nodeValue = value;
            this.get_element().title = value;
            this.raisePropertyChanged('text');
        }
    },

    get_displayAsHyperLink: function ()
    {
        return this._displayAsHyperLink;
    },
    set_displayAsHyperLink: function (value)
    {
        this._displayAsHyperLink = value;
    },

    get_multiSelectable: function ()
    {
        return this._multiSelectable;
    },
    set_multiSelectable: function (value)
    {
        this._multiSelectable = value;
    },
    get_checkbox: function ()
    {
        return this._checkbox;
    },

    /* Event handlers managers */
    add_click: function (handler)
    {
        this.get_events().addHandler('click', handler);
    },
    remove_click: function (handler)
    {
        this.get_events().removeHandler('click', handler);
    },
    raiseClick: function (evt)
    {
        var h = this.get_events().getHandler('click');
        if (h) h(this, evt);
    }
};

Inflectra.SpiraTest.Web.ServerControls.DropDownListItem.registerClass('Inflectra.SpiraTest.Web.ServerControls.DropDownListItem', Sys.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
