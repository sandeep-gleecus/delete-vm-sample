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
    ServiceContract(Name = "SourceCodeFileService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")
    ]
    interface ISourceCodeFileService : ITreeViewService, ISortedListService, INavigationService, IFormService, IItemSelectorService
    {
        [OperationContract]
        void SourceCode_SetSelectedBranch(int projectId, string branchKey);

        [OperationContract]
        int SourceCodeFile_CountForRevision(int projectId, string revisionKey);

        [OperationContract]
        string SourceCodeFile_OpenText(int projectId, string branchKey, string fileKey);

        [OperationContract]
        string SourceCodeFile_OpenMarkdown(int projectId, string branchKey, string fileKey);
	}
}
