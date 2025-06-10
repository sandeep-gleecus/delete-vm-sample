using System;
using System.Data;
using System.Diagnostics;
using System.Configuration;

using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This is a utility class for all the business objects that make use of hierarchical data
	/// and have the need to do reorder and similar operations
	/// </summary>
	/// <remarks>
	/// Currently Requirements and Releases fall into this category
	/// </remarks>
	public static class HierarchicalList
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.HierarchicalList::";

		#region Public Static Methods

		/// <summary>
		/// This function increments the alphanumeric indent level
		/// (e.g. AAAABAAAC + 1 => AAAABAAAD)
		/// </summary>
		/// <param name="existingIndentLevel">The existing indent level</param>
		/// <returns>The updated indent level element</returns>
		public static string IncrementIndentLevel (string existingIndentLevel)
		{
			const string METHOD_NAME = "IncrementIndentLevel";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

			System.Text.StringBuilder indentLevelBuilder = new System.Text.StringBuilder(existingIndentLevel);
			int length = indentLevelBuilder.Length;
			for (int i = length - 1; i >= 0; i--)
			{
				char element = indentLevelBuilder[i];
				//See if we have position overflow case
				if (element < 'Z')
				{
					//increment the position
					element ++;
					indentLevelBuilder[i] = element;
					break;
				}
				else
				{
					//otherwise set the position to A and move to next position
					indentLevelBuilder[i] = 'A';
				}			
			}
			//Logger.LogTraceEvent (CLASS_NAME + METHOD_NAME, "Update indent level from " + existingIndentLevel + " to " + indentLevelBuilder.ToString());
			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);

			return indentLevelBuilder.ToString();
		}

		/// <summary>Replaces the base of an indent level with a different base</summary>
		/// <param name="indentLevel">The input indent level</param>
		/// <param name="newIndentBase">The replacement indent level base</param>
		/// <param name="oldIndentBase">The indent level base to look for</param>
		/// <returns>The string with any replacements made</returns>
		/// <remarks>Similar to the String Replace function, except that it only changes one occurrence that has to start at the first character position</remarks>
		public static string ReplaceIndentLevelBase (string indentLevel, string oldIndentBase, string newIndentBase)
		{
			int baseLength = oldIndentBase.Length;

			string newIndentLevel = indentLevel;

			//See if the base of the provided indent level matches the old base
			if (indentLevel.Substring (0, baseLength) == oldIndentBase)
			{
				//Reconstruct the indent level with the new base plus the section after the base
				newIndentLevel = newIndentBase + indentLevel.Substring(baseLength, indentLevel.Length - baseLength);
			}

			return newIndentLevel;
		}

 		/// <summary>
		/// This function decrements the alphanumeric indent level
		/// (e.g. AAAABAAAC - 1 => AAAABAAAB)
		/// </summary>
		/// <param name="existingIndentLevel">The existing indent level</param>
		/// <returns>The updated indent level element</returns>
		public static string DecrementIndentLevel (string existingIndentLevel)
		{
			const string METHOD_NAME = "DecrementIndentLevel";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

			System.Text.StringBuilder indentLevelBuilder = new System.Text.StringBuilder(existingIndentLevel);
			int length = indentLevelBuilder.Length;
			for (int i = length - 1; i >= 0; i--)
			{
				char element = indentLevelBuilder[i];
				//See if we have position overflow case
				if (element > 'A')
				{
					//decrement the position
					element --;
					indentLevelBuilder[i] = element;
					break;
				}
				else
				{
					//otherwise set the position to Z and move to next position
					indentLevelBuilder[i] = 'Z';
				}			
			}
			//Logger.LogTraceEvent (CLASS_NAME + METHOD_NAME, "Update indent level from " + existingIndentLevel + " to " + indentLevelBuilder.ToString());
			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);

			return indentLevelBuilder.ToString();
		}

		/// <summary>
		/// This function determines if a requirement has a parent or not from its indent level
		/// </summary>
		/// <param name="indentLevel">The indent level of the requirement</param>
		/// <returns>TRUE if it has a parent requirement</returns>
		public static bool HasParent (string indentLevel)
		{
			const string METHOD_NAME = "HasParent";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

			bool hasParent = (indentLevel.Length > 3);

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);

			return hasParent;
		}

        /// <summary>
        /// This function determines if an indent level is a child of the parent
        /// </summary>
        /// <param name="parentIndentLevel">The indent level of the parent</param>
        /// <param name="indentLevel">The indent level of the item</param>
        /// <returns>TRUE if it is a child item</returns>
        public static bool IsChildOf(string indentLevel, string parentIndentLevel)
        {
            if (indentLevel.Length >= parentIndentLevel.Length + 3)
            {
                if (indentLevel.Substring(0, parentIndentLevel.Length) == parentIndentLevel)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This function determines if an indent level is a child of the parent or the parent itself
        /// </summary>
        /// <param name="parentIndentLevel">The indent level of the parent</param>
        /// <param name="indentLevel">The indent level of the item</param>
        /// <returns>TRUE if it is a child item</returns>
        public static bool IsSelfOrChildOf(string indentLevel, string parentIndentLevel)
        {
            if (indentLevel.Length >= parentIndentLevel.Length)
            {
                if (indentLevel.Substring(0, parentIndentLevel.Length) == parentIndentLevel)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }
		
		#endregion
	}
}
