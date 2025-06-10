using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a grid of generic graph/report data, similar to a dataset
    /// </summary>
    public class RemoteTableData
    {
        /// <summary>
        /// The list of columns in the table
        /// </summary>
        public List<RemoteTableColumn> Columns;

        /// <summary>
        /// The list of rows in the table
        /// </summary>
        public List<RemoteTableRow> Rows;
    }

    /// <summary>
    /// Represents a single column in the data table
    /// </summary>
    public class RemoteTableColumn
    {
        /// <summary>
        /// The system name of the column
        /// </summary>
        public string Name;

        /// <summary>
        /// The caption of the column (optional)
        /// </summary>
        public string Caption;

        /// <summary>
        /// The position of the column (starting at 1)
        /// </summary>
        public int Position;

        /// <summary>
        /// The type of data this column holds (short name)
        /// </summary>
        public string Type;

        /// <summary>
        /// The type of data this column holds (long name)
        /// </summary>
        public string TypeNameSpace;
    }

    /// <summary>
    /// Represents a single row in the data table
    /// </summary>
    public class RemoteTableRow
    {
        /// <summary>
        /// The number of the row in the table
        /// </summary>
        public int RowNumber;

        /// <summary>
        /// The list of items in the row
        /// </summary>
        public List<RemoteTableRowItem> Items;
    }

    /// <summary>
    /// Represents a single item in a row in the table. The intersection of a specific row and column
    /// </summary>
    public class RemoteTableRowItem
    {
        /// <summary>
        /// The system name of the column
        /// </summary>
        public string Name;

        /// <summary>
        /// Link to the column it references
        /// </summary>
        public RemoteTableColumn Column;

        /// <summary>
        /// The value of the item (dynamically typed)
        /// </summary>
        public object Value;
    }
}
