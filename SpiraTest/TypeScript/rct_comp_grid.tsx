//external dependendencies (js libraries)
declare var React: any;
declare var ReactDOM: any;

//inflectra services
declare var globalFunctions: any;
declare var SpiraContext: any;
declare var Inflectra: any;
declare var pageCallbacks: any;

// creates a simple table
// props contains meta information about the fields and then the array of entries themselves
// lots of use of objects (as opposed to arrays) to match up the meta information to a specific entry
let ReactGrid = (props) => {
    /*
     * =======================
     * FIRST create the header
     * =======================
     */

    // only render fields not set to hidden
    const visibleColumns = props.meta.filter(x => !x.hidden);
    const headerCells = visibleColumns.map((x, i) => <th key={i}>{x.name}</th>);

    /*
     * ====================
     * Create the data rows
     * ====================
     */
    // start with an array of rows to map through
    let rows = props.data.map((row, index) => {
        let dataCells = [];

        // then loop over each key in the specific object
        visibleColumns.forEach(col => {
            // only render fields not hidden
            if (!col.hidden) {
                var cellContents;
                // dropdowns are handled differently - need to match int saved as the value to the lookup name
                if (col.type) {
                    switch (col.type) {
                        case 'dropdown':
                            cellContents = col.dropdown.filter(item => item.id == row[col.ref])[0].name;
                            break;
                        case 'icon':
                            cellContents = <img src="row[col.ref]" className="w4 h4" />;
                            break;
                        default:
                            cellContents = row[col.ref];
                    }
                    // non dropdowns are rendered as standard text
                } else {
                    cellContents = row[col.ref];
                }
                dataCells.push(<td key={col.ref}>{cellContents}</td>);
            }
        });
        return (<tr key={index}>{dataCells}</tr>);
    });

    return (
        <div className="table-responsive">
            <table className="table table-condensed">
                <thead>
                    <tr>
                        {headerCells}
                    </tr>
                </thead>
                <tbody>
                    {rows}
                </tbody>
            </table>
        </div>
    )
}