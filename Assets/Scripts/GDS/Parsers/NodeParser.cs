using System;
using System.Xml;

namespace GDS.Parsers
{
    public abstract class NodeParser
    {
        public abstract bool CanHandle(Type t);

        public abstract object TryParse(XmlNode node, Type expectedType, in ParserContext context);

        public virtual void Error(string message, Exception e = null)
        {
            Console.WriteLine(message);
            if (e != null)
                Console.WriteLine(e.ToString());
        }
    }

    public struct ParserContext
    {
        public GDSParser GDSParser;
        public GDSParserManager Manager;
        public GDSParser.Context NodeContext;

    }
}
