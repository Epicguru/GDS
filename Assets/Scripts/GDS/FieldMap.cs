
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GDS
{
    internal class FieldMap
    {
        public readonly Type ForType;

        private readonly Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();

        public FieldMap(Type forType)
        {
            this.ForType = forType;
        }

        public FieldInfo TryGetField(string name)
        {
            if (fields.TryGetValue(name, out var found))
                return found;

            found = ForType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (found != null)
                fields.Add(name, found);

            return found;
        }
    }
}
