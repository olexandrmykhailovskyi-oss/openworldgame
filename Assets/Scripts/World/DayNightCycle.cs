using UnityEngine;

namespace OpenWorld.World
{
    public class DayNightCycle : MonoBehaviour
    {
        public float cycleDuration = 90f;
        public Light sun;
        float t;

        void Start()
        {
            if (sun == null)
            {
                var go = GameObject.Find("Directional Light");
                if (go != null) sun = go.GetComponent<Light>();
            }
        }

        void Update()
        {
            t += Time.deltaTime / cycleDuration;
            if (t > 1f) t -= 1f;
            float angle = t * 360f - 90f;
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(angle, -30f, 0f);
                float day = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI * 2f) * 0.7f + 0.3f);
                sun.intensity = Mathf.Lerp(0.12f, 1.18f, day);
                RenderSettings.ambientIntensity = Mathf.Lerp(0.35f, 1.05f, day);
                RenderSettings.fogDensity = Mathf.Lerp(0.0045f, 0.0022f, day);
            }
            bool isNight = angle > 160f || angle < 10f;
            UpdateStreetLights(isNight);
        }

        System.Collections.Generic.List<MeshRenderer> lampCache;
        void UpdateStreetLights(bool night)
        {
            if (lampCache == null)
            {
                lampCache = new System.Collections.Generic.List<MeshRenderer>();
                var all = FindObjectsOfType<MeshRenderer>();
                foreach (var r in all) if (r.gameObject.name == "LampHead") lampCache.Add(r);
                if (lampCache.Count == 0) return;
            }
            foreach (var r in lampCache)
            {
                if (r == null) continue;
                var mat = r.sharedMaterial;
                if (mat == null) continue;
                if (night) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.55f) * 1.6f); }
                else mat.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
            }
        }
    }
}
