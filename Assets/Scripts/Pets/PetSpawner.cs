using UnityEngine;

namespace OpenWorld.Pets
{
    public class PetSpawner : MonoBehaviour
    {
        public int dogCount = 6;
        public int catCount = 8;
        CityGenerator city;

        void Start()
        {
            city = FindObjectOfType<CityGenerator>();
            if (city == null) return;
            var rnd = new System.Random(5555);
            for (int i = 0; i < dogCount; i++) SpawnDog(rnd);
            for (int i = 0; i < catCount; i++) SpawnCat(rnd);
        }

        void SpawnDog(System.Random rnd)
        {
            Vector3 pos = RandomParkPosition(rnd);
            var go = new GameObject("Dog");
            go.transform.position = pos + Vector3.up * 0.5f;
            var col = go.AddComponent<CapsuleCollider>();
            col.height = 0.7f; col.radius = 0.32f; col.center = new Vector3(0f, 0.35f, 0f);
            go.AddComponent<Dog>();
        }

        void SpawnCat(System.Random rnd)
        {
            Vector3 pos = RandomParkPosition(rnd);
            var go = new GameObject("Cat");
            go.transform.position = pos + Vector3.up * 0.45f;
            var col = go.AddComponent<CapsuleCollider>();
            col.height = 0.55f; col.radius = 0.26f; col.center = new Vector3(0f, 0.28f, 0f);
            go.AddComponent<Cat>();
        }

        Vector3 RandomParkPosition(System.Random rnd)
        {
            for (int k = 0; k < 20; k++)
            {
                int bx = rnd.Next(city.blocksX);
                int bz = rnd.Next(city.blocksZ);
                Vector3 center = new Vector3(city.RoadLineX(bx) + city.Cell / 2f, 0f, city.RoadLineZ(bz) + city.Cell / 2f);
                float ox = ((float)rnd.NextDouble() - 0.5f) * (city.blockSize - 8f);
                float oz = ((float)rnd.NextDouble() - 0.5f) * (city.blockSize - 8f);
                return center + new Vector3(ox, 0f, oz);
            }
            return Vector3.zero;
        }
    }
}
