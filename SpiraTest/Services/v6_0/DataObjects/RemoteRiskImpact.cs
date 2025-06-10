using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
	/// <summary>
	/// Represents a risk impact in the project
	/// </summary>
	public class RemoteRiskImpact
	{
		/// <summary>
		/// The id of the risk impact (integer)
		/// </summary>
		public Nullable<int> RiskImpactId;

		/// <summary>
		/// The name of the risk impact (string)
		/// </summary>
		public string Name;

		/// <summary>
		/// Whether the impact is active or not (boolean)
		/// </summary>
		public bool Active;

		/// <summary>
		/// The display position of this impact
		/// </summary>
		public int Position;

		/// <summary>
		/// The hex color code associated with the impact (string)
		/// </summary>
		public string Color;

		/// <summary>
		/// The score value
		/// </summary>
		public int Score;
	}
}
