using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Provides the web service used to interacting with the various client-side source code file/folder list AJAX components
    /// </summary>
    [
    ServiceContract(Name = "SourceCodeRevisionFileService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ISourceCodeRevisionFileService : INavigationService
    {
        [OperationContract]
        DataItem SourceCode_RetrieveDataItem(int projectId, string fileKey, string revisionKey, string branchKey);

        [OperationContract]
        string SourceCodeFile_OpenText(int projectId, string branchKey, string fileKey, string revisionKey);

        [OperationContract]
        string SourceCodeFile_OpenMarkdown(int projectId, string branchKey, string fileKey, string revisionKey);

        [OperationContract]
        TextDiff SourceCodeFile_OpenSideBySideTextDiff(int projectId, string branchKey, string fileKey, string currentRevisionKey, string previousRevisionKey);

		[OperationContract]
		TextDiffPane SourceCodeFile_OpenUnifiedTextDiff(int projectId, string branchKey, string fileKey, string currentRevisionKey, string previousRevisionKey);

        [OperationContract]
        List<DataItem> SourceCode_RetrieveFilesForRevision(int projectId, string branchKey, string revisionKey, string themeBaseUrl);

        [OperationContract]
        List<DataItem> SourceCode_RetrieveRevisionsForFile(int projectId, string branchKey, string fileKey, string themeBaseUrl);

        [OperationContract]
        void SourceCode_UpdateDiffViewSetting(int projectId, bool isUnified);
    }
}
