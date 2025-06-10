<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DiagramPanel.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.DiagramPanel" %>
<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

<%-- TOOLBAR --%>
<div class="TabControlHeader bg-near-white br2 pt2">
    <div class="btn-group priority1">
		<tstsc:HyperLinkEx ID="lnkRefresh" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptMethod="tstucDiagramPanel.load_data(null, true)">
            <span class="fas fa-sync"></span>
            <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
		</tstsc:HyperLinkEx>
    </div>
</div>

<div id="outer-container" style="width: 100%">
    <div id="use-case-diagram" style="margin-left: 100px">
        <svg id="svgUseCaseDiagram" class="graphviz-hierarchical graphviz-large-text">
            <g />
        </svg>
    </div>
</div>

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Scripts>
        <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.graphlib-dot.js" Assembly="Web" />
        <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.dagre-d3.js" Assembly="Web" />
    </Scripts>
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    //Global IDs
    var lblMessage_id = '<%=lblMessage.ClientID%>';

    var unscaled_width;
    var unscaled_height;

    // Create and configure the renderer
    var render = dagreD3.render();
    var g;

    /* The User Control Class */
    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
    Inflectra.SpiraTest.Web.UserControls.DiagramPanel = function ()
    {
        this._userId = <%=UserId%>;
        this._projectId = <%=ProjectId%>;
        this._requirementId = -1;
        this._hasData = false;
        this._loadingComplete = false;

        Inflectra.SpiraTest.Web.UserControls.DiagramPanel.initializeBase(this);
    }
    Inflectra.SpiraTest.Web.UserControls.DiagramPanel.prototype =
    {
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.DiagramPanel.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.DiagramPanel.callBaseMethod(this, 'dispose');
        },

        /* Properties */
        get_artifactId: function()
        {
            return this._requirementId;
        },
        set_artifactId: function(value)
        {
            this._requirementId = value;
        },

        load_data: function(filters, loadNow)
        {
            //Check for IE, doesn't support Array.find which is needed by graphviz
            if (!Array.prototype.find) {
                $('#use-case-diagram').addClass('not-supported-ie');
            }
            else
            {
                if (loadNow)
                {
                    //Load the uml diagram in dot notation
                    Inflectra.SpiraTest.Web.Services.Ajax.RequirementStepService.RequirementStep_RetrieveAsDotNotation(this._projectId, this._requirementId, Function.createDelegate(this, this.load_data_success), Function.createDelegate(this, this.load_data_failure));
                }
            }
            this._loadingComplete = true;
        },
        load_data_success: function(dotNotation)
        {
            this.renderMap(dotNotation);
        },
        load_data_failure: function(ex)
        {
            globalFunctions.display_error($get(lblMessage_id), ex);
        },

        renderMap: function(dotNotation)
        {
            try
            {
                //Clear the initial content
                var inner = $("#use-case-diagram svg g")[0];
                globalFunctions.clearContent(inner);

                if (dotNotation)
                {
                    //Get the initial width of the container
                    var containerWidth = $get('outer-container').offsetWidth + 'px';

                    //create the graph from the notation
                    g = graphlibDot.read(dotNotation);

                    // Set margins, if not present
                    if (!g.graph().hasOwnProperty("marginx") &&
                        !g.graph().hasOwnProperty("marginy")) {
                        g.graph().marginx = 20;
                        g.graph().marginy = 20;
                    }

                    g.graph().transition = function(selection) {
                        return selection.transition().duration(200);
                    };

                    // Render the graph into svg g
                    d3.select("svg g").call(render, g);

                    //Next set the width of the SVG so that the scrollbars in the container DIV appear
                    var svgUseCaseDiagram = $get('svgUseCaseDiagram');
                    var bBox = svgUseCaseDiagram.getBBox();
                    unscaled_width = parseInt(bBox.width + 100);
                    unscaled_height = parseInt(bBox.height + 80);
                    svgUseCaseDiagram.style.width = unscaled_width + 'px';
                    svgUseCaseDiagram.style.height = unscaled_height + 'px';
                }
            }
            catch(err)
            {
                globalFunctions.display_error_message($get(lblMessage_id), err);
            }
        }
    }
    var tstucDiagramPanel = $create(Inflectra.SpiraTest.Web.UserControls.DiagramPanel);
</script>
