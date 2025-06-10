var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.ScreenshotCapture = function (element)
{
    this._pasteCatcher = null;
    this._imgPreview = null;
    this._htmlPasteHandler = null;
    this._width = '';
    this._height = '';
    this._clickItems = new Array();
    this._clickHandlers = new Array();

    //Load in all the passed parameters from the server-control
    Inflectra.SpiraTest.Web.ServerControls.ScreenshotCapture.initializeBase(this, [element]);
}

Inflectra.SpiraTest.Web.ServerControls.ScreenshotCapture.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.ScreenshotCapture.callBaseMethod(this, 'initialize');

        //Create the child controls
        this.createChildControls();

        //Register the window.onload event
        this.registerPasteHandlers();
    },
    dispose: function ()
    {
        if (this._htmlPasteHandler)
        {
            this._pasteCatcher.removeEventListener("paste", this._htmlPasteHandler);
            delete this._htmlPasteHandler;
        }
        this._clearClickHandlers();

        delete this._pasteCatcher;
        delete this._imgPreview;

        Inflectra.SpiraTest.Web.ServerControls.ScreenshotCapture.callBaseMethod(this, 'dispose');
    },

    // -------- Properties -------- //
    get_width: function ()
    {
        return this._width;
    },
    set_width: function (value)
    {
        this._width = value;
    },

    get_height: function ()
    {
        return this._height;
    },
    set_height: function (value)
    {
        this._height = value;
    },

    get_pasteCatcher: function ()
    {
        return this._pasteCatcher;
    },

    /*  =========================================================
    Event handlers managers
    =========================================================  */
    add_imagePaste: function (handler)
    {
        this.get_events().addHandler('imagePaste', handler);
    },
    remove_imagePaste: function (handler)
    {
        this.get_events().removeHandler('imagePaste', handler);
    },
    raise_imagePaste: function (data)
    {
        var h = this.get_events().getHandler('imagePaste');
        if (h) h(data);
    },

    // -------- Methods --------- //
    createChildControls: function ()
    {
        //Create the DIV that you paste into
        this._pasteCatcher = d.ce('div');
        this._pasteCatcher.className = 'Catcher';
        this._pasteCatcher.style.width = this._width;
        this._pasteCatcher.style.height = this._height;
        this.get_element().appendChild(this._pasteCatcher);

        //Add handler to focus
        var handler = Function.createDelegate(this, this.focusOnPasteCatcher);
        $addHandler(this._pasteCatcher, 'click', handler);
        this._clickItems.push(this._pasteCatcher);
        this._clickHandlers.push(handler);

        //Create the IMG that lets you preview
        this._imgPreview = d.ce('img');
        this._imgPreview.className = 'Preview';
        this._imgPreview.style.position = 'relative';
        this._imgPreview.style.top = '-' + this._height;
        this._imgPreview.style.width = this._width;
        this._imgPreview.style.height = this._height;
        this.get_element().appendChild(this._imgPreview);

        //Add handler to focus
        var handler = Function.createDelegate(this, this.focusOnPasteCatcher);
        $addHandler(this._imgPreview, 'click', handler);
        this._clickItems.push(this._imgPreview);
        this._clickHandlers.push(handler);

        //Put the instructional text below
        var p = d.ce('p');
        p.className = 'Notes';
        if (document.documentMode && document.documentMode < 11)
        {
            //IE < 11 doesn't support pasting images
            p.appendChild(d.createTextNode(resx.ScreenshotCapture_BrowserNotSupported));
            p.style.color = 'red';
        }
        else
        {
            p.appendChild(d.createTextNode(resx.ScreenshotCapture_Intro));
        }
        if (this._element.nextSibling)
        {
            this._element.parentNode.insertBefore(p, this._element.nextSibling);
        }
        else
        {
            this._element.parentNode.appendChild(p);
        }
    },
    registerPasteHandlers: function ()
    {
        //HTML5 clipboard API or older HTML4 RTE method
        this._pasteCatcher.contentEditable = true;
        this._htmlPasteHandler = Function.createDelegate(this, this.handlePasteHTML5);

        //For some reason, the event was not being captured correctly when we used $addHandler(...)
        this._pasteCatcher.addEventListener("paste", this._htmlPasteHandler);
    },

    _clearClickHandlers: function ()
    {
        for (var i = 0; i < this._clickItems.length; i++)
        {
            $removeHandler(this._clickItems[i], 'click', this._clickHandlers[i]);
            delete this._clickItems[i];
            delete this._clickHandlers[i];
        }
    },

    /* Event Handlers */

    focusOnPasteCatcher: function (e)
    {
        this._pasteCatcher.focus();
        $('#cke_editor1').hide();   //Sometimes it creates a rogue CKE tookbar
    },

    handlePasteHTML4: function (e)
    {
        // This is a cheap trick to make sure we read the data
        // AFTER it has been inserted.
        setTimeout(Function.createDelegate(this, this.handlePasteHTML4_checkInput), 1);
    },

    /* Parse the input for the new image */
    handlePasteHTML4_checkInput: function ()
    {
        if (this._pasteCatcher.childNodes.length < 1)
        {
            //Nothing was pasted
            alert(resx.FileArtifactUploadDialog_SupaPasteImageFirst);
        }

        // Store the pasted content in a variable
        var child = this._pasteCatcher.childNodes[0];

        // Clear the inner html to make sure we're always
        // getting the latest inserted content
        this._pasteCatcher.innerHTML = "";

        if (child)
        {
            // If the user pastes an image, the src attribute
            // will represent the image as a base64 encoded string.
            if (child.tagName === "IMG")
            {
                var data = child.src;
                this._imgPreview.src = data;
                this.raise_imagePaste(data);
            }
            else
            {
                alert(resx.ScreenshotCapture_InvalidImageFormat);
            }
        }
    },

    handlePasteHTML5: function (e)
    {
        //See if we have any HTML5 format data
        if (!e.clipboardData || !e.clipboardData.items)
        {
            //Try the HTML4 handler next
            this.handlePasteHTML4(e);
            return;
        }

        var blob = null;
        for (var i = 0; i < e.clipboardData.items.length; i++)
        {
            var item = e.clipboardData.items[i];
            if (item.type.indexOf("image") != -1)
            {
                blob = item.getAsFile();
                break;
            }
        }
        // load image if there is a pasted image
        if (!blob || blob == null)
        {
            alert(resx.ScreenshotCapture_InvalidImageFormat);
        }
        else
        {
            var reader = new FileReader();
            reader.onload = function (event)
            {
                //console.log(event.target.result); // data url!
            };
            reader.onloadend = Function.createCallback(this.handlePasteHTML5_loadend, { thisRef: this, reader: reader });
            reader.readAsDataURL(blob);
        }
    },
    handlePasteHTML5_loadend: function (obj, args)
    {
        var thisRef = args.thisRef;
        var reader = args.reader;

        var data = reader.result;
        thisRef._imgPreview.src = data;
        thisRef.raise_imagePaste(data);
    },
    clearPreview: function()
    {
        //Clears the preview image
        this._imgPreview.src = '';
    }
}

Inflectra.SpiraTest.Web.ServerControls.ScreenshotCapture.registerClass('Inflectra.SpiraTest.Web.ServerControls.ScreenshotCapture', Sys.UI.Control);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
