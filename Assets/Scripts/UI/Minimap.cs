using UnityEngine;

namespace OpenWorld
{
    public class Minimap : MonoBehaviour
    {
        public Transform followTarget;
        public float viewSize = 170f;
        public int textureSize = 256;
        public int uiSize = 220;

        RenderTexture texture;
        Camera mapCam;

        void Start()
        {
            texture = new RenderTexture(textureSize, textureSize, 16);

            var camGo = new GameObject("MinimapCamera");
            mapCam = camGo.AddComponent<Camera>();
            mapCam.orthographic = true;
            mapCam.orthographicSize = viewSize;
            mapCam.clearFlags = CameraClearFlags.SolidColor;
            mapCam.backgroundColor = new Color(0.12f, 0.16f, 0.12f);
            mapCam.targetTexture = texture;
            mapCam.farClipPlane = 600f;
            mapCam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        void LateUpdate()
        {
            if (mapCam == null) return;
            var car = CarController.ActiveCar;
            Transform t = car != null ? car.transform : followTarget;
            if (t == null) return;
            mapCam.transform.position = new Vector3(t.position.x, 250f, t.position.z);
        }

        void OnGUI()
        {
            if (texture == null) return;

            Rect frame = new Rect(Screen.width - uiSize - 26f, 20f, uiSize + 6f, uiSize + 6f);
            Rect map = new Rect(Screen.width - uiSize - 23f, 23f, uiSize, uiSize);
            Rect dot = new Rect(Screen.width - uiSize / 2f - 29f, 20f + uiSize / 2f, 6f, 6f);

            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(frame, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.DrawTexture(map, texture, ScaleMode.ScaleToFit, false);

            GUI.color = new Color(0.2f, 1f, 0.3f);
            GUI.DrawTexture(dot, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
