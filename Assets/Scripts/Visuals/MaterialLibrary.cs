using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld.Visuals
{
    public static class MaterialLibrary
    {
        static readonly Dictionary<Color, Material> windowCache = new Dictionary<Color, Material>();
        static readonly Dictionary<Color, Material> plainCache = new Dictionary<Color, Material>();

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

        static Texture2D GenerateWindowTexture(Color wall)
        {
            int size = 128;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            Color wallDark = wall * 0.85f;
            wallDark.a = 1f;
            Color windowOn = new Color(1f, 0.92f, 0.55f);
            Color windowOff = new Color(0.18f, 0.22f, 0.28f);
            Color frame = new Color(0.22f, 0.22f, 0.23f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isFrameX = (x % 32 == 0) || (x % 32 == 1);
                    bool isFrameY = (y % 32 == 0) || (y % 32 == 1);
                    bool isFrame = isFrameX || isFrameY;
                    if (isFrame)
                    {
                        tex.SetPixel(x, y, frame);
                    }
                    else
                    {
                        int wx = x / 32;
                        int wy = y / 32;
                        bool on = ((wx + wy) % 2 == 0) ^ ((x + y) % 7 == 0);
                        on = on && ((wx * 3 + wy * 5) % 4 != 0);
                        tex.SetPixel(x, y, on ? windowOn : windowOff);
                    }
                }
            }

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    Color c = tex.GetPixel(x, y);
                    if (c != frame) tex.SetPixel(x, y, Color.Lerp(wallDark, c, 0.95f));
                }

            tex.Apply();
            return tex;
        }
    }
}
