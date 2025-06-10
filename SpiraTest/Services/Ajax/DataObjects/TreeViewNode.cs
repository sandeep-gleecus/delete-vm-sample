using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents a single node in the AJAX treeview
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class TreeViewNode
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TreeViewNode()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nodeId">The id of the node</param>
        /// <param name="name">The name of the node</param>
        /// <param name="tooltip">The tooltip for the node</param>
        public TreeViewNode(string nodeId, string name, string tooltip)
        {
            this.NodeId = nodeId;
            this.Name = name;
            this.Tooltip = tooltip;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nodeId">The id of the node</param>
        /// <param name="name">The name of the node</param>
        /// <param name="tooltip">The tooltip for the node</param>
        /// <param name="nodeImageUrl">The image to use for the node (leave empty to use the default)</param>
        /// <param name="statusImageUrl">The image to use for displaying any status info</param>
        /// <param name="clickable">Is this node clickable</param>
        /// <param name="hasChildren">Does this entry have children</param>
        public TreeViewNode(string nodeId, string name, string tooltip, string nodeImageUrl, string statusImageUrl, bool clickable, bool hasChildren)
        {
            this.NodeId = nodeId;
            this.Name = name;
            this.Tooltip = tooltip;
            this.NodeImageUrl = nodeImageUrl;
            this.StatusImageUrl = statusImageUrl;
            this.Clickable = clickable;
            this.HasChildren = hasChildren;
        }

        /// <summary>
        /// The id of the node
        /// </summary>
        [DataMember(Name = "nodeId")]
        public string NodeId;

        /// <summary>
        /// The name of the node
        /// </summary>
        [DataMember(Name = "name")]
        public string Name;

        /// <summary>
        /// The tooltip of the node
        /// </summary>
        [DataMember(Name = "tooltip")]
        public string Tooltip;

        /// <summary>
        /// The image to use for the treeview node. Leave as null/empty to use the default folder icon
        /// </summary>
        [DataMember(Name = "nodeImageUrl")]
        public string NodeImageUrl;

        /// <summary>
        /// The image to use for any status indicators (optional)
        /// </summary>
        [DataMember(Name = "statusImageUrl")]
        public string StatusImageUrl;

        /// <summary>
        /// Is this entry clickable or just a label
        /// </summary>
        [DataMember(Name = "clickable")]
        public bool Clickable = true;

        /// <summary>
        /// Does this entry have children
        /// </summary>
        [DataMember(Name = "hasChildren")]
        public bool HasChildren = true;
    }
}