using System;
using System.Collections;
using System.Reflection;

namespace GDS.Refs
{
    public class FieldReferenceRequest : ReferenceRequest
    {
        public override Type ExpectedType
        {
            get
            {
                if (ListIndex != null)
                    return Owner.GetType().GetFirstGenericArgument();
                return Field.Type;
            }
            set
            {
                // Ignore.
            }
        }

        public readonly object Owner;
        public FieldWrapper Field;
        public int? ListIndex;

        public FieldReferenceRequest(object owner, string refID)
        {
            Owner = owner;
            RefID = refID;
        }

        public override void SupplyValue(object value)
        {
            if (value == null)
                return;

            if (!ExpectedType.IsInstanceOfType(value))
            {
                Console.WriteLine($"Cannot assign an object of type {value.GetType().FullName} to a field of type {ExpectedType.FullName} for this reference: {this}");
                return;
            }

            if (ListIndex != null)
            {
                var list = Owner as IList;
                list[ListIndex.Value] = value;
                return;
            }

            Field.SetValue(Owner, value);
        }

        public override string ToString()
        {
            if (ListIndex != null)
                return $"[List: {Field.Path}, #{ListIndex}] ({Owner.GetType().GetFirstGenericArgument().Name}) '{RefID}'";

            return $"[Field: {Field.Path}] ({Field.Type.Name}) '{RefID}'";
        }
    }
}
