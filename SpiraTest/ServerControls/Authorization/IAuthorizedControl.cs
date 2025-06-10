using System;
using System.ComponentModel;
using System.Web.UI;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls.Authorization
{
	/// <summary>
	/// Defines the standard authorization and methods that need to be implemented by
	/// any control that requires users to have a particular authorization level to operate,
	/// or that should only appear if certain parts of the application are enabled.
	/// </summary>
	public interface IAuthorizedControl
	{
		/// <summary>
		/// This is the type of artifact that the user's role needs to have permissions for
		/// </summary>
		DataModel.Artifact.ArtifactTypeEnum Authorized_ArtifactType
		{
			get;
			set;
		}

		/// <summary>
		/// This is the type of action that the user's role needs to have permissions for
		/// </summary>
		Project.PermissionEnum Authorized_Permission
		{
			get;
			set;
		}
	}
}
