

using System;
using System.Xml;

namespace GDS.Parsers
{
    class FloatParser : NodeParser
    {
        public override bool CanHandle(Type t) => t == typeof(float);

        public override object TryParse(XmlNode node, Type _, in ParserContext _2)
        {
            var text = node.InnerText;
            if (float.TryParse(text, out var value))
                return value;

            Error($"Failed to parse '{text}' as a float.");
            return null;
        }
    }
}
