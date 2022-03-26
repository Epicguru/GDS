

using System;
using System.Xml;

namespace GDS.Parsers
{
    class DoubleParser : NodeParser
    {
        public override bool CanHandle(Type t) => t == typeof(double);

        public override object TryParse(XmlNode node, Type _, in ParserContext _2)
        {
            var text = node.InnerText;
            if (double.TryParse(text, out var value))
                return value;

            Error($"Failed to parse '{text}' as a double.");
            return null;
        }
    }
}
