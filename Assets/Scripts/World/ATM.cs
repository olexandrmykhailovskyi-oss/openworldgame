using UnityEngine;
using OpenWorld.Economy;

namespace OpenWorld.World
{
    public class ATM : MonoBehaviour
    {
        public int reward = 250;
        public float interactDistance = 3.5f;
        public float cooldown = 45f;

        float nextUse;
        Transform player;

        void Update()
        {
            if (player == null)
            {
                var p = FindObjectOfType<PlayerController>();
                if (p != null) player = p.transform;
                else return;
            }

            float d = Vector3.Distance(player.position, transform.position);
            if (d > interactDistance) return;

            bool ready = Time.time >= nextUse;
            string hint = ready ? "E — ограбить банкомат ($" + reward + ")" : "Банкомат пуст... " + Mathf.CeilToInt(nextUse - Time.time) + "с";
            var car = player.GetComponent<CarInteraction>();
            bool inCar = car != null && car.CurrentCar != null;
            if (!inCar && GameManager.Instance != null) GameManager.Instance.Hint = hint;

            if (ready && Input.GetKeyDown(KeyCode.E))
            {
                if (PlayerWallet.Instance != null) PlayerWallet.Instance.AddMoney(reward);
                nextUse = Time.time + cooldown;
            }
        }
    }
}
