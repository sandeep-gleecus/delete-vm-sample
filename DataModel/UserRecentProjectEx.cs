using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.DataModel
{
	/// <summary>
	/// Extensions to the recent project entity
	/// </summary>
	public partial class UserRecentProject : Entity
	{
		/// <summary>
		/// Returns the project name
		/// </summary>
		public string ProjectName
		{
			get
			{
				return Project.Name;
			}
		}

		/// <summary>
		/// Returns the project description
		/// </summary>
		public string ProjectDescription
		{
			get
			{
				return Project.Description;
			}
		}
	}
}
