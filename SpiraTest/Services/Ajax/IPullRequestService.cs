using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the SortableGrid AJAX component for displaying/updating pull request tasks
    /// </summary>
    [
    ServiceContract(Name = "PullRequestService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface IPullRequestService : ISortedListService
    {
        [OperationContract]
        int PullRequest_Create(int projectId, string name, string sourceBranch, string destBranch, int? ownerId, int? releaseId);
    }
}
