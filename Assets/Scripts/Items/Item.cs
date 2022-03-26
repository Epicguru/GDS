using Items.Object;
using UnityEngine;

namespace Items
{
    /// <summary>
    /// An item is something than can be picked up or stored.
    /// Certain items may also be equipped.
    /// </summary>
    public class Item
    {
        public virtual string Label => Def.Label;
        public virtual Sprite Icon => Def.Icon;

        public int StackCount { get; private set; } = 1;
        public readonly ItemDef Def;

        public Item(ItemDef def)
        {
            Def = def ?? throw new System.ArgumentNullException(nameof(def));
        }

        public virtual DroppedItem MakeDroppedItem()
        {
            var prefab = Def.DroppedItem.Prefab;
            if (prefab == null)
                return null;

            var dropped = UnityEngine.Object.Instantiate(prefab);
            dropped.Item = this;
            return dropped;
        }

        public override string ToString()
        {
            return $"{StackCount}x {Label}";
        }
    }
}
