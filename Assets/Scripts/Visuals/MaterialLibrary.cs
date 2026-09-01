using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld.Visuals
{
    public static class MaterialLibrary
    {
        static readonly Dictionary<Color, Material> windowCache = new Dictionary<Color, Material>();
        static readonly Dictionary<Color, Material> plainCache = new Dictionary<Color, Material>();
        static readonly Dictionary<int, Material> asphaltCache = new Dictionary<int, Material>();
        static Texture2D asphaltTex;
        static Texture2D plasterTex;

        public static Material GetWindowMaterial(Color baseColor)
        {
            if (windowCache.TryGetValue(baseColor, out var m) && m != null) return m;
            var tex = GenerateWindowTexture(baseColor);
            var mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = tex;
            mat.color = Color.white;
            windowCache[baseColor] = mat;
            return mat;
        }

        public static Material GetPlain(Color c)
        {
            if (plainCache.TryGetValue(c, out var m) && m != null) return m;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = c;
            plainCache[c] = mat;
            return mat;
        }

        public static Material GetAsphalt()
        {
            int key = 0;
            if (asphaltCache.TryGetValue(key, out var m) && m != null) return m;
            if (asphaltTex == null) asphaltTex = GenerateAsphaltTexture();
            var mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = asphaltTex;
            mat.color = Color.white;
            asphaltCache[key] = mat;
            return mat;
        }

        public static Material GetPlaster(Color baseColor)
        {
            if (plasterTex == null) plasterTex = GeneratePlasterTexture();
            var mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = plasterTex;
            mat.color = baseColor;
            return mat;
        }

        static Texture2D GenerateWindowTexture(Color wall)
        {
            int size = 256;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            Color frame = new Color(0.26f, 0.26f, 0.27f);
            Color frameDark = new Color(0.18f, 0.18f, 0.19f);
            Color wallCol = wall * 0.9f; wallCol.a = 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int cellX = x % 32;
                    int cellY = y % 32;
                    int wx = x / 32;
                    int wy = y / 32;

                    bool isFrame = cellX < 2 || cellY < 2 || cellX == 31 || cellY == 31;
                    bool isInnerFrame = (cellX == 14 || cellX == 15) && cellY > 2 && cellY < 30;
                    if (isFrame || isInnerFrame)
                    {
                        bool isCorner = (cellX < 2 && cellY < 2) || (cellX >= 30 && cellY < 2);
                        tex.SetPixel(x, y, isCorner ? frameDark : frame);
                    }
                    else
                    {
                        bool leftPane = cellX < 14;
                        float n = Mathf.PerlinNoise(x * 0.05f + wx * 10f, y * 0.05f + wy * 10f);
                        bool on = false;
                        int seed = wx * 7 + wy * 13;
                        float r = (Mathf.Sin(seed * 12.9898f) * 43758.5f) % 1f;
                        if (r < 0) r = -r;
                        on = r > 0.42f;
                        if (n > 0.6f) on = !on;

                        Color glass;
                        if (on)
                        {
                            float flicker = 0.9f + Mathf.PerlinNoise(Time.time * 0.1f + seed, 0f) * 0.15f;
                            glass = new Color(1f * flicker, 0.93f * flicker, 0.55f * flicker);
                            if (leftPane && (x % 3 == 0)) glass = Color.Lerp(glass, new Color(0.6f, 0.75f, 1f), 0.18f);
                        }
                        else
                        {
                            float dark = 0.14f + n * 0.08f;
                            glass = new Color(dark + 0.04f, dark + 0.06f, dark + 0.12f);
                            if ((x + y) % 9 == 0) glass = Color.Lerp(glass, Color.white, 0.07f);
                        }
                        tex.SetPixel(x, y, Color.Lerp(wallCol, glass, 0.92f));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        static Texture2D GenerateAsphaltTexture()
        {
            int size = 128;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    float n2 = Mathf.PerlinNoise(x * 0.25f, y * 0.25f);
                    float c = 0.20f + n * 0.09f + n2 * 0.03f;
                    if ((x + y) % 17 == 0) c += 0.015f;
                    tex.SetPixel(x, y, new Color(c, c, c + 0.01f));
                }
            tex.Apply();
            return tex;
        }

        static Texture2D GeneratePlasterTexture()
        {
            int size = 128;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                    float c = 0.88f + n * 0.07f;
                    tex.SetPixel(x, y, new Color(c, c, c));
                }
            tex.Apply();
            return tex;
        }
    }
}
