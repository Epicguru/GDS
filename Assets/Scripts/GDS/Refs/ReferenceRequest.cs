using System;

namespace GDS.Refs
{
    public abstract class ReferenceRequest
    {
        /// <summary>
        /// Makes a <see cref="GenericReferenceRequest{T}"/>.
        /// </summary>
        public static GenericReferenceRequest<T> Make<T>(string refId, Action<T> onResolved)
        {
            return new GenericReferenceRequest<T>(refId, onResolved);
        }

        public string RefID;
        public abstract Type ExpectedType { get; set; }

        public abstract void SupplyValue(object value);

        public override string ToString()
        {
            return $"[{ExpectedType?.Name}] {RefID}";
        }
    }
}
