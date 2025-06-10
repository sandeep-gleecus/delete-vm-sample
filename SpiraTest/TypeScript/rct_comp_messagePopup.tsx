//external dependendencies (js libraries)
declare var React: any;			//React

//inflectra services
declare var Inflectra: any;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

class RctMessagePopup extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.handleClose = this.handleClose.bind(this);
        this.confirm = this.confirm.bind(this);
    }

    componentDidMount() {
        Mousetrap.bind("escape", (e) => this.handleClose());
    }

    //functions
    handleClose() {
        // if we are in a confirm mode, then pass a false back to the required function
        if (this.props.confirm) {
            this.props.confirm(false, this.props.refs || null);
        }
        //Unmount react component to allow proper reinitialisation
        globalFunctions.dlgGlobalDynamicClear();
        Mousetrap.unbind("escape");
    }
    confirm() {
        if (this.props.confirm) {
            this.props.confirm(true, this.props.refs || null);
        }
        //Unmount react component to allow proper reinitialisation
        globalFunctions.dlgGlobalDynamicClear();
        Mousetrap.unbind("escape");
    }

    render() {
        return (
            <React.Fragment>
                {this.props.isModal && 
                    <div
                        className="DialogBoxModalBackground visibility-none fade-in-50 fixed left0 right0 top0 bottom0"
                        onClick={this.handleClose}
                    ></div>
                }

                <div className="u-popup u-popup_down mw960 min-w9 max-h-insideHead-xs is-open">
                    <span
                        onClick={this.handleClose}
                        className="fas fa-times pointer fr o-70"
                    >
                    </span>

                    {this.props.message && 
                        <div
                            id="divMessagePopupBox"
                            className={"ma4 alert alert-" + (this.props.type || "default")}
                            role="alert"
                            >
                            {this.props.glyphClasses && <span className={this.props.glyphClasses}/>}
                            {this.props.message}
                        </div>
                    }

                    {this.props.children}

                    {this.props.confirm &&
                        <div
                            className="btn-group db ma4"
                            role="group"
                            >
                            <button
                                className="btn btn-primary"
                                onClick={this.confirm}
                                type="button"
                                >
                                {resx.Global_Yes}
                            </button>
                            <button
                                className="btn btn-default"
                                onClick={this.handleClose}
                                type="button"
                                >
                                {resx.Global_Cancel}
                            </button>
                        </div>
                    }
                </div>
            </React.Fragment>
        )
    }
}