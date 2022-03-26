using System;
using System.Xml;

namespace GDS.Parsers
{
    public class BoolParser : NodeParser
    {
        public override bool CanHandle(Type t) => t == typeof(bool);

        public override object TryParse(XmlNode node, Type expectedType, in ParserContext _)
        {
            string txt = node.InnerText;
            if (bool.TryParse(txt, out var b))
                return b;

            Error($"Failed to parse {txt} as a boolean.");
            return null;
        }
    }
}
