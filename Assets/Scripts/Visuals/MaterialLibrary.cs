using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld.Visuals
{
    public static class MaterialLibrary
    {
        static readonly Dictionary<Color, Material> windowCache = new Dictionary<Color, Material>();
        static readonly Dictionary<Color, Material> brickCache = new Dictionary<Color, Material>();
        static readonly Dictionary<Color, Material> plainCache = new Dictionary<Color, Material>();
        static readonly Dictionary<int, Material> asphaltCache = new Dictionary<int, Material>();
        static Texture2D asphaltTex;
        static Texture2D plasterTex;
        static Texture2D brickTex;

        public static Material GetWindowMaterial(Color baseColor)
        {
            if (windowCache.TryGetValue(baseColor, out var m) && m != null) return m;
            var tex = GenerateWindowGTA5(baseColor);
            var mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = tex;
            mat.color = Color.white;
            windowCache[baseColor] = mat;
            return mat;
        }

        public static Material GetBrickMaterial(Color baseColor)
        {
            if (brickCache.TryGetValue(baseColor, out var m) && m != null) return m;
            if (brickTex == null) brickTex = GenerateBrickTexture();
            var mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = brickTex;
            mat.color = baseColor;
            brickCache[baseColor] = mat;
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

        static Texture2D GenerateWindowGTA5(Color wall)
        {
            int size = 512;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;
            Color wallCol = wall * 0.88f; wallCol.a = 1f;
            Color frame = new Color(0.23f, 0.23f, 0.24f);
            Color frameDark = new Color(0.16f, 0.16f, 0.17f);
            Color sill = new Color(0.78f, 0.76f, 0.72f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int cell = 64;
                    int cx = x % cell;
                    int cy = y % cell;
                    int wx = x / cell;
                    int wy = y / cell;

                    bool isBorder = cx < 3 || cy < 3 || cx >= cell - 3 || cy >= cell - 3;
                    bool isVertDiv = (cx == 31 || cx == 32) && cy >= 3 && cy < cell - 3;
                    bool isSill = cy >= cell - 7 && cy < cell - 3 && cx >= 3 && cx < cell - 3;

                    if (isBorder || isVertDiv)
                    {
                        bool corner = (cx < 3 && cy < 3) || (cx >= cell - 3 && cy < 3);
                        tex.SetPixel(x, y, corner ? frameDark : frame);
                    }
                    else if (isSill)
                    {
                        float n = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                        Color c = Color.Lerp(sill, new Color(0.65f, 0.64f, 0.61f), n * 0.18f);
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        int seed = wx * 17 + wy * 37;
                        float r = Mathf.Abs(Mathf.Sin(seed * 12.9898f) * 43758.5f) % 1f;
                        float n = Mathf.PerlinNoise(x * 0.04f + wx * 5f, y * 0.04f + wy * 5f);
                        bool isAC = (cx > 22 && cx < 30 && cy > 8 && cy < 16) && (seed % 5 == 0);
                        if (isAC)
                        {
                            tex.SetPixel(x, y, new Color(0.52f, 0.52f, 0.53f));
                            continue;
                        }

                        bool on = r > 0.38f;
                        if (n > 0.62f) on = !on;
                        if (seed % 7 == 0 && (cx + cy) % 11 == 0) on = false;

                        Color glass;
                        if (on)
                        {
                            float flicker = 0.92f + Mathf.PerlinNoise(Time.time * 0.03f + seed, 0f) * 0.16f;
                            glass = new Color(1f * flicker, 0.94f * flicker, 0.58f * flicker);
                            float refl = Mathf.Clamp01((cx / (float)cell) * 0.6f + n * 0.2f);
                            glass = Color.Lerp(glass, new Color(0.72f, 0.85f, 1f), refl * 0.22f);
                            if ((x + y) % 13 == 0) glass = Color.Lerp(glass, Color.white, 0.12f);
                        }
                        else
                        {
                            float dark = 0.13f + n * 0.09f;
                            glass = new Color(dark + 0.03f, dark + 0.05f, dark + 0.11f);
                            float curtain = Mathf.Sin((cx + seed) * 0.4f) * 0.06f;
                            glass = Color.Lerp(glass, new Color(0.22f, 0.21f, 0.28f), Mathf.Abs(curtain));
                        }
                        Color baseWall = wallCol;
                        float stain = Mathf.PerlinNoise(x * 0.015f, y * 0.015f) * 0.08f;
                        baseWall = Color.Lerp(baseWall, new Color(0.35f, 0.32f, 0.30f), stain);
                        tex.SetPixel(x, y, Color.Lerp(baseWall, glass, 0.90f));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        static Texture2D GenerateAsphaltTexture()
        {
            int size = 256;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.06f, y * 0.06f);
                    float n2 = Mathf.PerlinNoise(x * 0.22f, y * 0.22f);
                    float crack = Mathf.PerlinNoise(x * 0.035f, y * 0.035f);
                    float c = 0.19f + n * 0.09f + n2 * 0.04f;
                    if (crack > 0.72f) c -= 0.04f;
                    if ((x + y) % 23 == 0) c += 0.012f;
                    if (x % 64 == 0 || y % 64 == 0) c += 0.018f;
                    tex.SetPixel(x, y, new Color(c, c, c + 0.012f));
                }
            tex.Apply();
            return tex;
        }

        static Texture2D GeneratePlasterTexture()
        {
            int size = 256;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.09f, y * 0.09f);
                    float n2 = Mathf.PerlinNoise(x * 0.03f, y * 0.03f);
                    float c = 0.87f + n * 0.08f + n2 * 0.04f;
                    float stain = Mathf.PerlinNoise(x * 0.015f, y * 0.015f);
                    if (stain > 0.75f) c -= 0.07f;
                    tex.SetPixel(x, y, new Color(c, c, c));
                }
            tex.Apply();
            return tex;
        }

        static Texture2D GenerateBrickTexture()
        {
            int size = 256;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            Color brick1 = new Color(0.62f, 0.33f, 0.25f);
            Color brick2 = new Color(0.58f, 0.30f, 0.22f);
            Color mortar = new Color(0.78f, 0.76f, 0.72f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    bool isMortarY = y % 32 < 3;
                    bool offset = (y / 32) % 2 == 1;
                    int xx = offset ? (x + 32) % 64 : x % 64;
                    bool isMortarX = xx % 64 < 3;
                    if (isMortarY || isMortarX)
                    {
                        tex.SetPixel(x, y, mortar);
                    }
                    else
                    {
                        float var = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.12f;
                        Color b = (xx + y) % 3 == 0 ? brick1 : brick2;
                        b = Color.Lerp(b, new Color(0.35f, 0.25f, 0.2f), var);
                        tex.SetPixel(x, y, b);
                    }
                }
            tex.Apply();
            return tex;
        }
    }
}
