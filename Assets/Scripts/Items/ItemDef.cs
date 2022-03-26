using Defs;
using UnityEngine;

namespace Items
{
    public class ItemDef : Def
    {
        /// <summary>
        /// The maximum number of items allowed in a stack of this type of items.
        /// Must be at least 1.
        /// </summary>
        public int MaxStackCount = 1;

        /// <summary>
        /// The general icon for this item.
        /// </summary>
        public Sprite Icon;

        public override void GetConfigErrors(DefConfigErrors report)
        {
            base.GetConfigErrors(report);

            if (string.IsNullOrWhiteSpace(Label))
                report.Error("Item has no label!", nameof(Label));

            if (string.IsNullOrWhiteSpace(Description))
                report.Error("Item has no description!", nameof(Description));

            if (Icon == null)
                report.Warn("Item is missing an icon. The placeholder icon will be used.", nameof(Icon));
        }
    }
}
