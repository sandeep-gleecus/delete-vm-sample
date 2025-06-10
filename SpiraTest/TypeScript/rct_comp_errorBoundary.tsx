//external dependendencies
declare var React: any
var resx = Inflectra.SpiraTest.Web.GlobalResources;

class ErrorBoundary extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.state = { error: null, errorInfo: null };
    }

    componentDidCatch(error, errorInfo) {
        // Catch errors in any components below and re-render with error message
        this.setState({
            error: error,
            errorInfo: errorInfo
        })
        console.log("react error found in the following stack: ", errorInfo.componentStack);
        // You can also log error messages to an error reporting service here
    }

    render() {
        if (this.state.errorInfo) {
            // Error path
            return (
                <div>
                    <h5
                        title={this.state.error && this.state.error.toString()}
                        style={{ backgroundColor: "yellow", color: "red", padding: ".33rem", border: "2px solid red" }}
                    >{resx.Global_Error}!</h5>
                </div>
            );
        }
        // Normally, just render children
        return this.props.children;
    }
}