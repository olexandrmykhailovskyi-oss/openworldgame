using UnityEngine;

namespace OpenWorld.Police
{
    [RequireComponent(typeof(Rigidbody))]
    public class PoliceCar : MonoBehaviour
    {
        public float chaseSpeed = 13f;
        public float turnSpeed = 140f;
        public float ramForce = 900f;

        Rigidbody rb;
        Transform player;
        Transform sirenRed;
        Transform sirenBlue;
        float sirenTimer;

        public static PoliceCar Create(Vector3 pos, float yaw)
        {
            var root = new GameObject("PoliceCar");
            root.tag = "Police";
            root.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));

            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 1650f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.centerOfMass = new Vector3(0f, -0.9f, 0.2f);
            rb.drag = 0.08f;
            rb.angularDrag = 0.5f;

            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.85f, 0f);
            col.size = new Vector3(1.95f, 1.1f, 4.6f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            body.transform.localScale = new Vector3(1.95f, 0.7f, 4.5f);
            var bodyMat = new Material(Shader.Find("Standard"));
            bodyMat.color = new Color(0.92f, 0.92f, 0.94f);
            body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "Stripe";
            stripe.transform.SetParent(root.transform, false);
            stripe.transform.localPosition = new Vector3(0f, 0.96f, 0f);
            stripe.transform.localScale = new Vector3(1.96f, 0.12f, 4.52f);
            var stripeMat = new Material(Shader.Find("Standard"));
            stripeMat.color = new Color(0.12f, 0.22f, 0.65f);
            stripe.GetComponent<MeshRenderer>().sharedMaterial = stripeMat;

            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(root.transform, false);
            cabin.transform.localPosition = new Vector3(0f, 1.38f, -0.15f);
            cabin.transform.localScale = new Vector3(1.72f, 0.55f, 2.2f);
            var cabMat = new Material(Shader.Find("Standard"));
            cabMat.color = new Color(0.18f, 0.22f, 0.32f);
            cabin.GetComponent<MeshRenderer>().sharedMaterial = cabMat;

            var sirenGo = new GameObject("Siren");
            sirenGo.transform.SetParent(root.transform, false);
            sirenGo.transform.localPosition = new Vector3(0f, 1.68f, 0.35f);

            var red = GameObject.CreatePrimitive(PrimitiveType.Cube);
            red.name = "SirenRed";
            red.transform.SetParent(sirenGo.transform, false);
            red.transform.localPosition = new Vector3(-0.35f, 0f, 0f);
            red.transform.localScale = new Vector3(0.45f, 0.22f, 0.35f);
            var redMat = new Material(Shader.Find("Standard"));
            redMat.color = new Color(1f, 0.12f, 0.12f);
            redMat.EnableKeyword("_EMISSION");
            redMat.SetColor("_EmissionColor", new Color(1f, 0.2f, 0.2f) * 1.8f);
            red.GetComponent<MeshRenderer>().sharedMaterial = redMat;

            var blue = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blue.name = "SirenBlue";
            blue.transform.SetParent(sirenGo.transform, false);
            blue.transform.localPosition = new Vector3(0.35f, 0f, 0f);
            blue.transform.localScale = new Vector3(0.45f, 0.22f, 0.35f);
            var blueMat = new Material(Shader.Find("Standard"));
            blueMat.color = new Color(0.12f, 0.35f, 1f);
            blueMat.EnableKeyword("_EMISSION");
            blueMat.SetColor("_EmissionColor", new Color(0.2f, 0.45f, 1f) * 1.8f);
            blue.GetComponent<MeshRenderer>().sharedMaterial = blueMat;

            var pc = root.AddComponent<PoliceCar>();
            pc.sirenRed = red.transform;
            pc.sirenBlue = blue.transform;
            return pc;
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            var p = FindObjectOfType<PlayerController>();
            if (p != null) player = p.transform;
        }

        void Update()
        {
            sirenTimer += Time.deltaTime * 10f;
            if (sirenRed != null) sirenRed.gameObject.SetActive(Mathf.Sin(sirenTimer) > 0f);
            if (sirenBlue != null) sirenBlue.gameObject.SetActive(Mathf.Sin(sirenTimer) < 0f);
            if (player == null)
            {
                var p = FindObjectOfType<PlayerController>();
                if (p != null) player = p.transform;
            }
        }

        void FixedUpdate()
        {
            Transform target = null;
            var activeCar = CarController.ActiveCar;
            if (activeCar != null) target = activeCar.transform;
            else if (player != null) target = player;

            if (target == null) return;
            Vector3 toPlayer = target.position - transform.position;
            toPlayer.y = 0f;
            float dist = toPlayer.magnitude;
            if (dist < 1.5f) return;

            Vector3 dir = toPlayer.normalized;
            var look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);

            float speed = dist > 28f ? chaseSpeed * 1.25f : chaseSpeed;
            rb.velocity = transform.forward * speed + Vector3.up * rb.velocity.y;

            if (dist < 4.8f)
            {
                Vector3 ndir = (target.position - transform.position).normalized + Vector3.up * 0.18f;
                if (activeCar != null)
                {
                    var carDmg = activeCar.GetComponent<Vehicle.CarDamage>();
                    if (carDmg != null) carDmg.ApplyDamage(12f);
                    var carRb = activeCar.GetComponent<Rigidbody>();
                    if (carRb != null) carRb.AddForce(ndir * ramForce * 0.85f);
                }
                else
                {
                    var prb = target.GetComponent<Rigidbody>();
                    if (prb != null) prb.AddForce(ndir * ramForce);
                    var dmg = target.GetComponent<Entities.Entity>();
                    if (dmg != null) dmg.TakeDamage(9);
                    var cc = target.GetComponent<CharacterController>();
                    if (cc != null && dmg == null)
                    {
                        var pe = target.GetComponent<Entities.Pedestrian>();
                        if (pe == null) target.GetComponent<Entities.Entity>()?.TakeDamage(9);
                    }
                }
            }
        }
    }
}
