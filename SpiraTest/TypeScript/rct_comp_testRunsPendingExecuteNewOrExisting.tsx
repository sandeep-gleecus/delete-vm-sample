//external dependendencies (js libraries)
declare var React: any;			//React

//inflectra services
declare var Inflectra: any;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

class RctTestRunsPendingExecuteNewOrExisting extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.ddlTestRunsPending = null;
        this.btnExecute = React.createRef();

        this.startExecuteProcess = this.startExecuteProcess.bind(this);
        this.handleClose = this.handleClose.bind(this);
        this.btnExecuteKeyDown = this.btnExecuteKeyDown.bind(this);
        this.btnCancelKeyDown = this.btnCancelKeyDown.bind(this);
    }
    componentDidMount() {
        // pressing escape will close the popup
        Mousetrap.bind("escape", (e) => this.handleClose(null));

        //Set focus on the execute button
        this.btnExecute.current.focus();

        //Create the ASP.NET AJAX CONTROL
        this.ddlTestRunsPending = $create(
            Inflectra.SpiraTest.Web.ServerControls.DropDownList, {
                multiSelectable: false,
                enabledCssClass: 'u-dropdown is-active',
                disabledCssClass: 'u-dropdown disabled'
            },
            null,
            null,
            document.getElementById('ddlExistingTestRunsPending')
        );

        //Data bind to the ASP.NET AJAX CONTROL
        //Turn the data array into an object for the control to use
        const dataForDdl = this.props.data.reverse().reduce((acc, item, index, arr) => {
            //We have to add a dummy character at the start because the component expects one and deletes it
            return { [globalFunctions.keyPrefix + item.id]: item.name, ...acc };
        }, {});
        this.ddlTestRunsPending.set_dataSource(dataForDdl);
        this.ddlTestRunsPending.dataBind();

        //Add the top item - which is to run a new test
        //set the selection to run a new test - to make the old way of working easier
        this.ddlTestRunsPending.addItem('', resx.TestRun_ExecuteNewTest);
        this.ddlTestRunsPending.set_selectedItem(resx.TestRun_ExecuteNewTest, false);
    }

    //kicks off the execute function passed in via props, then closes the dialog
    startExecuteProcess(e) {
        e.preventDefault();
        //Get the currently selected value from the ASP.NET dropdown and pass it into the function
        const selectedValue = this.ddlTestRunsPending.get_selectedItem().get_value();
        this.props.executeFunction(selectedValue);
        this.handleClose(null);
    }
    btnExecuteKeyDown(e) {
        if (e.key === "Enter") {
            this.startExecuteProcess(e);
        }
    }

    // handles closing the dialog
    handleClose(e) {
        //Destroy any ASP.NET AJAX components
        this.ddlTestRunsPending.dispose();
        delete this.ddlTestRunsPending;

        //Unmount react component to allow proper reinitialisation
        globalFunctions.dlgGlobalDynamicClear();
        Mousetrap.unbind("escape");
    }
    btnCancelKeyDown(e) {
        if (e.key === "Enter") {
            this.handleClose(e);
        }
    }

    render() {
        return (
            <React.Fragment>
                <div
                    className="DialogBoxModalBackground visibility-none fade-in-50 fixed left0 right0 top0 bottom0"
                    onClick={this.handleClose}
                    >
                </div>
                <div className="u-popup u-popup_down mw960 min-w9 max-h-insideHead-xs is-open">
                    <span
                        onClick={this.handleClose}
                        className="fas fa-times pointer fr o-70"
                    >
                    </span>
                    <div
                        id="divMessagePopupBox"
                        className={"ma4 alert alert-info"}
                        role="alert"
                        >
                    {resx.TestRun_ExistingPendingForUseAndTestId}
                    </div>
                    <div className="ma4">
                        <p>{resx.TestRun_ExistingPendingForUseAndTestId_info}</p>
                        <label
                            className="pr2"
                            htmlFor="ddlExistingTestRunsPending"
                            >
                            {resx.TestRun_ChooseTestToRun}:
                        </label>
                        <div id="ddlExistingTestRunsPending" />
                        <div
                            className="btn-group db my4"
                            role="group"
                            >
                            <button
                                className="btn btn-primary"
                                id="btnExistingTestRunsPending"
                                onClick={this.startExecuteProcess}
                                onKeyDown={this.btnExecuteKeyDown}
                                ref={this.btnExecute}
                                type="button"
                                >
                                {resx.TestCaseList_ExecuteTestCase}
                            </button>
                            <button
                                className="btn btn-default"
                                id="btnCancelExistingTestRunsPending"
                                onClick={this.handleClose}
                                onKeyDown={this.btnCancelKeyDown}
                                type="button"
                                >
                                {resx.Global_Cancel}
                            </button>
                        </div>
                    </div>
                </div>
            </React.Fragment>
        )
    }
};