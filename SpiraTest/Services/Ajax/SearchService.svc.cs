using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using System.IO;
using System.Data;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to display a list of search results in the Global Search box
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class SearchService : AjaxWebServiceBase, ISearchService
    {
        public const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.SearchService::";

        /// <summary>
        /// Retrieves the list of search results for a specific keyword
        /// </summary>
        /// <param name="userId">The id of the current user</param>
        /// <param name="keyword">The keyword we're searching on</param>
        /// <param name="pageIndex">The index of the page of results</param>
        /// <param name="pageSize">The size of the page</param>
        /// <returns>The search results</returns>
        public SearchResults RetrieveResults(string keyword, int pageIndex, int pageSize)
        {
			const string METHOD_NAME = "RetrieveResults";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Get the list of projects and artifacts that the current user can view
                Business.ProjectManager projectManager = new Business.ProjectManager();
                List<ProjectViewPermission> projectViewPermissions = projectManager.RetrieveProjectViewPermissionsForUser(userId);

                //Instantiate the artifact manager
                ArtifactManager artifactManager = new ArtifactManager();
                List<ArtifactManager.ProjectArtifactTypeFilter> projectArtifactList = new List<ArtifactManager.ProjectArtifactTypeFilter>();
                foreach (ProjectViewPermission projectViewPermission in projectViewPermissions)
                {
                    projectArtifactList.Add(new ArtifactManager.ProjectArtifactTypeFilter() { ArtifactTypeId = projectViewPermission.ArtifactTypeId, ProjectId = projectViewPermission.ProjectId });
                }
                //If we have SpiraPlan/Team also allow user searching
                if (ArtifactManager.IsSupportedByLicense(Artifact.ArtifactTypeEnum.User))
                {
                    projectArtifactList.Add(new ArtifactManager.ProjectArtifactTypeFilter() { ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.User, ProjectId = -1 });
                }

                SearchResults searchResults = new SearchResults();
                int count;
                List<ArtifactView> artifactResults = artifactManager.SearchByKeyword(keyword, pageIndex, pageSize, out count, projectArtifactList);
                searchResults.Count = count;

                //Create the array of search results and copy across the results
                foreach (ArtifactView artifactView in artifactResults)
                {
                    SearchResult searchResult = new SearchResult();
                    searchResult.ArtifactTypeId = artifactView.ArtifactTypeId;
                    searchResult.ProjectId = artifactView.ProjectId;
                    searchResult.Token = GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum)artifactView.ArtifactTypeId) + artifactView.ArtifactId;
                    searchResult.Title = artifactView.Name;
                    searchResult.Rank = artifactView.Rank;
                    searchResult.Description = artifactView.Description.StripHTML(false).SafeSubstring(0, 255);
                    if (artifactView.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.User)
                    {
                        //Users point to the resource details page
                        searchResult.Icon = UrlRewriterModule.ResolveUrl(UrlRewriterModule.ResolveUserAvatarUrl(artifactView.ArtifactId));
                        searchResult.IconAlt = Resources.Fields.User;
                        searchResult.Url = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Resources, artifactView.ProjectId, artifactView.ArtifactId));
                    }
                    else
                    {
                        searchResult.Icon = GlobalFunctions.GetIconForArtifactType((DataModel.Artifact.ArtifactTypeEnum)artifactView.ArtifactTypeId);
                        searchResult.IconAlt = Path.GetFileNameWithoutExtension(GlobalFunctions.GetIconForArtifactType((DataModel.Artifact.ArtifactTypeEnum)artifactView.ArtifactTypeId));
                        searchResult.Url = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)artifactView.ArtifactTypeId, artifactView.ProjectId, artifactView.ArtifactId));
                    }

                    searchResult.ProjectName = artifactView.ProjectName;
                    if (artifactView.LastUpdateDate.HasValue)
                    {
                        searchResult.LastUpdateDate = GlobalFunctions.LocalizeDate(artifactView.LastUpdateDate.Value).ToNiceString(GlobalFunctions.LocalizeDate(DateTime.UtcNow), "D");
                    }
                    searchResults.Values.Add(searchResult);
                }

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

                return searchResults;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
        }
    }
}
