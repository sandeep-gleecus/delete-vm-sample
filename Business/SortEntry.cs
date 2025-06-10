using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// The two different sort orders
    /// </summary>
    public enum SortDirection
    {
        Ascending = 1,
        Descending = 2
    }

    /// <summary>
    /// Represents a single sort entry in a LINQ expression
    /// </summary>
    public class SortEntry<EntityType, DataType>
    {
        /// <summary>
        /// The direction of the sort
        /// </summary>
        public SortDirection Direction
        {
            get;
            set;
        }

        /// <summary>
        /// The property being sorted on
        /// </summary>
        public Expression<Func<EntityType, DataType>> Expression
        {
            get;
            set;
        }
    }
}
