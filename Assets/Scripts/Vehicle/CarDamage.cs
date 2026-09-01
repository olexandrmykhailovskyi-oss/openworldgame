using UnityEngine;

namespace OpenWorld.Vehicle
{
    public class CarDamage : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float health = 100f;
        public float dentScale = 0.12f;

        Renderer bodyRenderer;
        Color originalColor;
        Transform smoke;
        float smokeThreshold = 40f;

        void Awake()
        {
            var body = transform.Find("Body");
            if (body != null) bodyRenderer = body.GetComponent<Renderer>();
            if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<Renderer>();
            if (bodyRenderer != null) originalColor = bodyRenderer.sharedMaterial.color;

            var s = GameObject.CreatePrimitive(PrimitiveType.Cube);
            s.name = "Smoke";
            s.transform.SetParent(transform, false);
            s.transform.localPosition = new Vector3(0f, 1.1f, 1.6f);
            s.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            var smCol = s.GetComponent<BoxCollider>();
            if (smCol != null) Destroy(smCol);
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.25f, 0.25f, 0.26f, 0.55f);
            s.GetComponent<MeshRenderer>().sharedMaterial = mat;
            smoke = s.transform;
            smoke.gameObject.SetActive(false);
        }

        void OnCollisionEnter(Collision col)
        {
            float dmg = col.relativeVelocity.magnitude * 1.8f;
            if (dmg > 6f) ApplyDamage(dmg);
        }

        public void ApplyDamage(float amount)
        {
            health = Mathf.Max(0f, health - amount);
            float t = health / maxHealth;

            if (bodyRenderer != null)
            {
                Color damaged = Color.Lerp(new Color(0.18f, 0.18f, 0.19f), originalColor, t);
                if (health < 55f) damaged = Color.Lerp(damaged, new Color(0.35f, 0.28f, 0.22f), (55f - health) / 55f * 0.35f);
                bodyRenderer.sharedMaterial.color = damaged;
            }

            float deform = (1f - t) * dentScale;
            transform.localScale = new Vector3(1f - deform * 0.4f, 1f - deform * 0.2f, 1f + deform * 0.5f);

            if (health < smokeThreshold && smoke != null && !smoke.gameObject.activeSelf)
                smoke.gameObject.SetActive(true);

            if (smoke != null && smoke.gameObject.activeSelf)
            {
                float s = 0.6f + (1f - t) * 0.9f + Mathf.Sin(Time.time * 8f) * 0.12f;
                smoke.localScale = new Vector3(s, s, s);
            }

            if (health <= 0f)
            {
                Explode();
            }

            var ctrl = GetComponent<CarController>();
            if (ctrl != null)
            {
                ctrl.motorTorque = Mathf.Lerp(650f, 1350f, t);
                ctrl.maxSpeedKmh = Mathf.Lerp(55f, 155f, t);
            }
        }

        void Explode()
        {
            if (smoke != null) smoke.localScale = new Vector3(2.2f, 2.2f, 2.2f);
            var rb = GetComponent<Rigidbody>();
            if (rb != null) rb.AddForce(Vector3.up * 4200f + Random.insideUnitSphere * 1600f);
            Invoke(nameof(Remove), 3.5f);
        }

        void Remove()
        {
            Destroy(gameObject);
        }

        public void Repair()
        {
            health = maxHealth;
            if (bodyRenderer != null) bodyRenderer.sharedMaterial.color = originalColor;
            transform.localScale = Vector3.one;
            if (smoke != null) smoke.gameObject.SetActive(false);
            var ctrl = GetComponent<CarController>();
            if (ctrl != null) { ctrl.motorTorque = 1350f; ctrl.maxSpeedKmh = 155f; }
        }

        public float Health01 => health / maxHealth;
    }
}
