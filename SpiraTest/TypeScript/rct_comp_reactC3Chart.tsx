declare var React: any;
declare var ReactDOM: any;
declare var c3: any;

class ReactC3Chart extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);

        this.chart;
    }
    componentDidMount() {
        this._loadChart();
    }
    componentDidUpdate(prevProps) {
        if (this.props.dataSource !== prevProps.dataSource) {
            this._updateChart(this.props.data.columns);
        } else {
            this._loadChart();
        }
    }


    _loadChart() {
        if (this.props.data.columns && this.props.data.type) {
            this.chart = c3.generate({
                bindto: ReactDOM.findDOMNode(this),
                axis: this.props.axis,
                bar: this.props.bar,
                color: this.props.color,
                data: this.props.data,
                donut: this.props.donut,
                gauge: this.props.gauge,
                legend: this.props.legend,
                tooltip: this.props.tooltip,
                size: this.props.size,
            });
        }
    }
    _updateChart(data) {
        if (data) {
            this.chart.load({
                columns: data
            })
        }
    }
    render() {
        return <div className={this.props.className} />;
    }
}