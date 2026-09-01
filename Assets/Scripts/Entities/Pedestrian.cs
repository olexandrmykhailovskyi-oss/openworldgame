using UnityEngine;

namespace OpenWorld.Entities
{
    public class Pedestrian : Entity
    {
        public enum State { Wander, Flee, Cower, CallPolice }

        public float walkSpeed = 1.6f;
        public float fleeSpeed = 4.2f;
        public float senseWeaponDist = 14f;
        public float senseGunshotDist = 32f;

        public State currentState;
        public bool IsCallingPolice { get; private set; }

        Vector3 target;
        float waitTimer;
        float fearTimer;
        float callTimer;
        CityGenerator city;
        Transform player;

        protected override void Awake()
        {
            base.Awake();
            city = FindObjectOfType<CityGenerator>();
            var p = FindObjectOfType<PlayerController>();
            if (p != null) player = p.transform;
            PickWanderTarget();
        }

        void Update()
        {
            if (!IsAlive) return;
            if (player == null)
            {
                var p = FindObjectOfType<PlayerController>();
                if (p != null) player = p.transform;
                else { WanderUpdate(); return; }
            }

            float distToPlayer = Vector3.Distance(transform.position, player.position);
            bool playerHasGun = HasPlayerWeapon();
            bool hearsShot = Time.time - lastGunshotTime < 1.5f && Vector3.Distance(transform.position, lastGunshotPos) < senseGunshotDist;

            if (currentState != State.Flee && currentState != State.CallPolice)
            {
                if (playerHasGun && distToPlayer < senseWeaponDist)
                {
                    EnterFlee("оружие у игрока");
                }
                else if (hearsShot)
                {
                    EnterFlee("выстрел");
                }
            }

            switch (currentState)
            {
                case State.Wander: WanderUpdate(); break;
                case State.Flee: FleeUpdate(); break;
                case State.Cower: CowerUpdate(); break;
                case State.CallPolice: CallPoliceUpdate(); break;
            }

            if (currentState == State.Flee && fearTimer > 0f)
            {
                fearTimer -= Time.deltaTime;
                if (fearTimer <= 0f && distToPlayer > 22f)
                {
                    if (Random.Range(0f, 1f) < 0.35f) EnterCallPolice();
                    else EnterWander();
                }
            }
        }

        void WanderUpdate()
        {
            if (waitTimer > 0f) { waitTimer -= Time.deltaTime; return; }
            Vector3 dir = target - transform.position; dir.y = 0f;
            if (dir.magnitude < 1f) { waitTimer = Random.Range(0.7f, 2.2f); PickWanderTarget(); return; }
            dir.Normalize();
            transform.position += dir * walkSpeed * Time.deltaTime;
            if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 6f * Time.deltaTime);
        }

        void FleeUpdate()
        {
            Vector3 away = transform.position - player.position; away.y = 0f;
            if (away.sqrMagnitude < 0.01f) away = transform.forward;
            away.Normalize();
            Vector3 fleeTarget = transform.position + away * 18f;
            if (city != null)
            {
                float halfX = city.TotalX / 2f; float halfZ = city.TotalZ / 2f;
                fleeTarget.x = Mathf.Clamp(fleeTarget.x, -halfX + 6f, halfX - 6f);
                fleeTarget.z = Mathf.Clamp(fleeTarget.z, -halfZ + 6f, halfZ - 6f);
            }
            Vector3 dir = fleeTarget - transform.position; dir.y = 0f; dir.Normalize();
            transform.position += dir * fleeSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 9f * Time.deltaTime);
        }

        void CowerUpdate()
        {
            fearTimer -= Time.deltaTime;
            if (fearTimer <= 0f) EnterWander();
        }

        void CallPoliceUpdate()
        {
            callTimer -= Time.deltaTime;
            IsCallingPolice = callTimer > 0f;
            if (callTimer <= 0f)
            {
                IsCallingPolice = false;
                if (Police.WantedSystem.Instance != null) Police.WantedSystem.Instance.ReportCrime(Police.CrimeType.Assault, transform.position);
                EnterFlee("вызвал полицию");
            }
        }

        void EnterFlee(string reason)
        {
            currentState = State.Flee;
            fearTimer = Random.Range(4f, 7f);
            IsCallingPolice = false;
        }

        void EnterCower()
        {
            currentState = State.Cower;
            fearTimer = Random.Range(2.5f, 4f);
        }

        void EnterCallPolice()
        {
            currentState = State.CallPolice;
            callTimer = 3.5f;
            IsCallingPolice = true;
        }

        void EnterWander()
        {
            currentState = State.Wander;
            IsCallingPolice = false;
            fearTimer = 0f;
            PickWanderTarget();
        }

        void PickWanderTarget()
        {
            if (city != null)
            {
                int bx = Random.Range(0, city.blocksX);
                int bz = Random.Range(0, city.blocksZ);
                Vector3 blockCenter = new Vector3(city.RoadLineX(bx) + city.Cell / 2f, 0f, city.RoadLineZ(bz) + city.Cell / 2f);
                Vector2 rnd = Random.insideUnitCircle * (city.blockSize / 2f - 3f);
                target = blockCenter + new Vector3(rnd.x, 0f, rnd.y);
                target.y = transform.position.y;
                return;
            }
            Vector2 r = Random.insideUnitCircle * 18f;
            target = transform.position + new Vector3(r.x, 0f, r.y);
        }

        bool HasPlayerWeapon()
        {
            if (player == null) return false;
            var inv = player.GetComponent<Weapon.WeaponInventory>();
            if (inv != null && inv.Current != null) return true;
            var pistol = player.GetComponent<Weapon.Pistol>();
            return pistol != null && pistol.enabled;
        }

        static Vector3 lastGunshotPos;
        static float lastGunshotTime = -999f;
        public static void NotifyGunshot(Vector3 pos)
        {
            lastGunshotPos = pos;
            lastGunshotTime = Time.time;
        }

        protected override void Die()
        {
            if (OpenWorld.Economy.PlayerWallet.Instance != null)
                OpenWorld.Economy.PlayerWallet.Instance.AddMoney(30);
            if (OpenWorld.Police.WantedSystem.Instance != null)
                OpenWorld.Police.WantedSystem.Instance.ReportCrime(Police.CrimeType.Murder, transform.position);
            base.Die();
        }
    }
}
