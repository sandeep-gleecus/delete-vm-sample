<%@ Page 
    AutoEventWireup="true" 
    CodeBehind="ReleasesMap.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.ReleasesMap" 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    Title="" 
%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="DiagramsStylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">

    <div id="tblMainBody" class="MainContent w-100 ">
        <div class="df flex-column min-h-insideHeadAndFoot">

            <div class="df items-center justify-between flex-wrap-xs mvw-100">
                <h1 class="w-100-xs fs-h3-xs fw-b-xs pl4 px3-xs">
                    <asp:Localize runat="server" Text="<%$Resources:Main,SiteMap_ReleasesMap %>" />
                    <span class="badge">
                        <asp:Localize runat="server" Text="<%$Resources:Main,Global_Beta %>" />
                    </span>
                </h1>
                <!-- Releases View Selector -->
                <asp:PlaceHolder ID="plcListBoardSelector" runat="server">
                    <span class="btn-group priority1 mb4-xs ml2 ml0-xs pr5 pr0-xs" role="group">
                        <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Releases, ProjectId, -6) %>' runat="server" title="<%$ Resources:Main,Global_Tree %>">
                            <span class="fas fa-indent"></span>
                        </a>
                        <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Releases, ProjectId, -9) %>' runat="server" title="<%$ Resources:Main,Global_Gantt %>">
                            <span class="fas fa-align-left"></span>
                        </a>
                        <a class="btn btn-default active" aria-selected="true" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Releases, ProjectId, -8) %>' runat="server" title="<%$ Resources:Main,Global_MindMap %>">
                            <span class="fas fa-project-diagram"></span>
                        </a>
                    </span>
                </asp:PlaceHolder>
            </div>

            <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

            <!-- Releases Mind Map Toolbar -->
            <div class="sticky top2 bg-white w-100 pt5 bb b-near-white bw2" role="toolbar">
                <div class="df px4">
			        <div class="btn-group priority1 pr4" role="group">
			            <button 
                            class="btn btn-default"
                            id="btnRefresh" 
                            onclick="reloadDiagram()"
                            runat="server" 
                            title="<%$Resources:Buttons,Refresh %>" 
                            type="button"
                            >
                            <i class="fas fa-sync"></i>
			            </button>
				        <tstsc:DropDownListEx ID="ddlShowLevel" Runat="server" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowLevel %>" AutoPostBack="False" DataTextField="Value" DataValueField="Key" CssClass="DropDownList" ClientScriptMethod="expandToLevel" Width="110px" />
                    </div>

                    <div class="input-group input-group-sm priority2 pr4" role="group">
                        <div class="input-group-btn">
			                <button
                                class="btn btn-default"
                                id="btnDecreaseZoom" 
                                onclick="decrease_zoom()"
                                runat="server" 
                                title="<%$Resources:Buttons,Decrease %>" 
                                type="button"
                                >
                                <i class="fas fa-minus"></i>
			                </button>
                        </div>
						<tstsc:TextBoxEx 
                            ID="txtZoomLevel" 
                            runat="server" 
                            SkinID="NarrowPlusFormControl"
                            Text="100"
                            TextMode="SingleLine"
                            />
                        <span class="input-group-addon">%</span>
                        <div class="input-group-btn">
                            <button
                                class="btn btn-default"
                                id="btnIncreaseZoom" 
                                onclick="increase_zoom()"
                                runat="server" 
                                title="<%$Resources:Buttons,Increase %>" 
                                type="button"
                                >
                                <i class="fas fa-plus"></i>
			                </button>
                            <button
                                class="btn btn-default"
                                id="btnResetZoom" 
                                onclick="reset_zoom()"
                                runat="server" 
                                title="<%$Resources:Buttons,Reset %>" 
                                type="button"
                                >
                                <i class="fas fa-search"></i>
			                </button>
                        </div>
                    </div>

                    <div class="u-checkbox-toggle pl2 priority3" role="group">
                        <tstsc:CheckBoxEx ID="chkIncludeAssociations" runat="server" Visible="false"
                                Text="<%$Resources:Main,RequirementsMap_IncludeAssociations%>" ClientScriptMethod="chkIncludeAssociations_click()" />
                    </div>   
                </div>
            </div>
                
            <div id="outer-container" class="w-100 px4 mt4">
                <div id="releases-model" class="graphviz-container">
                    <svg id="svgReleasesMap" class="graphviz-hierarchical">
                        <g/>
                    </svg>
                </div>
            </div>
        </div>

        <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
            <Scripts>
                <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.graphlib-dot.js" Assembly="Web" />
                <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.dagre-d3.js" Assembly="Web" />
            </Scripts>
            <Services>
                <asp:ServiceReference Path="~/Services/Ajax/ReleasesService.svc" />
            </Services>
        </tstsc:ScriptManagerProxyEx>
        <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
    </div>

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="cplScripts">
<script type="text/javascript">

    //Global IDs
    var lblMessage_id = '<%=lblMessage.ClientID%>';
    var g_projectId = <%=ProjectId%>;
    var txtZoomLevel_id = '<%=txtZoomLevel.ClientID%>';
    var ddlShowLevel_id = '<%=ddlShowLevel.ClientID%>';
    var g_baseUrl = '<%=this.ResolveUrl("~")%>';

    var unscaled_width;
    var unscaled_height;

    // Create and configure the renderer
    var render = dagreD3.render();
    var g;
    var g_isOverNameDesc = false;

    $(document).ready(function () {
        //Check for IE, doesn't support Array.find which is needed by graphviz
        if (!Array.prototype.find) {
            $('#releases-model').addClass('not-supported-ie');
        }
        else
        {
            loadReleasesMap();
        }

        //disable text box
        $('#' + txtZoomLevel_id).prop('disabled', true);
    });

    function loadReleasesMap()
    {
        var projectId = <%=ProjectId%>;
        var numberOfLevels = null; //All
        var ddlShowLevel = $find(ddlShowLevel_id);
        if (ddlShowLevel.get_selectedItem() && ddlShowLevel.get_selectedItem().get_value() && ddlShowLevel.get_selectedItem().get_value() != '' && ddlShowLevel.get_selectedItem().get_value() > 0)
        {
            numberOfLevels = ddlShowLevel.get_selectedItem().get_value();
        }
        Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService.Release_RetrieveAsDotNotation(projectId, numberOfLevels, loadReleasesMap_success, loadReleasesMap_failure);
    }
    function loadReleasesMap_success(dotNotation)
    {
        renderMap(dotNotation);
    }
    function loadReleasesMap_failure(ex)
    {
        globalFunctions.display_error($get(lblMessage_id), ex);
    }

    function renderMap(dotNotation)
    {
        try
        {
            //Get the initial width of the container
            var containerWidth = $get('outer-container').offsetWidth + 'px';

            //Check for IE11

            //create the graph from the notation
            g = graphlibDot.read(dotNotation);

            // Set margins, if not present
            if (!g.graph().hasOwnProperty("marginx") &&
                !g.graph().hasOwnProperty("marginy")) {
                g.graph().marginx = 20;
                g.graph().marginy = 20;
            }

            g.graph().transition = function(selection) {
                return selection.transition().duration(500);
            };

            // Render the graph into svg g
            d3.select("svg g").call(render, g);

            //Next set the width of the SVG so that the scrollbars in the container DIV appear
            var svgReleasesMap = $get('svgReleasesMap');
            var bBox = svgReleasesMap.getBBox();
            unscaled_width = parseInt(bBox.width);
            unscaled_height = parseInt(bBox.height);
            // add 50px to each dimension because otherwise the edges of the svg get cut off - not sure why (simon - 2019-07)
            svgReleasesMap.style.width = (unscaled_width + 50) +  'px';
            svgReleasesMap.style.height = (unscaled_height + 50) + 'px';

            //Next set the width of the DIV container
            //$get('releases-model').style.width = containerWidth;

            //Finally, register event handlers for node tooltips
            $("#releases-model svg g a[data-release-id]").on("mouseover", function(evt){
                node_displayTooltip(evt.target.getAttribute('data-release-id'));
            });
            $("#releases-model svg g a[data-release-id]").on("mouseout", function(evt){
                node_hideTooltip();
            });

        }
        catch(err)
        {
            globalFunctions.display_error_message($get(lblMessage_id), err);
        }
    }

    function node_displayTooltip(releaseId)
    {
        //Display the loading message
        ddrivetip(resx.Global_Loading);
        g_isOverNameDesc = true;   //Set the flag since asynchronous

        //Now get the real tooltip via Ajax web-service call
        Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService.RetrieveNameDesc(g_projectId, releaseId, null, node_displayTooltip_success);

    }
    function node_displayTooltip_success(tooltipData)
    {
        if (g_isOverNameDesc)
        {
            ddrivetip(tooltipData);
        }
    }
    function node_hideTooltip()
    {
        hideddrivetip();
        g_isOverNameDesc = false;
    }

    function reloadDiagram()
    {
        //Clear the elements
        var inner = $("#releases-model svg g")[0];
        globalFunctions.clearContent(inner);

        //Reload
        loadReleasesMap();
    }
    function expandToLevel()
    {
        //The settings are sent on reload so just reload
        reloadDiagram();
    }
    function increase_zoom()
    {
        var zoom = parseInt($('#' + txtZoomLevel_id).val());
        if (zoom < 100) {
            zoom += 25; //Increase by 25% increments
            $('#' + txtZoomLevel_id).val(zoom);
            changeZoom(zoom);
        }
    }
    function decrease_zoom()
    {
        var zoom = parseInt($('#' + txtZoomLevel_id).val());
        zoom -= 25; //Decrease by 25% increments, lowest is 25%
        if (zoom < 25)
        {
            zoom = 25;
        }
        $('#' + txtZoomLevel_id).val(zoom);
        changeZoom(zoom);
    }
    function reset_zoom()
    {
        zoom = 100; //reset to 100%
        $('#' + txtZoomLevel_id).val(zoom);
        changeZoom(zoom);
    }
    function changeZoom(percent)
    {        
        var scale = percent / 100;

        //Update the width of the SVG
        var svgReleasesMap = $get('svgReleasesMap');
        var width = parseInt(unscaled_width * scale) + 20;
        var height = parseInt(unscaled_height * scale) + 20;
        svgReleasesMap.style.width = width + 'px';
        svgReleasesMap.style.height = height + 'px';

        //Now actually scale the SVG
        var svg = d3.select("svg");
        var inner = d3.select("svg g");
        inner.attr("transform", "translate(0,0) scale(" + scale + ")");
    }
    /* TODO: Not implemented
    function export_as_image(format)
    {
        //Currently we only support SVG format export
        if (format == 'SVG')
        {
            //Get the SVG markup from C3 and have the server write it out
            var svgStr = $get('svgReleasesMap').firstChild.outerHTML;
            if (!svgStr) {
                //IE11
                var inner = $("#svgReleasesMap:first-child").html();
                svgStr = '<svg style="overflow: hidden;" width="' + unscaled_width + '" height="' + unscaled_height + '">' + inner + '</svg>';
            }
            if (!svgStr) {
                //Other, older browsers
                alert(resx.JqPlot_ImageExportNotSupport);
                return;
            }

            //Now we need to convert the SVG to a file
            var url = g_baseUrl + 'JqPlot/GraphImageSvg.ashx';
            var ajax = new XMLHttpRequest();
            ajax.open("POST", url, true);
            ajax.setRequestHeader('Content-Type', 'image/svg+xml');
            ajax.onreadystatechange = function () {
                if (ajax.readyState == 4) {
                    //Call the same URL, but this time, pass the guid for the file
                    window.location.href = url + "?guid=" + ajax.responseText;
                }
            }
            ajax.send(svgStr);
        }
    }*/
</script>
</asp:Content>
