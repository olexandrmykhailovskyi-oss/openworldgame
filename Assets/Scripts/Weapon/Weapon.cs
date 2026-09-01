using UnityEngine;

namespace OpenWorld.Weapon
{
    public abstract class Weapon : MonoBehaviour
    {
        public int damage = 30;
        public float range = 120f;
        public float fireRate = 0.22f;
        public int maxAmmo = 18;
        public int ammo = 18;
        public float reloadTime = 1.1f;

        protected float nextFire;
        protected bool reloading;
        protected Camera cam;

        protected virtual void Awake()
        {
            cam = GetComponentInParent<Camera>();
            if (cam == null) cam = Camera.main;
            ammo = maxAmmo;
        }

        protected virtual void Update()
        {
            var player = GetComponentInParent<OpenWorld.PlayerController>();
            if (player != null && !player.ControlEnabled) return;
            if (reloading) return;

            HandleInput();
        }

        protected abstract void HandleInput();

        protected void DoRaycast(Vector3 origin, Vector3 dir, int dmg, float rng)
        {
            Visuals.Effects.MuzzleFlash(origin, dir);
            Entities.Pedestrian.NotifyGunshot(origin);
            if (Police.WantedSystem.Instance != null) Police.WantedSystem.Instance.ReportCrime(Police.CrimeType.Assault, origin);
            if (Physics.Raycast(origin, dir, out RaycastHit hit, rng))
            {
                var entity = hit.collider.GetComponentInParent<OpenWorld.Entities.Entity>();
                if (entity != null) entity.TakeDamage(dmg);
                var dmgCar = hit.collider.GetComponentInParent<Vehicle.CarDamage>();
                if (dmgCar != null) dmgCar.ApplyDamage(dmg * 0.7f);
                var rb = hit.collider.attachedRigidbody;
                if (rb != null) rb.AddForce(dir * 260f);
            }
        }

        protected Vector3 AimOrigin()
        {
            if (cam != null) return cam.transform.position;
            return transform.position + Vector3.up * 1.6f;
        }

        protected Vector3 AimDir()
        {
            if (cam != null) return cam.transform.forward;
            return transform.forward;
        }

        protected void StartReload()
        {
            if (reloading) return;
            reloading = true;
            Invoke(nameof(FinishReload), reloadTime);
        }

        void FinishReload()
        {
            ammo = maxAmmo;
            reloading = false;
        }

        public bool IsReloading => reloading;
    }
}
