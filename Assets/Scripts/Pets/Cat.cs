using UnityEngine;

namespace OpenWorld.Pets
{
    public class Cat : MonoBehaviour
    {
        public float wanderSpeed = 1.35f;
        public float sitChance = 0.015f;

        Vector3 target;
        float wait;
        bool sitting;
        float sitTimer;

        void Start()
        {
            PickTarget();
            BuildVisual();
        }

        void BuildVisual()
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "CatBody";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            body.transform.localScale = new Vector3(0.38f, 0.32f, 0.62f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var c = body.GetComponent<CapsuleCollider>(); if (c != null) Destroy(c);
            var mat = new Material(Shader.Find("Standard")) { color = new Color(0.78f, 0.68f, 0.58f) };
            if (Random.Range(0f, 1f) < 0.5f) mat.color = new Color(0.92f, 0.88f, 0.85f);
            if (Random.Range(0f, 1f) < 0.25f) mat.color = new Color(0.18f, 0.18f, 0.19f);
            body.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "CatHead";
            head.transform.SetParent(transform, false);
            head.transform.localPosition = new Vector3(0f, 0.42f, 0.38f);
            head.transform.localScale = new Vector3(0.32f, 0.30f, 0.32f);
            if (head.GetComponent<SphereCollider>() != null) Destroy(head.GetComponent<SphereCollider>());
            head.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var earL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            earL.transform.SetParent(head.transform, false);
            earL.transform.localPosition = new Vector3(-0.11f, 0.14f, 0f);
            earL.transform.localScale = new Vector3(0.09f, 0.14f, 0.08f);
            if (earL.GetComponent<BoxCollider>() != null) Destroy(earL.GetComponent<BoxCollider>());
            earL.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var earR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            earR.transform.SetParent(head.transform, false);
            earR.transform.localPosition = new Vector3(0.11f, 0.14f, 0f);
            earR.transform.localScale = new Vector3(0.09f, 0.14f, 0.08f);
            if (earR.GetComponent<BoxCollider>() != null) Destroy(earR.GetComponent<BoxCollider>());
            earR.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tail.name = "Tail";
            tail.transform.SetParent(transform, false);
            tail.transform.localPosition = new Vector3(0f, 0.32f, -0.34f);
            tail.transform.localScale = new Vector3(0.08f, 0.42f, 0.08f);
            tail.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
            if (tail.GetComponent<CapsuleCollider>() != null) Destroy(tail.GetComponent<CapsuleCollider>());
            tail.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        void Update()
        {
            if (sitting)
            {
                sitTimer -= Time.deltaTime;
                if (sitTimer <= 0f) sitting = false;
                return;
            }

            if (wait > 0f) { wait -= Time.deltaTime; return; }

            if (Random.Range(0f, 1f) < sitChance) { sitting = true; sitTimer = Random.Range(2f, 4f); return; }

            Vector3 dir = target - transform.position; dir.y = 0f;
            if (dir.magnitude < 0.6f) { wait = Random.Range(0.6f, 1.8f); PickTarget(); return; }
            dir.Normalize();
            transform.position += dir * wanderSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);

            var player = FindObjectOfType<PlayerController>();
            if (player != null && Vector3.Distance(transform.position, player.transform.position) < 1.8f && Input.GetKeyDown(KeyCode.E))
            {
                GameManager.Instance.Hint = "Мурр~ ♥";
                sitting = true; sitTimer = 2.5f;
            }
        }

        void PickTarget()
        {
            var city = FindObjectOfType<CityGenerator>();
            if (city != null)
            {
                int bx = Random.Range(0, city.blocksX);
                int bz = Random.Range(0, city.blocksZ);
                Vector3 center = new Vector3(city.RoadLineX(bx) + city.Cell / 2f, 0f, city.RoadLineZ(bz) + city.Cell / 2f);
                Vector2 rnd = Random.insideUnitCircle * (city.blockSize / 2f - 4f);
                target = center + new Vector3(rnd.x, 0f, rnd.y);
                target.y = transform.position.y;
                return;
            }
            target = transform.position + new Vector3(Random.Range(-6f, 6f), 0f, Random.Range(-6f, 6f));
        }
    }
}
