using GDS.Parsers;
using System;
using System.Xml;

namespace Defs
{
    public class AssetParser : NodeParser
    {
        public override bool CanHandle(Type t) => t.IsSubclassOf(typeof(UnityEngine.Object));

        public override object TryParse(XmlNode node, Type expectedType, in ParserContext context)
        {
            string assetName = node.InnerText.Trim();

            var req = new AssetReferenceRequest(context.NodeContext.Parent, assetName, expectedType);
            req.Field = context.NodeContext.Field;
            req.ListIndex = context.NodeContext.ListIndex;

            DefLoader.AssetRequests.Add(req);
            return null;
        }
    }
}
