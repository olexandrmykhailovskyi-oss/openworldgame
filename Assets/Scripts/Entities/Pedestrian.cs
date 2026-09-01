using UnityEngine;

namespace OpenWorld.Entities
{
    public class Pedestrian : Entity
    {
        public float walkSpeed = 1.6f;
        public float wanderRadius = 18f;

        Vector3 target;
        float waitTimer;
        CityGenerator city;

        protected override void Awake()
        {
            base.Awake();
            city = FindObjectOfType<CityGenerator>();
            PickTarget();
        }

        void Update()
        {
            if (!IsAlive) return;

            if (waitTimer > 0f)
            {
                waitTimer -= Time.deltaTime;
                return;
            }

            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.magnitude < 1f)
            {
                waitTimer = Random.Range(0.5f, 2f);
                PickTarget();
                return;
            }

            dir.Normalize();
            transform.position += dir * walkSpeed * Time.deltaTime;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
        }

        void PickTarget()
        {
            Vector3 center = transform.position;
            if (city != null)
            {
                int bx = Random.Range(0, city.blocksX);
                int bz = Random.Range(0, city.blocksZ);
                float half = city.blockSize / 2f - 3f;
                Vector3 blockCenter = new Vector3(city.RoadLineX(bx) + city.Cell / 2f, 0f, city.RoadLineZ(bz) + city.Cell / 2f);
                Vector2 rnd = Random.insideUnitCircle * half;
                Vector3 candidate = blockCenter + new Vector3(rnd.x, 0f, rnd.y);
                candidate.y = transform.position.y;
                target = candidate;
                return;
            }
            Vector2 r = Random.insideUnitCircle * wanderRadius;
            target = center + new Vector3(r.x, 0f, r.y);
            target.y = center.y;
        }

        protected override void Die()
        {
            if (OpenWorld.Economy.PlayerWallet.Instance != null)
                OpenWorld.Economy.PlayerWallet.Instance.AddMoney(30);
            if (OpenWorld.Police.WantedSystem.Instance != null)
                OpenWorld.Police.WantedSystem.Instance.AddStar(1);
            base.Die();
        }
    }
}
