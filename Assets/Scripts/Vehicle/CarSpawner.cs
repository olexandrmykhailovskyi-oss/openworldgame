using UnityEngine;

namespace OpenWorld
{
    public class CarSpawner : MonoBehaviour
    {
        public CityGenerator city;
        public int carCount = 6;
        public int seed = 777;
        public float minDistanceFromOrigin = 20f;
        public Color[] colors =
        {
            new Color(0.85f, 0.15f, 0.15f),
            new Color(0.15f, 0.30f, 0.80f),
            new Color(0.90f, 0.75f, 0.10f),
            new Color(0.90f, 0.90f, 0.92f),
            new Color(0.10f, 0.60f, 0.30f),
            new Color(0.55f, 0.55f, 0.58f)
        };

        void Start()
        {
            if (city == null) city = FindObjectOfType<CityGenerator>();
            if (city == null) return;

            var rnd = new System.Random(seed);
            int placed = 0;
            int guard = 0;
            while (placed < carCount && guard++ < carCount * 30)
            {
                Vector3 p;
                float yaw;
                if (rnd.Next(2) == 0)
                {
                    int line = rnd.Next(city.blocksX + 1);
                    float side = rnd.Next(2) == 0 ? 1f : -1f;
                    float x = city.RoadLineX(line) + side * (city.roadWidth / 2f - 2.2f);
                    float z = ((float)rnd.NextDouble() * 2f - 1f) * (city.TotalZ / 2f - 12f);
                    p = new Vector3(x, 0f, z);
                    yaw = rnd.Next(2) == 0 ? 0f : 180f;
                }
                else
                {
                    int line = rnd.Next(city.blocksZ + 1);
                    float side = rnd.Next(2) == 0 ? 1f : -1f;
                    float z = city.RoadLineZ(line) + side * (city.roadWidth / 2f - 2.2f);
                    float x = ((float)rnd.NextDouble() * 2f - 1f) * (city.TotalX / 2f - 12f);
                    p = new Vector3(x, 0f, z);
                    yaw = rnd.Next(2) == 0 ? 90f : 270f;
                }

                if (Vector3.Distance(p, Vector3.zero) < minDistanceFromOrigin) continue;
                Color c = colors != null && colors.Length > 0 ? colors[placed % colors.Length] : Color.red;
                CarFactory.Create(p + Vector3.up * 0.4f, yaw, c);
                placed++;
            }
        }
    }
}
