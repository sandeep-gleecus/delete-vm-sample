declare var React: any;
declare var ReactDOM: any;
declare var c3: any;

class WorkspaceOverallCompletion extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);

        

        this.state = {
            requirementsDone: Math.round(this.props.workspace.percentComplete / 100 * this.props.workspace.requirementsAll),
            thresholds: SpiraContext.dashboard.completionBars.map(x => x.gaugeThreshold / 100 * this.props.workspace.requirementsAll),
            legendActive: this.setLegend(SpiraContext.dashboard.completionBars, this.props.workspace.percentComplete, "gaugeThreshold")
        }
    }

    componentDidMount() {
        

    }


    // returns an object of required result for the legend - the obj where the specified key has the lowest value in the legends array but it still greater than the passed in value
    // param: legends = array of objects
    // param: value = number to check against
    // param: key = key in the object to check against the value
    setLegend(legends, value, key) {
        // first find all matches where the key is greater than the value, then return the object with the lowest key
        const legendsArray = legends
            // first only get the legend values that have a 
            .filter(x => x[key] !== undefined && x[key] > value)
            .reduce((previous, current) => (previous[key] < current[key]) ? previous : current);

        return legendsArray || null;

    }

    render() {
        return (
            <div className="df flex-column">
                {this.props.workspace.requirementsAll > 0 ?
                    <React.Fragment>
                        <div className="flex items-center mb4">
                            <h4 className="di">{resx.Dashboards_Legend_Title}:</h4>
                            {SpiraContext.dashboard.completionBars.filter(x => x.gaugeThreshold !== undefined).map(x => (

                                <div
                                    className="mx3 fs-75 br3 px4 py2 white"
                                    style={{ backgroundColor: x.color }}
                                    >
                                    {x.label}
                                </div>

                            ))}
                        </div>


                        <div className="w9 self-center mb4">
                            <ReactC3Chart
                                data={{
                                    columns: [[this.state.legendActive ? this.state.legendActive.label : "", this.state.requirementsDone]],
                                    type: "gauge",
                                }}
                                color={{
                                    pattern: SpiraContext.dashboard.completionBars.filter(x => x.gaugeThreshold != "undefined").map(x => x.color),
                                    threshold: { values: this.state.thresholds }
                                }}
                                legend={{ show: false }}
                                tooltip={{ show: false }}
                                gauge={{

                                    label: {

                                        show: true,

                                        format: function (value, ratio) {

                                            return ratio ? Math.floor(ratio * 100) + "%" : "";

                                        }

                                    },

                                    min: 0,

                                    max: this.props.workspace.requirementsAll

                                }}
                                />
                        </div>
                    </React.Fragment>
                    :

                    <div

                        className="ma4 alert alert-info"

                        role="alert"

                        >

                        {resx.Global_NoDataToDisplay}

                    </div>

                }

            </div>
        );
    }
}
