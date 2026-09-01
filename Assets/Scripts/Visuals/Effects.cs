using UnityEngine;

namespace OpenWorld.Visuals
{
    public static class Effects
    {
        public static void MuzzleFlash(Vector3 pos, Vector3 dir)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = pos + dir * 0.7f;
            go.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.88f, 0.22f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 2.2f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var col = go.GetComponent<SphereCollider>();
            if (col != null) Object.Destroy(col);
            Object.Destroy(go, 0.06f);
        }

        public static void TireSmoke(Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = pos + Vector3.up * 0.25f;
            float s = 0.5f + Random.Range(0f, 0.35f);
            go.transform.localScale = new Vector3(s, s * 0.6f, s);
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.22f, 0.22f, 0.23f, 0.55f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var col = go.GetComponent<SphereCollider>();
            if (col != null) Object.Destroy(col);
            Object.Destroy(go, 0.9f);
        }

        public static void Explosion(Vector3 pos, float radius)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(radius * 0.6f, radius * 0.6f, radius * 0.6f);
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.42f, 0.08f, 0.7f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.15f) * 1.6f);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var col = go.GetComponent<SphereCollider>();
            if (col != null) Object.Destroy(col);
            Object.Destroy(go, 0.42f);
        }
    }
}
