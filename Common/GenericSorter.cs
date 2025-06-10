using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections;

namespace Inflectra.SpiraTest.Common
{
    /// <summary>
    /// Provides generic LINQ sorting functionality for classes where we don't know the sort field at compile-time
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericSorter<T>
    {
        /// <summary>
        /// Sorts by a specific field and direction
        /// </summary>
        /// <param name="source">The data source</param>
        /// <param name="sortBy">The sort field</param>
        /// <param name="sortAscending">Should we sort ascending</param>
        /// <returns></returns>
        public IList<T> Sort(IEnumerable<T> source, string sortBy, bool sortAscending = true)
        {

            var param = Expression.Parameter(typeof(T), "item");

            var sortExpression = Expression.Lambda<Func<T, object>>
                 (Expression.Convert(Expression.Property(param, sortBy), typeof(object)), param);

            if (sortAscending)
            {
                return (source.AsQueryable<T>().OrderBy<T, object>(sortExpression)).ToList();
            }
            else
            {
                return (source.AsQueryable<T>().OrderByDescending<T, object>(sortExpression)).ToList();
            }
        }
    }
}
