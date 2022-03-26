using System;

namespace GDS.Refs
{
    public class GenericReferenceRequest<T> : ReferenceRequest
    {
        public Action<T> OnReferenceResolved;

        public override Type ExpectedType
        {
            get
            {
                return typeof(T);
            }
            set
            {
                // Ignored.
            }
        }

        public GenericReferenceRequest(string refId, Action<T> onResolved)
        {
            base.RefID = refId;
            OnReferenceResolved = onResolved;
        }

        public override void SupplyValue(object value)
        {
            if (value is T t)
                OnReferenceResolved?.Invoke(t);
        }
    }
}
