using UnityEngine;

namespace OpenWorld
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        public WheelCollider[] wheels;
        public Transform[] wheelMeshes;

        public float motorTorque = 1850f;
        public float maxSteerAngle = 34f;
        public float brakeTorque = 5000f;
        public float maxSpeedKmh = 155f;
        public float downForce = 65f;
        public float grip = 1.15f;
        public float driftGrip = 0.52f;

        public bool ControlEnabled { get; set; }
        public static CarController ActiveCar;

        Rigidbody rb;
        float steerInput;
        float throttleInput;
        bool handbrake;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = new Vector3(0f, -0.9f, 0.3f);
            rb.drag = 0.08f;
            rb.angularDrag = 0.5f;
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
                steerInput = Mathf.MoveTowards(steerInput, 0f, 80f * Time.deltaTime);
                throttleInput = 0f;
                handbrake = true;
            }
        }

        void FixedUpdate()
        {
            float kmh = rb.velocity.magnitude * 3.6f;
            float speedFactor = Mathf.Clamp01(kmh / 80f);
            float steerFactor = Mathf.Lerp(1f, 0.42f, speedFactor);
            float curSteer = steerInput * steerFactor;

            for (int i = 0; i < wheels.Length; i++)
            {
                var w = wheels[i];
                if (w == null) continue;
                if (i < 2) w.steerAngle = curSteer;

                var sideways = w.sidewaysFriction;
                var forward = w.forwardFriction;
                if (handbrake)
                {
                    sideways.stiffness = driftGrip;
                    forward.stiffness = 0.75f;
                }
                else
                {
                    sideways.stiffness = grip;
                    forward.stiffness = 1.05f;
                }
                w.sidewaysFriction = sideways;
                w.forwardFriction = forward;

                if (handbrake)
                {
                    w.motorTorque = 0f;
                    w.brakeTorque = i >= 2 ? brakeTorque * 0.7f : brakeTorque * 0.12f;
                    continue;
                }

                w.brakeTorque = 0f;
                bool driven = i >= 2;
                bool canDrive = driven && Mathf.Abs(throttleInput) > 0.01f && kmh < maxSpeedKmh;
                w.motorTorque = canDrive ? throttleInput * motorTorque : 0f;
            }

            rb.AddForce(-transform.up * downForce * rb.velocity.magnitude);

            if (handbrake && rb.velocity.magnitude > 4f)
            {
                Vector3 lat = Vector3.Project(rb.velocity, transform.right);
                rb.AddForce(-lat * 0.55f, ForceMode.Acceleration);
                if (Random.Range(0f, 1f) < 0.35f && wheels != null && wheels.Length >= 4)
                {
                    Visuals.Effects.TireSmoke(wheels[2].transform.position);
                    Visuals.Effects.TireSmoke(wheels[3].transform.position);
                }
            }

            if (!handbrake && Mathf.Abs(throttleInput) > 0.1f && kmh > 20f)
            {
                rb.AddForce(transform.forward * throttleInput * 3.5f, ForceMode.Acceleration);
            }
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

            if (Mathf.Abs(steerInput) > 5f && rb.velocity.magnitude > 3f)
            {
                float tilt = -steerInput / maxSteerAngle * Mathf.Clamp01(rb.velocity.magnitude / 12f) * 2.8f;
                transform.localRotation *= Quaternion.Euler(0f, 0f, tilt * Time.deltaTime * 12f);
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
