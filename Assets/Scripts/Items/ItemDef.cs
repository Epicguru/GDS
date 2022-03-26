using System;
using Defs;
using UnityEngine;

namespace Items
{
    public class ItemDef : Def
    {
        private static readonly object[] args = new object[1];

        /// <summary>
        /// The maximum number of items allowed in a stack of this type of items.
        /// Must be at least 1.
        /// </summary>
        public int MaxStackCount = 1;

        /// <summary>
        /// The general icon for this item.
        /// </summary>
        public Sprite Icon;

        /// <summary>
        /// Properties for this item's dropped state.
        /// </summary>
        public DroppedItemProperties DroppedItem = new DroppedItemProperties();

        /// <summary>
        /// The class of the <see cref="Item"/> that this def should be used with.
        /// </summary>
        public Type ItemClass = typeof(Item);

        public Mesh Mesh;
        public Material Material;

        public override void GetConfigErrors(DefConfigErrors report)
        {
            base.GetConfigErrors(report);

            if (string.IsNullOrWhiteSpace(Label))
                report.Error("Item has no label!", nameof(Label));

            if (string.IsNullOrWhiteSpace(Description))
                report.Error("Item has no description!", nameof(Description));

            if (Icon == null)
                report.Warn("Item is missing an icon. The placeholder icon will be used.", nameof(Icon));

            if (MaxStackCount < 1)
            {
                report.Error($"Invalid {nameof(MaxStackCount)}: {MaxStackCount}. Must be at least 1. It has been changed to 1.");
                MaxStackCount = 1;
            }

            if (report.AssertNotNull(ItemClass, nameof(ItemClass)))
            {
                if (!ItemClass.IsAssignableFrom(typeof(Item)))
                    report.Error($"Item class '{ItemClass.FullName}' is invalid, because it does not inherit from Item!");
            }

            if (report.AssertNotNull(DroppedItem, nameof(DroppedItem)))
                DroppedItem.GetConfigErrors(report);
        }

        public virtual Item MakeItem()
        {
            args[0] = this;
            return Activator.CreateInstance(ItemClass, args) as Item;
        }
    }
}
