<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="GraphDetails.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.GraphDetails" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_Graphs %>" />
                    <small>
                        <tstsc:LabelEx CssClass="SubTitle" ID="lblGraphName" runat="server" />
                    </small>
                </h2>
                <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkBackToList" runat="server" NavigateUrl="Graphs.aspx">
                    <span class="fas fa-arrow-left"></span>
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_GraphDetails_BackToList %>" />
                </tstsc:HyperLinkEx>
                <p class="my4">
                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Admin_GraphDetails_Intro %>" />
                </p>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True"
                    DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />
            </div>
        </div>
        <div class="row data-entry-wide DataEntryForm">
            <div class="col-lg-12 col-sm-12">
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="txtNameLabel" runat="server" AssociatedControlID="txtName" Required="true" AppendColon="true" Text="<%$Resources:Fields,Name %>"/>
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
                        <tstsc:TextBoxEx ID="txtName" runat="server" MaxLength="50" />
                        <asp:RequiredFieldValidator ID="Requiredfieldvalidator1" runat="server" Text="*"
                            ErrorMessage="<%$Resources:Messages,Admin_GraphDetails_NameRequired %>" ControlToValidate="txtName" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="txtDescriptionLabel" runat="server" AssociatedControlID="txtDescription" Required="false" AppendColon="true" Text="<%$Resources:Fields,Description %>"/>
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
                        <tstsc:TextBoxEx ID="txtDescription" runat="server" TextMode="MultiLine" RichText="false" Width="100%" Height="80px"/>
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="chkActiveLabel" runat="server" AssociatedControlID="chkActive" Required="true" AppendColon="true" Text="<%$Resources:Fields,ActiveYn %>"/>
                    </div>
                    <div class="DataEntry col-sm-9">
                        <tstsc:CheckBoxYnEx ID="chkActive" runat="server" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="txtPositionLabel" runat="server" AssociatedControlID="txtPosition" Required="true" AppendColon="true" Text="<%$Resources:Fields,Position %>"/>
                    </div>
                    <div class="DataEntry col-sm-9">
                        <tstsc:TextBoxEx ID="txtPosition" runat="server" MaxLength="4" SkinID="NarrowControl" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
                        <tstsc:LabelEx ID="txtQueryLabel" runat="server" AssociatedControlID="txtQuery" Required="true" AppendColon="true" Text="<%$Resources:Fields,Query %>"/>
                    </div>
                    <div class="DataEntry data-very-wide col-sm-9">
                      <div class="btn-group">
                            <tstsc:DropDownListEx ID="ddlQuerySelection" runat="server" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Admin_ReportDetails_AddNewQuery %>" DataValueField="Key" DataTextField="Value" ClientScriptMethod="ddlQuerySelection_changed" />
                            <tstsc:HyperLinkEx ID="lnkQueryPreviewResults" runat="server"  NavigateUrl="javascript:void(0)" ClientScriptMethod="previewResults()" SkinID="ButtonDefault">
                                <span class="fas fa-table"></span>
                                <asp:Localize runat="server" Text="<%$Resources:Main,Admin_GraphDetails_DisplayDataGrid %>" />
                            </tstsc:HyperLinkEx>
                            <span class="btn btn-default btn-group-addon">
                                 <asp:Localize runat="server" Text="<%$Resources:Main,Admin_GraphDetails_PreviewGraph %>" />:
                            </span>
                            <tstsc:HyperLinkEx ID="lnkPreviewGraph_Donut" runat="server"  NavigateUrl="javascript:void(0)" ClientScriptMethod="previewGraph('donut')" SkinID="ButtonDefault" ToolTip="<%$Resources:Main,Graphs_DonutChart %>">
                                <span class="fas fa-chart-pie"></span>                               
                            </tstsc:HyperLinkEx>
                            <tstsc:HyperLinkEx ID="lnkPreviewGraph_Bar" runat="server"  NavigateUrl="javascript:void(0)" ClientScriptMethod="previewGraph('bar')" SkinID="ButtonDefault" ToolTip="<%$Resources:Main,Graphs_BarChart %>">
                                <span class="fas fa-chart-bar"></span>                               
                            </tstsc:HyperLinkEx>
                            <tstsc:HyperLinkEx ID="lnkPreviewGraph_Line" runat="server"  NavigateUrl="javascript:void(0)" ClientScriptMethod="previewGraph('line')" SkinID="ButtonDefault" ToolTip="<%$Resources:Main,Graphs_LineChart %>">
                                <span class="fas fa-chart-line"></span>                               
                            </tstsc:HyperLinkEx>
                        </div>
                        <br style="clear: both" />
                        <tstsc:MessageBox ID="lblQueryMessage" runat="server" SkinID="MessageBox" />
                        <tstsc:TextBoxEx ID="txtQuery" runat="server" TextMode="MultiLine" Height="100px"/>
                        <asp:RequiredFieldValidator ID="Requiredfieldvalidator2" runat="server" Text="*"
                            ErrorMessage="<%$Resources:Messages,Admin_GraphDetails_QueryRequired %>" ControlToValidate="txtQuery" />
                        <p class="alert alert-warning mt3">
                            <span class="fas fa-info-circle"></span>
                            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Admin_GraphDetails_QueryLegend %>" />
                        </p>
                        <p class="alert alert-warning mt3">
                            <span class="fas fa-info-circle"></span>
                            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Admin_GraphDetails_QueryLegend2 %>" />
                        </p>
                        <div id="divCustomQueryResults" style="overflow-y: scroll; overflow-x: auto; height:200px; width:800px; display: none; position: relative">
                            <div id="divCustomQueryResultsClose" class="parent-close pointer absolute pr2 pt2 right0 top0 z-2 o-1-on-parent-hover light-gray orange-hover transition-all fas fa-window-close" role="button" onclick="closeDataGrid(event)"></div>
                            <div id="divCustomQueryResultsInner" style="width:100%; height: 100%">
                            </div>
                        </div>
                        <div class="u-chart db relative" id="c3_customGraph" style="display: none;">
                            <div id="c3_customGraphClose" class="parent-close pointer absolute pr2 pt2 right0 top0 z-2 o-1-on-parent-hover light-gray orange-hover transition-all fas fa-window-close" role="button" onclick="closeGraph(event)"></div>
                            <div id="c3_customGraphInner" style="width:100%; height: 100%">
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-offset-3 col-lg-offset-2 btn-group pl3 pt4">
                        <tstsc:ButtonEx ID="btnUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save %>" CausesValidation="true" />
                        <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="false" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/ReportsService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/GraphingService.svc" />
        </Services>
    </tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        function ddlQuerySelection_changed(item)
        {
            //Insert the standard Entity SQL select clause for this reportable entity
            if (item && item.get_value() != '')
            {
                var entitySqlFormat = '<%=Inflectra.SpiraTest.Business.ReportManager.REPORT_ENTITY_SQL_FORMAT %>';
                var defaultSql = entitySqlFormat.replace('{0}', item.get_value());
                var txtQuery = $get('<%=txtQuery.ClientID %>');
                txtQuery.value += defaultSql + '\n';
            }
        }

        //Preview the query results
        function previewResults()
        {
            //Make sure we have a query specified
            var txtQuery = $get('<%=txtQuery.ClientID %>');
            var sql = txtQuery.value;
            if (sql == '')
            {
                alert (resx.ReportDetails_NeedToSpecifyCustomQuery);
                return;
            }

            var projectId = <%=ProjectId %>;
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.ReportsService.Reports_RetrieveCustomQueryData(projectId, sql, Function.createDelegate(this, this.previewResults_success), Function.createDelegate(this, this.previewResults_failure));
        }
        function previewResults_success(results)
        {
            globalFunctions.hide_spinner();

            //Put the results into the DIV container
            var divCustomQueryResults = $get('divCustomQueryResults');
            var divCustomQueryResultsInner = $get('divCustomQueryResultsInner');
            divCustomQueryResultsInner.innerHTML = results;
            divCustomQueryResults.style.display = 'block';
        }
        function previewResults_failure(exception)
        {
            globalFunctions.hide_spinner();
            globalFunctions.display_error($get('<%=lblQueryMessage.ClientID%>'), exception);
        }

        function previewGraph(type)
        {
            //Make sure we have a query specified
            var txtQuery = $get('<%=txtQuery.ClientID %>');
            var sql = txtQuery.value;
            if (sql == '')
            {
                alert (resx.ReportDetails_NeedToSpecifyCustomQuery);
                return;
            }

            var projectId = <%=ProjectId %>;
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.GraphingService.CustomGraph_RetrievePreview(projectId, sql, Function.createDelegate(this, this.previewGraph_success), Function.createDelegate(this, this.previewGraph_failure), type);
        }
        function previewGraph_success(results, type)
        {
            globalFunctions.hide_spinner();
            displayChart(results, type);
        }
        function previewGraph_failure(exception, type)
        {
            globalFunctions.hide_spinner();
            globalFunctions.display_error($get('<%=lblQueryMessage.ClientID%>'), exception);
        }

        function displayChart(dataSource, type)
        {
            //Chek that we have data
            if (!dataSource)
            {
                return;
            }

            var axisPoints = dataSource.XAxis;

            //General options for charts
            config = {
                bindto: d3.select('#c3_customGraphInner')
            };

            if (type == 'bar' || type == 'line')
            {
                //Stacked bar-chart or line graph
                var groups = new Array();
                var categories = new Array();
                var columns = new Array();
                var colors = new Object();
                for (var j = 0; j < dataSource.Series.length; j++)
                {
                    var groupName = dataSource.Series[j].Caption;
                    groups.push(groupName);
                    if (dataSource.Series[j].Color)
                    {
                        colors[groupName] = '#' + dataSource.Series[j].Color;
                    }

                    var column = new Array();
                    column.push(groupName);
                    for (var i = 0; i < axisPoints.length; i++)
                    {
                        var axisPoint = dataSource.XAxis[i];
                        if (dataSource.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id] != undefined)
                        {
                            var category = axisPoint.StringValue;
                            var value = dataSource.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id];
                            //Only add the x-axis categories for the first series
                            if (j == 0)
                            {
                                categories.push(category);
                            }
                            column.push(value);
                        }
                    }
                    columns.push(column);
                }

                if (type == 'line')
                {
                    //C3js Spline graph (sexier than line)
                    config.data = {
                        columns: columns,
                        type: 'spline',
                        colors: colors
                    };
                }
                if (type == 'bar')
                {
                    //C3js Stacked Bar Chart
                    config.data = {
                        columns: columns,
                        type: 'bar',
                        colors: colors,
                        groups: [groups]
                    };
                }
                config.axis = {
                    x: {
                        type: 'category',
                        categories: categories
                    }
                };
                config.grid = {
                    y: {
                        show: true
                    }
                };
            }

            if (type == 'donut')
            {
                //Donut graphs only support a single data-series
                if (dataSource.Series.length != 1)
                {
                    globalFunctions.display_error_message($get('<%=lblQueryMessage.ClientID%>'), resx.Graphs_NeedToHaveASingleDataSeries);
                    return;
                }

                var columns = new Array;
                for (var i = 0; i < axisPoints.length; i++)
                {
                    var axisPoint = dataSource.XAxis[i];
                    if (dataSource.Series[0].Values[globalFunctions.keyPrefix + axisPoint.Id] != undefined)
                    {
                        var column = [axisPoint.StringValue, dataSource.Series[0].Values[globalFunctions.keyPrefix + axisPoint.Id]];
                        columns.push(column);
                    }
                }

                config.data = {
                    columns: columns,
                    type: 'donut'
                };
                config.donut = {
                    label: {
                        format: function (value, ratio, id)
                        {
                            return d3.format('d')(value);
                        }
                    }
                };
            }

            //Show graph
            c3.generate(config);
            $('#c3_customGraph').show();
        }
        function closeDataGrid(e)
        {
            $get('divCustomQueryResults').style.display = 'none';
        }
        function closeGraph(e)
        {
            $get('c3_customGraph').style.display = 'none';
        }
    </script>
</asp:Content>
