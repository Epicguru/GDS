using Defs;
using Items.Object;
using UnityEngine;

namespace Items
{
    public class DroppedItemProperties
    {
        public static DroppedItem DefaultPrefab;

        public DroppedItem Prefab;
        public Vector2 Scale = Vector2.one;

        public void GetConfigErrors(DefConfigErrors report)
        {
            if (Prefab == null)
                Prefab = DefaultPrefab;
        }
    }
}
