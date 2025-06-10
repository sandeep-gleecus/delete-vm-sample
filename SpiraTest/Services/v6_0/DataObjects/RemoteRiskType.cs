using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
	/// <summary>
	/// Represents a risk type in the project
	/// </summary>
	public class RemoteRiskType
	{
		/// <summary>
		/// The id of the risk type
		/// </summary>
		public int RiskTypeId;

		/// <summary>
		/// The name of the risk type
		/// </summary>
		public string Name;

		/// <summary>
		/// The id of the workflow the risk type is associated with, for the current project
		/// </summary>
		public int? WorkflowId;

		/// <summary>
		/// Is this an active risk type
		/// </summary>
		public bool IsActive;

		/// <summary>
		/// The display position of this type
		/// </summary>
		public int Position;

		/// <summary>
		/// Is this the default risk type
		/// </summary>
		public bool IsDefault;
	}
}
