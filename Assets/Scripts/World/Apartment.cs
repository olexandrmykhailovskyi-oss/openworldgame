using UnityEngine;
using OpenWorld.Economy;

namespace OpenWorld.World
{
    public class Apartment : MonoBehaviour
    {
        public int price = 2500;
        public int incomePerMinute = 45;
        public bool owned;
        public float interactDistance = 3.5f;

        Transform player;
        Transform interiorPoint;
        float incomeTimer;

        void Start()
        {
            string key = "Apt_" + transform.position.x.ToString("0") + "_" + transform.position.z.ToString("0");
            owned = PlayerPrefs.GetInt(key, 0) == 1;

            var interior = new GameObject("Interior");
            interior.transform.SetParent(transform, false);
            interior.transform.localPosition = new Vector3(0f, 12f, 0f);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(interior.transform, false);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(8f, 0.3f, 8f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.55f, 0.45f, 0.35f) };
            interiorPoint = interior.transform;

            UpdateVisual();
        }

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

            var car = player.GetComponent<CarInteraction>();
            bool inCar = car != null && car.CurrentCar != null;
            if (inCar) return;

            if (owned)
            {
                incomeTimer += Time.deltaTime;
                if (incomeTimer >= 60f)
                {
                    incomeTimer = 0f;
                    if (PlayerWallet.Instance != null) PlayerWallet.Instance.AddMoney(incomePerMinute);
                }
            }

            if (!owned)
            {
                int money = PlayerWallet.Instance != null ? PlayerWallet.Instance.Money : 0;
                string hint = money >= price ? "E — купить квартиру $" + price : "Нужно $" + price + " (у тебя $" + money + ")";
                GameManager.Instance.Hint = hint;
                if (money >= price && Input.GetKeyDown(KeyCode.E)) Buy();
            }
            else
            {
                GameManager.Instance.Hint = "E — войти в квартиру | H — продать за $" + (price / 2);
                if (Input.GetKeyDown(KeyCode.E)) Enter();
                if (Input.GetKeyDown(KeyCode.H)) Sell();
            }
        }

        void Buy()
        {
            if (PlayerWallet.Instance != null && PlayerWallet.Instance.TrySpend(price))
            {
                owned = true;
                string key = "Apt_" + transform.position.x.ToString("0") + "_" + transform.position.z.ToString("0");
                PlayerPrefs.SetInt(key, 1);
                UpdateVisual();
            }
        }

        void Sell()
        {
            owned = false;
            string key = "Apt_" + transform.position.x.ToString("0") + "_" + transform.position.z.ToString("0");
            PlayerPrefs.SetInt(key, 0);
            if (PlayerWallet.Instance != null) PlayerWallet.Instance.AddMoney(price / 2);
            UpdateVisual();
        }

        void Enter()
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = interiorPoint.position + Vector3.up * 1.5f;
            if (cc != null) cc.enabled = true;
        }

        void UpdateVisual()
        {
            var rend = GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = owned ? new Color(0.3f, 0.85f, 0.4f) : new Color(0.85f, 0.7f, 0.2f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", (owned ? new Color(0.2f, 0.6f, 0.3f) : new Color(0.6f, 0.5f, 0.1f)) * 0.6f);
                rend.sharedMaterial = mat;
            }
        }
    }
}
