using UnityEngine;
using OpenWorld.Economy;

namespace OpenWorld.World
{
    public class RaceManager : MonoBehaviour
    {
        public int reward = 750;
        public float checkpointRadius = 7f;
        public Transform[] checkpoints;
        int current;
        bool active;
        float startTime;
        Transform player;

        void Update()
        {
            if (player == null)
            {
                var p = FindObjectOfType<PlayerController>();
                if (p != null) player = p.transform;
                else return;
            }

            var car = player.GetComponent<CarInteraction>();
            bool inCar = car != null && car.CurrentCar != null;
            Transform targetCar = inCar ? car.CurrentCar.transform : player;

            if (!active)
            {
                float d = Vector3.Distance(targetCar.position, transform.position);
                if (d < 6f && !inCar)
                {
                    GameManager.Instance.Hint = "E — начать гонку $" + reward;
                    if (Input.GetKeyDown(KeyCode.E)) StartRace();
                }
                return;
            }

            if (checkpoints == null || current >= checkpoints.Length) return;
            float dist = Vector3.Distance(targetCar.position, checkpoints[current].position);
            GameManager.Instance.Hint = "Гонка: чекпоинт " + (current + 1) + "/" + checkpoints.Length + "  " + dist.ToString("0") + "м";
            if (dist < checkpointRadius)
            {
                current++;
                if (current >= checkpoints.Length) FinishRace();
            }

            if (Input.GetKeyDown(KeyCode.H)) { active = false; GameManager.Instance.Hint = "Гонка отменена"; }
        }

        void StartRace()
        {
            active = true;
            current = 0;
            startTime = Time.time;
            CreateCheckpoints();
        }

        void CreateCheckpoints()
        {
            var city = FindObjectOfType<CityGenerator>();
            if (city == null) return;
            var rnd = new System.Random((int)(transform.position.x * 100f + transform.position.z));
            int count = 5;
            checkpoints = new Transform[count];
            Vector3 start = transform.position;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = city.GetRandomRoadPoint(rnd.Next(), out bool alongZ);
                p.y = 0.5f;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = "Checkpoint_" + i;
                go.transform.position = p + Vector3.up * 2f;
                go.transform.localScale = new Vector3(5f, 2f, 5f);
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.2f, 0.9f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.3f, 1f) * 1.2f);
                go.GetComponent<MeshRenderer>().sharedMaterial = mat;
                var col = go.GetComponent<CapsuleCollider>();
                if (col != null) Destroy(col);
                var ring = go.AddComponent<BoxCollider>();
                ring.isTrigger = true;
                ring.size = new Vector3(1f, 1f, 1f);
                checkpoints[i] = go.transform;
            }
        }

        void FinishRace()
        {
            active = false;
            float time = Time.time - startTime;
            int bonus = time < 45f ? 300 : 0;
            int total = reward + bonus;
            if (PlayerWallet.Instance != null) PlayerWallet.Instance.AddMoney(total);
            GameManager.Instance.Hint = "Финиш! +" + total + "$  время " + time.ToString("0.0") + "с";
            if (checkpoints != null) foreach (var c in checkpoints) if (c != null) Destroy(c.gameObject, 3f);
        }
    }
}
