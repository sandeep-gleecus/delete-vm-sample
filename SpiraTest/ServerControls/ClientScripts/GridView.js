var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.GridView = function(element)
{
    this._webServiceClass = "";
    this._isOverNameDesc = false;
    
    //Load in all the passed parameters from the server-control
    Inflectra.SpiraTest.Web.ServerControls.GridView.initializeBase(this, [element]);
}

Inflectra.SpiraTest.Web.ServerControls.GridView.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.GridView.callBaseMethod(this, 'initialize');
    },

    // -------- Properties -------- //

    get_webServiceClass: function ()
    {
        return this._webServiceClass;
    },
    set_webServiceClass: function (value)
    {
        this._webServiceClass = value;
    },

    // -------- Methods --------- //
    checkbox_changed: function ()
    {
        var boolAllChecked = true;

        //Find all the checkboxes in the grid
        var checkboxes = this._element.getElementsByTagName('input');
        if (checkboxes && checkboxes.length > 0)
        {
            for (var i = 0; i < checkboxes.length; i++)
            {
                var e = checkboxes[i];
                if (e.type == 'checkbox' && e.name.indexOf('chkCol') != -1)
                {
                    if (!e.checked)
                    {
                        boolAllChecked = false;
                        break;
                    }
                }
            }
        }

        checkboxes = this._element.getElementsByTagName('input');
        if (checkboxes && checkboxes.length > 0)
        {
            for (var i = 0; i < checkboxes.length; i++)
            {
                var e = checkboxes[i];
                if (e.type == 'checkbox' && e.name.indexOf('chkHead') != -1)
                {
                    e.checked = boolAllChecked;
                    break;
                }
            }
        }
    },
    select_all_checkboxes: function ()
    {
        //Get the state of the select all box
        var chkSelectAll = null;
        var checkboxes = this._element.getElementsByTagName('input');
        if (checkboxes && checkboxes.length > 0)
        {
            for (var i = 0; i < checkboxes.length; i++)
            {
                var e = checkboxes[i];
                if (e.type == 'checkbox' && e.name.indexOf('chkHead') != -1)
                {
                    chkSelectAll = e;
                    break;
                }
            }
        }
        if (chkSelectAll)
        {
            var chkState = chkSelectAll.checked;
            //Find all the checkboxes in the grid
            checkboxes = this._element.getElementsByTagName('input');
            if (checkboxes && checkboxes.length > 0)
            {
                for (var i = 0; i < checkboxes.length; i++)
                {
                    var e = checkboxes[i];
                    if (e.type == 'checkbox' && e.name.indexOf('chkCol') != -1)
                    {
                        e.checked = chkState;
                    }
                }
            }
        }
    },

    display_tooltip: function (artifactId, projectId)
    {
        //Display the loading message
        ddrivetip(resx.Global_Loading);
        this._isOverNameDesc = true;   //Set the flag since asynchronous

        //Now get the real tooltip via Ajax web-service call
        var webService = this.get_webServiceClass();
        webService.RetrieveNameDesc(projectId, artifactId, null, Function.createDelegate(this, this.display_tooltip_success), Function.createDelegate(this, this.operation_failure));
    },
    hide_tooltip: function ()
    {
        hideddrivetip();
        this._isOverNameDesc = false;
    },
    display_tooltip_success: function (tooltipData)
    {
        if (this._isOverNameDesc)
        {
            ddrivetip(tooltipData);
        }
    },

    operation_failure: function (exception)
    {
        //Fail quietly
        hideddrivetip();
    }
}

Inflectra.SpiraTest.Web.ServerControls.GridView.registerClass('Inflectra.SpiraTest.Web.ServerControls.GridView', Sys.UI.Control);
        
//  always end with this goodnight statement        
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
