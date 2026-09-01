using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class TrafficSpawner : MonoBehaviour
    {
        public CityGenerator city;
        public int carCount = 14;
        public int seed = 4242;
        public Color[] colors =
        {
            new Color(0.80f, 0.80f, 0.82f),
            new Color(0.10f, 0.10f, 0.12f),
            new Color(0.60f, 0.15f, 0.15f),
            new Color(0.20f, 0.35f, 0.60f),
            new Color(0.75f, 0.65f, 0.30f)
        };

        void Start()
        {
            if (city == null) city = FindObjectOfType<CityGenerator>();
            if (city == null) return;

            var rnd = new System.Random(seed);
            var used = new HashSet<string>();
            int placed = 0;
            int guard = 0;
            while (placed < carCount && guard++ < carCount * 10)
            {
                var intersection = new Vector2Int(rnd.Next(city.blocksX + 1), rnd.Next(city.blocksZ + 1));
                var key = intersection.x + ":" + intersection.y;
                if (!used.Add(key)) continue;
                Vector3 world = new Vector3(city.RoadLineX(intersection.x), 0f, city.RoadLineZ(intersection.y));
                if (Vector3.Distance(world, Vector3.zero) < 30f) continue;
                var dir = RandomDir(rnd);
                Color c = colors != null && colors.Length > 0 ? colors[placed % colors.Length] : Color.white;
                TrafficCar.Create(city, intersection, dir, c);
                placed++;
            }
        }

        static Vector2Int RandomDir(System.Random rnd)
        {
            switch (rnd.Next(4))
            {
                case 0: return new Vector2Int(1, 0);
                case 1: return new Vector2Int(-1, 0);
                case 2: return new Vector2Int(0, 1);
                default: return new Vector2Int(0, -1);
            }
        }
    }
}
