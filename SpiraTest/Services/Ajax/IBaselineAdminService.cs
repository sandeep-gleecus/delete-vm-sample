using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using System.ServiceModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>Provides the web service used to interacting with the various client-side artifact association AJAX components</summary>
	[ServiceContract(Name = "BaselineAdminService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
	interface IBaselineAdminService : ISortedListService
	{
    }
}
