using System.Xml;
using UnityEngine;

namespace Defs.Prefabs
{
    public class PrefabComponent
    {
        public Component Component;
        public XmlNode XmlNode;

        public override string ToString()
        {
            return Component.ToString();
        }
    }
}
