var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.ContextMenu = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.ContextMenu.initializeBase(this, [element]);

    this._width = 0;
    this._height = 0;
    this._left = 0;
    this._top = 0;
    this._items = null;
    this._checkboxClicked = false;
    this._parentControlId = '';
    this._clickElements = new Array();
    this._clickHandlers = new Array();
    this._themeFolder = '';
    this._docClickHandler = null;
    this._primaryKey = null;
}

Inflectra.SpiraTest.Web.ServerControls.ContextMenu.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.ContextMenu.callBaseMethod(this, 'initialize');

        //Get the pixel width of the dialog box
        this._width = this.get_element().style.width.substr(0, this.get_element().style.width.length - 2);

        //Create the menu if we have items
        if (this._items)
        {
            this.createMenu();
        }

        //Add a document click handler to close the dialog if clicked elsewhere
        this._docClickHandler = Function.createDelegate(this, this._onDocumentClick);
        $addHandler(document, 'click', this._docClickHandler);
    },
    dispose: function ()
    {
        //Add custom dispose actions here
        for (var i = 0; i < this._clickElements.length; i++)
        {
            $removeHandler(this._clickElements[i], 'click', this._clickHandlers[i]);
            delete this._clickElements[i];
            delete this._clickHandlers[i];
        }
        delete this._clickElements;
        delete this._clickHandlers;
        
        $removeHandler(document, 'click', this._docClickHandler);
        delete this._docClickHandler;

        $removeHandler(this.get_element(), 'mouseout', this._mouseOutHandler);
        Inflectra.SpiraTest.Web.ServerControls.ContextMenu.callBaseMethod(this, 'dispose');
    },

    /*  =========================================================
    The properties
    =========================================================  */
    get_items: function ()
    {
        return this._items;
    },
    set_items: function (value)
    {
        this._items = value;
    },

    get_themeFolder: function ()
    {
        return this._themeFolder;
    },
    set_themeFolder: function (value)
    {
        this._themeFolder = value;
    },

    get_parentControlId: function ()
    {
        return this._parentControlId;
    },
    set_parentControlId: function (value)
    {
        this._parentControlId = value;
    },

    get_primaryKey: function ()
    {
        return this._primaryKey;
    },
    set_primaryKey: function (value)
    {
        this._primaryKey = value;
    },

    /*  =========================================================
    The methods
    =========================================================  */
    display: function (evt, primaryKey, offsetX, offsetY)
    {
        //Make sure we have some items
        if (!this._items || this._items.length == 0)
        {
            return;
        }

        //Set the primary key
        this._primaryKey = primaryKey;

        //Auto-positions next to the cursor - pass in the event
        var posx = 0;
        var posy = 0;
        if (evt.clientX || evt.clientY)
        {
            posx = evt.clientX + globalFunctions.getScrollLeft() - 5;
            posy = evt.clientY + globalFunctions.getScrollTop() - 10;

            //Update the height based on the current offsetHeight
            if (this._element.offsetHeight)
            {
                this._height = this._element.offsetHeight;
            }

            //Find out how close the mouse is to the bottom of the window
            var windowHeight;
            if (d.all)
            {
                windowHeight = (d.documentElement.clientHeight == 0) ? d.body.clientHeight : d.documentElement.clientHeight;
            }
            else
            {
                windowHeight = window.innerHeight;
            }
            windowHeight += globalFunctions.getScrollTop();
            if (posy + parseInt(this._height) > windowHeight)
            {
                posy = windowHeight - parseInt(this._height);
            }

            //Find out how close the mouse is to the right of the window
            var windowWidth;
            if (document.all)
            {
                windowWidth = (d.documentElement.clientWidth == 0) ? d.body.clientWidth : d.documentElement.clientWidth;
            }
            else
            {
                windowWidth = window.innerWidth;
            }
            windowWidth += d.body.scrollLeft + d.documentElement.scrollLeft;
            if (posx + parseInt(this._width) > windowWidth)
            {
                posx = windowWidth - parseInt(this._width) - 80;
            }
        }
        else if (evt.pageX || evt.pageY)
        {
            posx = evt.pageX - 5;
            posy = evt.pageY - 10;
        }
        this._left = posx;
        this._top = posy;

        //Add on any offsets
        if (offsetX)
        {
            this._left += offsetX;
        }
        if (offsetY)
        {
            this._top += offsetY;
        }

        //See if any checkboxes were selected (for controls that support this)
        var parentControl = $find(this._parentControlId);
        if (parentControl && parentControl.get_selected_items && parentControl.set_selected_item)
        {
            //If none selected, need to add the current item
            if (parentControl.get_selected_items().length == 0)
            {
                parentControl.set_selected_item(primaryKey);
                this._checkboxClicked = true;
            }
        }

        //Make visible and position
        this.get_element().style.left = this._left + 'px';
        this.get_element().style.top = this._top + 'px';
        this.get_element().style.display = 'block';
    },

    add_divider: function ()
    {
        if (!this._items)
        {
            this._items = new Array();
        }
        var item = {};
        item.divider = true;
        this._items.push(item);
    },

    add_item: function (caption, imageUrl, confirmationMessage, clientScriptMethod, commandName, commandArgument, glyphIconCssClass)
    {
        if (!this._items)
        {
            this._items = new Array();
        }
        var item = {};
        item.divider = false;
        item.caption = caption;
        item.imageUrl = imageUrl;
        item.glyphIconCssClass = glyphIconCssClass;
        item.confirmationMessage = confirmationMessage;
        item.clientScriptMethod = clientScriptMethod;
        item.commandName = commandName;
        item.commandArgument = commandArgument;
        this._items.push(item);
    },

    createMenu: function ()
    {
        //Create a table
        this.get_element().style.diplay = 'none';
        var table = d.ce('table');
        this.get_element().appendChild(table);
        var tbody = d.ce('tbody');
        table.appendChild(tbody);

        //Create each menu item as a table row
        for (var i = 0; i < this._items.length; i++)
        {
            var item = this._items[i];
            var tr = d.ce('tr');
            tbody.appendChild(tr);
            var td = d.ce('td');
            tr.appendChild(td);
            //See if this is a divider entry
            if (item.divider)
            {
                tr.className = 'Divider';
                td.appendChild(d.createTextNode(' '));
            }
            else
            {
                var a = d.ce('a');
                td.appendChild(a);
                a.href = 'javascript:void(0)';

                //See if we have an image/glyph
                if (item.imageUrl && item.imageUrl != '')
                {
                    var img = d.ce('img');
                    img.className = "w4 h4";
                    a.appendChild(img);
                    img.src = this._themeFolder + item.imageUrl;
                }
                else if (item.glyphIconCssClass && item.glyphIconCssClass != '')
                {
                    var span = d.ce('span');
                    span.className = item.glyphIconCssClass;
                    a.appendChild(span);
                }
                else
                {
                    var span = d.ce('span');
                    span.className = 'Spacer';
                    a.appendChild(span);
                }

                a.appendChild(d.createTextNode(item.caption));

                //Add the click handler
                var clickHandler = Function.createCallback(this._onItemClicked, { thisRef: this, commandName: item.commandName, commandArgument: item.commandArgument, clientScriptMethod: item.clientScriptMethod, confirmationMessage: item.confirmationMessage });
                $addHandler(a, 'click', clickHandler);
                this._clickElements.push(a);
                this._clickHandlers.push(clickHandler);
            }
        }

        //Calculate an appoximate height based on number of elements, we'll update it later
        this._height = this._items.length * 25;
    },

    /*  =========================================================
    Event handlers
    =========================================================  */
    _onDocumentClick: function(evt)
    {
        this.get_element().style.display = 'none';

        //Unselect the checkbox if we selected it
        var parentControl = $find(this._parentControlId);
        if (this._checkboxClicked && parentControl && parentControl.get_selected_items && parentControl.set_selected_item)
        {
            //If exactly one still selected, delect it
            var selectedItems = parentControl.get_selected_items();
            if (selectedItems.length == 1)
            {
                parentControl.set_selected_item(selectedItems[0]);
                this._checkboxClicked = false;
            }
        }
    },
    _onItemClicked: function (evt, args)
    {
        var thisRef = args.thisRef;

        //See if we need to display a confirmation
        if (args.confirmationMessage && args.confirmationMessage != '')
        {
            if (!confirm(args.confirmationMessage))
            {
                return;
            }
        }

        //See if we have a control command or client script to execute
        if (args.clientScriptMethod && args.clientScriptMethod != '')
        {
            var command = args.clientScriptMethod + '(evt)';
            eval(command);
        }
        else
        {
            //Get a handle on the parent control
            var parentControl = $find(thisRef._parentControlId);
            if (parentControl)
            {
                //call the appropriate function
                if (args.commandArgument)
                {
                    parentControl[args.commandName](args.commandArgument, thisRef._primaryKey);
                }
                else
                {
                    parentControl[args.commandName](thisRef._primaryKey);
                }
            }
        }
    }
}
Inflectra.SpiraTest.Web.ServerControls.ContextMenu.registerClass('Inflectra.SpiraTest.Web.ServerControls.ContextMenu', Sys.UI.Control);
        
//  always end with this goodnight statement        
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();