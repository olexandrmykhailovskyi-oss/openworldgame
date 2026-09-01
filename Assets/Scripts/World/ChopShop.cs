using UnityEngine;
using OpenWorld.Economy;

namespace OpenWorld.World
{
    public class ChopShop : MonoBehaviour
    {
        public int baseReward = 600;
        public float interactDistance = 7f;
        Transform player;

        void Update()
        {
            if (player == null)
            {
                var p = FindObjectOfType<PlayerController>();
                if (p != null) player = p.transform;
                else return;
            }

            var carInteraction = player.GetComponent<CarInteraction>();
            bool inCar = carInteraction != null && carInteraction.CurrentCar != null;
            if (!inCar) return;

            float d = Vector3.Distance(carInteraction.CurrentCar.transform.position, transform.position);
            if (d > interactDistance) return;

            var car = carInteraction.CurrentCar;
            var dmg = car.GetComponent<Vehicle.CarDamage>();
            float healthFactor = dmg != null ? dmg.Health01 : 1f;
            int reward = Mathf.RoundToInt(baseReward * (0.5f + healthFactor * 0.5f));

            GameManager.Instance.Hint = "E — сдать угнанную тачку $" + reward;
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (PlayerWallet.Instance != null) PlayerWallet.Instance.AddMoney(reward);
                var ci = player.GetComponent<CarInteraction>();
                if (ci != null) ci.ForceClear();
                Destroy(car.gameObject);
                GameManager.Instance.SetCameraTarget(player);
                player.position = transform.position + new Vector3(4f, 0.5f, 0f);
            }
        }
    }
}
