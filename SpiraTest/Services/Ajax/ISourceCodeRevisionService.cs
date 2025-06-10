using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to interacting with the various client-side source code revision list AJAX components
    /// </summary>
    [
    ServiceContract(Name = "SourceCodeRevisionService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ISourceCodeRevisionService : ISortedListService, INavigationService, IFormService
    {
        [OperationContract]
        List<DateTimeFrequencyEntry> SourceCodeRevision_RetrieveCommitCounts(int projectId, string dateRange);

        [OperationContract]
        int SourceCodeRevision_Count(int projectId, ArtifactReference artifact);

        [OperationContract]
        int SourceCodeRevision_CountForFile(int projectId, string fileKey);

        [OperationContract]
        int SourceCodeRevision_CountForPullRequest(int projectId, int pullRequestId);

        [OperationContract]
        List<DataItem> SourceCodeRevision_RetrieveRecent(int projectId, string branchKey, int numberRows);

        [OperationContract]
        List<string> SourceCodeRevision_RetrieveBranches(int projectId, string revisionKey);
    }
}
