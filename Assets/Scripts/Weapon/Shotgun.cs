using UnityEngine;

namespace OpenWorld.Weapon
{
    public class Shotgun : Weapon
    {
        public int pellets = 8;
        public float spread = 6f;

        protected override void Awake()
        {
            base.Awake();
            damage = 18;
            range = 45f;
            fireRate = 0.68f;
            maxAmmo = 8;
            ammo = maxAmmo;
            reloadTime = 1.6f;
        }

        protected override void HandleInput()
        {
            if (Input.GetMouseButtonDown(0) && Time.time >= nextFire)
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
            Vector3 origin = AimOrigin();
            Vector3 fwd = AimDir();
            for (int i = 0; i < pellets; i++)
            {
                Vector3 dir = fwd + new Vector3(Random.Range(-spread, spread) * 0.01f, Random.Range(-spread, spread) * 0.01f, 0f);
                dir.Normalize();
                DoRaycast(origin, dir, damage, range);
            }
        }
    }
}
