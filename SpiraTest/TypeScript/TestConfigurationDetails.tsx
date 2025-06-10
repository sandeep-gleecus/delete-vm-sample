//TypeScript React file that supports TestConfigurationDetails.aspx

//external dependendencies (js libraries)
declare var React: any;
declare var ReactDOM: any;
declare var $: any;
declare var $find: any;
declare var $create: any;
declare var $get: any;

//inflectra services
declare var globalFunctions: any;
declare var SpiraContext: any;
declare var Inflectra: any;
declare var resx: any;

//global object accessible by the aspx page
var testConfigurationDetails = {
    populateTestConfigurationsPanelId: '' as string,
    grdTestConfigurationsId: '' as string,

    displayPopulatePanel: function (testConfigurationSetId, btnPopulateId)
    {
        ReactDOM.render(
            <PopulateTestConfigurationsBox
                domId={testConfigurationDetails.populateTestConfigurationsPanelId}
                projectId={SpiraContext.ProjectId}
                testConfigurationSetId={testConfigurationSetId}
                btnPopulateId={btnPopulateId}
                grdTestConfigurationsId={testConfigurationDetails.grdTestConfigurationsId}
                />,
            document.getElementById(testConfigurationDetails.populateTestConfigurationsPanelId)
        );
    }
};

//the parent react component that handles all state
class PopulateTestConfigurationsBox extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.state = {
            parameters: [] as Array<any>,
            maxNumberOfRows: 5,
            ddlTestCaseParameter: null as any,
            ddlCustomList: null as any,
            messageBoxId: 'populateTestConfigurations_messageBox' as string
        }
        //Event handling bindings
        this.addParameter = this.addParameter.bind(this);
        this.populateClick = this.populateClick.bind(this);
        this.closeClick = this.closeClick.bind(this);
        this.populateClick = this.populateClick.bind(this);
        this.populate_success = this.populate_success.bind(this);
        this.removeParameter = this.removeParameter.bind(this);
        this.retrieveParameters_success = this.retrieveParameters_success.bind(this);
        this.retrieveCustomLists_success = this.retrieveCustomLists_success.bind(this);
    }

    componentDidMount() {
        //Create any ASP.NET AJAX components
        this.state.ddlTestCaseParameter = $create(
            Inflectra.SpiraTest.Web.ServerControls.DropDownList, {
                multiSelectable:
                    false,
                enabledCssClass: 'u-dropdown is-active',
                disabledCssClass: 'u-dropdown disabled'
            },
            null,
            null,
            document.getElementById('ddlTestCaseParameter')
        );
        this.state.ddlCustomList = $create(
            Inflectra.SpiraTest.Web.ServerControls.DropDownList, {
                multiSelectable: false,
                enabledCssClass: 'u-dropdown is-active',
                disabledCssClass: 'u-dropdown disabled'
            },
            null,
            null,
            document.getElementById('ddlCustomList')
        );

        //Populate the default entries
        this.state.ddlTestCaseParameter.addItem('', resx.Global_PleaseSelect);
        this.state.ddlCustomList.addItem('', resx.Global_PleaseSelect);

        //Load the two data sets using AJAX
        var self = this;
        Inflectra.SpiraTest.Web.Services.Ajax.TestConfigurationService.TestConfiguration_RetrieveParameters(
            this.props.projectId,
            self.retrieveParameters_success,
            self.retrieveParameters_failure);
        Inflectra.SpiraTest.Web.Services.Ajax.TestConfigurationService.TestConfiguration_RetrieveCustomLists(
            this.props.projectId,
            self.retrieveCustomLists_success,
            self.retrieveCustomLists_failure);
    }

    componentWillUnmount() {
        //Destroy any ASP.NET AJAX components
        this.state.ddlTestCaseParameter.dispose();
        this.state.ddlCustomList.dispose();

        delete this.state.ddlTestCaseParameter;
        delete this.state.ddlCustomList;
    }

    //close out the panel and clear react
    closeClick(e) {
        //Re-enable the Populate button
        e.stopPropagation();
        $("#" + this.props.btnPopulateId).removeClass('disabled');

        //Unmount react component to allow proper reinitialisation
        ReactDOM.unmountComponentAtNode(document.getElementById(this.props.domId));
    }

    //Submit the data
    populateClick(e) {
        //Don't allow more than 5 parameters (will create too many entries)
        if (this.state.parameters.length > this.state.maxNumberOfRows)
        {
            alert(resx.TestConfigurationDetails_TooManyParameters);
            return;
        }

        //Warn them that this will delete all their existing configurations
        if (confirm(resx.TestConfigurationDetails_ConfirmPopulation))
        {
            //Actually do the population
            var testParameters = {};
            for (var i = 0; i < this.state.parameters.length; i++)
            {
                var parameterId = this.state.parameters[i].parameterId;
                var customListId = this.state.parameters[i].customListId;
                testParameters[globalFunctions.keyPrefix + parameterId] = customListId;
            }

            globalFunctions.display_spinner();
            var self = this;
            Inflectra.SpiraTest.Web.Services.Ajax.TestConfigurationService.TestConfiguration_Populate(
                this.props.projectId,
                this.props.testConfigurationSetId,
                testParameters,
                self.populate_success,
                self.populate_failure);
        }
    }

    populate_success(data) {
        globalFunctions.hide_spinner();
        $("#" + this.props.btnPopulateId).removeClass('disabled');

        //Unmount react component to allow proper reinitialisation
        ReactDOM.unmountComponentAtNode(document.getElementById(this.props.domId));

        //Reload the test configurations grid
        $find(this.props.grdTestConfigurationsId).load_data();
    }
    populate_failure(exception) {
        globalFunctions.hide_spinner();
        globalFunctions.display_error($get(this.state.messageBoxId), exception);
    }

    closeMessageBox() {
        document.getElementById(this.state.messageBoxId).className = 'alert alert-hidden';
    }

    addParameter() {
        //Get the new values
        var parameterId = this.state.ddlTestCaseParameter.get_selectedItem().get_value();
        var parameterName = this.state.ddlTestCaseParameter.get_selectedItem().get_text();
        var customListId = this.state.ddlCustomList.get_selectedItem().get_value();
        var customListName = this.state.ddlCustomList.get_selectedItem().get_text();

        //Make sure we have a value selected
        if (!parameterId || parameterId == '' || !customListId || customListId == '')
        {
            alert(resx.TestConfigurationDetails_NoParameterOrCustomList);
            return;
        }

        //Make sure there is space to add an extra parameter
        if (this.state.parameters.length === this.state.maxNumberOfRows) {
            alert(resx.TestConfigurationDetails_TooManyParameters);
            return;
        }

        //Make sure the parameter is not already in the list
        var matches = false;
        for (var i = 0; i < this.state.parameters.length; i++)
        {
            if (this.state.parameters[i].parameterId == parameterId)
            {
                matches = true;
            }
        }

        if (!matches)
        {
            var newEntry = {
                parameterId: parameterId,
                parameterName: parameterName,
                customListId: customListId,
                customListName: customListName
            };
            this.setState({
                parameters: this.state.parameters.concat([newEntry])
            });

            //finally we disable the relevant parameter from the dropdown as it can only be used once
            this.state.ddlTestCaseParameter.get_selectedItem()
        }
    }

    removeParameter(index:number) {
        //Remove and reset the state
        var newParameters = JSON.parse(JSON.stringify(this.state.parameters));
        newParameters.splice(this.state.parameters[index], 1);
        this.setState({
            parameters: newParameters
        });
    }

    
    retrieveParameters_success(data) {
        this.state.ddlTestCaseParameter.set_dataSource(data);
        this.state.ddlTestCaseParameter.dataBind();
    }
    retrieveParameters_failure(exception) {
        globalFunctions.display_error($get(this.state.messageBoxId), exception);
    }
    retrieveCustomLists_success(data) {
        this.state.ddlCustomList.set_dataSource(data);
        this.state.ddlCustomList.dataBind();
    }
    retrieveCustomLists_failure(exception) {
        globalFunctions.display_error($get(this.state.messageBoxId), exception);
    }

    render() {
        var saveButtonClasses = (this.state.parameters.length > 0 && this.state.parameters.length <= this.state.maxNumberOfRows) ? "btn btn-primary" : "btn btn-primary disabled"; //only enable the save button if 1+ parameters are entered;
        return (
            <div>
                <div id={this.state.messageBoxId} className="alert alert-hidden" role="alert">
                    <button className="close" type="button" data-hide="alert" aria-label="Close" onClick={this.closeMessageBox}>
                        <span>&times;</span>
                    </button>
                    <span id={this.state.messageBoxId + '_text'}></span>
                </div>
                <div className="bg-off-white ba b-vlight-gray br2 pa4 clearfix">
                    <p className="mb3">
                        {resx.TestConfigurationDetails_PleaseChooseParametersToPopulate}
                    </p>
                    <form className={this.state.parameters.length ? "u-box_3 pb4 mb4 px0 bb b-light-gray" : "u-box_3 mb4 px0"}>
                        <ul className="u-box_list u-box_children-1">
                            <li className="mt0 pa0 mb2">
                                <label htmlFor="ddlTestCaseParameter" className="required pr2">{resx.TestConfiguration_TestCaseParameter}:</label>
                                <div id="ddlTestCaseParameter" />
                            </li>
                            <li className="mt0 pa0 mb2">
                                <label htmlFor="ddlCustomList" className="required pr2">{resx.TestConfiguration_CustomList}:</label>
                                <div id="ddlCustomList" />
                            </li>
                            <li className="mt0 pa0 mb2">
                                <button
                                    type="button"
                                    className="btn btn-sm btn-default ml_u-box-label-md ml_u-box-label-sm ml_u-box-label-xs"
                                    id="btnAddParameter"
                                    onClick={this.addParameter}
                                >
                                    {resx.Global_Add}
                                </button>
                            </li>
                        </ul>
                    </form>
                    <div className="u-box_3">
                        {this.state.parameters.map((parameter, index) =>
                            <ul className="u-box_list u-box_children-1">
                                <li className="mt0 pa0 mb2">
                                    <label className="pr2 u-hidden-md u-hidden-lg">{resx.TestConfiguration_TestCaseParameter}:</label>
                                    <span className="color-inherit">{parameter.parameterName}</span>
                                </li>
                                <li className="mt0 pa0 mb2">
                                    <label className="pr2 u-hidden-md u-hidden-lg">{resx.TestConfiguration_CustomList}:</label>
                                    <span className="color-inherit">{parameter.customListName}</span>
                                </li>
                                <li className="mt0 pa0 mb2">
                                    <button
                                        type="button"
                                        className="btn btn-default btn-sm ml_u-box-label-md ml_u-box-label-sm ml_u-box-label-xs"
                                        onClick={this.removeParameter.bind(null, index)}>
                                        <span className="fas fa-times fa-fw" />
                                        {resx.Global_Remove}
                                    </button>
                                </li>
                            </ul>
                        )}
                    </div>
                    <div className="btn-group mt3">
                        <div
                            className={saveButtonClasses}
                            onClick={this.populateClick}
                        >
                            {resx.Global_Populate}
                        </div>
                        <div
                            className="btn btn-default"
                            onClick={this.closeClick}
                        >
                            {resx.Global_Cancel}
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}
