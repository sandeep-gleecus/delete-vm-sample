using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using System.Collections.Generic;
using System.ServiceModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Used to hold any global Ajax web services that don't fit anywhere else
    /// </summary>
    [
    ServiceContract(Name = "GlobalService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax"),
    ]
    interface IGlobalService : INavigationService
    {
        [OperationContract]
        bool CollapsiblePanel_RetrieveState(string pageId, string panelId);

        [OperationContract]
        List<KeyValuePair> CollapsiblePanel_RetrieveStateAll(string pageId);

        [OperationContract]
        void CollapsiblePanel_UpdateState(string pageId, string panelId, bool isCollapsed);

        [OperationContract]
        void TabControl_UpdateState(string pageId, string controlId, string selectedTab);

        [OperationContract]
        bool UserSettings_GuidedTour_RetrieveSeen(string tour);

        [OperationContract]
        void UserSettings_GuidedTour_SetSeen(string tour);

        [OperationContract]
        void UserSettings_GuidedTour_SetNavigationLinkCount(string tour);

        [OperationContract]
        void UserSettings_ColorMode_Set(string colorMode);

        [OperationContract]
        void UserSettings_UpdateRecentArtifact(int projectId, int artifactTypeId, int artifactId);

        [OperationContract]
        string Global_GetStandardReportUrl(int projectId, string reportToken, string filter, int? reportFormatId);
    }
}
