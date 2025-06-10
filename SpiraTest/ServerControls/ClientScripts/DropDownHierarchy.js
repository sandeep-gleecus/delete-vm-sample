var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy = function (element)
{
	this._element = element;

	if (!element)
	{
		this._element = d.ce('div');
	}
    //ARIA
    this._element.setAttribute('role', 'combobox');
    this._element.setAttribute('aria-owns', this._element.id + '_list,' + this._element.id + '_text');

	Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy.initializeBase(this, [this._element]);

	/* Enumerators */
	this.ClassNamesEnum =
    {
    	valuePanelClassName: 'value',
    	itemsPanelClassName: 'list dropdown-popup DropDownHierarchy-menu',
    	itemClassName: 'item'
    };

	/* Init internal items */
    this._valueTextBox = d.ce('input');
    this._valueTextBox.id = this._element.id + '_text';
	this._valueTextBox.type = 'text';
	this._valueTextBox.length = 255;    //Max length
	this._valueTextBox.autocomplete = 'off';
	//ARIA
	this._valueTextBox.setAttribute('role', 'textbox');
	this._valueTextBox.setAttribute('aria-autocomplete', 'list');
	this._valueTextBox.setAttribute('aria-owns', this._element.id + '_list');
	this._valueTextDiv = d.ce('div');
	this._valueHidden = d.ce('input');
	this._valueHidden.id = this._element.id + '_Value';
	this._valueHidden.type = 'hidden';
	this._listDiv = d.ce('div');
	//ARIA
	this._listDiv.setAttribute('role', 'listbox');
	this._textNode = d.createTextNode('');
	this._navigateLink = d.ce('a');
	this._navigateLaunch = d.ce('div');

    /* Define the button classes and their structure */
	this._menuDown = d.ce('div');
	this._menuDown.id = this._element.id + '_menuDown';
	this._menuDown.className = 'fas fa-chevron-down menu-down';

	spanLaunch = d.ce('span');
	spanLaunch.className = "btn-launch_content glyphicon fas fa-arrow-right";
	this._navigateLaunch.className = "btn-launch";
	this._navigateLink.appendChild(spanLaunch);
	this._navigateLaunch.appendChild(this._navigateLink);

	/* Internal variables*/
	this._controlClicked = false;
	this._itemClicked = false;
	this._expanded = false;
	this._selectedItem = null;
	this._itemImage = '';
	this._summaryItemImage = '';
	this._alternateItemImage = '';
	this._baseUrl = '';
	this._itemKeys = null;
	this._itemValues = null;
	this._dataBindIndex = 0;
	this._renderedFullList = false;
	this._oldText = '';
	this._themeFolder = '';
	this._navigateLaunchDisplayed = false;
	this._enabled = true;
	this._enabledCssClass = 'DropDownHierarchy DropDown';
	this._disabledCssClass = 'DropDownHierarchy DropDown Disabled';
	this._hidden = false;
	this.navigateToText = '';
	this._isMobileDevice = false; //this is not linked to the server control

	//Datasource
	this._dataSource = null;
};

Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy.prototype =
{
	/* Init / Dispose */
	initialize: function ()
	{
		Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy.callBaseMethod(this, 'initialize');

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

		//Create a navigate menu item if we have a URL
		if (this._baseUrl != undefined && this._baseUrl != '' && this._element.parentNode && this._themeFolder != '')
		{
		    // add attribute to the parent div to differentiate this from menus without a url
		    this._element.setAttribute('role', 'combobox launcher');

			this._navigateLaunchDisplayed = true;
			this._navigateLink.setAttribute('title', this.navigateToText);
			this._navigateLink.setAttribute('target', '_blank');
			this._valueTextDiv.appendChild(this._navigateLaunch);

			this.updateNavigateLaunch();
		}

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
		if (this._isMobileDevice) {
		    $(this._listDiv).removeClass('u-popup u-popup_full_keep-nav ov-y-auto is-open');
		}
		
		if (this._listWidth)
		{
			this._listDiv.style.width = this._listWidth;
		}
		else
		{
			this._listDiv.style.width = this.get_element().style.width;
		}

		//If we have a datasource then databind
		if (this._dataSource)
		{
			this.dataBind();
		}
	},
	figureSize: function ()
	{
		var width = this._element.style.width;
		if (width.substr(width.length - 1, 1) == "%")
		{
			if (this._element.offsetWidth > 24)
				width = String(this._element.offsetWidth - 24) + "px";
			else
				width = "0px";
		}
		else if (width.substr(width.length - 2, 2) == "px")
		{
			var styleWidth = parseInt(width.substr(0, width.length - 2));
			width = String(styleWidth - 24) + "px";
		}
		if (width != null) this._valueTextBox.style.width = width;
	},
	dispose: function ()
	{
		$clearHandlers(this.get_element());
		$clearHandlers(this._valueTextBox);
		$clearHandlers(this._listDiv);
		$clearHandlers(this._navigateLaunch);
		$clearHandlers(this._navigateLink);

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
		delete this._navigateLaunch;
		delete this._navigateLink;
        delete this._menuDown;

		Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy.callBaseMethod(this, 'dispose');
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
			this.display_popup(true);
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
		//Call the method to display the dropdown (if not already displayed and not on the input (or on the launcher button to navigate to currently selected item)
		if ($(targ).hasClass("btn-launch") || $(targ).hasClass("btn-launch_content"))
		{
		    //do nothing
		}
		else
		{
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
		}
	},
	_onItemClick: function (itm)
	{
		this._itemClicked = true;
		this.set_selectedItem(itm.get_value());

	},
	_onDocumentClick: function (args)
	{
		if (!this._controlClicked && this._expanded)
		{
		    this._expanded = false;
		    //open the list and reset the main element controls 
		    $(this._listDiv).addClass('dn').removeClass('dib');
		    $(this._element).addClass('is-closed').removeClass('is-open');
		    this._menuDown.className = 'fas fa-chevron-down menu-down';

		    //and make it full screen on mobile
			if (this._isMobileDevice) {
			    $(this._listDiv).removeClass('u-popup u-popup_full_keep-nav ov-y-auto is-open');
			}
		}
		this._controlClicked = false;
		this._itemClicked = false;
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
	_onAddItem: function (value, indent, summary, alternate, active, text)
	{
		var item = new Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchyItem();
		item.initialize();

		//Set properties
		item.set_parent(this);
		item.set_value(value);
		item.set_indent(indent);
		item.set_summary(summary);
		item.set_alternate(alternate);
		item.set_active(active);
		item.set_text(text);

		//Add the item
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
		//Add the event handler
		item.get_events().addHandler('click', this._onItemClickHandler);

		//Finally if this is the selected value, set as such (if not already set)
		if (value == this._valueHidden.value && this._selectedItem != item)
		{
			this._selectedItem = item;
			this._valueTextBox.value = text;
			this._valueTextBox.title = text;
			this._oldText = text;
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

		item.get_events().removeHandler('click', this._onItemClickHandler);

		Array.remove(this._listDiv._items, item);
		if (this._rendered && item.get_active() == 'Y')
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

	/* Private methods */
	_getposOffset: function (what, offsettype)
	{
		var totaloffset = (offsettype == "left") ? what.offsetLeft : what.offsetTop;
		var parentEl = what.offsetParent;
		while (parentEl != null)
		{
			//if we have an absolutely positioned element then stop there
			if (parentEl.style.position == 'absolute')
			{
				parentEl = null;
			}
			else
			{
				totaloffset = (offsettype == "left") ? totaloffset + parentEl.offsetLeft : totaloffset + parentEl.offsetTop;
				parentEl = parentEl.offsetParent;
			}
		}
		return totaloffset;
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

	/* public methods */
	update_state: function (state)
	{
		var element = this.get_element();
		if (!state || state == 'hidden')
		{
			if (this.get_hidden())
			{
				element.style.display = 'none';
				if (this._navigateLink)
				{
					this._navigateLink.style.display = 'none';
				}
			}
			else
			{
				element.style.display = 'inline-block';
				if (this._navigateLink)
				{
					this._navigateLink.style.display = 'inline';
				}
			}
		}
	},
	get_items: function ()
	{
		return this._listDiv._items;
	},
	updateNavigateLaunch: function ()
	{
		if (this._navigateLaunchDisplayed)
		{
			if (this._valueHidden.value == '')
			{
				this._navigateLink.href = 'javascript:void(0)';
			    $(this._navigateLaunch).addClass('disabled');
				$clearHandlers(this._navigateLaunch);
			}
			else
			{
			    var url = this._baseUrl.replace(globalFunctions.artifactIdToken, '' + this._valueHidden.value);
				this._navigateLink.href = url;
			    $(this._navigateLaunch).removeClass('disabled');
			}
		}
	},
	display_popup: function (onlyShowMatches)
	{
	    if (this._expanded) {
	        $(this._listDiv).addClass('dib').removeClass('dn');
	        $(this._element).addClass('is-open').removeClass('is-closed');
	        this._menuDown.className = 'fas fa-chevron-up menu-down';

            //specific classes for showing the full dropdown on a mobile device
	        if (!onlyShowMatches && this._isMobileDevice) {
	            $(this._listDiv).addClass('u-popup u-popup_full_keep-nav ov-y-auto is-open');
	        }
	    } else {
	        $(this._listDiv).addClass('dn').removeClass('dib');
	        $(this._element).addClass('is-closed').removeClass('is-open');
	        this._menuDown.className = 'fas fa-chevron-down menu-down';

	        if (this._isMobileDevice) {
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
		for (var i = 0; i < this._listDiv._items.length; i++)
		{
			var item = this._listDiv._items[i];
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
					element.className = this.ClassNamesEnum.itemClassName;
					this._listDiv.appendChild(element);
				}
			}
		}
		//If we're rendering the full-list set the flag otherwise unset
		this._renderedFullList = !onlyMatchingItems;
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
	get_themeFolder: function ()
	{
		return this._themeFolder;
	},
	set_themeFolder: function (value)
	{
		this._themeFolder = value;
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

	get_itemImage: function ()
	{
		if (arguments.length !== 0) throw Error.parameterCount();

		return this._itemImage;
	},
	set_itemImage: function (value)
	{
		if (this._itemImage != value)
		{
			this._itemImage = value;
			this.raisePropertyChanged('itemImage');
		}
	},

	get_summaryItemImage: function ()
	{
		if (arguments.length !== 0) throw Error.parameterCount();

		return this._summaryItemImage;
	},
	set_summaryItemImage: function (value)
	{
		if (this._summaryItemImage != value)
		{
			this._summaryItemImage = value;
			this.raisePropertyChanged('summaryItemImage');
		}
	},

	get_alternateItemImage: function ()
	{
		if (arguments.length !== 0) throw Error.parameterCount();

		return this._alternateItemImage;
	},
	set_alternateItemImage: function (value)
	{
		if (this._alternateItemImage != value)
		{
			this._alternateItemImage = value;
			this.raisePropertyChanged('alternateItemImage');
		}
	},

	get_baseUrl: function ()
	{
		if (arguments.length !== 0) throw Error.parameterCount();

		return this._baseUrl;
	},
	set_baseUrl: function (value)
	{
		if (this._baseUrl != value)
		{
			this._baseUrl = value;
			this.raisePropertyChanged('baseUrl');
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
	get_navigateToText: function () {
		if (this.navigateToText == '') {
			return resx.DropDownHierarchy_NavigateToArtifact;
		}

		return this.navigateToText;
	},
	set_navigateToText: function (value) {

		if (value == '') {
			this.navigateToText = resx.DropDownHierarchy_NavigateToArtifact
		}
		else {
			this.navigateToText = value;
		}
		this.raisePropertyChanged('navigateToText');
	},
	get_expanded: function ()
	{
		return this._expanded;
	},

	get_selectedItem: function ()
	{
		return this._selectedItem;
	},
	set_selectedItem: function (value)
	{
		//First load the value, so that the correct data is stored right from the start
		if (this._valueHidden.value != value)
		{
			this._valueHidden.value = value;
		}

		//If items loaded, set the text otherwise it will get set during the databind
		var itm = this._onGetItem(value);

		if (itm && this._selectedItem != itm)
		{
			this._selectedItem = itm;
			this._valueTextBox.value = itm.get_text();
			this._valueTextBox.title = itm.get_text();
			this._oldText = this._valueTextBox.value;
			this.raisePropertyChanged('selectedItem');
			this.updateNavigateLaunch();
			this.raiseSelectedItemChanged();
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
		var h = this.get_events().getHandler('selectedItemChanged');
		if (h) h(this._selectedItem);
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

	/* Public Methods */
	activate: function (evt)
	{
		//Called when an associated label is clicked
		this._valueTextBox.select();
	},
	dataBind: function ()
	{
		//Load the items from the datasource, need to use timeout to avoid locking browser thread
		//First copy into an array so that we can split up the rendering into batches
		this._isLoaded = false;
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
				//Split up the item into value|indent
				var itemKey = this._itemKeys[this._dataBindIndex];
				if (itemKey != '_type')
				{
					var items = itemKey.split('_');
					var value = items[0];
					var indent = items[1];
					var summary = items[2];
					var alternate = items[3];
					var active = items[4];
					this._onAddItem(value, indent, summary, alternate, active, this._itemValues[this._dataBindIndex]);
				}
				this._dataBindIndex++;
				localIndex++;
			}
			setTimeout(Function.createDelegate(this, this._dataBindRow), 0);
		}
		else
		{
			//Indicate data is loaded
			this._isLoaded = true;
			this.raise_loaded();
		}
	}
};

Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy.registerClass('Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy', Sys.UI.Control);

/* Dropdown list item definition */
Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchyItem = function (element)
{
	this._element = element;

	if (!element) this._element = d.ce('div');

	Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchyItem.initializeBase(this, [this._element]);

	/* Internal variables*/
	this._value;
	this._text;
	this._indent;
	this._summary;
	this._alternate;
	this._active;
	this._parent;

	// Element references
	this._img = null;
	this._textNode = null;
};

Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchyItem.prototype =
{
	/* Init / Dispose */
	initialize: function ()
	{
		Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchyItem.callBaseMethod(this, 'initialize');

		//ARIA
		this._element.setAttribute('role', 'option');

		/* Event delegates */
		this._onClickHandler = Function.createDelegate(this, this._onClick);

		// add the image and text-node
		this._img = d.ce('img');
		this._img.className = "w4 h4";
		this._element.appendChild(this._img);
		this._textNode = d.createTextNode('');
		this._element.appendChild(this._textNode);

		$addHandlers(this._element, { 'click': this._onClickHandler }, this);
	},
	dispose: function ()
	{
		$clearHandlers(this.get_element());
		if (this._onClickHandler)
		{
			delete this._onClickHandler;
		}
		Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchyItem.callBaseMethod(this, 'dispose');
	},

	/* Event Handlers */
	_onClick: function ()
	{
		this.raiseClick();
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
	get_indent: function ()
	{
		return this._indent;
	},
	set_indent: function (indent)
	{
		if (this._indent != indent)
		{
			this._indent = indent;
			this.get_element().style.paddingLeft = ((indent * 10) + 0) + "px";
			if (indent == 0)
			{
				this._img.style.display = 'none';
			}
			this.raisePropertyChanged('indent');
		}
	},
	get_summary: function ()
	{
		return this._summary;
	},
	set_summary: function (summary)
	{
		if (this._summary != summary)
		{
			this._summary = summary;
			if (summary == 'Y')
			{
				this.get_element().style.fontWeight = 'bold';
				this._img.src = this._parent.get_themeFolder() + this._parent.get_summaryItemImage();
			}
			else
			{
				this._img.src = this._parent.get_themeFolder() + this._parent.get_itemImage();
			}
			this.raisePropertyChanged('summary');
		}
	},
	get_alternate: function ()
	{
		return this._alternate;
	},
	set_alternate: function (alternate)
	{
		if (this._alternate != alternate)
		{
			this._alternate = alternate;
			if (alternate == 'Y')
			{
				this._img.src = this._parent.get_themeFolder() + this._parent.get_alternateItemImage();
			}
			this.raisePropertyChanged('alternate');
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

	/* Event handlers managers */
	add_click: function (handler)
	{
		this.get_events().addHandler('click', handler);
	},
	remove_click: function (handler)
	{
		this.get_events().removeHandler('click', handler);
	},
	raiseClick: function ()
	{
		var h = this.get_events().getHandler('click');
		if (h) h(this);
	}
};

Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchyItem.registerClass('Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchyItem', Sys.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
