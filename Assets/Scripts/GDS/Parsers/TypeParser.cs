using System;
using System.Xml;

namespace GDS.Parsers
{
    public class TypeParser : NodeParser
    {
        public override bool CanHandle(Type t) => t == typeof(Type);

        public override object TryParse(XmlNode node, Type _, in ParserContext context)
        {
            return context.Manager.ResolveType(node.InnerText.Trim());
        }
    }
}
