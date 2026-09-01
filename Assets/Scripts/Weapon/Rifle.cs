using UnityEngine;

namespace OpenWorld.Weapon
{
    public class Rifle : Weapon
    {
        protected override void Awake()
        {
            base.Awake();
            damage = 22;
            range = 180f;
            fireRate = 0.10f;
            maxAmmo = 30;
            ammo = maxAmmo;
            reloadTime = 1.35f;
        }

        protected override void HandleInput()
        {
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
            Vector3 origin = AimOrigin();
            Vector3 dir = AimDir() + new Vector3(Random.Range(-1f, 1f) * 0.008f, Random.Range(-1f, 1f) * 0.008f, 0f);
            dir.Normalize();
            DoRaycast(origin, dir, damage, range);
        }
    }
}
