using UnityEngine;

namespace OpenWorld.Jobs
{
    public class JobMarker : MonoBehaviour
    {
        Transform targetAnchor;
        float bob;

        void Start()
        {
            var col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(col.GetComponent<Collider>());
            col.transform.SetParent(transform, false);
            col.transform.localPosition = Vector3.up * 1f;
            col.transform.localScale = new Vector3(0.35f, 1f, 0.35f);
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.85f, 0.15f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.2f) * 1.4f);
            col.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(arrow.GetComponent<Collider>());
            arrow.transform.SetParent(transform, false);
            arrow.transform.localPosition = Vector3.up * 3.2f;
            arrow.transform.localScale = new Vector3(0.3f, 0.6f, 0.3f);
            arrow.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        void Update()
        {
            if (JobManager.Instance == null || !JobManager.Instance.HasJob)
            {
                if (gameObject.activeSelf) gameObject.SetActive(false);
                return;
            }
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            transform.position = JobManager.Instance.TargetPos + Vector3.up * (2f + Mathf.Sin(Time.time * 2f) * 0.35f);
            transform.Rotate(Vector3.up * 70f * Time.deltaTime, Space.World);
        }
    }
}
