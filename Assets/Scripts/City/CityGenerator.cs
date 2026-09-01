using System.Collections.Generic;
using UnityEngine;
using OpenWorld.Visuals;

namespace OpenWorld
{
    public class CityGenerator : MonoBehaviour
    {
        [Header("Размер города")]
        public int blocksX = 12;
        public int blocksZ = 12;
        public float blockSize = 68f;
        public float roadWidth = 14f;

        [Header("Здания")]
        public Vector2 buildingSize = new Vector2(12f, 22f);
        public Vector2 buildingHeight = new Vector2(8f, 50f);
        [Range(0f, 1f)] public float parkChance = 0.12f;
        [Range(0f, 1f)] public float emptyLotChance = 0.08f;

        [Header("Оформление")]
        public bool generateLamps = true;
        public bool generateTrees = true;
        public bool generateMountains = true;
        public int seed = 20260901;

        public float Cell => blockSize + roadWidth;
        public float TotalX => blocksX * Cell + roadWidth;
        public float TotalZ => blocksZ * Cell + roadWidth;

        static readonly Color[] BuildingPalette =
        {
            new Color(0.82f, 0.80f, 0.76f),
            new Color(0.70f, 0.68f, 0.66f),
            new Color(0.60f, 0.62f, 0.68f),
            new Color(0.75f, 0.65f, 0.55f),
            new Color(0.55f, 0.58f, 0.55f),
            new Color(0.85f, 0.84f, 0.86f),
            new Color(0.65f, 0.60f, 0.58f),
            new Color(0.50f, 0.55f, 0.62f)
        };

        readonly Dictionary<Color, Material> matCache = new Dictionary<Color, Material>();
        Material lampMat;
        Transform root;

        void Start()
        {
            if (root == null) Generate();
        }

        public void Generate()
        {
            Clear();
            var rnd = new System.Random(seed);
            root = new GameObject("City").transform;
            root.SetParent(transform, false);
            root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            CreateGround();
            CreateRoads();
            for (int x = 0; x < blocksX; x++)
                for (int z = 0; z < blocksZ; z++)
                    CreateBlock(x, z, rnd);

            if (generateLamps) CreateLamps();
            if (generateMountains) CreateMountains(rnd);
        }

        public void Clear()
        {
            if (root == null) return;
            Destroy(root.gameObject);
            root = null;
        }

        Material Mat(Color c)
        {
            if (!matCache.TryGetValue(c, out var m) || m == null)
            {
                m = new Material(Shader.Find("Standard"));
                if (m != null) m.color = c;
                matCache[c] = m;
            }
            return m;
        }

        Material LampMat()
        {
            if (lampMat == null)
            {
                lampMat = new Material(Shader.Find("Standard"));
                if (lampMat != null)
                {
                    lampMat.color = new Color(1f, 0.95f, 0.75f);
                    lampMat.EnableKeyword("_EMISSION");
                    lampMat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.55f) * 1.6f);
                }
            }
            return lampMat;
        }

        GameObject CreateBox(string name, Vector3 pos, Vector3 size, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = Mat(color);
            go.isStatic = true;
            return go;
        }

        void CreateGround()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "Ground";
            go.transform.SetParent(root, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(TotalX * 3f / 10f, 1f, TotalZ * 3f / 10f);
            go.GetComponent<MeshRenderer>().sharedMaterial = Mat(new Color(0.28f, 0.38f, 0.24f));
            go.isStatic = true;
        }

        void CreateRoads()
        {
            Material asphaltMat = MaterialLibrary.GetAsphalt();
            for (int i = 0; i <= blocksX; i++)
            {
                float x = -TotalX / 2f + roadWidth / 2f + i * Cell;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "RoadZ_" + i;
                go.transform.SetParent(root, false);
                go.transform.localPosition = new Vector3(x, 0.03f, 0f);
                go.transform.localScale = new Vector3(roadWidth, 0.06f, TotalZ);
                go.GetComponent<MeshRenderer>().sharedMaterial = asphaltMat;
                go.isStatic = true;
            }
            for (int i = 0; i <= blocksZ; i++)
            {
                float z = -TotalZ / 2f + roadWidth / 2f + i * Cell;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "RoadX_" + i;
                go.transform.SetParent(root, false);
                go.transform.localPosition = new Vector3(0f, 0.032f, z);
                go.transform.localScale = new Vector3(TotalX, 0.064f, roadWidth);
                go.GetComponent<MeshRenderer>().sharedMaterial = asphaltMat;
                go.isStatic = true;
            }
            CreateRoadMarkings();
        }

        void CreateRoadMarkings()
        {
            Color white = new Color(0.92f, 0.92f, 0.90f);
            float dashLen = 3.5f;
            float gap = 6f;
            for (int i = 0; i <= blocksX; i++)
            {
                float x = -TotalX / 2f + roadWidth / 2f + i * Cell;
                for (float z = -TotalZ / 2f + 8f; z < TotalZ / 2f - 8f; z += dashLen + gap)
                {
                    CreateBox("MarkZ", new Vector3(x, 0.07f, z), new Vector3(0.4f, 0.02f, dashLen), white);
                }
            }
            for (int i = 0; i <= blocksZ; i++)
            {
                float z = -TotalZ / 2f + roadWidth / 2f + i * Cell;
                for (float x = -TotalX / 2f + 8f; x < TotalX / 2f - 8f; x += dashLen + gap)
                {
                    CreateBox("MarkX", new Vector3(x, 0.072f, z), new Vector3(dashLen, 0.02f, 0.4f), white);
                }
            }
        }

        Vector3 BlockCenter(int x, int z)
        {
            float ox = -TotalX / 2f + roadWidth + x * Cell + blockSize / 2f;
            float oz = -TotalZ / 2f + roadWidth + z * Cell + blockSize / 2f;
            return new Vector3(ox, 0f, oz);
        }

        void CreateBlock(int bx, int bz, System.Random rnd)
        {
            Vector3 center = BlockCenter(bx, bz);
            bool park = (float)rnd.NextDouble() < parkChance;
            Color slabColor = park ? new Color(0.30f, 0.45f, 0.26f) : new Color(0.62f, 0.62f, 0.63f);
            CreateBox("Block_" + bx + "_" + bz, center + Vector3.up * 0.15f, new Vector3(blockSize, 0.3f, blockSize), slabColor);

            if (park)
            {
                if (generateTrees) CreateTrees(center, rnd);
                return;
            }

            if ((float)rnd.NextDouble() < emptyLotChance) return;

            CreateBuildings(center, rnd);
        }

        void CreateBuildings(Vector3 center, System.Random rnd)
        {
            float margin = 4f;
            float usable = blockSize - margin * 2f;
            int n = 2 + rnd.Next(3);
            float step = usable / n;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((float)rnd.NextDouble() < 0.15f) continue;
                    float w = step * (0.6f + 0.35f * (float)rnd.NextDouble());
                    float d = step * (0.6f + 0.35f * (float)rnd.NextDouble());
                    float h = Mathf.Lerp(buildingHeight.x, buildingHeight.y, Mathf.Pow((float)rnd.NextDouble(), 1.6f));
                    Vector3 bp = center + new Vector3(-usable / 2f + (i + 0.5f) * step, 0f, -usable / 2f + (j + 0.5f) * step);
                    Vector3 pos = bp + Vector3.up * (0.3f + h / 2f);
                    Vector3 size = new Vector3(w, h, d);
                    Color col = BuildingPalette[rnd.Next(BuildingPalette.Length)];
                    var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    b.name = "Building";
                    b.transform.SetParent(root, false);
                    b.transform.localPosition = pos;
                    b.transform.localScale = size;
                    b.GetComponent<MeshRenderer>().sharedMaterial = MaterialLibrary.GetWindowMaterial(col);
                    b.isStatic = true;
                }
            }
        }

        void CreateTrees(Vector3 center, System.Random rnd)
        {
            Color trunkColor = new Color(0.36f, 0.25f, 0.16f);
            Color leavesColor = new Color(0.20f, 0.42f, 0.18f);
            int count = 4 + rnd.Next(5);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = new Vector2((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f) * (blockSize - 8f);
                Vector3 p = center + new Vector3(offset.x, 0f, offset.y);

                var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Trunk";
                trunk.transform.SetParent(root, false);
                trunk.transform.localPosition = p + Vector3.up * 1.8f;
                trunk.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);
                trunk.GetComponent<MeshRenderer>().sharedMaterial = Mat(trunkColor);
                trunk.isStatic = true;

                var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaves.name = "Leaves";
                leaves.transform.SetParent(root, false);
                leaves.transform.localPosition = p + Vector3.up * 4.4f;
                leaves.transform.localScale = Vector3.one * 3.4f;
                leaves.GetComponent<MeshRenderer>().sharedMaterial = Mat(leavesColor);
                leaves.isStatic = true;
            }
        }

        void CreateLamps()
        {
            float s = blockSize / 2f - 1.2f;
            for (int x = 0; x < blocksX; x++)
            {
                for (int z = 0; z < blocksZ; z++)
                {
                    if ((x + z) % 2 != 0) continue;
                    Vector3 c = BlockCenter(x, z);
                    Vector2[] corners = { new Vector2(s, s), new Vector2(-s, -s) };

                    foreach (var corner in corners)
                    {
                        Vector3 p = c + new Vector3(corner.x, 0f, corner.y);

                        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        pole.name = "LampPole";
                        pole.transform.SetParent(root, false);
                        pole.transform.localPosition = p + Vector3.up * 2.85f;
                        pole.transform.localScale = new Vector3(0.22f, 2.75f, 0.22f);
                        pole.GetComponent<MeshRenderer>().sharedMaterial = Mat(new Color(0.30f, 0.30f, 0.32f));
                        pole.isStatic = true;

                        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        head.name = "LampHead";
                        head.transform.SetParent(root, false);
                        head.transform.localPosition = p + Vector3.up * 5.6f;
                        head.transform.localScale = Vector3.one * 0.55f;
                        head.GetComponent<MeshRenderer>().sharedMaterial = LampMat();
                        head.isStatic = true;
                    }
                }
            }
        }

        void CreateMountains(System.Random rnd)
        {
            Color[] rock =
            {
                new Color(0.52f, 0.48f, 0.44f),
                new Color(0.45f, 0.40f, 0.36f),
                new Color(0.58f, 0.52f, 0.42f),
                new Color(0.38f, 0.36f, 0.34f),
                new Color(0.50f, 0.45f, 0.40f)
            };

            float halfX = TotalX / 2f;
            float halfZ = TotalZ / 2f;
            float dist = 85f;
            float step = 34f;

            for (float x = -halfX - dist; x <= halfX + dist; x += step)
            {
                float jN = ((float)rnd.NextDouble() - 0.5f) * 18f;
                float hN = 60f + (float)rnd.NextDouble() * 95f;
                float sxN = 58f + (float)rnd.NextDouble() * 62f;
                float szN = 42f + (float)rnd.NextDouble() * 44f;
                Vector3 pN = new Vector3(x + jN, hN / 2f, halfZ + dist + jN);
                CreateBox("Mountain", pN, new Vector3(sxN, hN, szN), rock[rnd.Next(rock.Length)]);
                if ((float)rnd.NextDouble() < 0.45f)
                {
                    var peak = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    peak.name = "Peak";
                    peak.transform.SetParent(root, false);
                    peak.transform.localPosition = pN + Vector3.up * (hN / 2f - 1f);
                    float pr = 16f + (float)rnd.NextDouble() * 14f;
                    peak.transform.localScale = new Vector3(pr * 1.1f, pr, pr * 0.95f);
                    peak.GetComponent<MeshRenderer>().sharedMaterial = Mat(rock[rnd.Next(rock.Length)]);
                    peak.isStatic = true;
                }

                float jS = ((float)rnd.NextDouble() - 0.5f) * 18f;
                float hS = 60f + (float)rnd.NextDouble() * 95f;
                float sxS = 58f + (float)rnd.NextDouble() * 62f;
                float szS = 42f + (float)rnd.NextDouble() * 44f;
                Vector3 pS = new Vector3(x + jS, hS / 2f, -halfZ - dist + jS);
                CreateBox("Mountain", pS, new Vector3(sxS, hS, szS), rock[rnd.Next(rock.Length)]);
                if ((float)rnd.NextDouble() < 0.45f)
                {
                    var peak = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    peak.name = "Peak";
                    peak.transform.SetParent(root, false);
                    peak.transform.localPosition = pS + Vector3.up * (hS / 2f - 1f);
                    float pr = 16f + (float)rnd.NextDouble() * 14f;
                    peak.transform.localScale = new Vector3(pr * 1.1f, pr, pr * 0.95f);
                    peak.GetComponent<MeshRenderer>().sharedMaterial = Mat(rock[rnd.Next(rock.Length)]);
                    peak.isStatic = true;
                }
            }

            for (float z = -halfZ - dist; z <= halfZ + dist; z += step)
            {
                float jW = ((float)rnd.NextDouble() - 0.5f) * 18f;
                float hW = 60f + (float)rnd.NextDouble() * 95f;
                float sxW = 42f + (float)rnd.NextDouble() * 44f;
                float szW = 58f + (float)rnd.NextDouble() * 62f;
                Vector3 pW = new Vector3(-halfX - dist + jW, hW / 2f, z + jW);
                CreateBox("Mountain", pW, new Vector3(sxW, hW, szW), rock[rnd.Next(rock.Length)]);
                if ((float)rnd.NextDouble() < 0.45f)
                {
                    var peak = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    peak.name = "Peak";
                    peak.transform.SetParent(root, false);
                    peak.transform.localPosition = pW + Vector3.up * (hW / 2f - 1f);
                    float pr = 16f + (float)rnd.NextDouble() * 14f;
                    peak.transform.localScale = new Vector3(pr * 1.1f, pr, pr * 0.95f);
                    peak.GetComponent<MeshRenderer>().sharedMaterial = Mat(rock[rnd.Next(rock.Length)]);
                    peak.isStatic = true;
                }

                float jE = ((float)rnd.NextDouble() - 0.5f) * 18f;
                float hE = 60f + (float)rnd.NextDouble() * 95f;
                float sxE = 42f + (float)rnd.NextDouble() * 44f;
                float szE = 58f + (float)rnd.NextDouble() * 62f;
                Vector3 pE = new Vector3(halfX + dist + jE, hE / 2f, z + jE);
                CreateBox("Mountain", pE, new Vector3(sxE, hE, szE), rock[rnd.Next(rock.Length)]);
                if ((float)rnd.NextDouble() < 0.45f)
                {
                    var peak = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    peak.name = "Peak";
                    peak.transform.SetParent(root, false);
                    peak.transform.localPosition = pE + Vector3.up * (hE / 2f - 1f);
                    float pr = 16f + (float)rnd.NextDouble() * 14f;
                    peak.transform.localScale = new Vector3(pr * 1.1f, pr, pr * 0.95f);
                    peak.GetComponent<MeshRenderer>().sharedMaterial = Mat(rock[rnd.Next(rock.Length)]);
                    peak.isStatic = true;
                }
            }
        }

        public float RoadLineX(int i)
        {
            return -TotalX / 2f + roadWidth / 2f + i * Cell;
        }

        public float RoadLineZ(int i)
        {
            return -TotalZ / 2f + roadWidth / 2f + i * Cell;
        }

        public Vector3 GetRandomRoadPoint(int seedValue, out bool alongZ)
        {
            var rnd = new System.Random(seedValue);
            alongZ = rnd.Next(2) == 0;
            float t = (float)rnd.NextDouble() * 2f - 1f;
            if (alongZ)
            {
                int line = rnd.Next(blocksX + 1);
                float x = -TotalX / 2f + roadWidth / 2f + line * Cell;
                return new Vector3(x, 0f, t * (TotalZ / 2f - roadWidth));
            }
            int lineZ = rnd.Next(blocksZ + 1);
            float z = -TotalZ / 2f + roadWidth / 2f + lineZ * Cell;
            return new Vector3(t * (TotalX / 2f - roadWidth), 0f, z);
        }
    }
}
