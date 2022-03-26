using System.Collections.Generic;
using UnityEngine;

namespace Defs.Prefabs
{
    public class Prefab : Def
    {
        public static Prefab LatestPrefab;

        public GameObject GameObject;
        public List<PrefabComponent> Comps = new List<PrefabComponent>();

        public Prefab()
        {
            LatestPrefab = this;
            Label = "My Object";
            Description = "This is a prefab";
            GameObject = PrefabManager.CreateNewPrefab(Label);
        }

        public override void GetConfigErrors(DefConfigErrors report)
        {
            base.GetConfigErrors(report);
            report.AssertNotNull(Comps, nameof(Comps));
        }

        public virtual GameObject Instantiate()
        {
            return null;
        }
    }
}
