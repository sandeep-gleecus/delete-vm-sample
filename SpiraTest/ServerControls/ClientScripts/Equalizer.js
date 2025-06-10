var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.Equalizer = function (element)
{
    this._percentBlue = 0;
    this._percentGray = 0;
    this._percentGreen = 0;
    this._percentOrange = 0;
    this._percentRed = 0;
    this._percentYellow = 0;
    this._percentDarkGray = 0;
    this._toolTip = '';
    this._isFirefox = false;
    this._pixelWidth = 100;
    this._primaryKey = null;

    this._mouseOverHandler = null;
    this._mouseOutHandler = null;

    //Load in all the passed parameters from the server-control
    Inflectra.SpiraTest.Web.ServerControls.Equalizer.initializeBase(this, [element]);
}

Inflectra.SpiraTest.Web.ServerControls.Equalizer.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.Equalizer.callBaseMethod(this, 'initialize');

        //Register the mouse over/out handler for tooltips
        this._mouseOverHandler = Function.createDelegate(this, this._onMouseOver);
        this._mouseOutHandler = Function.createDelegate(this, this._onMouseOut);
        $addHandler(this.get_element(), 'mouseover', this._mouseOverHandler);
        $addHandler(this.get_element(), 'mouseout', this._mouseOutHandler);

        //Render out the colors
        this.display();
    },
    dispose: function ()
    {
        if (this.get_element() && this._mouseOverHandler)
        {
            $removeHandler(this.get_element(), 'mouseover', this._mouseOverHandler);
            delete this._mouseOverHandler;
        }
        if (this.get_element() && this._mouseOutHandler)
        {
            $removeHandler(this.get_element(), 'mouseout', this._mouseOutHandler);
            delete this._mouseOutHandler;
        }

        Inflectra.SpiraTest.Web.ServerControls.Equalizer.callBaseMethod(this, 'dispose');
    },

    // -------- Properties -------- //
    get_primaryKey: function ()
    {
        return this._primaryKey;
    },
    set_primaryKey: function (value)
    {
        this._primaryKey = value;
    },
    get_percentBlue: function ()
    {
        return this._percentBlue;
    },
    set_percentBlue: function (value)
    {
        this._percentBlue = value;
    },

    get_percentGray: function ()
    {
        return this._percentGray;
    },
    set_percentGray: function (value)
    {
        this._percentGray = value;
    },

    get_percentDarkGray: function ()
    {
        return this._percentDarkGray;
    },
    set_percentDarkGray: function (value)
    {
        this._percentDarkGray = value;
    },

    get_percentGreen: function ()
    {
        return this._percentGreen;
    },
    set_percentGreen: function (value)
    {
        this._percentGreen = value;
    },

    get_percentOrange: function ()
    {
        return this._percentOrange;
    },
    set_percentOrange: function (value)
    {
        this._percentOrange = value;
    },

    get_percentRed: function ()
    {
        return this._percentRed;
    },
    set_percentRed: function (value)
    {
        this._percentRed = value;
    },

    get_percentYellow: function ()
    {
        return this._percentYellow;
    },
    set_percentYellow: function (value)
    {
        this._percentYellow = value;
    },

    get_toolTip: function ()
    {
        return this._toolTip;
    },
    set_toolTip: function (value)
    {
        this._toolTip = value;
    },

    get_isFirefox: function ()
    {
        return this._isFirefox;
    },
    set_isFirefox: function (value)
    {
        this._isFirefox = value;
    },

    get_pixelWidth: function ()
    {
        return this._pixelWidth;
    },
    set_pixelWidth: function (value)
    {
        this._pixelWidth = value;
    },

    // -------- Methods --------- //
    resetValues: function()
    {
        this._percentBlue = 0;
        this._percentGray = 0;
        this._percentGreen = 0;
        this._percentOrange = 0;
        this._percentRed = 0;
        this._percentYellow = 0;
        this._percentDarkGray = 0;
    },
    display: function ()
    {
        //Clear out any existing content
        globalFunctions.clearContent(this.get_element());

        //Create the various color sections in the equalizer as spans
        //Need to add additional tags in the case of Firefox which doesn't support inline SPAN widths
        //Green
        var span = d.ce('span');
        span.className = 'EqualizerGreen';
        if (this._isFirefox)
        {
            span.style.paddingLeft = this._calculateWidth(this.get_percentGreen());
        }
        else
        {
            span.style.width = this._calculateWidth(this.get_percentGreen());
        }
        this.get_element().appendChild(span);

        //Red
        span = d.ce('span');
        span.className = 'EqualizerRed';
        if (this._isFirefox)
        {
            span.style.paddingLeft = this._calculateWidth(this.get_percentRed());
        }
        else
        {
            span.style.width = this._calculateWidth(this.get_percentRed());
        }
        this.get_element().appendChild(span);

        //Orange
        span = d.ce('span');
        span.className = 'EqualizerOrange';
        if (this._isFirefox)
        {
            span.style.paddingLeft = this._calculateWidth(this.get_percentOrange());
        }
        else
        {
            span.style.width = this._calculateWidth(this.get_percentOrange());
        }
        this.get_element().appendChild(span);

        //Yellow
        span = d.ce('span');
        span.className = 'EqualizerYellow';
        if (this._isFirefox)
        {
            span.style.paddingLeft = this._calculateWidth(this.get_percentYellow());
        }
        else
        {
            span.style.width = this._calculateWidth(this.get_percentYellow());
        }
        this.get_element().appendChild(span);

        //Gray
        span = d.ce('span');
        span.className = 'EqualizerGray';
        if (this._isFirefox)
        {
            span.style.paddingLeft = this._calculateWidth(this.get_percentGray());
        }
        else
        {
            span.style.width = this._calculateWidth(this.get_percentGray());
        }
        this.get_element().appendChild(span);

        //Dark Gray
        span = d.ce('span');
        span.className = 'EqualizerDarkGray';
        if (this._isFirefox)
        {
            span.style.paddingLeft = this._calculateWidth(this.get_percentDarkGray());
        }
        else
        {
            span.style.width = this._calculateWidth(this.get_percentDarkGray());
        }
        this.get_element().appendChild(span);

        //Blue
        span = d.ce('span');
        span.className = 'EqualizerBlue';
        if (this._isFirefox)
        {
            span.style.paddingLeft = this._calculateWidth(this.get_percentBlue());
        }
        else
        {
            span.style.width = this._calculateWidth(this.get_percentBlue());
        }
        this.get_element().appendChild(span);
    },

    _calculateWidth: function (percent)
    {
        if (!percent)
        {
            return '0px';
        }
        if (this._pixelWidth && this._pixelWidth != 100)
        {
            return ((percent * this._pixelWidth) / 100) + 'px';
        }
        else
        {
            return percent + 'px';
        }
    },

    /* Event Handlers */
    _onMouseOver: function (evt)
    {
        if (this.get_toolTip() && this.get_toolTip() != '')
        {
            ddrivetip(this.get_toolTip());
        }
    },
    _onMouseOut: function (evt)
    {
        hideddrivetip();
    }
}

Inflectra.SpiraTest.Web.ServerControls.Equalizer.registerClass('Inflectra.SpiraTest.Web.ServerControls.Equalizer', Sys.UI.Control);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
