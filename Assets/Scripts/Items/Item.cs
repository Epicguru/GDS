namespace Items
{
    /// <summary>
    /// An item is something than can be picked up or stored.
    /// Certain items may also be equipped.
    /// </summary>
    public class Item
    {
        public virtual string Label => Def.Label;
        public int StackCount { get; private set; } = 1;
        public readonly ItemDef Def;

        public Item(ItemDef def)
        {
            Def = def ?? throw new System.ArgumentNullException(nameof(def));
        }

        public override string ToString()
        {
            return $"{StackCount}x{Label}";
        }
    }
}
