using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using System.ServiceModel.Syndication;

namespace Inflectra.SpiraTest.Web.Feeds
{
    /// <summary>
    /// Displays an RSS feed of a user's saved search
    /// </summary>
    [ServiceContract(Name = "Syndication", Namespace = "Inflectra.SpiraTest.Web.Feeds")]
    public interface ISyndication
    {
        [OperationContract, WebGet(UriTemplate = "SavedSearch?userId={userId}&rssToken={rssToken}&savedFilterId={savedFilterId}")]
        Rss20FeedFormatter SavedSearch(int userId, string rssToken, int savedFilterId);

        [OperationContract, WebGet(UriTemplate = "AssignedIncidents?userId={userId}&rssToken={rssToken}")]
        Rss20FeedFormatter AssignedIncidents(int userId, string rssToken);

        [OperationContract, WebGet(UriTemplate = "AssignedDocuments?userId={userId}&rssToken={rssToken}")]
        Rss20FeedFormatter AssignedDocuments(int userId, string rssToken);

        [OperationContract, WebGet(UriTemplate = "AssignedRequirements?userId={userId}&rssToken={rssToken}")]
        Rss20FeedFormatter AssignedRequirements(int userId, string rssToken);

        [OperationContract, WebGet(UriTemplate = "AssignedTasks?userId={userId}&rssToken={rssToken}")]
        Rss20FeedFormatter AssignedTasks(int userId, string rssToken);

        [OperationContract, WebGet(UriTemplate = "AssignedRisks?userId={userId}&rssToken={rssToken}")]
        Rss20FeedFormatter AssignedRisks(int userId, string rssToken);

        [OperationContract, WebGet(UriTemplate = "AssignedTestCases?userId={userId}&rssToken={rssToken}")]
        Rss20FeedFormatter AssignedTestCases(int userId, string rssToken);

        [OperationContract, WebGet(UriTemplate = "AssignedTestSets?userId={userId}&rssToken={rssToken}")]
        Rss20FeedFormatter AssignedTestSets(int userId, string rssToken);

        [OperationContract, WebGet(UriTemplate = "DetectedIncidents?userId={userId}&rssToken={rssToken}")]
        Rss20FeedFormatter DetectedIncidents(int userId, string rssToken);

        [OperationContract, WebGet(UriTemplate = "SubscribedArtifacts?userId={userId}&rssToken={rssToken}")]
        Rss20FeedFormatter SubscribedArtifacts(int userId, string rssToken);
    }
}
