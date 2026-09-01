using UnityEngine;

namespace OpenWorld.Pets
{
    public class Dog : MonoBehaviour
    {
        public float followSpeed = 5f;
        public float wanderSpeed = 1.8f;
        public float followDistance = 2.2f;
        public float barkDistance = 10f;

        Transform player;
        Vector3 wanderTarget;
        float barkTimer;

        void Start()
        {
            var p = FindObjectOfType<PlayerController>();
            if (p != null) player = p.transform;
            PickWander();
            BuildVisual();
        }

        void BuildVisual()
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "DogBody";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.4f, 0.85f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var col = body.GetComponent<CapsuleCollider>(); if (col != null) Destroy(col);
            body.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.55f, 0.38f, 0.22f) };

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "DogHead";
            head.transform.SetParent(transform, false);
            head.transform.localPosition = new Vector3(0f, 0.55f, 0.45f);
            head.transform.localScale = new Vector3(0.42f, 0.38f, 0.45f);
            var hc = head.GetComponent<SphereCollider>(); if (hc != null) Destroy(hc);
            head.GetComponent<MeshRenderer>().sharedMaterial = body.GetComponent<MeshRenderer>().sharedMaterial;

            var earL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            earL.transform.SetParent(head.transform, false);
            earL.transform.localPosition = new Vector3(-0.18f, 0.05f, 0f);
            earL.transform.localScale = new Vector3(0.12f, 0.28f, 0.22f);
            if (earL.GetComponent<BoxCollider>() != null) Destroy(earL.GetComponent<BoxCollider>());
            earL.GetComponent<MeshRenderer>().sharedMaterial = body.GetComponent<MeshRenderer>().sharedMaterial;

            var earR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            earR.transform.SetParent(head.transform, false);
            earR.transform.localPosition = new Vector3(0.18f, 0.05f, 0f);
            earR.transform.localScale = new Vector3(0.12f, 0.28f, 0.22f);
            if (earR.GetComponent<BoxCollider>() != null) Destroy(earR.GetComponent<BoxCollider>());
            earR.GetComponent<MeshRenderer>().sharedMaterial = body.GetComponent<MeshRenderer>().sharedMaterial;

            var tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tail.name = "Tail";
            tail.transform.SetParent(transform, false);
            tail.transform.localPosition = new Vector3(0f, 0.42f, -0.42f);
            tail.transform.localScale = new Vector3(0.12f, 0.35f, 0.12f);
            tail.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            if (tail.GetComponent<CapsuleCollider>() != null) Destroy(tail.GetComponent<CapsuleCollider>());
            tail.GetComponent<MeshRenderer>().sharedMaterial = body.GetComponent<MeshRenderer>().sharedMaterial;
        }

        void Update()
        {
            if (player == null) { var p = FindObjectOfType<PlayerController>(); if (p != null) player = p.transform; else { Wander(); return; } }

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > followDistance)
            {
                Vector3 dir = (player.position - transform.position); dir.y = 0f; dir.Normalize();
                transform.position += dir * followSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
                barkTimer -= Time.deltaTime;
                if (dist < barkDistance && barkTimer <= 0f && Random.Range(0f, 1f) < 0.02f) { barkTimer = 3f; }
            }
            else
            {
                Wander();
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(player.position - transform.position), 3f * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.E) && dist < 2.2f)
            {
                GameManager.Instance.Hint = "Гав! Хороший пёс ♥";
            }
        }

        void Wander()
        {
            Vector3 dir = wanderTarget - transform.position; dir.y = 0f;
            if (dir.magnitude < 0.7f) PickWander();
            else { dir.Normalize(); transform.position += dir * wanderSpeed * Time.deltaTime; }
        }

        void PickWander()
        {
            if (player != null) wanderTarget = player.position + new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f));
            else wanderTarget = transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            wanderTarget.y = transform.position.y;
        }
    }
}
