
using System;
using System.Collections.Generic;
using System.Reflection;
using GDS.Parsers;

namespace GDS
{
    public class GDSParserManager
    {
        public Func<string, Type> TypeResolver;

        private readonly List<NodeParser> parsers = new List<NodeParser>();
        private readonly Dictionary<Type, NodeParser> typeToParser = new Dictionary<Type, NodeParser>();

        public void ClearParsers()
        {
            parsers.Clear();
            typeToParser.Clear();
        }

        public Type ResolveType(string name)
        {
            if (TypeResolver != null)
                return TypeResolver(name);

            return Type.GetType(name);
        }

        public int AddParsersFromAssembly(Assembly a)
        {
            if (a == null)
                return 0;

            int added = 0;
            foreach (var type in a.GetTypes())
            {
                if (!type.IsAbstract && typeof(NodeParser).IsAssignableFrom(type))
                {
                    try
                    {
                        var instance = Activator.CreateInstance(type) as NodeParser;
                        if (AddParser(instance))
                            added++;
                    }
                    catch { }
                }
            }
            return added;
        }

        public NodeParser TryGetParserFor(Type t, bool allowFromCache = true, bool saveToCache = true)
        {
            if (t == null)
                return null;

            if (allowFromCache && typeToParser.TryGetValue(t, out var found))
                return found;

            found = null;
            foreach (var parser in parsers)
            {
                if (parser.CanHandle(t))
                {
                    found = parser;
                    break;
                }
            }

            if (found == null)
                return null;

            if (saveToCache)
                typeToParser.Add(t, found);

            return found;
        }

        public bool AddParser(NodeParser parser)
        {
            if (parser == null || parsers.Contains(parser))
                return false;

            parsers.Add(parser);
            return true;
        }

        public bool RemoveParser(NodeParser parser)
        {
            if (parser == null || !parsers.Contains(parser))
                return false;

            parsers.Remove(parser);
            return true;
        }

        public bool RemoveParser(Type type)
        {
            if (type == null)
                return false;

            for (int i = 0; i < parsers.Count; i++)
            {
                var parser = parsers[i];
                if (parser.GetType().IsAssignableFrom(type))
                {
                    parsers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public bool RemoveParser<T>() where T : NodeParser
        {
            for (int i = 0; i < parsers.Count; i++)
            {
                var parser = parsers[i];
                if (parser is T)
                {
                    parsers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
    }
}
