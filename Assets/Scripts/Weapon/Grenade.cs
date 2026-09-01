using UnityEngine;

namespace OpenWorld.Weapon
{
    public class Grenade : MonoBehaviour
    {
        public float fuse = 3f;
        public float radius = 9f;
        public int damage = 90;
        public float force = 900f;

        float timer;
        Rigidbody rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            timer = fuse;
            Invoke(nameof(Explode), fuse);
        }

        void Update()
        {
            timer -= Time.deltaTime;
        }

        void Explode()
        {
            var hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var c in hits)
            {
                var ent = c.GetComponentInParent<Entities.Entity>();
                if (ent != null)
                {
                    float d = Vector3.Distance(transform.position, ent.transform.position);
                    float mult = 1f - Mathf.Clamp01(d / radius);
                    ent.TakeDamage(Mathf.RoundToInt(damage * mult));
                }
                var car = c.GetComponentInParent<Vehicle.CarDamage>();
                if (car != null) car.ApplyDamage(damage * 0.8f);
                var r = c.attachedRigidbody;
                if (r != null)
                {
                    Vector3 dir = (r.transform.position - transform.position).normalized + Vector3.up * 0.4f;
                    r.AddForce(dir.normalized * force);
                }
            }

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = transform.position;
            sphere.transform.localScale = new Vector3(radius * 1.6f, radius * 1.6f, radius * 1.6f);
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.45f, 0.12f, 0.5f);
            sphere.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Destroy(sphere, 0.45f);
            Destroy(gameObject);
        }
    }
}
