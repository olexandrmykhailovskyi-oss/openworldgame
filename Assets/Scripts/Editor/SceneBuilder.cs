using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using OpenWorld.Economy;
using OpenWorld.Jobs;
using OpenWorld.Entities;
using OpenWorld.Weapon;
using OpenWorld.World;
using OpenWorld.Police;
using OpenWorld.Pets;

namespace OpenWorld.EditorTools
{
    [InitializeOnLoad]
    public static class SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        static bool waiting;

        static SceneBuilder()
        {
            EditorApplication.delayCall += TryBuild;
        }

        static void TryBuild()
        {
            if (File.Exists(ScenePath)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                if (!waiting)
                {
                    waiting = true;
                    EditorApplication.update += WaitForReady;
                }
                return;
            }
            Build();
        }

        static void WaitForReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= WaitForReady;
            waiting = false;
            if (!File.Exists(ScenePath)) Build();
        }

        [MenuItem("OpenWorld/Собрать демо-сцену")]
        static void MenuBuild()
        {
            Build();
        }

        static void Build()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sunGo = new GameObject("Directional Light");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sunGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            sun.intensity = 1.15f;
            sun.color = new Color(1f, 0.96f, 0.88f);
            sun.shadows = LightShadows.Soft;

            var procShader = Shader.Find("Skybox/Procedural");
            if (procShader != null)
            {
                var skyMat = new Material(procShader);
                skyMat.SetFloat("_SunSize", 0.04f);
                skyMat.SetFloat("_SunSizeConvergence", 5f);
                skyMat.SetFloat("_AtmosphereThickness", 1.05f);
                skyMat.SetColor("_SkyTint", new Color(0.50f, 0.50f, 0.60f));
                skyMat.SetColor("_GroundColor", new Color(0.38f, 0.45f, 0.48f));
                skyMat.SetFloat("_Exposure", 1.25f);
                RenderSettings.skybox = skyMat;
            }
            else
            {
                var sky = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
                if (sky != null) RenderSettings.skybox = sky;
            }
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.05f;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.68f, 0.74f, 0.82f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.0022f;

            var cityGo = new GameObject("CityGenerator");
            var city = cityGo.AddComponent<CityGenerator>();

            var playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGo.name = "Player";
            playerGo.tag = "Player";
            var capsuleCol = playerGo.GetComponent<CapsuleCollider>();
            if (capsuleCol != null) Object.DestroyImmediate(capsuleCol);
            playerGo.transform.position = new Vector3(0f, 1.1f, 0f);
            playerGo.transform.localScale = new Vector3(1f, 0.9f, 1f);
            playerGo.GetComponent<MeshRenderer>().sharedMaterial =
                LoadOrCreateMaterial("Assets/Materials/M_Player.mat", new Color(0.2f, 0.45f, 0.9f));

            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = Vector3.zero;
            playerGo.AddComponent<PlayerController>();
            playerGo.AddComponent<CarInteraction>();
            var ent = playerGo.AddComponent<Entities.Entity>();
            ent.maxHealth = 100;
            playerGo.AddComponent<Pistol>();
            playerGo.AddComponent<Shotgun>();
            playerGo.AddComponent<Rifle>();
            playerGo.AddComponent<WeaponInventory>();

            var camGo = new GameObject("PlayerCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.farClipPlane = 2000f;
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<ThirdPersonCamera>().target = playerGo.transform;
            camGo.transform.position = new Vector3(0f, 3.5f, -7f);
            camGo.transform.rotation = Quaternion.Euler(15f, 0f, 0f);

            var spawnerGo = new GameObject("CarSpawner");
            var spawner = spawnerGo.AddComponent<CarSpawner>();
            spawner.city = city;
            spawner.carCount = 6;
            spawner.seed = 777;
            spawner.colors = new[]
            {
                new Color(0.85f, 0.15f, 0.15f),
                new Color(0.15f, 0.30f, 0.80f),
                new Color(0.90f, 0.75f, 0.10f),
                new Color(0.90f, 0.90f, 0.92f),
                new Color(0.10f, 0.60f, 0.30f),
                new Color(0.55f, 0.55f, 0.58f)
            };

            var trafficGo = new GameObject("TrafficSpawner");
            var traffic = trafficGo.AddComponent<TrafficSpawner>();
            traffic.city = city;
            traffic.carCount = 14;
            traffic.seed = 4242;

            var minimapGo = new GameObject("Minimap");
            minimapGo.AddComponent<Minimap>().followTarget = playerGo.transform;

            var walletGo = new GameObject("Wallet");
            walletGo.AddComponent<PlayerWallet>();

            var jobManGo = new GameObject("JobManager");
            jobManGo.AddComponent<JobManager>();

            var markerGo = new GameObject("JobMarker");
            markerGo.AddComponent<JobMarker>();

            CreateJobGivers(city, playerGo.transform);
            CreateATMs(city);
            CreateMoneyPickups(city);
            CreatePedestrians(city);
            CreateApartments(city);
            CreateChopShop(city);
            CreateRace(city, playerGo.transform);

            var wantedGo = new GameObject("WantedSystem");
            wantedGo.AddComponent<Police.WantedSystem>();

            var dayNightGo = new GameObject("DayNightCycle");
            var dnc = dayNightGo.AddComponent<World.DayNightCycle>();
            dnc.sun = sun;
            dnc.cycleDuration = 90f;

            var petSpawnerGo = new GameObject("PetSpawner");
            var petSpawner = petSpawnerGo.AddComponent<PetSpawner>();
            petSpawner.dogCount = 7;
            petSpawner.catCount = 10;

            CreateShops(city);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("OpenWorld: демо-сцена собрана: " + ScenePath + ". Жми Play!");
        }

        static void CreateJobGivers(CityGenerator city, Transform player)
        {
            var infos = new[] {
                new { type = JobType.Taxi, color = new Color(1f, 0.85f, 0.1f), bx = 6, bz = 7 },
                new { type = JobType.Courier, color = new Color(0.2f, 0.6f, 1f), bx = 7, bz = 6 },
                new { type = JobType.Collect, color = new Color(0.3f, 0.9f, 0.4f), bx = 5, bz = 6 }
            };
            foreach (var info in infos)
            {
                Vector3 center = new Vector3(city.RoadLineX(info.bx) + city.Cell / 2f, 0f, city.RoadLineZ(info.bz) + city.Cell / 2f);
                Vector3 pos = center + new Vector3(0f, 0.8f, 0f);
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "JobGiver_" + info.type;
                go.transform.position = pos;
                go.transform.localScale = new Vector3(2f, 2.6f, 2f);
                var mat = new Material(Shader.Find("Standard"));
                mat.color = info.color;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", info.color * 0.6f);
                go.GetComponent<MeshRenderer>().sharedMaterial = mat;
                var giver = go.AddComponent<JobGiver>();
                giver.jobType = info.type;
                giver.player = player;
            }
        }

        static void CreateATMs(CityGenerator city)
        {
            var rnd = new System.Random(991);
            for (int i = 0; i < 5; i++)
            {
                int bx = rnd.Next(city.blocksX);
                int bz = rnd.Next(city.blocksZ);
                Vector3 center = new Vector3(city.RoadLineX(bx) + city.Cell / 2f, 0f, city.RoadLineZ(bz) + city.Cell / 2f);
                float off = city.blockSize / 2f - 2f;
                Vector3 pos = center + new Vector3((rnd.Next(2) == 0 ? off : -off), 0.6f, (rnd.Next(2) == 0 ? off : -off));
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "ATM";
                go.transform.position = pos;
                go.transform.localScale = new Vector3(1.2f, 1.8f, 0.7f);
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.15f, 0.45f, 0.22f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 0.35f) * 0.5f);
                go.GetComponent<MeshRenderer>().sharedMaterial = mat;
                go.AddComponent<ATM>();
            }
        }

        static void CreateMoneyPickups(CityGenerator city)
        {
            var rnd = new System.Random(12345);
            for (int i = 0; i < 36; i++)
            {
                Vector3 p = city.GetRandomRoadPoint(rnd.Next(), out bool alongZ);
                float side = (rnd.Next(2) == 0 ? 1f : -1f) * 4f;
                if (alongZ) p.x += side;
                else p.z += side;
                p.y = 0.7f;
                var go = new GameObject("MoneyPickup");
                go.transform.position = p;
                var trigger = go.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(1.2f, 1.2f, 1.2f);
                trigger.center = Vector3.zero;
                var vis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                vis.name = "Coin";
                vis.transform.SetParent(go.transform, false);
                vis.transform.localPosition = Vector3.zero;
                vis.transform.localScale = new Vector3(0.9f, 0.15f, 0.9f);
                var col = vis.GetComponent<CapsuleCollider>();
                if (col != null) Object.DestroyImmediate(col);
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.84f, 0.15f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.4f) * 0.9f);
                vis.GetComponent<MeshRenderer>().sharedMaterial = mat;
                var pickup = go.AddComponent<MoneyPickup>();
                pickup.amount = 12 + rnd.Next(28);
            }
        }

        static void CreatePedestrians(CityGenerator city)
        {
            var rnd = new System.Random(7777);
            for (int i = 0; i < 28; i++)
            {
                int bx = rnd.Next(city.blocksX);
                int bz = rnd.Next(city.blocksZ);
                Vector3 center = new Vector3(city.RoadLineX(bx) + city.Cell / 2f, 0f, city.RoadLineZ(bz) + city.Cell / 2f);
                Vector2 off = new Vector2((float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f) * (city.blockSize - 6f);
                Vector3 pos = center + new Vector3(off.x, 1f, off.y);
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = "Pedestrian";
                go.transform.position = pos;
                go.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                var capCol = go.GetComponent<CapsuleCollider>();
                if (capCol != null) Object.DestroyImmediate(capCol);
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.55f + (float)rnd.NextDouble() * 0.35f, 0.45f + (float)rnd.NextDouble() * 0.35f, 0.5f + (float)rnd.NextDouble() * 0.4f);
                go.GetComponent<MeshRenderer>().sharedMaterial = mat;
                var cc = go.AddComponent<CapsuleCollider>();
                cc.height = 1.8f;
                cc.radius = 0.35f;
                cc.center = new Vector3(0f, 0.9f, 0f);
                go.AddComponent<Pedestrian>();
            }
        }

        static void CreateApartments(CityGenerator city)
        {
            var infos = new[] {
                new { price = 1800, income = 35, bx = 4, bz = 4 },
                new { price = 3500, income = 75, bx = 8, bz = 8 },
                new { price = 6200, income = 140, bx = 5, bz = 9 }
            };
            foreach (var info in infos)
            {
                Vector3 center = new Vector3(city.RoadLineX(info.bx) + city.Cell / 2f, 0f, city.RoadLineZ(info.bz) + city.Cell / 2f);
                Vector3 pos = center + new Vector3(0f, 1f, 0f);
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Apartment";
                go.transform.position = pos;
                go.transform.localScale = new Vector3(6f, 5f, 6f);
                var apt = go.AddComponent<Apartment>();
                apt.price = info.price;
                apt.incomePerMinute = info.income;
            }
        }

        static void CreateChopShop(CityGenerator city)
        {
            Vector3 pos = new Vector3(city.TotalX / 2f - 22f, 0.6f, -city.TotalZ / 2f + 22f);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "ChopShop";
            go.transform.position = pos;
            go.transform.localScale = new Vector3(10f, 3f, 12f);
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.45f, 0.12f, 0.12f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.7f, 0.15f, 0.15f) * 0.7f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            go.AddComponent<ChopShop>();
        }

        static void CreateRace(CityGenerator city, Transform player)
        {
            Vector3 pos = new Vector3(22f, 0.5f, 22f);
            var go = new GameObject("RaceStart");
            go.transform.position = pos;
            var race = go.AddComponent<RaceManager>();
            race.reward = 750;
        }

        static void CreateShops(CityGenerator city)
        {
            Vector3 wCenter = new Vector3(city.RoadLineX(2) + city.Cell / 2f, 0f, city.RoadLineZ(8) + city.Cell / 2f);
            Vector3 wPos = wCenter + new Vector3(0f, 1f, 0f);
            var wGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wGo.name = "WeaponShop";
            wGo.transform.position = wPos;
            wGo.transform.localScale = new Vector3(5f, 3f, 5f);
            var wMat = new Material(Shader.Find("Standard"));
            wMat.color = new Color(0.55f, 0.25f, 0.15f);
            wMat.EnableKeyword("_EMISSION");
            wMat.SetColor("_EmissionColor", new Color(0.8f, 0.35f, 0.15f) * 0.6f);
            wGo.GetComponent<MeshRenderer>().sharedMaterial = wMat;
            wGo.AddComponent<WeaponShop>();

            Vector3 cCenter = new Vector3(city.RoadLineX(9) + city.Cell / 2f, 0f, city.RoadLineZ(3) + city.Cell / 2f);
            Vector3 cPos = cCenter + new Vector3(0f, 1f, 0f);
            var cGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cGo.name = "CarShop";
            cGo.transform.position = cPos;
            cGo.transform.localScale = new Vector3(7f, 3f, 7f);
            var cMat = new Material(Shader.Find("Standard"));
            cMat.color = new Color(0.15f, 0.35f, 0.65f);
            cMat.EnableKeyword("_EMISSION");
            cMat.SetColor("_EmissionColor", new Color(0.2f, 0.5f, 0.9f) * 0.6f);
            cGo.GetComponent<MeshRenderer>().sharedMaterial = cMat;
            cGo.AddComponent<CarShop>();
        }

        static Material LoadOrCreateMaterial(string path, Color color)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
