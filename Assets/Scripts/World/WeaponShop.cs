using UnityEngine;
using OpenWorld.Economy;

namespace OpenWorld.World
{
    public class WeaponShop : MonoBehaviour
    {
        public float interactDistance = 3.5f;
        Transform player;

        struct Offer
        {
            public string name;
            public int price;
            public System.Action buy;
            public Offer(string n, int p, System.Action b) { name = n; price = p; buy = b; }
        }

        void Update()
        {
            if (player == null) { var p = FindObjectOfType<PlayerController>(); if (p != null) player = p.transform; else return; }
            float d = Vector3.Distance(player.position, transform.position);
            if (d > interactDistance) return;
            var car = player.GetComponent<CarInteraction>();
            if (car != null && car.CurrentCar != null) return;
            GameManager.Instance.Hint = "E — магазин оружия | 1-Пистолет $500 2-Дробовик $1200 3-Винтовка $2500";
            if (Input.GetKeyDown(KeyCode.Alpha1)) TryBuy(500, () => GiveAmmo<Weapon.Pistol>(18));
            if (Input.GetKeyDown(KeyCode.Alpha2)) TryBuy(1200, () => GiveAmmo<Weapon.Shotgun>(8));
            if (Input.GetKeyDown(KeyCode.Alpha3)) TryBuy(2500, () => GiveAmmo<Weapon.Rifle>(30));
            if (Input.GetKeyDown(KeyCode.E)) TryBuy(500, () => GiveAmmo<Weapon.Pistol>(18));
        }

        void TryBuy(int price, System.Action give)
        {
            if (PlayerWallet.Instance == null) return;
            if (PlayerWallet.Instance.Money < price) { GameManager.Instance.Hint = "Нужно $" + price; return; }
            if (PlayerWallet.Instance.TrySpend(price)) { give?.Invoke(); GameManager.Instance.Hint = "Куплено за $" + price; }
        }

        void GiveAmmo<T>(int amount) where T : Weapon.Weapon
        {
            var inv = player.GetComponent<Weapon.WeaponInventory>();
            if (inv == null) return;
            var w = inv.GetComponent<T>();
            if (w != null) w.ammo = Mathf.Min(w.ammo + amount, w.maxAmmo);
        }
    }
}
