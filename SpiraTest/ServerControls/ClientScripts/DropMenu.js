var d = document;
d.ce = d.createElement;
d.ct = d.createTextNode;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

/* DropMenu */
Inflectra.SpiraTest.Web.ServerControls.DropMenu = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.DropMenu.initializeBase(this, [element]);

    this._enabled = true;
    this._authorized = true;
    this._authorizedDropMenuItems = true; // default permission to acces the dropmenu down button
    this._buttonTextSelectsItem = false;
    this._menuWidth = '165px';  //default menu width
    this._cssClass = '';
    this._menuCssClass = '';
    this._imageUrl = '';
    this._glyphIconCssClass = '';
    this._text = '';
    this._clientScriptMethod = '';
    this._clientScriptControlClientId = '';
    this._themeFolder = '';
    this._items = null;
    this._disappeardelay = 1000; //menu disappear speed onMouseout (in miliseconds)
    this._yoffset = 1; // Any padding between the element raising the event and the menu itself
    this._hidemenu_onclick = true;
    this._confirmationMessage = '';
    this._navigateUrl = '';
    this._imageAltText = '';
    this._internalClick = false;    //Don't close on internal clicks

    //inner elements
    this._dropMenuPopup = null;
    this._imageTextDiv = null;
    this._popupButton = null;
    this._dropMenuAction = null;

    //handler references
    this._onDocumentClickHandler = null;
    this._clickElements = new Array();
    this._clickHandlers = new Array();
}
Inflectra.SpiraTest.Web.ServerControls.DropMenu.prototype =
{
    /* Constructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.DropMenu.callBaseMethod(this, 'initialize');

        //Create the button
        this._dropMenuAction = d.ce('button');
        this._dropMenuAction.type = 'button';
        this._dropMenuAction.className = 'drop-menu-action';
        this._dropMenuAction.setAttribute('role', 'button');
        this._element.appendChild(this._dropMenuAction);

        //Create the icon/text div
        this._imageTextDiv = d.ce('span');
        this._imageTextDiv.className = 'drop-menu-text';
        this._dropMenuAction.appendChild(this._imageTextDiv);

        //Create the menu icon
        if (this._imageUrl && this._imageUrl != '')
        {
            var img = d.ce('img');
            img.className = "w4 h4";
            this._imageTextDiv.appendChild(img);
            img.src = this._imageUrl;
            if (this._imageAltText && this._imageAltText != '')
            {
                img.alt = this._imageAltText;
                img.title = this._imageAltText;
            }
            else
            {
                img.alt = this._text;
                img.title = this._text;
            }
        }
        if (this._glyphIconCssClass && this._glyphIconCssClass != '')
        {
            var span = d.ce('span');
            this._imageTextDiv.appendChild(span);
            span.className = this._glyphIconCssClass;
        }

        //Add the menu text
        if (this._text && this._text != '')
        {
            var span = d.ce('span');
            this._imageTextDiv.appendChild(span);
            span.appendChild(d.ct(this._text));
        }

        //Add the popup click div if items are present and permissions are sufficient
        if (this._items && this._items.length > 0 && this._authorizedDropMenuItems)
        {
            this._popupButton = d.ce('button');
            this._popupButton.type = 'button';
            this._popupButton.setAttribute('role', 'button');
            this._popupButton.className = 'drop-menu-toggle';
            this._element.appendChild(this._popupButton);
        }

        //Make sure control enabled and authorized
        if (this._enabled && this._authorized)
        {
            if (this._authorizedDropMenuItems)
            {
                //Create the popup menu
                this._dropMenuPopup = d.ce('ul');
                this._dropMenuPopup.id = this.get_element().id + '_popup';
                this._dropMenuPopup.setAttribute('role', 'menu');
                this.get_element().appendChild(this._dropMenuPopup);
                if (this._menuCssClass)
                {
                    this._dropMenuPopup.className = this._menuCssClass;
                }
                this._dropMenuPopup.style.visibility = 'hidden';
                if (this._menuWidth && this._menuWidth.Value)
                {
                    this._dropMenuPopup.style.minWidth = this._menuWidth.Value + 'px';
                }

                //Populate the menu items
                this._populateMenu(this.get_buttonTextSelectsItem());
            }

            //Add the event handlers
            //If the main icon has an action, fire that, otherwise it does same as the click on the popup button
            if (this._clientScriptMethod && this._clientScriptMethod != '')
            {
                var onImageTextClickHandler = Function.createDelegate(this, this._onImageTextClick);
                $addHandler(this._dropMenuAction, 'click', onImageTextClickHandler);
                this._clickElements.push(this._dropMenuAction);
                this._clickHandlers.push(onImageTextClickHandler);
            }
            else if (!this._navigateUrl || this._navigateUrl == '')
            {
                var onPopupButtonClickHandler = Function.createDelegate(this, this._onPopupButtonClick);
                $addHandler(this._dropMenuAction, 'click', onPopupButtonClickHandler);
                this._clickElements.push(this._dropMenuAction);
                this._clickHandlers.push(onPopupButtonClickHandler);
            }

            //Add the event handler to the popup DIV
            if (this._authorizedDropMenuItems)
            {
                if (this._popupButton)
                {
                    var onPopupButtonClickHandler = Function.createDelegate(this, this._onPopupButtonClick);
                    $addHandler(this._popupButton, 'click', onPopupButtonClickHandler);
                    this._clickElements.push(this._popupButton);
                    this._clickHandlers.push(onPopupButtonClickHandler);
                }

                //See if we want clicking on the background to close the menu
                if (this._hidemenu_onclick)
                {
                    this._onDocumentClickHandler = Function.createDelegate(this, this._onDocumentClick);
                    $addHandlers(document, { 'click': this._onDocumentClickHandler }, this);
                }
            }
        }
    },
    dispose: function ()
    {
        //Click handlers
        for (var i = 0; i < this._clickElements.length; i++)
        {
            $removeHandler(this._clickElements[i], 'click', this._clickHandlers[i]);
            delete this._clickElements[i];
            delete this._clickHandlers[i];
        }
        delete this._clickElements;
        delete this._clickHandlers;
        delete this._onDocumentClickHandler;

        if (this._dropMenuPopup)
        {
            this.get_element().removeChild(this._dropMenuPopup);
            delete this._dropMenuPopup;
        }
        if (this._popupButton)
        {
            delete this._popupButton;
        }
        delete this._imageTextDiv;
        delete this._dropMenuAction;

        Inflectra.SpiraTest.Web.ServerControls.DropMenu.callBaseMethod(this, 'dispose');
    },

    /* Properties */
    get_element: function ()
    {
        return this._element;
    },

    get_enabled: function ()
    {
        return this._enabled;
    },
    set_enabled: function (value)
    {
        this._enabled = value;
        this._element.style.opacity = (this._enabled) ? '1.0' : '0.5';
    },

    get_authorized: function ()
    {
        return this._authorized;
    },
    set_authorized: function (value)
    {
        this._authorized = value;
    },

    get_buttonTextSelectsItem: function () {
        return this._buttonTextSelectsItem;
    },
    set_buttonTextSelectsItem: function (value) {
        this._buttonTextSelectsItem = value;
    },

    get_authorizedDropMenuItems: function () {
        return this._authorizedDropMenuItems;
    },
    set_authorizedDropMenuItems: function (value) {
        this._authorizedDropMenuItems = value;
    },

    get_menuWidth: function ()
    {
        return this._menuWidth;
    },
    set_menuWidth: function (value)
    {
        this._menuWidth = value;
    },

    get_confirmationMessage: function ()
    {
        return this._confirmationMessage;
    },
    set_confirmationMessage: function (value)
    {
        this._confirmationMessage = value;
    },

    get_cssClass: function ()
    {
        return this._cssClass;
    },
    set_cssClass: function (value)
    {
        this._cssClass = value;
    },

    get_menuCssClass: function ()
    {
        return this._menuCssClass;
    },
    set_menuCssClass: function (value)
    {
        this._menuCssClass = value;
    },

    get_imageUrl: function ()
    {
        return this._imageUrl;
    },
    set_imageUrl: function (value)
    {
        this._imageUrl = value;
    },

    get_glyphIconCssClass: function ()
    {
        return this._glyphIconCssClass;
    },
    set_glyphIconCssClass: function (value)
    {
        this._glyphIconCssClass = value;
    },

    get_text: function ()
    {
        return this._text;
    },
    set_text: function (value)
    {
        this._text = value;
    },

    get_clientScriptMethod: function ()
    {
        return this._clientScriptMethod;
    },
    set_clientScriptMethod: function (value)
    {
        this._clientScriptMethod = value;
    },

    get_clientScriptControlClientId: function ()
    {
        return this._clientScriptControlClientId;
    },
    set_clientScriptControlClientId: function (value)
    {
        this._clientScriptControlClientId = value;
    },

    get_themeFolder: function ()
    {
        return this._themeFolder;
    },
    set_themeFolder: function (value)
    {
        this._themeFolder = value;
    },

    get_imageAltText: function ()
    {
        return this._imageAltText;
    },
    set_imageAltText: function (value)
    {
        this._imageAltText = value;
    },

    get_navigateUrl: function ()
    {
        return this._navigateUrl;
    },
    set_navigateUrl: function (value)
    {
        this._navigateUrl = value;
    },

    //Used to set the list of menu items
    get_items: function ()
    {
        return this._items;
    },
    set_items: function (value)
    {
        //Need to make sure we get the expected array of controls
        var e = Function._validateParams(arguments, [{ name: 'value', type: Array, elementType: Inflectra.SpiraTest.Web.ServerControls.DropMenuItem, elementMayBeNull: false}]);
        if (e) throw e;

        this._items = value;
    },

    /* Public Methods */

    showMenu: function (e)
    {
        //Let the event bubble so that other menus close, also set the flag so that the click doesn't close the menu
        this._internalClick = true;

        this._showHide(e, "visible", "hidden");
        var obj = this.get_element();
        //var position = $(obj).position();
        //this._dropMenuPopup.x = position.left;
        //this._dropMenuPopup.y = position.top + this._yoffset;
        //this._dropMenuPopup.style.left = this._dropMenuPopup.x - globalFunctions.clearbrowseredge(obj, "rightedge") + "px";
        //this._dropMenuPopup.style.top = this._dropMenuPopup.y - globalFunctions.clearbrowseredge(obj, "bottomedge") + obj.offsetHeight + "px";

        return false;
    },

    hideMenu: function (e)
    {
        if (this._dropMenuPopup)
        {
            this._dropMenuPopup.style.visibility = 'hidden';
        }
    },

    //Updates the text and icon (called after set_imageUrl / set_text)
    update_menu: function ()
    {
        var imgOrGlyph = this._imageTextDiv.childNodes[0];
        var span = this._imageTextDiv.childNodes[1];
        if (imgOrGlyph.tagName == 'IMG')
        {
            imgOrGlyph.src = this.get_imageUrl();
            if (this.get_imageAltText() && this.get_imageAltText() != '')
            {
                imgOrGlyph.alt = this.get_imageAltText();
                imgOrGlyph.title = this.get_imageAltText();
            }
            else
            {
                imgOrGlyph.alt = this.get_text();
                imgOrGlyph.title = this.get_text();
            }
        }
        else
        {
            imgOrGlyph.className = this.get_glyphIconCssClass();
        }
        globalFunctions.clearContent(span);
        span.appendChild(d.createTextNode(this.get_text()));
    },

    /* Private Methods */

    _showHide: function (e, visible, hidden)
    {
        //this._dropMenuPopup.style.left = this._dropMenuPopup.style.top = "-500px";

        if (e.type == "click" && this._dropMenuPopup.style.visibility == hidden)
        {
            this._dropMenuPopup.style.visibility = visible;
        }
        else if (e.type == "click")
        {
            this._dropMenuPopup.style.visibility = hidden;
        }
    },

    // bool: buttonTextSelectsItem is used in cases where the drop menu is a selector
    // eg source code branches, where the button text is an item in the list and that item should be shown as selected
    refreshItems: function(buttonTextSelectsItem)
    {
        //Remove the items
        globalFunctions.clearContent(this._dropMenuPopup);

        //Reload the data
        this._populateMenu(buttonTextSelectsItem);
    },

    // bool: buttonTextSelectsItem is used in cases where the drop menu is a selector
    _populateMenu: function (buttonTextSelectsItem)
    {
        var obj = this._dropMenuPopup;

        //Loop through all the items
        if (this._items)
        {
            for (var i = 0; i < this._items.length; i++)
            {
                var item = this._items[i];

                //See if we have a divider
                if (item.get_divider())
                {
                    var div = d.ce('li');
                    div.className = 'divider';
                    obj.appendChild(div);
                }
                else if (item.get_visible())
                {
                    var p = d.ce('li');
                    obj.appendChild(p);
                    //We make the icon and text the link in all cases
                    var value = item.get_value();
                    if (value == null || value == '')
                    {
                        p.appendChild(d.ct('\u00a0\u00a0'));
                        var a = d.ce('a');
                        a.setAttribute('role', 'menuitem');

                        //Set any css class on this particular item
                        if (item.get_itemCssClass() && item.get_itemCssClass() != '') {
                            a.className = item.get_itemCssClass();
                        }

                        //See if we have a server control function, navigate url or just a global JS call
                        var scriptMethod = item.get_scriptMethod();
                        if (item.get_navigateUrl() && item.get_navigateUrl() != '')
                        {
                            a.href = item.get_navigateUrl();
                        }
                        else if (scriptMethod.indexOf('javascript:') != -1)
                        {
                            a.href = scriptMethod;
                        }
                        else
                        {
                            //See if we have a confirmation check
                            if (item.get_confirmation() && item.get_confirmationMessage()) {
                                if (this._clientScriptControlClientId) {
                                    a.href = 'javascript:globalFunctions.globalConfirm(\'' + item.get_confirmationMessage() + '\', "warning", function(isConfirm) { if (isConfirm) { $find(\'' + this._clientScriptControlClientId + '\').' + scriptMethod + '}})';
                                }
                                else {
                                    a.href = 'javascript:globalFunctions.globalConfirm(\'' + item.get_confirmationMessage() + '\', "warning", function(isConfirm) { if (isConfirm) {' + scriptMethod + '}})';
                                }
                            }
                            else {
                                if (this._clientScriptControlClientId) {
                                    a.href = 'javascript: $find(\'' + this._clientScriptControlClientId + '\').' + scriptMethod;
                                }
                                else {
                                    a.href = 'javascript:' + scriptMethod;
                                }
                            }
                        }
                        p.appendChild(a);
                        if (item.get_imageUrl() && item.get_imageUrl() != '')
                        {
                            var img = d.ce('img');
                            img.className = "w4 h4";
                            img.alt = item.get_altText();
                            img.src = item.get_imageUrl();
                            a.appendChild(img);
                        }
                        if (item.get_glyphIconCssClass() && item.get_glyphIconCssClass() != '')
                        {
                            var span = d.ce('span');
                            span.className = item.get_glyphIconCssClass();
                            a.appendChild(span);
                        }
                    }
                    else
                    {
                        var a = d.ce('a');
                        a.setAttribute('role', 'menuitem');
                        //Mark the item as selected if we are matching the item text to the button text
                        if (buttonTextSelectsItem && item._name == this.get_text()) {
                            a.className = "bg-light-gray o-60";
                        }
                        //Else unset the classname completely - ahead of getting any item specific classes applied below
                        else
                        {
                            a.className = "";
                        }
                        if (item.get_itemCssClass() && item.get_itemCssClass() != '') {
                            a.className += " " + item.get_itemCssClass();
                        }

                        p.appendChild(a);

                        if (item.get_imageUrl() && item.get_imageUrl() != '')
                        {
                            var img = d.ce('img');
                            img.className = "w4 h4";
                            img.alt = item.get_altText();
                            img.src = item.get_imageUrl();
                            a.appendChild(img);
                        }
                        if (item.get_glyphIconCssClass() && item.get_glyphIconCssClass() != '')
                        {
                            var span = d.ce('span');
                            span.className = item.get_glyphIconCssClass();
                            a.appendChild(span);
                        }
                        //See if we have a server control function, navigate url or just a global JS call
                        var scriptMethod = item.get_scriptMethod();
                        if (item.get_navigateUrl() && item.get_navigateUrl() != '')
                        {
                            a.href = item.get_navigateUrl();
                        }
                        else if (scriptMethod.indexOf('javascript:') != -1)
                        {
                            a.href = scriptMethod;
                        }
                        else
                        {
                            //See if we have a confirmation check
                            if (item.get_confirmation() && item.get_confirmationMessage())
                            {
                                if (this._clientScriptControlClientId) {
                                    a.href = 'javascript:globalFunctions.globalConfirm(\'' + item.get_confirmationMessage() + '\', "warning", function(isConfirm) { if (isConfirm) { $find(\'' + this._clientScriptControlClientId + '\').' + scriptMethod + '}})';
                                }
                                else {
                                    a.href = 'javascript:globalFunctions.globalConfirm(\'' + item.get_confirmationMessage() + '\', "warning", function(isConfirm) { if (isConfirm) {' + scriptMethod + '}})';
                                }
                            }
                            else
                            {
                                if (this._clientScriptControlClientId) {
                                    a.href = 'javascript: $find(\'' + this._clientScriptControlClientId + '\').' + scriptMethod;
                                }
                                else {
                                    a.href = 'javascript:' + scriptMethod;
                                }
                            }
                        }
                        var span = d.ce('span');
                        a.appendChild(span);
                        span.appendChild(d.ct(value));
                    }
                }
            }
        }

        //Make sure the menu isn't too long for the page
        var height = obj.offsetTop + obj.offsetHeight;
        var windowedge = document.all && !window.opera ? globalFunctions.ietruebody().scrollTop + globalFunctions.ietruebody().clientHeight - 15 : window.pageYOffset + window.innerHeight - 18

        var amountOver = height - windowedge;
        if (amountOver > 0)
        {
            //Set a fixed height and make the DIV scroll
            var maxHeight = windowedge - obj.offsetTop - 15;
            obj.style.height = maxHeight + 'px';
            obj.style.overflowY = 'scroll';
        }
    },

    /* Event Handlers */

    _onDocumentClick: function (evt)
    {
        if (!this._internalClick)
        {
            this.hideMenu();
        }
        this._internalClick = false;
    },

    _onImageTextClick: function (evt)
    {
        //Make sure enabled
        if (!this.get_enabled() || !this.get_authorized())
        {
            return;
        }

        //See if the user needs to confirm the action
        if (this.get_confirmationMessage() && this.get_confirmationMessage() != '')
        {
            globalFunctions.globalConfirm(
                this.get_confirmationMessage(),
                "warning",
                this._onImageTextClickConfirmed,
                { thisRef: this, evt: evt },
                null
            )
        }
        else 
        {
            //Call the client event handler
            this._onImageTextClickConfirmed(true, { thisRef: this, evt: evt });
        }
    },
    //this gets passed as thisRef to allow external functions to call this function correctly
    _onImageTextClickConfirmed: function (isConfirmed, refs) 
    {
        // evt is passed in from the function above via the globalConfirm - separate it out here, so that clientScriptMethod can make use of it - which it does via the aspx string - not an object so the strings must exactly match
        var evt = refs.evt;
        var thisRef = refs.thisRef;
        if (isConfirmed)
        {
            if (thisRef._clientScriptControlClientId && thisRef._clientScriptControlClientId != '')
            {
                eval('$find(\'' + thisRef._clientScriptControlClientId + '\').' + thisRef._clientScriptMethod);
            }
            else
            {
                eval(thisRef._clientScriptMethod);
            }
        }
    },

    _onPopupButtonClick: function (evt)
    {
        //Make sure enabled
        if (!this.get_enabled() || !this.get_authorized())
        {
            return;
        }

        //Display the popup menu if we have items in the menu
        if (this.get_items() && this.get_items().length > 0)
        {
            this.showMenu(evt);
        }
    },

    _onImageMouseOverOut: function (evt, args)
    {
        args.img.src = args.url;
    }
};
Inflectra.SpiraTest.Web.ServerControls.DropMenu.registerClass('Inflectra.SpiraTest.Web.ServerControls.DropMenu', Sys.UI.Control);

/* DropMenuItem */
Inflectra.SpiraTest.Web.ServerControls.DropMenuItem = function (name, value, imageUrl, glyphIconCssClass, itemCssClass, scriptMethod, altText, visible, navigateUrl, divider, confirmation, confirmationMessage)
{
    Inflectra.SpiraTest.Web.ServerControls.DropMenuItem.initializeBase(this);

    this._name = name;
    this._value = value;
    this._imageUrl = imageUrl;
    this._glyphIconCssClass = glyphIconCssClass;
    this._itemCssClass = itemCssClass;
    this._scriptMethod = scriptMethod;
    this._altText = altText;
    this._visible = visible;
    this._element = null;
    this._navigateUrl = navigateUrl;
    this._divider = divider;
    this._confirmation = confirmation;
    this._confirmationMessage = confirmationMessage;
}
Inflectra.SpiraTest.Web.ServerControls.DropMenuItem.prototype =
{
    /* Constructors */
    initialize: function()
    {
        Inflectra.SpiraTest.Web.ServerControls.DropMenuItem.callBaseMethod(this, 'initialize');
    },
    dispose: function()
    {        
        Inflectra.SpiraTest.Web.ServerControls.DropMenuItem.callBaseMethod(this, 'dispose');
    },
    
    /* Properties */   
    get_element : function()
    {
        return this._element;
    },
    set_element : function(value)
    {
        this._element = value;
    },

    get_name : function()
    {
        return this._name;
    },
    set_name : function(value)
    {
        this._name = value;
    },

    get_value : function()
    {
        return this._value;
    },
    set_value : function(value)
    {
        this._value = value;
    },

    get_navigateUrl: function ()
    {
        return this._navigateUrl;
    },
    set_navigateUrl: function (value)
    {
        this._navigateUrl = value;
    },

    get_imageUrl : function()
    {
        return this._imageUrl;
    },
    set_imageUrl : function(value)
    {
        this._imageUrl = value;
    },

    get_glyphIconCssClass: function ()
    {
        return this._glyphIconCssClass;
    },
    set_glyphIconCssClass: function (value)
    {
        this._glyphIconCssClass = value;
    },

    get_itemCssClass: function ()
    {
        return this._itemCssClass;
    },
    set_itemCssClass: function (value)
    {
        this._itemCssClass = value;
    },

    get_scriptMethod : function()
    {
        return this._scriptMethod;
    },
    set_scriptMethod : function(value)
    {
        this._scriptMethod = value;
    },

    get_altText: function ()
    {
        return this._altText;
    },
    set_altText: function (value)
    {
        this._altText = value;
    },

    get_visible : function()
    {
        return this._visible;
    },
    set_visible : function(value)
    {
        this._visible = value;
    },

    get_divider: function ()
    {
        return this._divider;
    },
    set_divider: function (value)
    {
        this._divider = value;
    },

    get_confirmation: function ()
    {
        return this._confirmation;
    },
    set_confirmation: function (value)
    {
        this._confirmation = value;
    },

    get_confirmationMessage: function ()
    {
        return this._confirmationMessage;
    },
    set_confirmationMessage: function (value)
    {
        this._confirmationMessage = value;
    }
};
Inflectra.SpiraTest.Web.ServerControls.DropMenuItem.registerClass('Inflectra.SpiraTest.Web.ServerControls.DropMenuItem', Sys.Component);

//  always end with this goodnight statement        
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
