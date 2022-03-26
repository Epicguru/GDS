
using System;
using System.Xml;

namespace GDS.Parsers
{
    class DecimalParser : NodeParser
    {
        public override bool CanHandle(Type t) => t == typeof(decimal);

        public override object TryParse(XmlNode node, Type _, in ParserContext _2)
        {
            var text = node.InnerText;
            if (decimal.TryParse(text, out var value))
                return value;

            Error($"Failed to parse '{text}' as a decimal.");
            return null;
        }
    }
}
