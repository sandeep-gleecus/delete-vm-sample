/* ASP.NET AJAX wrapper class for the C3/D3 JS library */
var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.JqPlot = function (element) {
    Inflectra.SpiraTest.Web.ServerControls.JqPlot.initializeBase(this, [element]);

    this._webServiceClass = null;
    this._projectId = -1;
    this._themeFolder = '';
    this._graphHeight = '';
    this._graphWidth = '';
    this._dataGridCssClass = '';
    this._title = '';
    this._errorMessageControlId = '';
    this._dataSource = null;
    this._autoLoad = true;
    this._graphDiv = null;
    this._dataGridDiv = null;
    this._dataGridDialog = null;
    this._innerDiv = null;
    this._chart = null;
    this._graphId = -1;
    this._graphType = -1;
    this._artifactTypeId = -1;
    this._filters = null;
    this._dateRange = '';
    this._webPartUniqueId = '';
    this._iframe = null;
    this._downloadUrl = '';
    this._baseUrl;
    this._xAxisField = '';
    this._groupByField = '';
    this._dateFormat = 'm/d/Y';
    this._customGraphType = null;
    this._customGraphId = null;

    //Graph types
    this._graphType_DateRangeGraphs = 1;
    this._graphType_SummaryGraphs = 2;
    this._graphType_SnapshotGraphs = 3;
    this._graphType_CustomGraphs = 4;

    //Series types (snapshot charts only)
    this._seriesType_Bar = 1;
    this._seriesType_Line = 2;
    this._seriesType_CumulativeBar = 3;

    //Custom graph types
    this.customGraphTypeEnum = {
        bar: 0,
        line: 1,
        donut: 2
    };

    //handler references
    this._clickElements = new Array();
    this._clickHandlers = new Array();

    //Datetime constants
    this._ppcMN = new Array(resx.DatePicker_January, resx.DatePicker_February, resx.DatePicker_March, resx.DatePicker_April, resx.DatePicker_May, resx.DatePicker_June, resx.DatePicker_July, resx.DatePicker_August, resx.DatePicker_September, resx.DatePicker_October, resx.DatePicker_November, resx.DatePicker_December);
}
Inflectra.SpiraTest.Web.ServerControls.JqPlot.prototype =
{
    initialize: function () {
        Inflectra.SpiraTest.Web.ServerControls.JqPlot.callBaseMethod(this, 'initialize');

        //Create any container items
        this.createChildControls();

        //Now load in the data if autoload set
        if (this._autoLoad) {
            this.load_data();
        }

        //Make sure that the chart resizes on window load and resize to ensure responsiveness
        $(window).on("resize", Function.createDelegate(this, this.replotGraph));
    },
    dispose: function () {
        //Click handlers
        for (var i = 0; i < this._clickElements.length; i++) {
            $removeHandler(this._clickElements[i], 'click', this._clickHandlers[i]);
            delete this._clickElements[i];
            delete this._clickHandlers[i];
        }
        delete this._clickElements;
        delete this._clickHandlers;
        delete this._dataGridDialog;

        //Other objects
        delete this._iframe;
        delete this._chart;
        delete this._innerDiv;
        delete this._graphDiv;
        delete this._dataGridDiv;

        Inflectra.SpiraTest.Web.ServerControls.JqPlot.callBaseMethod(this, 'dispose');
    },

    /*  =========================================================
    The properties
    =========================================================  */
    get_element: function () {
        return this._element;
    },

    get_projectId: function () {
        return this._projectId;
    },
    set_projectId: function (value) {
        this._projectId = value;
    },

    get_themeFolder: function () {
        return this._themeFolder;
    },
    set_themeFolder: function (value) {
        this._themeFolder = value;
    },

    get_dateFormat: function () {
        return this._dateFormat;
    },
    set_dateFormat: function (dateFormat) {
        if (this._dateFormat != dateFormat) {
            this._dateFormat = dateFormat;
        }
    },

    get_errorMessageControlId: function () {
        return this._errorMessageControlId;
    },
    set_errorMessageControlId: function (value) {
        this._errorMessageControlId = value;
    },

    get_webServiceClass: function () {
        return this._webServiceClass;
    },
    set_webServiceClass: function (value) {
        this._webServiceClass = value;
    },

    get_title: function () {
        return this._title;
    },
    set_title: function (value) {
        this._title = value;
    },

    get_dataGridCssClass: function () {
        return this._dataGridCssClass;
    },
    set_dataGridCssClass: function (value) {
        this._dataGridCssClass = value;
    },

    get_graphHeight: function () {
        return this._graphHeight;
    },
    set_graphHeight: function (value) {
        this._graphHeight = value;
    },

    get_graphWidth: function () {
        return this._graphWidth;
    },
    set_graphWidth: function (value) {
        this._graphWidth = value;
    },

    get_graphId: function () {
        return this._graphId;
    },
    set_graphId: function (value, reload) {
        this._graphId = value;
        //Need to reload?
        if (reload) {
            this.load_data();
        }
    },

    get_graphType: function () {
        return this._graphType;
    },
    set_graphType: function (value) {
        this._graphType = value;
    },

    get_artifactTypeId: function () {
        return this._artifactTypeId;
    },
    set_artifactTypeId: function (value) {
        this._artifactTypeId = value;
    },

    get_xAxisField: function () {
        return this._xAxisField;
    },
    set_xAxisField: function (value, reload) {
        this._xAxisField = value;
        //Need to reload?
        if (reload) {
            this.load_data();
        }
    },

    get_groupByField: function () {
        return this._groupByField;
    },
    set_groupByField: function (value, reload) {
        this._groupByField = value;
        //Need to reload?
        if (reload) {
            this.load_data();
        }
    },

    get_filters: function () {
        return this._filters;
    },
    set_filters: function (value, reload) {
        this._filters = value;
        //Need to reload?
        if (reload) {
            this.load_data();
        }
    },

    get_dateRange: function () {
        return this._dateRange;
    },
    set_dateRange: function (value, reload) {
        this._dateRange = value;
        //Need to reload?
        if (reload) {
            this.load_data();
        }
    },

    get_webPartUniqueId: function () {
        return this._webPartUniqueId;
    },
    set_webPartUniqueId: function (value) {
        this._webPartUniqueId = value;
    },

    get_downloadUrl: function () {
        return this._downloadUrl;
    },
    set_downloadUrl: function (value) {
        this._downloadUrl = value;
    },

    get_baseUrl: function () {
        return this._baseUrl;
    },
    set_baseUrl: function (value) {
        this._baseUrl = value;
    },

    get_customGraphType: function () {
        return this._customGraphType;
    },
    set_customGraphType: function (value, reload) {
        this._customGraphType = value;
        //Need to reload?
        if (reload) {
            this.load_data();
        }
    },

    get_customGraphId: function () {
        return this._customGraphId;
    },
    set_customGraphId: function (value, reload) {
        this._customGraphId = value;
        //Need to reload?
        if (reload) {
            this.load_data();
        }
    },

    /*  =========================================================
    Public Methods
    =========================================================  */
    createChildControls: function () {
        this._graphDiv = d.ce('div');
        this._graphDiv.className = 'u-chart db';
        this._graphDiv.id = this.get_element().id + '_graph';
        this._graphDiv.style.width = this.get_graphWidth();
        this._graphDiv.style.height = this.get_graphHeight();
        this._dataGridDiv = d.ce('div');
        this.get_element().appendChild(this._graphDiv);
        this.get_element().appendChild(this._dataGridDiv);

        //We now create the link that displays the datagrid
        var divGroup = d.ce('div');
        divGroup.className = 'btn-group';
        divGroup.role = 'group';
        this._dataGridDiv.appendChild(divGroup);

        var a = d.ce('a');
        a.href = 'javascript:void(0)';
        a.className = "btn btn-default";
        a.title = resx.JqPlot_DisplayDataGrid;
        a.appendChild(d.ce('span')).className = 'fas fa-table';
        divGroup.appendChild(a);
        var onDataGridDisplayClickHandler = Function.createDelegate(this, this._onDataGridDisplayClick);
        $addHandler(a, 'click', onDataGridDisplayClickHandler);
        this._clickElements.push(a);
        this._clickHandlers.push(onDataGridDisplayClickHandler);

        divSave = d.ce('div');
        divSave.className = 'btn-group priority3';
        divSave.role = 'group';
        divGroup.appendChild(divSave);

        // create the button for the dropdown to save different image types
        buttonSave = d.ce('button');
        buttonSave.type = 'button';
        buttonSave.className = 'btn btn-default dropdown-toggle';
        buttonSave.setAttribute("data-toggle", "dropdown");
        buttonSave.setAttribute("aria-haspopup", "true");
        buttonSave.setAttribute("aria-expanded", "false");
        buttonSave.title = resx.JqPlot_SaveAs;
        buttonSave.appendChild(d.ce('span')).className = 'far fa-image';
        divSave.appendChild(buttonSave);

        span = d.ce('span');
        span.className = "caret";
        buttonSave.appendChild(span);

        ul = d.ce('ul');
        ul.className = "dropdown-menu";
        divSave.appendChild(ul);

        //JPEG
        liJpeg = d.ce('li');
        a = d.ce('a');
        a.href = 'javascript:void(0)';
        a.appendChild(d.createTextNode('JPEG'));
        liJpeg.appendChild(a);
        var onSaveJpegClickHandler = Function.createCallback(this._onImageSaveClick, { thisRef: this, format: 'JPEG' });
        $addHandler(a, 'click', onSaveJpegClickHandler);
        this._clickElements.push(a);
        this._clickHandlers.push(onSaveJpegClickHandler);
        //BMP
        liBmp = d.ce('li');
        a = d.ce('a');
        a.href = 'javascript:void(0)';
        a.appendChild(d.createTextNode('BMP'));
        liBmp.appendChild(a);
        var onSaveBmpClickHandler = Function.createCallback(this._onImageSaveClick, { thisRef: this, format: 'BMP' });
        $addHandler(a, 'click', onSaveBmpClickHandler);
        this._clickElements.push(a);
        this._clickHandlers.push(onSaveBmpClickHandler);

        //PNG
        liPng = d.ce('li');
        a = d.ce('a');
        a.href = 'javascript:void(0)';
        a.appendChild(d.createTextNode('PNG'));
        liPng.appendChild(a);
        var onSavePngClickHandler = Function.createCallback(this._onImageSaveClick, { thisRef: this, format: 'PNG' });
        $addHandler(a, 'click', onSavePngClickHandler);
        this._clickElements.push(a);
        this._clickHandlers.push(onSavePngClickHandler);

        //add the formats to the dropdown list
        ul.appendChild(liJpeg);
        ul.appendChild(liBmp);
        ul.appendChild(liPng);

        //Create the popup dialog box
        var div = d.ce('div');
        div.className = 'PopupPanel';
        div.style.display = 'none';
        div.style.position = 'absolute';
        div.style.top = '0px';
        var title = (this.get_title()) ? this.get_title() : '\xa0';
        $('body').append(div);

        //Create a scrolling div to hold the table
        this._innerDiv = d.ce('div');
        this._innerDiv.className = "pa4 popup_child-scrollbox scrollbox"
        this._innerDiv.style.overflow = 'auto';
        div.appendChild(this._innerDiv);


        //Add the link to download the data as CSV
        div.appendChild(d.ce('br'));
        a = d.ce('a');
        a.href = 'javascript:void(0)';
        a.className = "btn btn-default";
        a.appendChild(d.ce('span')).className = 'far fa-file-excel';
        a.appendChild(d.createTextNode(' ' + resx.JqPlot_DownloadData));
        div.appendChild(a);

        //Add the click handler
        var onDownloadDataClickHandler = Function.createDelegate(this, this._onDownloadDataClick);
        $addHandler(a, 'click', onDownloadDataClickHandler);
        this._clickElements.push(a);
        this._clickHandlers.push(onDownloadDataClickHandler);

        //Turn the elements into a proper dialog box panel - setting it to modal if viewed on a desktop device
        var isModal = $('body').hasClass('mobile-device');
        this._dataGridDialog = $create(Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel, { themeFolder: this.get_themeFolder(), errorMessageControlId: this.get_errorMessageControlId(), title: title, persistent: true, modal: isModal }, null, null, div);

        //Create the IFRAME that holds the CSV data
        this._iframe = d.ce('iframe');
        this._iframe.style.width = '1px';
        this._iframe.style.height = '1px';
        this._iframe.style.border = 'none';
        this._iframe.frameBorder = 0;
        div.appendChild(this._iframe);
    },

    replotGraph: function () {
        if (this._chart) {
            this._chart.flush();
        }
    },

    load_data: function () {
        //Load the data from the web service
        var webService = this.get_webServiceClass();
        if (this.get_graphType() == this._graphType_DateRangeGraphs) {
            globalFunctions.display_spinner();
            webService.RetrieveDateRange(this.get_projectId(), this.get_graphId(), this.get_dateRange(), this.get_filters(), Function.createDelegate(this, this.load_data_success), Function.createDelegate(this, this.operation_failure));
        }
        if (this.get_graphType() == this._graphType_SummaryGraphs) {
            //For this graph, need to make sure we have the two fields chosen
            var xAxisField = this.get_xAxisField();
            var groupByField = this.get_groupByField();
            if (xAxisField && xAxisField != '' && groupByField && groupByField != '') {
                globalFunctions.display_spinner();
                webService.RetrieveSummary(this.get_projectId(), this.get_artifactTypeId(), xAxisField, groupByField, Function.createDelegate(this, this.load_data_success), Function.createDelegate(this, this.operation_failure));
            }
        }
        if (this.get_graphType() == this._graphType_SnapshotGraphs) {
            $(this._graphDiv).addClass('chart-snapshot');
            globalFunctions.display_spinner();
            webService.RetrieveSnapshot(this.get_projectId(), this.get_graphId(), this.get_filters(), Function.createDelegate(this, this.load_data_success), Function.createDelegate(this, this.operation_failure));
        }
        if (this.get_graphType() == this._graphType_CustomGraphs) {
            globalFunctions.display_spinner();
            if (this.get_customGraphId() && this.get_customGraphId() > 0) {
                webService.CustomGraph_Retrieve(this.get_projectId(), this.get_customGraphId(), Function.createDelegate(this, this.load_data_success), Function.createDelegate(this, this.operation_failure));
            }
        }
    },
    load_data_success: function (dataSource) {
        //Create the plot using the data
        globalFunctions.hide_spinner();
        this.createDataGrid(dataSource);
        this.createChart(dataSource);
    },

    createDataGrid: function (dataSource) {
        //If the data is null, just clear the current grid
        if (!dataSource) {
            this._clearContent(this._innerDiv);
            return;
        }

        //If this is a custom graph, change the title
        if (this.get_graphType() == this._graphType_CustomGraphs) {
            this.set_title(dataSource.options);
        }

        //Create a new table to hold the datagrid
        var table = d.ce('table');
        table.style.width = '100%';
        var thead = d.ce('thead');
        table.appendChild(thead);
        table.className = this.get_dataGridCssClass() + " priority1";
        //The graph title
        var tr = d.ce('tr');
        tr.className = 'SubHeader';
        thead.appendChild(tr);
        var th = d.ce('th');
        tr.appendChild(th);
        th.colSpan = dataSource.Series.length + 1;
        th.style.textAlign = 'center';
        th.appendChild(d.createTextNode(this.get_title()));

        //The column headings
        tr = d.ce('tr');
        tr.className = 'SubHeader';
        thead.appendChild(tr);
        //Add the name of the x-axis
        th = d.ce('th');
        tr.appendChild(th);
        th.appendChild(d.createTextNode(dataSource.XAxisCaption));

        //Get the data series names
        for (var j = 0; j < dataSource.Series.length; j++) {
            var series = dataSource.Series[j];
            th = d.ce('th');
            tr.appendChild(th);
            th.appendChild(d.createTextNode(series.Caption));
        }

        //Now the data-rows
        var tbody = d.ce('tbody');
        table.appendChild(tbody);
        for (var i = 0; i < dataSource.XAxis.length; i++) {
            var axisPoint = dataSource.XAxis[i];
            var tr = d.ce('tr');
            tbody.appendChild(tr);
            //The axis name
            var td = d.ce('td');
            tr.appendChild(td);
            td.appendChild(d.createTextNode(axisPoint.StringValue));

            //The data series
            for (var j = 0; j < dataSource.Series.length; j++) {
                var series = dataSource.Series[j];
                td = d.ce('td');
                tr.appendChild(td);
                var value = series.Values[globalFunctions.keyPrefix + axisPoint.Id];
                if (value != undefined) {
                    td.appendChild(d.createTextNode(value + ''));
                }
            }
        }

        //Add the data grid to the dialog box inner div
        this._clearContent(this._innerDiv);
        this._innerDiv.appendChild(table);
    },

    createChart: function (dataSource) {
        //If the data is null, just clear the current graph
        if (!dataSource) {
            this._clearContent(this._graphDiv);
            this._chart = null;
            return;
        }

        //The actual plot is generated by the C3/D3 code, so we just pass the options to that
        var id = this._graphDiv.id;
        var axisPoints = dataSource.XAxis;
        var title = this.get_title();
        var minXValue, maxXValue;
        var maxYValue;

        //General options for charts
        config = {
            bindto: d3.select('#' + id)
        };

        //The different types of graph need to be rendered separately
        if (this.get_graphType() == this._graphType_DateRangeGraphs) {
            var groups = new Array();
            var categories = new Array();
            var columns = new Array();
            var colors = new Object();
            var order = 'desc';

            //First the axis column
            var column = new Array();
            column.push('x');
            for (var i = 0; i < axisPoints.length; i++) {
                var date = new Date(parseInt(dataSource.XAxis[i].DateValue.substr(6)));
                column.push(date);
            }
            columns.push(column);

            //Next the value columns
            for (var j = 0; j < dataSource.Series.length; j++) {
                var groupName = dataSource.Series[j].Caption;
                if (dataSource.options && dataSource.options == 'stacked') {
                    order = null;   //Don't order by value
                    groups.push(groupName);
                }
                if (dataSource.Series[j].Color) {
                    colors[groupName] = '#' + dataSource.Series[j].Color;
                }

                var column = new Array();
                column.push(groupName);
                for (var i = 0; i < axisPoints.length; i++) {
                    var axisPoint = dataSource.XAxis[i];
                    var date = new Date(parseInt(axisPoint.DateValue.substr(6)));
                    if (!minXValue && i == 0) {
                        minXValue = date;
                    }
                    if (!maxXValue && i == (axisPoints.length - 1)) {
                        maxXValue = date;
                    }
                    if (dataSource.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id] != undefined) {
                        var value = dataSource.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id];
                        if (!maxYValue || value > maxYValue) {
                            maxYValue = value;
                        }
                        column.push(value);
                    }
                }
                columns.push(column);
            }

            //C3js Area-Spline Chart
            config.data = {
                x: 'x',
                columns: columns,
                type: 'area-spline',
                colors: colors,
                groups: [groups],
                order: order
            };
            config.axis = {
                x: {
                    type: 'timeseries',
                    tick: {
                        format: function (x) {
                            var year = x.getFullYear();
                            var month = x.getMonth();
                            var day = x.getDate();
                            return this.dateFormat(year, month, day);
                        }.bind(this)
                    }
                }
            };
            config.grid = {
                y: {
                    show: true
                }
            };
        }
        if (this.get_graphType() == this._graphType_SummaryGraphs) {
            var groups = new Array();
            var categories = new Array();
            var columns = new Array();
            var colors = new Object();
            for (var j = 0; j < dataSource.Series.length; j++) {
                var groupName = dataSource.Series[j].Caption;
                groups.push(groupName);
                if (dataSource.Series[j].Color) {
                    colors[groupName] = '#' + dataSource.Series[j].Color;
                }

                var column = new Array();
                column.push(groupName);
                for (var i = 0; i < axisPoints.length; i++) {
                    var axisPoint = dataSource.XAxis[i];
                    if (dataSource.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id] != undefined) {
                        var category = axisPoint.StringValue;
                        var value = dataSource.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id];
                        //Only add the x-axis categories for the first series
                        if (j == 0) {
                            categories.push(category);
                        }
                        column.push(value);
                    }
                }
                columns.push(column);
            }

            //C3js Stacked Bar Chart
            config.data = {
                columns: columns,
                type: 'bar',
                colors: colors,
                groups: [groups]
            };
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
        if (this.get_graphType() == this._graphType_SnapshotGraphs) {
            var groups = new Array();
            var categories = new Array();
            var columns = new Array();
            var colors = new Object();
            var types = new Object();
            for (var j = 0; j < dataSource.Series.length; j++) {
                var groupName = dataSource.Series[j].Caption;
                if (dataSource.Series[j].Color) {
                    colors[groupName] = '#' + dataSource.Series[j].Color;
                }

                //See if we have a bar or line renderer to override the default
                if (dataSource.Series[j].Type) {
                    if (dataSource.Series[j].Type == this._seriesType_Bar) {
                        groups.push(groupName);
                    }
                    if (dataSource.Series[j].Type == this._seriesType_CumulativeBar) {
                        groups.push(groupName);
                    }
                    if (dataSource.Series[j].Type == this._seriesType_Line) {
                        //Override type to Line
                        types[groupName] = 'line';
                    }
                }

                var column = new Array();
                column.push(groupName);
                for (var i = 0; i < axisPoints.length; i++) {
                    var axisPoint = dataSource.XAxis[i];
                    if (dataSource.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id] != undefined) {
                        var category = axisPoint.StringValue;
                        var value = dataSource.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id];
                        //Only add the x-axis categories for the first series
                        if (j == 0) {
                            categories.push(category);
                        }
                        //If this series is a cumulative bar, need to subtract the total of other stacked bars so far
                        //So that it's relative to the existing stacked bars
                        if (dataSource.Series[j].Type == this._seriesType_CumulativeBar) {
                            for (var k = j - 1; k >= 0; k--) {
                                var itemValue = dataSource.Series[k].Values[globalFunctions.keyPrefix + axisPoint.Id];;
                                value -= itemValue;
                                //If we hit a cumulative bar, don't subtract any further bars,
                                //but check to make sure that the cumulative bar was not exceeded by the constituent items
                                if (dataSource.Series[k].Type == this._seriesType_CumulativeBar) {
                                    var childTotal = 0;
                                    for (var m = k - 1; m >= 0; m--) {
                                        childTotal += dataSource.Series[m].Values[globalFunctions.keyPrefix + axisPoint.Id];
                                    }
                                    if (childTotal > itemValue) {
                                        var delta = childTotal - itemValue;
                                        value -= delta;
                                    }
                                    break;
                                }
                            }

                            //Bars can never be negative (lines can)
                            if (dataSource.Series[j].Type == this._seriesType_CumulativeBar && value < 0) {
                                value = 0;
                            }
                        }
                        column.push(value);
                    }
                }
                columns.push(column);
            }

            //C3js Stacked Bar Chart
            config.data = {
                columns: columns,
                type: 'bar',
                types: types,
                colors: colors,
                groups: [groups]
            };
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
        if (this.get_graphType() == this._graphType_CustomGraphs) {
            var axisPoints = dataSource.XAxis;

            //See what type of custom graph we have
            if (this._customGraphType == this.customGraphTypeEnum.bar || this._customGraphType == this.customGraphTypeEnum.line) {
                //Stacked bar-chart or line graph
                var groups = new Array();
                var categories = new Array();
                var columns = new Array();
                var colors = new Object();
                for (var j = 0; j < dataSource.Series.length; j++) {
                    var groupName = dataSource.Series[j].Caption;
                    groups.push(groupName);
                    if (dataSource.Series[j].Color) {
                        colors[groupName] = '#' + dataSource.Series[j].Color;
                    }

                    var column = new Array();
                    column.push(groupName);
                    for (var i = 0; i < axisPoints.length; i++) {
                        var axisPoint = dataSource.XAxis[i];
                        if (dataSource.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id] != undefined) {
                            var category = axisPoint.StringValue;
                            var value = dataSource.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id];
                            //Only add the x-axis categories for the first series
                            if (j == 0) {
                                categories.push(category);
                            }
                            column.push(value);
                        }
                    }
                    columns.push(column);
                }

                if (this._customGraphType == this.customGraphTypeEnum.line) {
                    //C3js Spline graph (sexier than line)
                    config.data = {
                        columns: columns,
                        type: 'spline',
                        colors: colors
                    };
                }
                if (this._customGraphType == this.customGraphTypeEnum.bar) {
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

            if (this._customGraphType == this.customGraphTypeEnum.donut) {
                //Donut graphs only support a single data-series
                if (dataSource.Series.length != 1) {
                    var messageBox = document.getElementById(this.get_errorMessageControlId());
                    globalFunctions.display_error_message(messageBox, resx.Graphs_NeedToHaveASingleDataSeries);
                    return;
                }

                var columns = new Array;
                for (var i = 0; i < axisPoints.length; i++) {
                    var axisPoint = dataSource.XAxis[i];
                    if (dataSource.Series[0].Values[globalFunctions.keyPrefix + axisPoint.Id] != undefined) {
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
                        format: function (value, ratio, id) {
                            return d3.format('d')(value);
                        }
                    }
                };
            }
        }

        if (this._chart) {
            this._clearContent(this._graphDiv);
            this._chart = null;
        }
        this._chart = c3.generate(config);

        //Force a replot to ensure it scales correctly
        this.replotGraph();
    },

    dateFormat: function (year, month, date) {
        var crt = "";
        var str = "";
        var chars = this._dateFormat.length;
        for (var i = 0; i < chars; ++i) {
            crt = this._dateFormat.charAt(i);
            switch (crt) {
                case "M": str += this._ppcMN[month]; break;
                case "m": str += (month < 9) ? ("0" + (++month)) : ++month; break;
                case "Y": str += year; break;
                case "y": str += year.substring(2); break;
                case "d": str += ((this._dateFormat.indexOf("m") != -1) && (date < 10)) ? ("0" + date) : date; break;
                default: str += crt;
            }
        }
        return unescape(str);
    },

    update_settings: function () {
        //Updates the graph settings on the server-side using the WebParts personalization framework
        //The property names need to match the properties stored on the graph WebPart user controls
        var settings = {};
        //The settings varies by the type of graph
        if (this.get_graphType() == this._graphType_DateRangeGraphs) {
            settings.SelectedGraph = globalFunctions.serializeValueInt(this.get_graphId());
            settings.SelectedDateRange = globalFunctions.serializeValueDateRange(this.get_dateRange());
        }
        if (this.get_graphType() == this._graphType_CustomGraphs) {
            if (this.get_customGraphId()) {
                settings.GraphId = globalFunctions.serializeValueInt(this.get_customGraphId());
            }
            settings.GraphType = globalFunctions.serializeValueInt(this.get_customGraphType());
        }
        if (this.get_graphType() == this._graphType_SummaryGraphs) {
            settings.XAxisField = globalFunctions.serializeValueString(this.get_xAxisField());
            settings.GroupByField = globalFunctions.serializeValueString(this.get_groupByField());
        }
        if (this.get_graphType() == this._graphType_SnapshotGraphs) {
            settings.SelectedGraph = globalFunctions.serializeValueInt(this.get_graphId());
        }

        //See if we have any known filters to handle
        for (var filter in this._filters)
        {
            if (this._filters[filter])
            {
                //It's already serialized
                settings[filter] = this._filters[filter];
            }
        }

        var webService = this.get_webServiceClass();
        webService.UpdateSettings(this.get_projectId(), this.get_webPartUniqueId(), settings, null, null);
    },

    operation_failure: function (exception) {
        //Populate the error message control if we have one (if not use alert instead)
        globalFunctions.hide_spinner();
        //Display validation exceptions in a friendly manner
        var messageBox = document.getElementById(this.get_errorMessageControlId());
        globalFunctions.display_error(messageBox, exception);
    },
    display_error: function (message) {
        //If we have a display element, use that otherwise revert to an alert
        globalFunctions.display_error_message($get(this.get_errorMessageControlId()), message);
    },
    display_info: function (message) {
        //If we have a display element, use that otherwise revert to an alert
        globalFunctions.display_info_message($get(this.get_errorMessageControlId()), message);
    },

    /* Private Methods */
    _clearContent: function (element) {
        if (element.firstChild) {
            while (element.firstChild) {
                element.removeChild(element.firstChild);
            }
        }
    },

    /* Event Handlers */
    _onDataGridDisplayClick: function (evt) {
        //Display the dialog box
        this._dataGridDialog.display(evt);
    },
    _onDownloadDataClick: function (evt) {
        //Download into an IFRAME
        var iframe = this._iframe;
        var url = this._downloadUrl.replace(globalFunctions.projectIdToken, this.get_projectId());
        url = url.replace(globalFunctions.artifactIdToken, this.get_graphId());

        //Get the current culture info
        var cultureName = Sys.CultureInfo.CurrentCulture.name;

        //Pass the appropriate parameters for this type of graph
        if (this.get_graphType() == this._graphType_DateRangeGraphs) {
            url += '?dateRange=' + this.get_dateRange();
            url += '&cultureName=' + cultureName;
            //Handle any filters
            for (var filter in this.get_filters()) {
                //remove the key prefix
                if (filter[0] == globalFunctions.keyPrefix) {
                    url += '&' + filter.substr(1) + '=' + this.get_filters()[filter];
                }
                else {
                    url += '&' + filter + '=' + this.get_filters()[filter];
                }
            }
            iframe.src = url;
        }
        if (this.get_graphType() == this._graphType_SnapshotGraphs) {
            url += '?cultureName=' + cultureName;
            //Handle any filters
            for (var filter in this.get_filters()) {
                //remove the key prefix
                if (filter[0] == globalFunctions.keyPrefix) {
                    url += '&' + filter.substr(1) + '=' + this.get_filters()[filter];
                }
                else {
                    url += '&' + filter + '=' + this.get_filters()[filter];
                }
            }
            iframe.src = url;
        }
        if (this.get_graphType() == this._graphType_SummaryGraphs) {
            url += '?artifactTypeId=' + this.get_artifactTypeId();
            url += '&groupByField=' + this.get_groupByField();
            url += '&xAxisField=' + this.get_xAxisField();
            url += '&cultureName=' + cultureName;
            iframe.src = url;
        }
        if (this.get_graphType() == this._graphType_CustomGraphs) {
            url += '?cultureName=' + cultureName;
            url += '&customGraphId=' + this.get_customGraphId();
            iframe.src = url;
        }
    },
    _onImageSaveClick: function (evt, args) {
        var format = args.format;
        var thisRef = args.thisRef;

        //Get the graph in canvas format and send to the web page that
        //then reloads it in a new browser window
        var id = thisRef.get_element().id + '_graph';
        var canvas = d.ce("canvas");
        canvas.width = $("#" + id).width();
        canvas.height = $("#" + id).height();

        //use d3 to tidy up the SVG so that we don't get weird black fills messing up the image
		//from here: https://stackoverflow.com/questions/37701361/exporting-c3-js-line-charts-to-png-images-does-not-work
		d3.select("#" + id + " svg").selectAll("path").attr("fill", "none");
		//fix no axes
		d3.select("#" + id + " svg").selectAll("path.domain").attr("stroke", "black");
		//fix no tick
		d3.select("#" + id + " svg").selectAll(".tick line").attr("stroke", "black");

		//Get the SVG markup from C3 and write to the new Canvas
		var svg = $("#" + id).find('svg')[0].outerHTML;
        
        if (!svg) {
            //Other, older browsers
            alert(resx.JqPlot_ImageExportNotSupport);
            return;
        }

		canvg(canvas, svg, { log: true });

        //Draw a white background behind
        var context = canvas.getContext("2d"); // returns the 2d context object
        context.globalCompositeOperation = 'destination-over';
        context.fillStyle = "#ffffff"; // sets color
        context.fillRect(0, 0, canvas.width, canvas.height); // sets top left location points x,y and then width and height

        //Now we need to convert the canvas to an image file
        var url;
        var canvasData;
        if (format == 'JPEG') {
            url = thisRef.get_baseUrl() + 'JqPlot/GraphImageJpeg.ashx';
            canvasData = Canvas2Image.saveAsJPEG(canvas);
        }
        if (format == 'BMP') {
            url = thisRef.get_baseUrl() + 'JqPlot/GraphImageBmp.ashx';
            canvasData = Canvas2Image.saveAsBMP(canvas);
        }
        if (format == 'PNG') {
            url = thisRef.get_baseUrl() + 'JqPlot/GraphImagePng.ashx';
            canvasData = Canvas2Image.saveAsPNG(canvas);
        }
        if (url && canvasData) {
            var ajax = new XMLHttpRequest();
            ajax.open("POST", url, true);
            ajax.setRequestHeader('Content-Type', 'canvas/upload');
            ajax.onreadystatechange = function () {
                if (ajax.readyState == 4) {
                    //Call the same URL, but this time, pass the guid for the file
                    window.location.href = url + "?guid=" + ajax.responseText;
                }
            }
            ajax.send(canvasData);
        }
    }
}
Inflectra.SpiraTest.Web.ServerControls.JqPlot.registerClass('Inflectra.SpiraTest.Web.ServerControls.JqPlot', Sys.UI.Control);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();

// attaches a handler that is fired first after web service is returned, before specific handler is called   
// specifically, if the web service errored because of authentication failure (auth cookie expired)   
// then redirect the page to the login page.  
Sys.Net.WebRequestManager.add_completedRequest(On_WebRequestCompleted);

function On_WebRequestCompleted(sender, eventArgs) {
    if (sender.get_statusCode() === 500) {
        if (sender.get_object().Message === "Authentication failed.") {
            window.location.href = window.location.protocol + "//" + window.location.host + window.location.pathname.substring(0, window.location.pathname.indexOf('/', 1)) + '/Login.aspx?ReturnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
        }
    }
}
