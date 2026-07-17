using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core
{
    /// <summary>
    /// Keeps shared UI shell alive and ensures a fallback camera exists
    /// for menu scenes that do not include their own Main Camera.
    /// </summary>
    public class PersistentUIInitializer : MonoBehaviour
    {
        private Camera _fallbackCamera;
        private Canvas[] _persistentCanvases;
        private GraphicRaycaster[] _persistentRaycasters;
        private bool _docsCanvasHidden;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureFallbackCamera();

            var canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                // Above gameplay HUDs (EscapeRoomCanvas uses 200).
                canvas.sortingOrder = 250;
            }

            if (GetComponent<AdaptiveCanvasGuard>() == null && GetComponent<CanvasScaler>() != null)
            {
                gameObject.AddComponent<AdaptiveCanvasGuard>();
            }

            if (GetComponent<UiLayoutFixer>() == null)
            {
                gameObject.AddComponent<UiLayoutFixer>();
            }

            if (GetComponent<PauseButtonUI>() == null)
            {
                gameObject.AddComponent<PauseButtonUI>();
            }

            // Cache canvases/raycasters once; PersistentUI survives scene loads (DDOL).
            _persistentCanvases = GetComponentsInChildren<Canvas>(true);
            _persistentRaycasters = GetComponentsInChildren<GraphicRaycaster>(true);
            ApplyDocsSceneCanvasVisibility(SceneManager.GetActiveScene().name);

            StripMissingScripts(gameObject);
        }

        private static void StripMissingScripts(GameObject target)
        {
#if UNITY_EDITOR
            // Remove missing-script components on the root and all descendants.
            foreach (var t in target.GetComponentsInChildren<Transform>(true))
            {
                UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }
#endif
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureFallbackCamera();
            ApplyDocsSceneCanvasVisibility(scene.name);
        }

        private void ApplyDocsSceneCanvasVisibility(string sceneName)
        {
            // Architecture docs viewer owns its own canvas; disable PersistentUI canvases to avoid overlap.
            var hideForDocs = sceneName == "EscapeRoomArchitecture";
            if (hideForDocs == _docsCanvasHidden)
            {
                return;
            }

            _docsCanvasHidden = hideForDocs;

            if (_persistentCanvases != null)
            {
                foreach (var c in _persistentCanvases)
                {
                    if (c != null)
                    {
                        c.enabled = !hideForDocs;
                    }
                }
            }

            if (_persistentRaycasters != null)
            {
                foreach (var r in _persistentRaycasters)
                {
                    if (r != null)
                    {
                        r.enabled = !hideForDocs;
                    }
                }
            }
        }

        private void EnsureFallbackCamera()
        {
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Camera sceneCamera = null;
            foreach (var camera in cameras)
            {
                if (camera == _fallbackCamera)
                {
                    continue;
                }

                if (camera.enabled && camera.gameObject.activeInHierarchy)
                {
                    sceneCamera = camera;
                    break;
                }
            }

            if (sceneCamera != null)
            {
                if (_fallbackCamera != null)
                {
                    _fallbackCamera.enabled = false;
                    var listener = _fallbackCamera.GetComponent<AudioListener>();
                    if (listener != null)
                    {
                        listener.enabled = false;
                    }
                }

                return;
            }

            if (_fallbackCamera == null)
            {
                var cameraObject = new GameObject("UIFallbackCamera");
                cameraObject.transform.SetParent(transform, false);
                _fallbackCamera = cameraObject.AddComponent<Camera>();
                _fallbackCamera.clearFlags = CameraClearFlags.SolidColor;
                _fallbackCamera.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
                _fallbackCamera.orthographic = true;
                _fallbackCamera.orthographicSize = 5f;
                _fallbackCamera.depth = -100f;
                _fallbackCamera.cullingMask = 0;
                cameraObject.AddComponent<AudioListener>();
            }

            _fallbackCamera.enabled = true;
            var fallbackListener = _fallbackCamera.GetComponent<AudioListener>();
            if (fallbackListener != null)
            {
                fallbackListener.enabled = true;
            }
        }
    }
}
