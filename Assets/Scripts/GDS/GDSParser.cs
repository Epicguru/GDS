using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using GDS.Parsers;
using GDS.Refs;

namespace GDS
{
    public class GDSParser
    {
        public readonly XmlDocument Document;
        public readonly GDSParserManager Parser;
        public Dictionary<string, object> ReferenceObjects => referenceObjects;

        private delegate object CustomParser(XmlNode node);
        private readonly Dictionary<Type, FieldMap> fieldMaps = new Dictionary<Type, FieldMap>();
        private readonly Dictionary<Type, CustomParser> customParsers = new Dictionary<Type, CustomParser>();
        private readonly Dictionary<string, object> referenceObjects = new Dictionary<string, object>();
        private readonly List<ReferenceRequest> referenceRequests = new List<ReferenceRequest>();

        public GDSParser(XmlDocument doc, GDSParserManager parser = null)
        {
            this.Document = doc;
            this.Parser = parser ?? new GDSParserManager();
        }

        public void LogReferences()
        {
            foreach (var r in referenceRequests)
            {
                Console.WriteLine($" - {r}");
            }
        }

        private FieldMap GetFieldMap(Type type)
        {
            return fieldMaps.TryGetValue(type, out var found) ? found : fieldMaps.AddAndReturn(type, new FieldMap(type));
        }

        private CustomParser TryGetCustomParser(Type type)
        {
            if (customParsers.TryGetValue(type, out var found))
                return found;

            var method = type.GetMethod("ParseFromXML", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                customParsers.Add(type, null);
                return null;
            }

            if (!method.IsStatic)
            {
                Console.WriteLine($"Custom parse method '{method.DeclaringType.FullName}.{method.Name}' should be static.");
                customParsers.Add(type, null);
                return null;
            }

            if (method.ReturnType == typeof(void))
            {
                Console.WriteLine($"Custom parse method '{method.DeclaringType.FullName}.{method.Name}' should have a return value!");
                customParsers.Add(type, null);
                return null;
            }

            var args = method.GetParameters();
            if (args.Length != 1)
            {
                Console.WriteLine($"Custom parse method '{method.DeclaringType.FullName}.{method.Name}' should have exactly one input parameter of type {nameof(XmlNode)}.");
                customParsers.Add(type, null);
                return null;
            }

            if (!args[0].ParameterType.IsAssignableFrom(typeof(XmlNode)))
            {
                Console.WriteLine($"Custom parse method '{method.DeclaringType.FullName}.{method.Name}' should have exactly one input parameter of type {nameof(XmlNode)}.");
                customParsers.Add(type, null);
                return null;
            }

            if (args[0].IsOut)
            {
                Console.WriteLine($"Custom parse method '{method.DeclaringType.FullName}.{method.Name}' has invalid decoration on the node parameter.");
                customParsers.Add(type, null);
                return null;
            }

            object Parse(XmlNode node) => method.Invoke(null, new object[] { node });
            customParsers.Add(type, Parse);

            return Parse;
        }

        private IList MakeGenericList(Type type, int capacity)
        {
            return Activator.CreateInstance(typeof(List<>).MakeGenericType(type), capacity) as IList;
        }

        public IEnumerable<object> MakeAllObjects(Type defaultType, bool includeAbstracts = false)
        {
            var root = Document.GetRootNode();
            foreach (XmlNode node in root)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                if (!includeAbstracts && node.EvaluateBoolAttribute("Abstract", false))
                    continue;

                object obj = null;
                try
                {
                    obj = NodeToObject(node, defaultType, default);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                if (obj != null)
                    yield return obj;
            }
        }

        public object NodeToObject(XmlNode node, Type expectedType, in Context context)
        {
            string overrideClass = node.TryGetAttr("Class");
            Type type = (overrideClass != null) ? this.Parser.ResolveType(overrideClass) : expectedType;

            if (type == null)
                throw new Exception("No type!");

            if (node.IsXmlRefNode(type) && context.Parent != null && !context.ListNoRef)
            {
                FieldReferenceRequest req = new FieldReferenceRequest(context.Parent, node.InnerText);
                req.Field = context.Field;
                req.ListIndex = context.ListIndex;
                referenceRequests.Add(req);
                return null;
            }

            bool noRef = node.EvaluateBoolAttribute("NoRef", false) || context.ListNoRef;

            // Custom C# parser, using static ParseFromXML method.
            var csharpParser = TryGetCustomParser(type);
            if (csharpParser != null)
            {
                var parsed = csharpParser(node);
                MaybeInvokeConstructed(parsed, node);
                if (!noRef)
                    MaybeRegisterReference(parsed);
                return parsed;
            }

            // Try find regular parser.
            var parser = Parser.TryGetParserFor(type);
            if (parser != null)
            {
                var parsed = parser.TryParse(node, type, new ParserContext()
                {
                    GDSParser = this,
                    Manager = Parser,
                    NodeContext = context
                });
                MaybeInvokeConstructed(parsed, node);
                if (!noRef)
                    MaybeRegisterReference(parsed);
                return parsed;
            }

            // List types...
            if (type.IsListType())
                return NodeToList(node, type, context);

            // Fall back to creating from class fields.
            return NodeToClass(node, type, noRef);
        }

        private object NodeToList(XmlNode node, Type type, in Context context)
        {
            if (!node.HasChildNodes)
                return null;

            if (!node.IsListNode())
                Console.WriteLine($"The node {node.Name} is a list, but it's items are not named <li>. Change item names to all be <li> or add the \"IsList\"=\"True\" attribute.");

            bool noRef = node.EvaluateBoolAttribute("NoRef", false);
            var listType = type.GetFirstGenericArgument();
            var list = MakeGenericList(listType, node.ChildNodes.Count);

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                    continue;

                if (!child.HasChildNodes)
                    continue;

                int index = list.Count;
                var newItem = NodeToObject(child, listType, new Context()
                {
                    Parent = list,
                    Field = context.Field, // Pass through the list field, because the list item does not have a field. Used for references.
                    ListIndex = index,
                    ListNoRef = noRef
                });
                list.Add(newItem);
            }
            return list;
        }

        private object NodeToClass(XmlNode node, Type type, bool noRef)
        {
            if (type.IsAbstract)
                throw new Exception($"Cannot do NodeToClass for abstract class: '{type.FullName}'");

            var instance = Activator.CreateInstance(type);
            var map = GetFieldMap(type);

            bool anyNodes = false;
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element)
                    continue;

                if (!child.HasChildNodes)
                    continue;

                // TODO handle lists, custom parsers.

                var field = map.TryGetField(child.Name);
                if (field == null)
                {
                    Console.WriteLine($"Failed to find field '{child.Name}' in {type.FullName}");
                    continue;
                }

                Type subType = field.FieldType.ExtractUnderlyingType();
                var value = NodeToObject(child, subType, new Context()
                {
                    Field = field,
                    Parent = instance
                });
                field.SetValue(instance, value);
                anyNodes = true;
            }

            if (!anyNodes)
                Console.WriteLine($"Warning: No data provided for class node {node.Name}, of type {type.Name}."); 

            MaybeInvokeConstructed(instance, node);
            if (!noRef)
                MaybeRegisterReference(instance);
            return instance;
        }

        private void MaybeInvokeConstructed(object obj, XmlNode node)
        {
            if (obj is IOnXmlConstruction cons)
                cons.OnConstructedFromXml(node);
        }

        private void MaybeRegisterReference(object obj)
        {
            if (obj == null || !obj.GetType().IsXmlRefType())
                return;

            string key = (obj as IXmlReference).XmlReferenceID;
            if (key == null)
            {
                Console.WriteLine($"A {obj.GetType().Name} ({obj}) returned a null XmlReferenceID!");
                return;
            }

            referenceObjects[key] = obj;
        }

        public void InjectReference(in IXmlReference obj)
        {
            referenceObjects[obj.XmlReferenceID] = obj;
        }

        public void InjectReference(string key, object obj)
        {
            referenceObjects[key] = obj;
        }

        public bool TryGetReferenceObject(string key, out object value) => referenceObjects.TryGetValue(key, out value);

        public void AddReferenceRequest(ReferenceRequest request)
        {
            if (request == null)
                return;

            if(!referenceRequests.Contains(request))
                referenceRequests.Add(request);
        }

        public void ResolveReferences()
        {
            foreach (var r in referenceRequests)
            {
                if (!referenceObjects.TryGetValue(r.RefID, out var refObj))
                {
                    Console.WriteLine($"Failed to find reference object: {r}");
                    continue;
                }

                r.SupplyValue(refObj);
            }
        }

        public struct Context
        {
            public object Parent;
            public FieldInfo Field;
            public int? ListIndex;
            public bool ListNoRef;
        }
    }
}
