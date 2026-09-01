using UnityEngine;

namespace OpenWorld.Weapon
{
    public class WeaponInventory : MonoBehaviour
    {
        public Weapon[] weapons;
        public int currentIndex;

        void Awake()
        {
            weapons = GetComponents<Weapon>();
            if (weapons.Length == 0) return;
            Select(0);
        }

        void Update()
        {
            if (weapons.Length == 0) return;

            if (Input.GetKeyDown(KeyCode.Alpha1)) Select(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) && weapons.Length > 1) Select(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) && weapons.Length > 2) Select(2);
            if (Input.GetKeyDown(KeyCode.Alpha4) && weapons.Length > 3) Select(3);

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.01f) Select((currentIndex + 1) % weapons.Length);
            else if (scroll < -0.01f) Select((currentIndex - 1 + weapons.Length) % weapons.Length);

            if (Input.GetKeyDown(KeyCode.G)) ThrowGrenade();
        }

        void Select(int idx)
        {
            for (int i = 0; i < weapons.Length; i++)
                weapons[i].enabled = (i == idx);
            currentIndex = idx;
        }

        void ThrowGrenade()
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            Vector3 pos = cam.transform.position + cam.transform.forward * 1.2f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "GrenadeProj";
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 1.2f;
            rb.AddForce(cam.transform.forward * 620f + Vector3.up * 180f);
            go.AddComponent<Grenade>();
            var col = go.GetComponent<SphereCollider>();
            if (col != null) col.radius = 0.5f;
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.25f, 0.22f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        public Weapon Current => weapons != null && weapons.Length > 0 ? weapons[currentIndex] : null;
        public string CurrentName => Current != null ? Current.GetType().Name : "";
    }
}
