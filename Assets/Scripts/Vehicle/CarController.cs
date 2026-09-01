using UnityEngine;

namespace OpenWorld
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        public WheelCollider[] wheels;
        public Transform[] wheelMeshes;

        public float motorTorque = 1300f;
        public float maxSteerAngle = 32f;
        public float brakeTorque = 4000f;
        public float maxSpeedKmh = 140f;
        public float downForce = 40f;

        public bool ControlEnabled { get; set; }
        public static CarController ActiveCar;

        Rigidbody rb;
        float steerInput;
        float throttleInput;
        bool handbrake;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = new Vector3(0f, -0.35f, 0f);
        }

        void Update()
        {
            if (ControlEnabled)
            {
                steerInput = Input.GetAxis("Horizontal") * maxSteerAngle;
                throttleInput = Input.GetAxis("Vertical");
                handbrake = Input.GetKey(KeyCode.Space);
            }
            else
            {
                steerInput = 0f;
                throttleInput = 0f;
                handbrake = true;
            }
        }

        void FixedUpdate()
        {
            float kmh = rb.velocity.magnitude * 3.6f;
            for (int i = 0; i < wheels.Length; i++)
            {
                var w = wheels[i];
                if (w == null) continue;
                if (i < 2) w.steerAngle = steerInput;

                if (handbrake)
                {
                    w.motorTorque = 0f;
                    w.brakeTorque = brakeTorque;
                    continue;
                }

                w.brakeTorque = 0f;
                bool driven = i >= 2;
                w.motorTorque = driven && Mathf.Abs(throttleInput) > 0.01f && kmh < maxSpeedKmh
                    ? throttleInput * motorTorque
                    : 0f;
            }

            rb.AddForce(-transform.up * downForce * rb.velocity.magnitude);
        }

        void LateUpdate()
        {
            if (wheels == null || wheelMeshes == null) return;
            int count = Mathf.Min(wheels.Length, wheelMeshes.Length);
            for (int i = 0; i < count; i++)
            {
                if (wheels[i] == null || wheelMeshes[i] == null) continue;
                wheels[i].GetWorldPose(out Vector3 pos, out Quaternion quat);
                wheelMeshes[i].position = pos;
                wheelMeshes[i].rotation = quat;
            }
        }

        public void Park()
        {
            ControlEnabled = false;
            handbrake = true;
            steerInput = 0f;
            throttleInput = 0f;
        }

        public float SpeedKmh => rb != null ? rb.velocity.magnitude * 3.6f : 0f;
    }
}
