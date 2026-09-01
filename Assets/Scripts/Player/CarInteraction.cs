using UnityEngine;
using OpenWorld.Vehicle;

namespace OpenWorld
{
    public class CarInteraction : MonoBehaviour
    {
        public float enterDistance = 4.5f;
        public float exitSideOffset = 2.2f;

        public CarController CurrentCar { get; private set; }

        CarController nearestCar;
        TrafficCar nearestTraffic;
        PlayerController player;
        CharacterController cc;
        Renderer[] renderers;

        void Awake()
        {
            player = GetComponent<PlayerController>();
            cc = GetComponent<CharacterController>();
            renderers = GetComponentsInChildren<Renderer>();
        }

        void Update()
        {
            if (CurrentCar == null)
            {
                nearestCar = FindNearestCar();
                nearestTraffic = nearestCar == null ? FindNearestTraffic() : null;

                if (nearestCar != null)
                {
                    GameManager.Instance.Hint = "E — сесть в машину";
                    if (Input.GetKeyDown(KeyCode.E)) Enter(nearestCar);
                }
                else if (nearestTraffic != null)
                {
                    GameManager.Instance.Hint = "E — угнать машину";
                    if (Input.GetKeyDown(KeyCode.E)) Hijack(nearestTraffic);
                }
                else
                {
                    if (GameManager.Instance.Hint == "E — сесть в машину" || GameManager.Instance.Hint == "E — угнать машину")
                        GameManager.Instance.Hint = "";
                }
            }
            else
            {
                GameManager.Instance.Hint = "E — выйти из машины";
                if (Input.GetKeyDown(KeyCode.E)) Exit();
            }
        }

        CarController FindNearestCar()
        {
            CarController best = null;
            float bestD = enterDistance;
            var cars = GameObject.FindGameObjectsWithTag("Car");
            foreach (var go in cars)
            {
                var car = go.GetComponent<CarController>();
                if (car == null || car.ControlEnabled) continue;
                float d = Vector3.Distance(transform.position, go.transform.position);
                if (d < bestD)
                {
                    bestD = d;
                    best = car;
                }
            }
            return best;
        }

        TrafficCar FindNearestTraffic()
        {
            TrafficCar best = null;
            float bestD = enterDistance;
            var all = FindObjectsOfType<TrafficCar>();
            foreach (var t in all)
            {
                float d = Vector3.Distance(transform.position, t.transform.position);
                if (d < bestD)
                {
                    bestD = d;
                    best = t;
                }
            }
            return best;
        }

        void Hijack(TrafficCar traffic)
        {
            Vector3 pos = traffic.transform.position;
            Quaternion rot = traffic.transform.rotation;
            Color col = Color.gray;
            var mr = traffic.GetComponentInChildren<MeshRenderer>();
            if (mr != null && mr.sharedMaterial != null) col = mr.sharedMaterial.color;
            Destroy(traffic.gameObject);
            var car = CarFactory.Create(pos + Vector3.up * 0.15f, rot.eulerAngles.y, col);
            car.gameObject.AddComponent<CarDamage>();
            Enter(car);
        }

        void Enter(CarController car)
        {
            CurrentCar = car;
            car.ControlEnabled = true;
            CarController.ActiveCar = car;
            if (car.GetComponent<CarDamage>() == null) car.gameObject.AddComponent<CarDamage>();
            player.ControlEnabled = false;
            cc.enabled = false;
            SetVisible(false);
            GameManager.Instance.SetCameraTarget(car.transform);
        }

        void Exit()
        {
            var car = CurrentCar;
            CurrentCar = null;
            CarController.ActiveCar = null;
            car.Park();
            Vector3 side = car.transform.TransformPoint(new Vector3(-exitSideOffset, 0f, 0f));
            transform.position = new Vector3(side.x, side.y + 0.5f, side.z);
            SetVisible(true);
            cc.enabled = true;
            player.ControlEnabled = true;
            GameManager.Instance.SetCameraTarget(transform);
        }

        void SetVisible(bool visible)
        {
            foreach (var r in renderers)
                if (r != null) r.enabled = visible;
        }

        public void ForceClear()
        {
            CurrentCar = null;
            CarController.ActiveCar = null;
            SetVisible(true);
            if (cc != null) cc.enabled = true;
            if (player != null) player.ControlEnabled = true;
        }
    }
}
