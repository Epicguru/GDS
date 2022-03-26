using System;
using System.Xml;

namespace GDS.Parsers
{
    public class CharParser : NodeParser
    {
        public override bool CanHandle(Type t) => t == typeof(char);

        public override object TryParse(XmlNode node, Type expectedType, in ParserContext _)
        {
            string txt = node.InnerText;
            if (char.TryParse(txt, out var b))
                return b;

            Error($"Failed to parse {txt} as a character (char).");
            return null;
        }
    }
}
