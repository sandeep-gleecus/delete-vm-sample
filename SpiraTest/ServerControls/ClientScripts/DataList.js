var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.DataList = function(element)
{
    this._webServiceClass = "";
    this._isOverNameDesc = false;
    
    //Load in all the passed parameters from the server-control
    Inflectra.SpiraTest.Web.ServerControls.DataList.initializeBase(this, [element]);
}

Inflectra.SpiraTest.Web.ServerControls.DataList.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.DataList.callBaseMethod(this, 'initialize');
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
    }
}

Inflectra.SpiraTest.Web.ServerControls.DataList.registerClass('Inflectra.SpiraTest.Web.ServerControls.DataList', Sys.UI.Control);
        
//  always end with this goodnight statement        
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
