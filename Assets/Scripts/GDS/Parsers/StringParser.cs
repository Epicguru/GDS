
using System;
using System.Xml;

namespace GDS.Parsers
{
    class StringParser : NodeParser
    {
        public override bool CanHandle(Type t) => t == typeof(string);

        public override object TryParse(XmlNode node, Type _, in ParserContext _2) => node.InnerText;
    }
}
