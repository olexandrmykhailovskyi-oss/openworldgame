using UnityEngine;

namespace OpenWorld
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float walkDistance = 5.5f;
        public float carDistance = 9f;
        public float height = 2.0f;
        public float minPitch = -35f;
        public float maxPitch = 65f;
        public float sensitivity = 3f;

        float yaw;
        float pitch = 15f;
        float distance;
        float lastMouseTime;

        void Start()
        {
            distance = walkDistance;
            if (target != null) yaw = target.eulerAngles.y;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            if (target == null) return;

            if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            float mx = Input.GetAxis("Mouse X") * sensitivity;
            float my = Input.GetAxis("Mouse Y") * sensitivity;
            if (Mathf.Abs(mx) + Mathf.Abs(my) > 0.001f) lastMouseTime = Time.time;
            yaw += mx;
            pitch = Mathf.Clamp(pitch - my, minPitch, maxPitch);

            var car = target.GetComponentInParent<CarController>();
            bool driving = car != null && car.ControlEnabled;
            if (driving && Time.time - lastMouseTime > 1.5f)
            {
                yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, Time.deltaTime * 2.5f);
            }

            float want = driving ? carDistance : walkDistance;
            distance = Mathf.Lerp(distance, want, Time.deltaTime * 3f);

            var rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = target.position + Vector3.up * height - rot * Vector3.forward * distance;
            transform.rotation = rot;
        }
    }
}
