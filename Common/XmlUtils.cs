using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Inflectra.SpiraTest.Common
{
    public static class XmlUtils
    {
        public static XmlNode RenameNode(XmlNode node, string namespaceURI, string qualifiedName)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                XmlElement oldElement = (XmlElement)node;
                XmlElement newElement =
                node.OwnerDocument.CreateElement(qualifiedName, namespaceURI);

                while (oldElement.HasAttributes)
                {
                    newElement.SetAttributeNode(oldElement.RemoveAttributeNode(oldElement.Attributes[0]));
                }

                while (oldElement.HasChildNodes)
                {
                    newElement.AppendChild(oldElement.FirstChild);
                }

                if (oldElement.ParentNode != null)
                {
                    oldElement.ParentNode.ReplaceChild(newElement, oldElement);
                }

                return newElement;
            }
            else
            {
                return null;
            }
        }
    }
}
