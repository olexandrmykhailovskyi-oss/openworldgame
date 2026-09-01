using UnityEngine;
using OpenWorld;
using OpenWorld.Economy;

namespace OpenWorld.World
{
    public class CarShop : MonoBehaviour
    {
        public float interactDistance = 5f;
        Transform player;

        void Update()
        {
            if (player == null) { var p = FindObjectOfType<PlayerController>(); if (p != null) player = p.transform; else return; }
            float d = Vector3.Distance(player.position, transform.position);
            if (d > interactDistance) return;
            var car = player.GetComponent<CarInteraction>();
            if (car != null && car.CurrentCar != null) return;

            GameManager.Instance.Hint = "E — автосалон | 1-Седан $1800 2-Спорт $4500 3-Внедорожник $7000";
            if (Input.GetKeyDown(KeyCode.Alpha1)) TryBuy(1800, new Color(0.85f, 0.15f, 0.15f));
            if (Input.GetKeyDown(KeyCode.Alpha2)) TryBuy(4500, new Color(0.15f, 0.9f, 0.85f));
            if (Input.GetKeyDown(KeyCode.Alpha3)) TryBuy(7000, new Color(0.22f, 0.22f, 0.24f));
            if (Input.GetKeyDown(KeyCode.E)) TryBuy(1800, new Color(0.85f, 0.15f, 0.15f));
        }

        void TryBuy(int price, Color col)
        {
            if (PlayerWallet.Instance == null) return;
            if (PlayerWallet.Instance.Money < price) { GameManager.Instance.Hint = "Нужно $" + price; return; }
            if (!PlayerWallet.Instance.TrySpend(price)) return;
            Vector3 pos = transform.position + transform.forward * 6f + Vector3.up * 0.4f;
            var city = FindObjectOfType<CityGenerator>();
            var car = CarFactory.Create(pos, transform.eulerAngles.y, col);
            car.gameObject.AddComponent<Vehicle.CarDamage>();
            GameManager.Instance.Hint = "Куплено авто за $" + price;
        }
    }
}
