using UnityEngine;

namespace Defs.Prefabs
{
    public class PrefabComponent
    {
        public readonly Component Component;

        public PrefabComponent(Component component)
        {
            this.Component = component;
        }

        public override string ToString()
        {
            return Component.ToString();
        }
    }
}
