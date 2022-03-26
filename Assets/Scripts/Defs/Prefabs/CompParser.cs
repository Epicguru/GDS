using GDS.Parsers;
using System;
using System.Xml;

namespace Defs.Prefabs
{
    public class CompParser : NodeParser
    {
        public override bool CanHandle(Type t) => t == typeof(PrefabComponent);

        public override object TryParse(XmlNode node, Type expectedType, in ParserContext context)
        {
            string compClassName = node.Name;
            var go = Prefab.LatestPrefab.GameObject;
            var type = context.Manager.ResolveType(compClassName);

            var comp = go.AddComponent(type);
            context.GDSParser.PopulateClassFromXML(node, comp);

            return new PrefabComponent()
            {
                Component = comp,
                XmlNode = node
            };
        }
    }
}
