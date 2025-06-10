declare var React: any;
declare var ReactDOM: any;
declare var c3: any;
declare var gantt: any;

class WorkspaceRelativeSize extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.LEGEND_HIDE_ABOVE_COUNT = 10;

        this.state = {
            childrenRQCounts: null,
        }
    }

    componentDidMount() {
        this.childrenRQCountsSet();
    }

    // each workspace type has a specific child workspace type. The 'biggest' workspace available is the one that represents the children of the workspace we are on the dashboard for
    childrenRQCountsSet() {
        let countArray = null;
        // start from the top of the tree and go down each level in turn, if the one above does not exist.
        // this function could be rewritten as a switch statement based on the workspace type, but should still check that the right array exists (ie that a portfolio has programs returned)
        if (this.props.portfolios && this.props.portfolios.length) {
            countArray = this.props.portfolios.map(x => [`${x.workspaceName} (ID:${x.workspaceId})`, x.requirementsAll]);
        } else if (this.props.programs && this.props.programs.length) {
            countArray = this.props.programs.map(x => [`${x.workspaceName} (ID:${x.workspaceId})`, x.requirementsAll]);
        } else if (this.props.products && this.props.products.length) {
            countArray = this.props.products.map(x => [`${x.workspaceName} (ID:${x.workspaceId})`, x.requirementsAll]);
        } else if (this.props.releases && this.props.releases.length) {
            countArray = this.props.releases.map(x => [`${x.workspaceName} (ID:${x.workspaceId})`, x.requirementsAll]);

            // if optionally showing sprints even when there are releases, add them here
            if (this.props.showSprints && this.props.sprints && this.props.sprints.length) {
                const sprintsArray = this.props.sprints.map(x => [`${x.workspaceName} (ID:${x.workspaceId})`, x.requirementsAll]);
                countArray.push(...sprintsArray);
            }

        } else if (this.props.sprints && this.props.sprints.length) {
            countArray = this.props.sprints.map(x => [`${x.workspaceName} (ID:${x.workspaceId})`, x.requirementsAll]);
        }
        
        if (countArray) {
            // only show the items that have a requirement count greater than zero - this makes the legend easier to read
            const filteredArray = countArray.filter(x => x[1] > 0);
            this.setState({ childrenRQCounts: filteredArray });
        }
    }

    render() {
        return (
            <React.Fragment>
                {this.state.childrenRQCounts && this.state.childrenRQCounts.length > 0 ?
                    <div className="mw9 df mt4 mx-auto">
                        <ReactC3Chart
                            data={{
                                columns: this.state.childrenRQCounts,
                                type: "donut",
                            }}
                            donut={{
                                label: {
                                    format: (value, ratio, id) => value
                                }
                            }}
                            legend={{
                                hide: this.state.childrenRQCounts.length > this.LEGEND_HIDE_ABOVE_COUNT
                            }}
                        />
                    </div>
                    :
                    <div
                        className="ma4 alert alert-info"
                        role="alert"
                        >
                        {resx.Global_NoDataToDisplay}
                    </div>
                }
            </React.Fragment>
        );
    }
}
