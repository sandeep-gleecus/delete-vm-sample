declare var React: any;
declare var ReactDOM: any;

class ReactGantt extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);

        //Setting available scales
        this.scaleConfigs = [
            // minutes
            {
                scales: [
                    { subscale_unit: "minute", unit: "hour", step: 1, format: "%H" },
                    { unit: "minute", step: 1, format: "%H:%i" }
                ]
            },
            // hours
            {
                scales: [
                    { subscale_unit: "hour", unit: "day", step: 1, format: "%j %M" },
                    { unit: "hour", step: 1, format: "%H:%i" }

                ]
            },
            // days
            {
                scales: [
                    { subscale_unit: "day", unit: "month", step: 1, format: "%F" },
                    { unit: "day", step: 1, format: "%j" }
                ]
            },
            // weeks
            {
                scales: [
                    { subscale_unit: "week", unit: "month", step: 1, date: "%F" },
                    {
                        unit: "week", step: 1, template: function (date) {
                            var dateToStr = gantt.date.date_to_str("%d %M");
                            var endDate = gantt.date.add(gantt.date.add(date, 1, "week"), -1, "day");
                            return dateToStr(date) + " - " + dateToStr(endDate);
                        }
                    }
                ]
            },
            // months
            {
                scales: [
                    { subscale_unit: "month", unit: "year", step: 1, format: "%Y" },
                    { unit: "month", step: 1, format: "%M" }
                ]
            },
            // quarters
            {
                scales: [
                    { subscale_unit: "month", unit: "year", step: 3, format: "%Y" },
                    {
                        unit: "month", step: 3, template: function (date) {
                            var dateToStr = gantt.date.date_to_str("%M");
                            var endDate = gantt.date.add(gantt.date.add(date, 3, "month"), -1, "day");
                            return dateToStr(date) + " - " + dateToStr(endDate);
                        }
                    }
                ]
            },
            // years
            {
                scales: [
                    { subscale_unit: "year", unit: "year", step: 1, date: "%Y" },
                    {
                        unit: "year", step: 5, template: function (date) {
                            var dateToStr = gantt.date.date_to_str("%Y");
                            var endDate = gantt.date.add(gantt.date.add(date, 5, "year"), -1, "day");
                            return dateToStr(date) + " - " + dateToStr(endDate);
                        }
                    }
                ]
            },
            // decades
            {
                scales: [
                    {
                        subscale_unit: "year", unit: "year", step: 10, template: function (date) {
                            var dateToStr = gantt.date.date_to_str("%Y");
                            var endDate = gantt.date.add(gantt.date.add(date, 10, "year"), -1, "day");
                            return dateToStr(date) + " - " + dateToStr(endDate);
                        }
                    },
                    {
                        unit: "year", step: 100, template: function (date) {
                            var dateToStr = gantt.date.date_to_str("%Y");
                            var endDate = gantt.date.add(gantt.date.add(date, 100, "year"), -1, "day");
                            return dateToStr(date) + " - " + dateToStr(endDate);
                        }
                    }
                ]
            }
        ]
    }
    componentDidMount() {
        this._updateChart();
    }
    componentDidUpdate() {
        this._updateChart();
    }
    _updateChart() {
        if (this.props.tasks && this.props.tasks.data && this.props.tasks.data.length) {
            gantt.config.xml_date = this.props.config.xml_date;
            gantt.config.readonly = this.props.config.readonly;
            gantt.config.root_id = this.props.config.root_id;
            gantt.config.columns = this.props.config.columns;
            gantt.templates.grid_row_class = this.props.templateGridRowClass;

            if (this.props.isCustomTaskClass) {
                gantt.templates.task_class = (start, end, task) => task.taskBgClass || "";
            }

            gantt.templates.grid_folder = function (item) {
                return "<div class='gantt_tree_icon " + (item.iconClass ? "workspace-" + item.iconClass : (item.$open ? " gantt_folder_open" : "gantt_folder_closed")) + "'></div>";
            };
            gantt.templates.grid_file = function (item) {
                return "<div class='gantt_tree_icon " + (item.iconClass ? "workspace-" + item.iconClass : "gantt_file") + "'></div>";
            };

            gantt.init(this.ganttContainer);
            gantt.parse(this.props.tasks);

            gantt.attachEvent("onBeforeLightbox", function (task_id) {
                return this.props.isLightboxEnabled ? true : false;
            });


            if (this.props.zoomToFit) this.zoomToFit();
        }
    }

    applyConfig(config, dates) {
        if (config.scales[0].date) {
            gantt.templates.date_scale = null;
        }
        else {
            gantt.templates.date_scale = config.scales[0].template;
        }

        gantt.config.scales = config.scales;

        if (dates && dates.start_date && dates.start_date) {
            gantt.config.start_date = gantt.date.add(dates.start_date, -1, config.scales[0].subscale_unit);
            gantt.config.end_date = gantt.date.add(gantt.date[config.scales[0].subscale_unit + "_start"](dates.end_date), 2, config.scales[0].subscale_unit);
        } else {
            gantt.config.start_date = gantt.config.end_date = null;
        }
    }


    zoomToFit() {
        var project = gantt.getSubtaskDates(),
            areaWidth = gantt.$task.offsetWidth;

        for (var i = 0; i < this.scaleConfigs.length; i++) {
            var columnCount = this.getUnitsBetween(project.start_date, project.end_date, this.scaleConfigs[i].scales[0].subscale_unit, this.scaleConfigs[i].scales[0].step);
            if ((columnCount + 2) * gantt.config.min_column_width <= areaWidth) {
                break;
            }
        }

        if (i == this.scaleConfigs.length) {
            i--;
        }

        this.applyConfig(this.scaleConfigs[i], project);
        gantt.render();
    }

    // get number of columns in timeline
    getUnitsBetween(from, to, unit, step) {
        var start = new Date(from),
            end = new Date(to);
        var units = 0;
        while (start.valueOf() < end.valueOf()) {
            units++;
            start = gantt.date.add(start, step, unit);
        }
        return units;
    }

    render() {
        return (
            <div
                ref={(input) => { this.ganttContainer = input }}
                style={{ width: '100%', height: '100%' }}
            ></div>
        );
    }
}