using System;
using System.Collections.Generic;

namespace Defs
{
    public static class DefDatabase
    {
        private static readonly List<Def> emptyDefList = new List<Def>();

        public static IEnumerable<Def> AllDefs => allDefs;
        public static int TotalDefCount => allDefs.Count;

        private static readonly HashSet<Def> allDefs = new HashSet<Def>();
        private static readonly Dictionary<string, Def> idToDef = new Dictionary<string, Def>();
        private static readonly Dictionary<Type, List<Def>> defsByType = new Dictionary<Type, List<Def>>();

        /// <summary>
        /// Attempts to add a def to the database. Returns true when successful.
        /// In order for the def to be added, it must have a unique <see cref="Def.DefID"/>,
        /// and not already be added.
        /// </summary>
        /// <param name="def">The def to add.</param>
        /// <returns>True if added successfully, false otherwise.</returns>
        public static bool AddDef(Def def)
        {
            if (def == null)
                return false;

            if (!allDefs.Add(def))
                return false;

            if (!idToDef.TryAdd(def.DefID, def))
            {
                allDefs.Remove(def);
                return false;
            }

            if (!defsByType.TryGetValue(def.GetType(), out var list))
            {
                list = new List<Def>();
                defsByType.Add(def.GetType(), list);
            }
            list.Add(def);

            return true;
        }

        /// <summary>
        /// Tries to get a def based on it's <see cref="Def.DefID"/>.
        /// Returns null if the def could not be found. See also <see cref="Get{T}(string)"/>
        /// </summary>
        public static Def Get(string id)
        {
            if (id == null || !idToDef.TryGetValue(id, out var found))
                return null;
            return found;
        }

        /// <summary>
        /// Tries to get a def based on it's <see cref="Def.DefID"/>.
        /// Returns null if the def could not be found, or if the def was not of the expected type <typeparamref name="T"/>.
        /// See also <see cref="Get(string)"/>.
        /// </summary>
        public static T Get<T>(string id) where T : Def
        {
            if (id == null || !idToDef.TryGetValue(id, out var found))
                return null;
            return found as T;
        }

        /// <summary>
        /// Gets a list of all defs of a particular type.
        /// Will never return null.
        /// </summary>
        public static IReadOnlyList<Def> GetAllDefsOfType<T>() where T : Def => GetAllDefsOfType(typeof(T));

        /// <summary>
        /// Gets a list of all defs of a particular type.
        /// Will never return null.
        /// </summary>
        public static IReadOnlyList<Def> GetAllDefsOfType(Type type)
        {
            if (type == null || !defsByType.TryGetValue(type, out var list))
                return emptyDefList;
            return list;
        }
    }
}
