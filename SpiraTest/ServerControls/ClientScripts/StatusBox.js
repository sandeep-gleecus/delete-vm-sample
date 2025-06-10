var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.StatusBox = function (element)
{
    this._text = '';
    this._navigateUrl = '';
    this._cssClass = '';
    this._dataCssClass = '';

    //Load in all the passed parameters from the server-control
    Inflectra.SpiraTest.Web.ServerControls.StatusBox.initializeBase(this, [element]);
}

Inflectra.SpiraTest.Web.ServerControls.StatusBox.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.StatusBox.callBaseMethod(this, 'initialize');

        //Display the text/url/color
        this.display();
    },
    dispose: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.StatusBox.callBaseMethod(this, 'dispose');
    },

    // -------- Properties -------- //
    get_text: function ()
    {
        return this._text;
    },
    set_text: function (value)
    {
        this._text = value;
    },

    get_navigateUrl: function ()
    {
        return this._navigateUrl;
    },
    set_navigateUrl: function (value)
    {
        this._navigateUrl = value;
    },

    get_cssClass: function ()
    {
        return this._cssClass;
    },
    set_cssClass: function (value)
    {
        this._cssClass = value;
    },

    get_dataCssClass: function ()
    {
        return this._dataCssClass;
    },
    set_dataCssClass: function (value)
    {
        this._dataCssClass = value;
    },

    // -------- Methods --------- //
    display: function ()
    {
        //Clear out any existing content
        globalFunctions.clearContent(this.get_element());

        //See if we have a CSS class or color specified for the data element
        var classOrColor = this._dataCssClass;

        if (classOrColor && classOrColor.length > 0) {
            if (classOrColor.substr(0, 1) == '#') {
                //Update the CSS class with the base CSS class and change the color specifically
                this.get_element().className = this._cssClass;
                this.get_element().style.backgroundColor = classOrColor;
            }
            else
            {
                //Update the CSS class with the base CSS class and the data-field CSS class
                this.get_element().className = this._cssClass + ' ' + classOrColor;
            }
        }
        else
        {
            this.get_element().className = this._cssClass;
            this.get_element().style.backgroundColor = '';
        }

        //See if we have a hyperlink
        if (this._navigateUrl && this._navigateUrl != '')
        {
            //Create the hyperlink
            var a = d.ce('a');
            this.get_element().appendChild(a);
            a.href = this._navigateUrl;
            var textNode = d.createTextNode(this._text);
            a.appendChild(textNode);
        }
        else
        {
            //Just append the text as a text node
            var textNode = d.createTextNode(this._text);
            this.get_element().appendChild(textNode);
        }
    }
};

Inflectra.SpiraTest.Web.ServerControls.StatusBox.registerClass('Inflectra.SpiraTest.Web.ServerControls.StatusBox', Sys.UI.Control);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
