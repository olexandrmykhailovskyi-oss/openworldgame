using UnityEngine;
using OpenWorld.Economy;

namespace OpenWorld.Jobs
{
    public enum JobType { None, Taxi, Courier, Collect }

    public class JobManager : MonoBehaviour
    {
        public static JobManager Instance { get; private set; }

        public JobType ActiveJob { get; private set; } = JobType.None;
        public Vector3 TargetPos { get; private set; }
        public int Reward { get; private set; }
        public string Description { get; private set; } = "";

        CityGenerator city;
        System.Random rnd;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            city = FindObjectOfType<CityGenerator>();
            rnd = new System.Random();
        }

        public bool HasJob => ActiveJob != JobType.None;

        public bool StartJob(JobType type)
        {
            if (HasJob) return false;
            if (city == null) city = FindObjectOfType<CityGenerator>();
            ActiveJob = type;

            Vector3 playerPos = Vector3.zero;
            var player = FindObjectOfType<PlayerController>();
            if (player != null) playerPos = player.transform.position;

            switch (type)
            {
                case JobType.Taxi:
                    TargetPos = RandomRoadFarFrom(playerPos, 140f);
                    Reward = 120 + rnd.Next(80);
                    Description = "Такси: отвези пассажира";
                    break;
                case JobType.Courier:
                    TargetPos = RandomBlockEntrance();
                    Reward = 90 + rnd.Next(60);
                    Description = "Курьер: доставь посылку";
                    break;
                case JobType.Collect:
                    TargetPos = RandomRoadFarFrom(playerPos, 100f);
                    Reward = 60 + rnd.Next(40);
                    Description = "Сбор: забери груз";
                    break;
            }
            return true;
        }

        public void CompleteJob()
        {
            if (!HasJob) return;
            if (PlayerWallet.Instance != null) PlayerWallet.Instance.AddMoney(Reward);
            ClearJob();
        }

        public void FailJob()
        {
            ClearJob();
        }

        void ClearJob()
        {
            ActiveJob = JobType.None;
            Description = "";
            Reward = 0;
        }

        public float DistanceToTarget(Vector3 from)
        {
            Vector3 d = TargetPos - from;
            d.y = 0f;
            return d.magnitude;
        }

        public bool IsNearTarget(Vector3 pos, float radius = 6f)
        {
            return DistanceToTarget(pos) < radius;
        }

        Vector3 RandomRoadFarFrom(Vector3 origin, float minDist)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 p = city.GetRandomRoadPoint(rnd.Next(), out bool alongZ);
                p.y = 0.5f;
                if (Vector3.Distance(new Vector3(p.x, 0f, p.z), new Vector3(origin.x, 0f, origin.z)) > minDist) return p;
            }
            return city.GetRandomRoadPoint(rnd.Next(), out bool a2);
        }

        Vector3 RandomBlockEntrance()
        {
            int bx = rnd.Next(city.blocksX);
            int bz = rnd.Next(city.blocksZ);
            float half = city.blockSize / 2f;
            Vector3 c = new Vector3(city.RoadLineX(bx) + city.Cell / 2f, 0.5f, city.RoadLineZ(bz) + city.Cell / 2f);
            Vector2 side = Random.insideUnitCircle.normalized * (half + 2f);
            return c + new Vector3(side.x, 0f, side.y);
        }
    }
}
