#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EditorTools
{
    /// <summary>
    /// Removes Missing Script components left after deleted Part4 / renamed types.
    /// </summary>
    public static class MissingScriptCleanup
    {
        [MenuItem("DesignPatterns/Cleanup Missing Scripts")]
        public static void CleanupFromMenu()
        {
            var removed = CleanupAllScenes();
            Debug.Log(removed > 0
                ? $"Removed {removed} missing script component(s)."
                : "No missing scripts found in project scenes.");
        }

        public static void CleanupBatch()
        {
            var removed = CleanupAllScenes();
            Debug.Log(removed > 0
                ? $"Removed {removed} missing script component(s)."
                : "No missing scripts found in project scenes.");
            EditorApplication.Exit(0);
        }

        private static int CleanupAllScenes()
        {
            var removed = 0;
            var scenePaths = new[]
            {
                "Assets/Scenes/Bootstrap.unity",
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/TicTacToe.unity",
                "Assets/Scenes/Adventure.unity",
                "Assets/Scenes/EscapeRoom.unity",
                "Assets/Scenes/EscapeRoomArchitecture.unity"
            };

            foreach (var path in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                removed += CleanupScene(scene);
                EditorSceneManager.SaveScene(scene);
            }

            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("TextMesh Pro"))
                {
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(path);
                var count = RemoveMissing(root);
                if (count > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    removed += count;
                }

                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            return removed;
        }

        private static int CleanupScene(Scene scene)
        {
            var removed = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                removed += RemoveMissing(root);
            }

            return removed;
        }

        private static int RemoveMissing(GameObject root)
        {
            var removed = 0;
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
            }

            return removed;
        }
    }
}
#endif
