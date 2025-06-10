using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
	/// <summary>
	/// Represents a risk probability in the project
	/// </summary>
	public class RemoteRiskProbability
	{
		/// <summary>
		/// The id of the risk probability (integer)
		/// </summary>
		public Nullable<int> RiskProbabilityId;

		/// <summary>
		/// The name of the risk probability (string)
		/// </summary>
		public string Name;

		/// <summary>
		/// Whether the probability is active or not (boolean)
		/// </summary>
		public bool Active;

		/// <summary>
		/// The display position of this probability
		/// </summary>
		public int Position;

		/// <summary>
		/// The hex color code associated with the probability (string)
		/// </summary>
		public string Color;

		/// <summary>
		/// The score value
		/// </summary>
		public int Score;
	}
}
