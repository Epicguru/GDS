using System;
using GDS.Refs;

namespace Defs
{
    public class AssetReferenceRequest : FieldReferenceRequest
    {
        public Type AssetType;

        public AssetReferenceRequest(object owner, string refID, Type assetType) : base(owner, refID)
        {
            this.AssetType = assetType;
        }
    }
}
