using System;
using System.Collections;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>This class stores information on any Sorting, Filtering, and Pagniation needed for the sorted grin.</summary>
	/// <remarks>I know this is new, but my OCD hates passing 800+ properties into a Business function when we could
	/// just load it into a single class.</remarks>
	public class SortFilter
	{
		/// <summary>Creates a new instance of the class.</summary>
		public SortFilter()
		{
			StartingPage = 1;
			PageSize = int.MaxValue;
		}

		/// <summary>The name of the field to sort on.</summary>
		public string SortProperty { get; set; }

		/// <summary>The direction to sort. Ascending = True, Descending = False</summary>
		public bool SortAscending { get; set; }

		/// <summary>A hashtable of the filters. Key is the field name, Value is the value we are filtering on.</summary>
		public Hashtable FilterList { get; set; }

		/// <summary>The page number to retrieve. Default = 1</summary>
		public int StartingPage { get; set; }

		/// <summary>The number of rows per page. Default = int.Max</summary>
		public int PageSize { get; set; }

		/// <summary>THe starting row number to retrieve. Calculated, 1-BASED.</summary>
		public int StartingRowNumber
		{
			get
			{
				// Using the Math.Max prevents negative numbers (0, negative)
				//   from being used. UINTs cannot be used here because the LINQ
				//   funtions expect ints only - even those that really can't take
				//   them. (i.e. Skip(), Take())
				return ((Math.Max(StartingPage, 1) - 1) * Math.Max(PageSize, 1)) + 1;
			}
		}

		/// <summary>The string for the given sotr property. Calculated.</summary>
		public string SortExpression
		{
			get
			{
				string retVal = null;
				if (!string.IsNullOrWhiteSpace(SortProperty))
				{
					retVal = SortProperty
						+ " "
						+ (SortAscending ? "ASC" : "DESC");
				}

				return retVal;
			}
		}
	}
}
