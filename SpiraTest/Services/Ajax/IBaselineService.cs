using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using System.ServiceModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>Provides the web service used to interacting with the various client-side artifact association AJAX components</summary>
	[ServiceContract(Name = "BaselineService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
	interface IBaselineService : ISortedListService
	{
		[OperationContract]
		int Baseline_Count(int projectId, ArtifactReference releaseId);

        [OperationContract]
        int Baseline_Create(int projectId, int releaseId, string name, string description);
    }
}
