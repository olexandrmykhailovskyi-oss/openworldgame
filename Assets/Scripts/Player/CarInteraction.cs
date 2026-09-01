using UnityEngine;

namespace OpenWorld
{
    public class CarInteraction : MonoBehaviour
    {
        public float enterDistance = 4.5f;
        public float exitSideOffset = 2.2f;

        public CarController CurrentCar { get; private set; }

        CarController nearest;
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
                nearest = FindNearest();
                GameManager.Instance.Hint = nearest != null ? "E — сесть в машину" : "";
                if (nearest != null && Input.GetKeyDown(KeyCode.E)) Enter(nearest);
            }
            else
            {
                GameManager.Instance.Hint = "E — выйти из машины";
                if (Input.GetKeyDown(KeyCode.E)) Exit();
            }
        }

        CarController FindNearest()
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

        void Enter(CarController car)
        {
            CurrentCar = car;
            car.ControlEnabled = true;
            CarController.ActiveCar = car;
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
    }
}
