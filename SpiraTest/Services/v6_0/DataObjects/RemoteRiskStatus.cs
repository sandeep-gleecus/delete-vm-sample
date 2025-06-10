using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
	/// <summary>
	/// Represents a risk status in the project
	/// </summary>
	public class RemoteRiskStatus
	{
		/// <summary>
		/// The id of the risk status
		/// </summary>
		public int RiskStatusId;

		/// <summary>
		/// The name of the risk status
		/// </summary>
		public string Name;

		/// <summary>
		/// Is this an active risk status
		/// </summary>
		public bool Active;

		/// <summary>
		/// The display position of this status
		/// </summary>
		public int Position;
	}
}
