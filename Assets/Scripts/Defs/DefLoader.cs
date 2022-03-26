using GDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utility;

namespace Defs
{
    public class DefLoader : MonoBehaviour
    {
        public static List<AssetReferenceRequest> AssetRequests = new List<AssetReferenceRequest>();

        private static GDSBuilder builder;
        private static GDSParserManager parserManager;
        private static readonly Dictionary<string, Type> localTypes = new Dictionary<string, Type>();

        private static Type ResolveType(string name)
        {
            if (localTypes.TryGetValue(name, out var found))
                return found;
            return Type.GetType(name);
        }

        private void Awake()
        {
            UnitySystemConsoleRedirector.Redirect();

            foreach (var type in typeof(DefLoader).Assembly.GetTypes())
            {
                if (type.IsCompilerGenerated())
                    continue;

                string name = type.Name;

                if (localTypes.ContainsKey(name))
                {
                    Debug.LogWarning($"Duplicate class name: '{name}'. References to these classes will have to be fully qualified in xml nodes.");
                    localTypes.Remove(name);
                    continue;
                }
                localTypes.Add(name, type);
            }

            StartLoad();
            Debug.Log(Application.dataPath);
            LoadAllFrom(Application.dataPath);

            var defs = FinalizeLoad();
            foreach (var def in defs)
            {
                if (!DefDatabase.AddDef(def))
                    Debug.LogError($"Failed to register loaded def '{def}', probably due to duplicate def ID.");
            }

            var errors = new DefConfigErrors();
            foreach (var def in defs)
            {
                errors.CurrentDef = def;
                def.GetConfigErrors(errors);
            }

            if (errors.Errors.Count > 0)
                Debug.LogError($"There are {errors.Errors.Count} def errors:");
            else
                Debug.Log("There are no def errors.");
            foreach (var error in errors.Errors)
                Debug.LogError(error);

            if (errors.Warnings.Count > 0)
                Debug.LogWarning($"There are {errors.Warnings.Count} def warnings:");
            else
                Debug.Log("There are no def errors.");
            foreach (var warning in errors.Warnings)
                Debug.LogWarning(warning);

            foreach (var def in defs)
                def.PostLoad();

            Debug.Log($"Finished loading {DefDatabase.TotalDefCount} defs.");
        }

        public static void StartLoad()
        {
            if (builder != null)
            {
                Debug.LogError("Load is already started.");
                return;
            }

            if (parserManager == null)
            {
                parserManager = new GDSParserManager();
                parserManager.AddParsersFromAssembly(typeof(GDSParserManager).Assembly);
                parserManager.AddParsersFromAssembly(typeof(DefLoader).Assembly);
                parserManager.TypeResolver = ResolveType;
            }

            builder = new GDSBuilder();
        }

        public static void LoadAllFrom(string folderPath)
        {
            if (builder == null)
            {
                Debug.LogError("Must call StartLoad() before calling LoadAllFrom()");
                return;
            }

            if (!Directory.Exists(folderPath))
                return;

            foreach (var file in Directory.EnumerateFiles(folderPath, "*.xml", SearchOption.AllDirectories))
            {
                XmlDocument doc;
                try
                {
                    doc = new XmlDocument();
                    doc.Load(file);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load or parse def file '{file}':");
                    Debug.LogError(e);
                    continue;
                }

                try
                {
                    builder.Digest(doc);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to digest def file '{file}':");
                    Debug.LogError(e);
                    continue;
                }
            }
        }

        public static List<Def> FinalizeLoad()
        {
            if (builder == null)
            {
                Debug.LogError("Must call StartLoad() before calling FinalizeLoad()");
                return null;
            }

            XmlDocument doc;
            try
            {
                doc = builder.Process();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to process built xml:");
                Debug.LogError(e);
                return null;
            }
            finally
            {
                builder = null;
            }

            Debug.Log(doc.MakePrettyXml());

            var parser = new GDSParser(doc, parserManager);
            var list = parser.MakeAllObjects(typeof(Def)).Cast<Def>().ToList();
            foreach (var def in list)
                def.PreResolveReferences(parser);

            // Load assets...
            Debug.Log($"Loading {AssetRequests.Count} assets refs...");
            foreach (var req in AssetRequests)
            {
                var listHandle = Addressables.LoadResourceLocationsAsync(req.RefID);
                var addresses = listHandle.WaitForCompletion();

                bool isComp = req.AssetType.IsSubclassOf(typeof(Component));
                foreach (var item in addresses)
                {
                    if (req.AssetType.IsAssignableFrom(item.ResourceType))
                    {
                        var handle = Addressables.LoadAssetAsync<UnityEngine.Object>(item);
                        req.SupplyValue(handle.WaitForCompletion());
                        break;
                    }

                    if (isComp && item.ResourceType == typeof(GameObject))
                    {
                        var handle = Addressables.LoadAssetAsync<GameObject>(item);
                        var obj = handle.WaitForCompletion();
                        if (!obj.TryGetComponent(req.AssetType, out var found))
                        {
                            Addressables.Release(handle);
                            continue;
                        }
                        req.SupplyValue(found);
                    }
                }

                Addressables.Release(listHandle);
            }
            
            parser.ResolveReferences();

            return list;
        }
    }
}
