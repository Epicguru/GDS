using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace GDS
{
    public class GDSBuilder
    {
        private static readonly HashSet<string> doNotInheritAttributes = new HashSet<string>
        {
            "Inherit",
            "Parent",
            "Abstract"
        };

        private XmlDocument doc;

        public void Digest(XmlDocument other)
        {
            if (other == null)
                return;

            if (doc == null)
            {
                doc = other.CloneNode(true) as XmlDocument;
                return;
            }

            var docRoot = doc.GetRootNode();
            var otherRoot = other.GetRootNode();
            Debug.Assert(docRoot != null);
            Debug.Assert(otherRoot != null);
            Debug.Assert(docRoot.Name == otherRoot.Name);

            if (!otherRoot.HasChildNodes)
                return;

            foreach (XmlNode child in otherRoot.ChildNodes)
            {
                var existing = docRoot[child.Name];
                if (existing != null)
                    throw new Exception($"Duplicated name: {existing.Name}");

                // TODO detect parent...

                var clone = doc.ImportNode(child, true);
                docRoot.AppendChild(clone);
            }
        }

        public XmlDocument Process()
        {
            if (doc == null)
                return null;

            var docRoot = doc.GetRootNode();
            var bin = new List<XmlNode>();

            foreach (XmlNode node in docRoot)
            {
                if (node.NodeType != XmlNodeType.Element || node.EvaluateBoolAttribute("Abstract", false))
                    continue;

                var line = MakeInheritanceLine(node);
                if (line == null)
                    continue; // No inheritance.

                // The node needs to be deleted. It will be replaced with the one built from inheritance line.
                bin.Add(node);

                XmlNode created = null;
                foreach (var item in line)
                {
                    if (created == null)
                    {
                        // Have to clone like this because XmlNode's can't be renamed...
                        created = doc.CreateElement(node.Name);
                        item.MergeAttributesInto(created, null);
                        foreach (XmlNode child in item.ChildNodes)
                            created.AppendChild(child.CloneNode(true));
                        docRoot.AppendChild(created);
                        continue;
                    }

                    Merge(item, created, docRoot);
                }

                // Clear parent attr to mark as complete.
                var parentNameAttr = doc.CreateAttribute("ParentName");
                parentNameAttr.Value = node.TryGetAttr("Parent");
                created.Attributes.Append(parentNameAttr);
                created.Attributes.RemoveNamedItem("Parent");
            }

            foreach (var item in bin)
                docRoot.RemoveChild(item);

            return doc;
        }

        private List<XmlNode> MakeInheritanceLine(XmlNode node)
        {
            if (node.TryGetAttr("Parent") == null)
                return null;

            var list = new List<XmlNode>();
            var docRoot = doc.GetRootNode();

            do
            {
                list.Insert(0, node);
                string parentName = node.TryGetAttr("Parent");
                if (parentName != null)
                {
                    node = docRoot[parentName];
                    if (node == null)
                        throw new Exception($"Failed to find parent called '{parentName}'");
                }
                else
                {
                    node = null;
                }

            } while (node != null);

            return list;
        }

        private void Merge(XmlNode node, XmlNode into, XmlNode intoParent)
        {
            if (into == null)
            {
                // There is nothing to merge into.
                // Create deep clone and simply stick in into the parent.
                var clone = node.CloneNode(true);
                intoParent.AppendChild(clone);
                return;
            }

            bool inherit = node.EvaluateBoolAttribute("Inherit", true);
            if (!inherit || node.IsValueNode())
            {
                // Replace old with new.
                var clone = node.CloneNode(true);
                intoParent.ReplaceChild(clone, into);
                if (inherit)
                    clone.AddAttributesFromOtherIfMissing(into, doNotInheritAttributes);
                clone.TryRemoveAttr("Inherit");
                return;
            }

            if (node.IsListNode())
            {
                // Append all new ones rather than merging.
                foreach (XmlNode child in node.ChildNodes)
                {
                    var clone = child.CloneNode(true);
                    into.AppendChild(clone);
                }
                return;
            }

            node.MergeAttributesInto(into, doNotInheritAttributes);
            foreach (XmlNode child in node.ChildNodes)
            {
                var target = into[child.Name];
                Merge(child, target, into);
            }
        }
    }
}
