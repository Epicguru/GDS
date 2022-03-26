using UnityEngine;

namespace Items.Object
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DroppedItem : MonoBehaviour
    {
        public Item Item;
        public int A, B, C;
        public string Message = "Default";

        public string DefID;
        public ItemDef Def;

        private SpriteRenderer sprRenderer;

        private void Awake()
        {
            DefID = Def.DefID;
            Debug.Log("AWAKE");
        }

        private void Start()
        {
            Debug.Log("START");
            sprRenderer = GetComponent<SpriteRenderer>();
            sprRenderer.sprite = Item.Icon;
            transform.localScale = new Vector3(Item.Def.DroppedItem.Scale.x, Item.Def.DroppedItem.Scale.y, 1);
        }

        private void Update()
        {
            Debug.Log("UPDATE");
        }

        private void OnEnable()
        {
            Debug.Log("ENABLE");
        }
    }
}
