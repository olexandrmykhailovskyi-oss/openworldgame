using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld.Police
{
    public enum CrimeType { Brandishing, Assault, Theft, Vandalism, Murder, Explosion, MassMurder }

    public class WantedSystem : MonoBehaviour
    {
        public static WantedSystem Instance { get; private set; }

        public int Stars { get; private set; }
        public float decayTime = 25f;
        public float spawnRadius = 55f;

        float lastCrimeTime;
        CityGenerator city;
        List<float> recentMurders = new List<float>();
        int massMurderStreak;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            city = FindObjectOfType<CityGenerator>();
        }

        void Update()
        {
            if (Stars <= 0) return;
            recentMurders.RemoveAll(t => Time.time - t > 30f);
            if (Time.time - lastCrimeTime > decayTime && !IsPlayerSeen())
            {
                Stars--;
                lastCrimeTime = Time.time;
                if (Stars == 0) { DespawnPolice(); massMurderStreak = 0; }
            }
        }

        public void AddStar(int count = 1)
        {
            Stars = Mathf.Clamp(Stars + count, 0, 5);
            lastCrimeTime = Time.time;
            SpawnPolice();
        }

        public void ReportCrime(CrimeType type, Vector3 pos)
        {
            int stars = 0;
            switch (type)
            {
                case CrimeType.Brandishing: stars = 1; break;
                case CrimeType.Assault: stars = 1; break;
                case CrimeType.Theft: stars = 1; break;
                case CrimeType.Vandalism: stars = 1; break;
                case CrimeType.Murder: stars = 2; break;
                case CrimeType.Explosion: stars = 2; break;
                case CrimeType.MassMurder: stars = 3; break;
            }

            if (type == CrimeType.Murder || type == CrimeType.Explosion)
            {
                recentMurders.Add(Time.time);
                recentMurders.RemoveAll(t => Time.time - t > 30f);
                if (recentMurders.Count >= 3)
                {
                    massMurderStreak++;
                    if (massMurderStreak >= 2) stars = Mathf.Max(stars, 3);
                    else stars = Mathf.Max(stars, 2);
                    if (recentMurders.Count >= 5) ReportCrime(CrimeType.MassMurder, pos);
                }
            }

            if (type == CrimeType.Brandishing && Stars >= 2) return;
            AddStar(stars);
        }

        public void ClearWanted()
        {
            Stars = 0;
            recentMurders.Clear();
            massMurderStreak = 0;
            DespawnPolice();
        }

        bool IsPlayerSeen()
        {
            var player = FindObjectOfType<PlayerController>();
            if (player == null) return false;
            var cops = FindObjectsOfType<PoliceCar>();
            foreach (var c in cops)
                if (Vector3.Distance(c.transform.position, player.transform.position) < 42f) return true;
            foreach (var ped in FindObjectsOfType<Entities.Pedestrian>())
                if (ped.IsCallingPolice && Vector3.Distance(ped.transform.position, player.transform.position) < 25f) return true;
            return false;
        }

        void SpawnPolice()
        {
            if (city == null) city = FindObjectOfType<CityGenerator>();
            if (city == null) return;
            var player = FindObjectOfType<PlayerController>();
            if (player == null) return;
            int toSpawn = Mathf.Clamp(Stars, 1, 3);
            if (Stars >= 4) toSpawn = 4;
            if (Stars == 5) toSpawn = 5;
            var existing = FindObjectsOfType<PoliceCar>().Length;
            for (int i = existing; i < toSpawn; i++)
            {
                var rnd = new System.Random((int)(Time.time * 1000) + i * 997);
                Vector3 pos = city.GetRandomRoadPoint(rnd.Next(), out bool alongZ);
                float far = Vector3.Distance(new Vector3(pos.x, 0, pos.z), new Vector3(player.transform.position.x, 0, player.transform.position.z));
                if (far < spawnRadius) continue;
                pos.y = 0.3f;
                float yaw = alongZ ? (rnd.Next(2) == 0 ? 0f : 180f) : (rnd.Next(2) == 0 ? 90f : 270f);
                PoliceCar.Create(pos, yaw);
            }
        }

        void DespawnPolice()
        {
            var cops = FindObjectsOfType<PoliceCar>();
            foreach (var c in cops) Destroy(c.gameObject, 0.2f);
        }
    }
}
