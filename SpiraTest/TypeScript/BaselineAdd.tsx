//external dependendencies (js libraries)
declare var React: any;			//React

//inflectra services
declare var Inflectra: any;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

class RctBaselineAdd extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);

        this.state = {
            name: "",
            description: ""
        }

        this.handleClose = this.handleClose.bind(this);
        this.handleNameChange = this.handleNameChange.bind(this);
        this.handleDescriptionChange = this.handleDescriptionChange.bind(this);
        this.addBaseline = this.addBaseline.bind(this);
        this.addBaselineSuccess = this.addBaselineSuccess.bind(this);
    }

    componentDidMount() {
        //disable the external add baseline button
        document.getElementById(this.props.lnkAddBtnId).classList.add("disabled");
    }

    //functions
    handleClose() {
        if (document.getElementById(this.props.domId)) {
            ReactDOM.unmountComponentAtNode(document.getElementById(this.props.domId));
        }
        // stop disabling the add baseline button
        document.getElementById(this.props.lnkAddBtnId).classList.remove("disabled");
    }
    handleNameChange(event) {
        this.setState({ name: event.target.value });
    }
    handleDescriptionChange(event) {
        this.setState({ description: event.target.value });
    }
    addBaseline(event) {
        if (this.state.name.length > 0) {
            Inflectra.SpiraTest.Web.Services.Ajax.BaselineService.Baseline_Create(
                SpiraContext.ProjectId,
                SpiraContext.ArtifactId,
                this.state.name,
                this.state.description,
                this.addBaselineSuccess,
                this.addBaselineFailure
            )
        }
    }

    addBaselineSuccess() {
        //refresh the grid, then close the popup
        $find(this.props.gridId).load_data();
        this.handleClose();
    }
    addBaselineFailure() {

    }

    render() {
        return (
            <div className="well clearfix">
                <span
                    onClick={this.handleClose}
                    className="fas fa-times pointer fr o-70"
                >
                </span>
                <div className="u-box_3 w-100 mw720">
                    <ul className="u-box_list pl0">
                        <li className="ma0 mb4 pa0">
                            <label className="fw-b required">
                                {resx.Global_Name}:
                            </label>
                            <input
                                type="text"
                                className="u-input is-active"
                                value={this.state.name}
                                onChange={this.handleNameChange}
                            />
        				</li>
                        <li class="ma0 mb4 pa0">
                            <label className="fw-b">
                                {resx.Global_Description}:
                            </label>
                            <textarea
                                type="text"
                                className="u-input is-active"
                                value={this.state.description}
                                onChange={this.handleDescriptionChange}
                                />
                        </li>
                        <li className="mb4 pa0 ml_u-box-label">
                            <div
                                className="btn-group db"
                                role="group"
                                >
                                <button
                                    className="btn btn-primary"
                                    onClick={this.addBaseline}
                                    disabled={this.state.name.length === 0}
                                    type="button"
                                    >
                                    {this.state.name.length === 0 ? resx.Global_NameRequired : resx.Global_Add}
                                </button>
                                <button
                                    className="btn btn-default"
                                    onClick={this.handleClose}
                                    type="button"
                                    >
                                    {resx.Global_Cancel}
                                </button>
                            </div>
                        </li>
                    </ul>
                </div>
            </div>
        )
    }
}