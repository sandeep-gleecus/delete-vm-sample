using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    [ServiceContract(Name = "ITreeViewService", Namespace = "Inflectra.SpiraTest.Web.Services.Ajax")]
    interface ITreeViewService
    {
        [OperationContract]
        List<TreeViewNode> TreeView_GetNodes(int containerId, string parentId);

        [OperationContract]
        void TreeView_SetSelectedNode(int containerId, string nodeId);

        [OperationContract]
        List<string> TreeView_GetExpandedNodes(int containerId);

        [OperationContract]
        string TreeView_GetNodeTooltip(string nodeId);

        [OperationContract]
        void TreeView_DragDestination(int projectId, int[] artifactIds, int nodeId);

        #region Methods only needed for treeviews that are editable

        [OperationContract]
        JsonDictionaryOfStrings TreeView_GetAllNodes(int containerId);

        [OperationContract]
        string TreeView_AddNode(int containerId, string name, string parentNodeId, string description);
        
        [OperationContract]
        void TreeView_UpdateNode(int containerId, string nodeId, string name, string parentNodeId, string description);

        [OperationContract]
        void TreeView_DeleteNode(int containerId, string nodeId);

        [OperationContract]
        string TreeView_GetParentNode(int containerId, string nodeId);

        #endregion
    }
}
