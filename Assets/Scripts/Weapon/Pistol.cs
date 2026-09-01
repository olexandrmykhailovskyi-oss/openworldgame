using UnityEngine;

namespace OpenWorld.Weapon
{
    public class Pistol : MonoBehaviour
    {
        public int damage = 35;
        public float range = 120f;
        public float fireRate = 0.22f;
        public int ammo = 18;
        public int maxAmmo = 18;
        public float reloadTime = 1.1f;

        float nextFire;
        bool reloading;
        Camera cam;

        void Awake()
        {
            cam = GetComponentInParent<Camera>();
            if (cam == null) cam = Camera.main;
        }

        void Update()
        {
            var player = GetComponentInParent<OpenWorld.PlayerController>();
            if (player != null && !player.ControlEnabled) return;
            if (reloading) return;

            if (Input.GetMouseButton(0) && Time.time >= nextFire)
            {
                if (ammo <= 0) { StartReload(); return; }
                Fire();
            }

            if (Input.GetKeyDown(KeyCode.R) && ammo < maxAmmo) StartReload();
        }

        void Fire()
        {
            nextFire = Time.time + fireRate;
            ammo--;

            Vector3 origin = cam != null ? cam.transform.position : transform.position + Vector3.up * 1.6f;
            Vector3 dir = cam != null ? cam.transform.forward : transform.forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, range))
            {
                var entity = hit.collider.GetComponentInParent<OpenWorld.Entities.Entity>();
                if (entity != null) entity.TakeDamage(damage);

                var rb = hit.collider.attachedRigidbody;
                if (rb != null) rb.AddForce(dir * 260f);
            }
        }

        void StartReload()
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
