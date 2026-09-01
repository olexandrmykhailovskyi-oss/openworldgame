using UnityEngine;
using OpenWorld.Economy;

namespace OpenWorld.Economy
{
    public class MoneyPickup : MonoBehaviour
    {
        public int amount = 25;
        public float spinSpeed = 90f;
        public float bobSpeed = 1.2f;
        public float bobHeight = 0.25f;

        Vector3 startPos;

        void Start()
        {
            startPos = transform.position;
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void Update()
        {
            transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World);
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = startPos + Vector3.up * bob;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (PlayerWallet.Instance != null) PlayerWallet.Instance.AddMoney(amount);
            Destroy(gameObject);
        }
    }
}
