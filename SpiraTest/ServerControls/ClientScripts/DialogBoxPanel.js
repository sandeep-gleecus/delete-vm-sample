Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');
var d = document;
d.ce = d.createElement;

Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel.initializeBase(this, [element]);

    this._element = element;
    this._persistent = false;
    this._modal = false;
    this._themeFolder = '';
    this._errorMessageControlId = '';
    this._mouseOutHandler = null;
    this._closeButtonHandler = null;
    this._innerDiv = null;
    this._closeButton = null;
    this._background = null;
    this._ajaxClientId = '';
    this._width = 0;
    this._height = 0;
    this._left = -1;
    this._top = -1;
    this._displayTimeoutId = -1;
    this._closeTimeoutId = -1;
    this._title = '';
    this._titleBar = null;
    this._visual = null;

    //ARIA
    this._element.setAttribute('role', 'dialog');
	this._element.setAttribute('aria-labelledby', this._element.id + '_title');
	// we add visibility and display none classes AND the inline style DN here (as well as in the c# at AddAttributesToRender as a belt and braces approach - each is required for correct functioning throughout the app)
	this._element.classList.add('visibility-none', 'dn');
	this._element.style.display = 'none';
}

Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel.callBaseMethod(this, 'initialize');

        //Create the inner objects

        //The close button
        this._closeButton = d.ce('button');
        this._closeButton.setAttribute('role', 'button');
        this._closeButton.setAttribute('aria-label', 'Close');
        this._closeButton.type = 'button';
        this._closeButton.className = 'DialogClose close';

        span = d.ce('span');
        span.setAttribute('aria-hidden', 'true');
        span.className = 'fas fa-times';

        this._closeButton.appendChild(span);
        this.get_element().appendChild(this._closeButton);

        //The title bar
        this._titleBar = d.ce('h1');
        this._titleBar.id = this._element.id + '_title';
        if (this._element.firstChild)
        {
            this._element.insertBefore(this._titleBar, this._element.firstChild);
        }
        else
        {
            this._element.appendChild(this._titleBar);
        }

        //If this is not modal, add a drag handler to the title bar
        if (!this.get_modal())
        {
            this._titleBar.style.cursor = 'move';
            this._titleMouseDownHandler = Function.createDelegate(this, this._onTitleMouseDown);
            $addHandler(this._titleBar, 'mousedown', this._titleMouseDownHandler);
        }

        //The inner content div
        this._innerDiv = d.ce('div');
        this._innerDiv.style.width = '100%';
        this._innerDiv.style.overflow = 'hidden';
        this.get_element().appendChild(this._innerDiv);

        //Add the event handlers to the element when the mouse leaves and enters
        this._mouseOutHandler = Function.createCallback(this._onMouseOut, { thisRef: this });
        this._mouseOverHandler = Function.createCallback(this._onMouseOver, { thisRef: this });
        $addHandler(this.get_element(), 'mouseout', this._mouseOutHandler);
        $addHandler(this.get_element(), 'mouseover', this._mouseOverHandler);

        //Add the event handler to the close button
        this._closeButtonHandler = Function.createDelegate(this, this._onCloseClick);
        $addHandler(this._closeButton, 'click', this._closeButtonHandler);

        //Create a background element if we have the modal flag set - we put the popup itself inside the background to help with scrolling issues
        if (this.get_modal())
        {
            this._background = d.ce('div');
			this.get_element().parentNode.appendChild(this._background);
			this._background.className = 'DialogBoxModalBackground fixed visibility-none top0 left0 vw-100 vh-100 ov-auto';
			this.get_element().addEventListener('click', function (event) { event.stopPropagation(); });
			this.get_element().removeAttribute("style");
			this.get_element().classList.add("dn");
			this._background.appendChild(this.get_element());
			this._background.addEventListener('click', this._closeButtonHandler);
        }


    },
    dispose: function ()
    {
        //Add custom dispose actions here
        $removeHandler(this.get_element(), 'mouseout', this._mouseOutHandler);
        $removeHandler(this._closeButton, 'click', this._closeButtonHandler);

        if (this.get_modal())
        {
            delete this._background;
        }
        else
        {
            $removeHandler(this._titleBar, 'mousedown', this._titleMouseDownHandler);
            delete this._titleMouseDownHandler;
        }

        //Delete other elements
        delete this._titleBar;
        delete this._closeButton;

        Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel.callBaseMethod(this, 'dispose');
    },

    /*  =========================================================
    The properties
    =========================================================  */
    get_persistent: function ()
    {
        return this._persistent;
    },
    set_persistent: function (value)
    {
        this._persistent = value;
    },

    get_modal: function ()
    {
        return this._modal;
    },
    set_modal: function (value)
    {
        this._modal = value;
    },

    get_title: function ()
    {
        return this._title;
    },
    set_title: function (value)
    {
        this._title = value;
    },

    get_titleBar: function ()
    {
        return this._titleBar;
    },

    get_themeFolder: function ()
    {
        return this._themeFolder;
    },
    set_themeFolder: function (value)
    {
        this._themeFolder = value;
    },

    get_ajaxClientId: function ()
    {
        return this._ajaxClientId;
    },
    set_ajaxClientId: function (value)
    {
        this._ajaxClientId = value;
    },

    get_left: function ()
    {
        return this._left;
    },
    set_left: function (value)
    {
        this._left = value;
    },

    get_top: function ()
    {
        return this._top;
    },
    set_top: function (value)
    {
        this._top = value;
    },

    get_errorMessageControlId: function ()
    {
        return this._errorMessageControlId;
    },
    set_errorMessageControlId: function (value)
    {
        this._errorMessageControlId = value;
    },

    /*  =========================================================
    Public methods
    =========================================================  */
    updateTitle: function()
    {
        //Update the title if we have one
        if (this._titleBar && this._title && this._title != '')
        {
            if (this._titleBar.firstChild)
            {
                while (this._titleBar.firstChild)
                {
                    this._titleBar.removeChild(this._titleBar.firstChild);
                }
            }
            this._titleBar.appendChild(d.createTextNode(this._title));
        }
    },
    display_delayed: function (evt, delay)
    {
        //Called if we want to display after a short delay
        var thisRef = this;
        if (this._displayTimeoutId == -1)
        {
            this._displayTimeoutId = setTimeout(function () { thisRef.display(evt) }, delay);
        }
    },
    display: function (evt)
    {
        //Called if we want to display immediately
        this._displayTimeoutId == -1
        var themeName = this._themeFolder.replace(/App_Themes/, '').replace(/\//g, '');

        //Loads any ajax controls in the tab page
        if (this._ajaxClientId && this._ajaxClientId != '')
        {
            //Make sure that the necessary methods exist of the component
            var clientComponent = $find(this._ajaxClientId);
            if (clientComponent && clientComponent.load_data)
            {
                clientComponent.load_data();
            }
        }

        //Update the title if we have one
        if (this._titleBar && this._title && this._title != '')
        {
            if (this._titleBar.firstChild)
            {
                while (this._titleBar.firstChild)
                {
                    this._titleBar.removeChild(this._titleBar.firstChild);
                }
            }
            this._titleBar.appendChild(d.createTextNode(this._title));
		}

		//Change the classlist before we start working out the height and position, because for some reason a class that sets display none makes the height appear as zero to JS (or negative to Jquery)
		//Note that the element still has a style of display none on it so the class of dn to display none should make no difference to jQuery but it does (note that JS still views the height at zero because the element has not yet been rendered)
		//So this is a weird hack to get things to work as of 6.10 but should be revisited more fully at a later stage
		this._element.classList.remove('visibility-none', 'dn');

        //Get the pixel width and height of the dialog box if not set already
        if (!this._height || this._height < 1)
        {
            this._height = parseInt($(this.get_element()).height());
        }
        if (!this._width || this._width < 1)
        {
            this._width = parseInt($(this.get_element()).width());
        }

        //Autoposition if passed in arguments
        if (evt)
        {
            this.auto_position(evt);
        }

        //Ensure dialog box is not wider than the screen size if modal
        if (this._modal)
        {
            var windowWidth = $(window).width();
            if (this._width > (0.9 * windowWidth))
            {
                this._width = 0.9 * windowWidth;
                //Set the width
                $(this._element).outerWidth(this._width);
            }
        }

        //Variables used to position modals in center of page (along with css)
        var marginWidth = -1 * (this._width / 2);
        var marginHeight = (-1 * (this._height / 2));

        //Make sure the top of a modal popup is visible on the page
        var windowHeight = (-1 * ($(window).height() / 2));
        if ( marginHeight < (windowHeight + 50) )
        {
            marginHeight = windowHeight + 50;
        }

        //Make visible and position
        //If viewing on mobile device, make it go fullscreent
        if (SpiraContext.IsMobile)
        {
            this.get_element().classList.add('mobile-fullscreen');
        }
        else
        {
            if (!this._left || this._left == -1 || this.get_modal())
            {
				this.get_element().classList.add('popup-centered-h');
            }
            else
            {
                this.get_element().style.left = this._left + 'px';
            }
            if (!this._top || this._top == -1 || this.get_modal())
            {
				this.get_element().classList.add('popup-centered-v');
            }
            else
            {
                this.get_element().style.top = this._top + 'px';
            }
		}
		//Note this should be where we remove the visibility and display none classes, but that causes problems.
		this._element.style.removeProperty("display");

        //Display the modal background
        if (this.get_modal())
        {
			this._background.classList.add('fade-in-50');
            this._background.classList.remove('fade-out-50');
        }

        //Raise the display event
        this.raise_displayed();
    },
    display_noPositionChange: function ()
    {
        //This is called if we want to re-display it without changing position in any way
		this._element.classList.remove('visibility-none', 'dn');
		this._element.style.removeProperty("display");
    },
    close_delayed: function (delay)
    {
        //Called if we want to close after a short delay - if not visible, just stop open timer
        if (this._displayTimeoutId != -1)
        {
            clearTimeout(this._displayTimeoutId);
            this._displayTimeoutId = -1;
        }
        else
        {
            var thisRef = this;
            this._closeTimeoutId = setTimeout(function () { thisRef.close() }, delay);
        }

    },
    close: function ()
    {
        if (this._closeTimeoutId != -1)
        {
            clearTimeout(this._closeTimeoutId);
            this._closeTimeoutId = -1;
        }
        this._onCloseClick();
    },
    auto_position: function (evt)
    {
        //Auto-positions next to the cursor - pass in the event
        var posx = 0;
        var posy = 0;
        if (evt.clientX || evt.clientY)
        {
            posx = evt.clientX + globalFunctions.getScrollLeft() - 5;
            posy = evt.clientY + globalFunctions.getScrollTop() - 10;

            //Find out how close the mouse is to the bottom of the window
            var windowHeight = window.innerHeight;
            windowHeight += globalFunctions.getScrollTop();
            if (posy + parseInt(this._height) > windowHeight)
            {
                posy = windowHeight - parseInt(this._height) - 50;
            }

            //Find out how close the mouse is to the right of the window
            var windowWidth = window.innerWidth;
            windowWidth += document.body.scrollLeft + document.documentElement.scrollLeft;
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
        this.set_left(posx);
        this.set_top(posy);
    },

    /*  =========================================================
    The event handler managers
    =========================================================  */
    add_closed: function (handler)
    {
        this.get_events().addHandler('closed', handler);
    },
    remove_closed: function (handler)
    {
        this.get_events().removeHandler('closed', handler);
    },
    raise_closed: function ()
    {
        var h = this.get_events().getHandler('closed');
        if (h) h();
    },

    add_displayed: function (handler)
    {
        this.get_events().addHandler('displayed', handler);
    },
    remove_displayed: function (handler)
    {
        this.get_events().removeHandler('displayed', handler);
    },
    raise_displayed: function ()
    {
        var h = this.get_events().getHandler('displayed');
        if (h) h();
    },


    /*  =========================================================
    Event handlers
    =========================================================  */
    _onMouseOver: function (evt, e)
    {
        if (!e.thisRef.get_persistent())
        {
            //Stop any delayed-close actions
            clearTimeout(e.thisRef._closeTimeoutId);
        }
    },
    _onMouseOut: function (evt, e)
    {
        if (!e.thisRef.get_persistent())
        {
            //Hide the dialog box when we leave the dialog box bounding box
            var posx = 0;
            var posy = 0;
            if (!evt)
            {
                var evt = window.event;
            }
            if (evt.pageX || evt.pageY)
            {
                posx = evt.pageX;
                posy = evt.pageY;
            }
            else if (evt.clientX || evt.clientY)
            {
                posx = evt.clientX + globalFunctions.getScrollLeft();
                posy = evt.clientY + globalFunctions.getScrollTop();
            }
            if (posx < e.thisRef._left || posx > (e.thisRef._left + e.thisRef._width) || posy < e.thisRef._top || posy > (e.thisRef._top + e.thisRef._height))
            {
                //Close with a 1 second delay
                e.thisRef.close_delayed(1000);
            }
        }
    },
    _onCloseClick: function ()
    {
        //Clear the timer
        if (this._displayTimeoutId != -1)
        {
            clearTimeout(this._displayTimeoutId);
            this._displayTimeoutId = -1;
        }

        if (this.get_modal())
        {
			this._background.classList.add('fade-out-50');
			this._background.classList.remove('fade-in-50');
        }

        //Hide the dialog box and raise the closed event
		this._element.classList.add('visibility-none', 'dn');
		this._element.style.display = "none";
        this.raise_closed();
    },
    _onTitleMouseDown: function (evt)
    {
        window._event = evt; // Needed internally by _DragDropManager

        this._visual = this.get_element().cloneNode(true);
		this._visual.classList.remove('visibility-none');        
        this._visual.style.opacity = '0.7';
        this._visual.style.zIndex = 99999;
        this.get_element().parentNode.appendChild(this._visual);
        var location = Sys.UI.DomElement.getLocation(this.get_element());
        Sys.UI.DomElement.setLocation(this._visual, location.x, location.y);
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.startDragDrop(this, this._visual, null);
        evt.preventDefault();
    },

    // IDragSource interface methods
    get_dragDataType: function ()
    {
        return 'Nothing';
    },

    getDragData: function (context)
    {
        return '';
    },

    get_dragMode: function ()
    {
        return Inflectra.SpiraTest.Web.ServerControls.DragMode.Move;
    },

    onDragStart: function () { },

    onDrag: function () { },

    onDragEnd: function (canceled)
    {
        //Get the coordinates
        var visualLeft = this._visual.style.left;
        var visualTop = this._visual.style.top;

        //Clear the visual
        if (this._visual)
        {
            this.get_element().parentNode.removeChild(this._visual);
        }

        //Move the actual dialog box
        this.get_element().style.left = visualLeft;
        this.get_element().style.top = visualTop;
    }
}
Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel.registerClass('Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel', Sys.UI.Control);

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
    }
}  
