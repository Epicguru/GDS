
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace GDS
{
    public static class XmlExtensions
    {
        public static bool IsListType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

        public static Type GetFirstGenericArgument(this Type type) => type.GenericTypeArguments[0];

        public static bool IsEnumType(this Type type) => type.ExtractUnderlyingType().IsEnum;

        public static Type ExtractUnderlyingType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;

        public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;

        public static bool IsValueNode(this XmlNode node) => node.HasChildNodes && node.ChildNodes.Count == 1 && node.ChildNodes[0].NodeType == XmlNodeType.Text;

        public static void SetNodeValue(this XmlNode node, string txt)
        {
            (node.ChildNodes[0] as XmlText).Value = txt;
        }

        public static string GetNodeValue(this XmlNode node) => (node.ChildNodes[0] as XmlText)?.Value;

        public static bool IsListNode(this XmlNode node)
        {
            if (!node.HasChildNodes)
                return false;

            if (node.EvaluateBoolAttribute("IsList", false))
                return true;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name != "li")
                    return false;
            }
            return true;
        }

        public static XmlNode GetRootNode(this XmlDocument doc)
        {
            foreach (XmlNode child in doc.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                    return child;
            }
            return null;
        }

        public static string TryGetAttr(this XmlNode node, string attrName) => node.Attributes[attrName]?.Value;

        public static void TryRemoveAttr(this XmlNode node, string attrName)
        {
            node.Attributes.RemoveNamedItem(attrName);
        }

        public static bool EvaluateBoolAttribute(this XmlNode node, string attrName, bool defaultValue)
        {
            var attr = node.TryGetAttr(attrName);
            if (attr == null)
                return defaultValue;

            return bool.TryParse(attr, out bool b) ? b : defaultValue;
        }

        public static void MergeAttributesInto(this XmlNode self, XmlNode other, HashSet<string> dontInherit)
        {
            foreach (XmlAttribute attr in self.Attributes)
            {
                var toReplace = other.Attributes[attr.Name];
                if (toReplace != null)
                {
                    toReplace.Value = attr.Value;
                }
                else
                {
                    other.Attributes.Append(attr.CloneNode(true) as XmlAttribute);
                }
            }

            if (dontInherit != null)
            {
                foreach (var name in dontInherit)
                {
                    var found = other.Attributes[name];
                    if (found != null)
                        other.Attributes.Remove(found);
                }
            }
        }

        public static void AddAttributesFromOtherIfMissing(this XmlNode self, XmlNode other, HashSet<string> except)
        {
            foreach (XmlAttribute attr in other.Attributes)
            {
                if (except != null && except.Contains(attr.Name))
                    continue;

                if (self.Attributes[attr.Name] == null)
                    self.Attributes.Append(attr.CloneNode(true) as XmlAttribute);
            }
        }

        public static V AddAndReturn<K, V>(this Dictionary<K, V> dictionary, K key, V value)
        {
            dictionary.Add(key, value);
            return value;
        }

        public static bool IsXmlRefType(this Type type) => type.GetInterfaces().Contains(typeof(IXmlReference));

        public static bool IsXmlRefNode(this XmlNode node, Type type)
        {
            if (node.EvaluateBoolAttribute("NoRef", false))
                return false;

            return type.IsXmlRefType();
        }

        public static string GetPath(this FieldInfo field) => $"{field.DeclaringType.FullName}.{field.Name}";

        public static string MakePrettyXml(this XmlNode node)
        {
            var str = new StringBuilder(512);
            var settings = new XmlWriterSettings()
            {
                Indent = true
            };
            using var writer = XmlWriter.Create(str, settings);
            node.WriteTo(writer);
            writer.Close();
            return str.ToString();
        }
    }
}
