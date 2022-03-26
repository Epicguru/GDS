using System;
using System.Xml;

namespace GDS.Parsers
{
    public class EnumParser : NodeParser
    {
        public override bool CanHandle(Type t)
        {
            return t.IsEnum;
        }

        public override object TryParse(XmlNode node, Type expectedType, in ParserContext _)
        {
            if (Enum.TryParse(expectedType, node.InnerText, out var result))
                return result;

            Error($"Failed to parse {node.InnerText} as enum {expectedType.Name}");
            return null;
        }
    }
}
