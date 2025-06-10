using System;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.ServerControls;

namespace Inflectra.SpiraTest.Web.UserControls
{
	/// <summary>
	/// Interface for controls that provide common elements in artifact details pages
	/// </summary>
	public interface IArtifactUserControl
	{
		void LoadAndBindData (bool dataBind);
        MessageBox MessageLabelHandle { set; }
        int ArtifactId { get; set; }
        DataModel.Artifact.ArtifactTypeEnum ArtifactTypeEnum { get; set; }
        bool HasData { get; set; }
	}
}
