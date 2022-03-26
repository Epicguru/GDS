using UnityEngine;
using UnityEngine.SceneManagement;

namespace Defs.Prefabs
{
    public static class PrefabManager
    {
        private static Scene scene;

        public static void Init()
        {
            scene = SceneManager.CreateScene("Runtime Prefabs");
        }

        public static GameObject CreateNewPrefab(string name)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            SceneManager.MoveGameObjectToScene(go, scene);
            return go;
        }
    }
}
