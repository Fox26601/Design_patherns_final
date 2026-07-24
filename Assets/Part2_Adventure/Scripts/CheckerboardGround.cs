using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Applies a black/white checkerboard texture to the adventure ground plane.
    /// </summary>
    public class CheckerboardGround : MonoBehaviour
    {
        [SerializeField] private int cellsPerSide = 16;
        [SerializeField] private int pixelsPerCell = 16;
        [SerializeField] private Color lightColor = Color.white;
        [SerializeField] private Color darkColor = new(0.12f, 0.12f, 0.12f, 1f);
        [SerializeField] private float tiling = 8f;

        public static void EnsureOn(GameObject ground)
        {
            if (ground == null)
            {
                return;
            }

            var checker = ground.GetComponent<CheckerboardGround>();
            if (checker == null)
            {
                checker = ground.AddComponent<CheckerboardGround>();
            }

            checker.Apply();
        }

        private void Awake()
        {
            Apply();
        }

        public void Apply()
        {
            var meshRenderer = GetComponent<Renderer>();
            if (meshRenderer == null)
            {
                return;
            }

            var texture = BuildCheckerTexture();
            var shader = Shader.Find("Unlit/Texture")
                         ?? Shader.Find("Unlit/Transparent")
                         ?? Shader.Find("Standard");
            var material = new Material(shader);
            material.mainTexture = texture;
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }

            material.mainTextureScale = new Vector2(tiling, tiling);
            meshRenderer.material = material;
        }

        private Texture2D BuildCheckerTexture()
        {
            var size = Mathf.Max(2, cellsPerSide) * Mathf.Max(1, pixelsPerCell);
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                name = "AdventureCheckerboard"
            };

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var cellX = x / pixelsPerCell;
                    var cellY = y / pixelsPerCell;
                    var isDark = ((cellX + cellY) & 1) == 1;
                    texture.SetPixel(x, y, isDark ? darkColor : lightColor);
                }
            }

            texture.Apply();
            return texture;
        }
    }
}
