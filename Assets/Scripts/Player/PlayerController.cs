using UnityEngine;

namespace OpenWorld
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float walkSpeed = 4f;
        public float runSpeed = 8f;
        public float jumpHeight = 1.2f;
        public float gravity = -20f;
        public float turnSpeed = 10f;

        public bool ControlEnabled { get; set; } = true;

        CharacterController cc;
        float vy;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (!ControlEnabled) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            var cam = Camera.main;
            Transform basis = cam != null ? cam.transform : transform;
            Vector3 dir = basis.right * h + basis.forward * v;
            dir.y = 0f;
            if (dir.sqrMagnitude > 1f) dir.Normalize();

            float speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? runSpeed : walkSpeed;

            if (cc.isGrounded)
            {
                if (vy < 0f) vy = -1f;
                if (Input.GetKeyDown(KeyCode.Space))
                    vy = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            vy += gravity * Time.deltaTime;

            Vector3 move = dir * speed + Vector3.up * vy;
            cc.Move(move * Time.deltaTime);

            if (dir.sqrMagnitude > 0.001f)
            {
                var look = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
            }
        }
    }
}
