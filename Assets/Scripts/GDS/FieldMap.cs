
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GDS
{
    internal class FieldMap
    {
        public readonly Type ForType;

        private readonly Dictionary<string, FieldWrapper> fields = new Dictionary<string, FieldWrapper>();

        public FieldMap(Type forType)
        {
            this.ForType = forType;
        }

        public FieldWrapper TryGetField(string name)
        {
            if (fields.TryGetValue(name, out var found))
                return found;

            var prop = ForType.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop != null)
            {
                var wrapper = new FieldWrapper(prop);
                fields.Add(wrapper.Name, wrapper);
                return wrapper;
            }

            var field = ForType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                var wrapper = new FieldWrapper(field);
                fields.Add(wrapper.Name, wrapper);
                return wrapper;
            }

            fields.Add(name, null);
            return null;
        }
    }

    public class FieldWrapper
    {
        public string Name => field?.Name ?? property.Name;
        public Type Type => field?.FieldType ?? property.PropertyType;
        public string Path => field?.GetPath() ?? property.GetPath();

        private readonly FieldInfo field;
        private readonly PropertyInfo property;

        public FieldWrapper(FieldInfo field)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public FieldWrapper(PropertyInfo property)
        {
            this.property = property ?? throw new ArgumentNullException(nameof(property));
        }

        public object GetValue(object obj)
        {
            if (field != null)
                return field.GetValue(obj);
            return property.GetValue(obj);
        }

        public void SetValue(object obj, object value)
        {
            if (field != null)
                field.SetValue(obj, value);
            else
                property.SetValue(obj, value);
        }
    }
}
