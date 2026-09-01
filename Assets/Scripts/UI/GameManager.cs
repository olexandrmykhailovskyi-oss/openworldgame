using UnityEngine;
using OpenWorld.Economy;
using OpenWorld.Jobs;
using OpenWorld.Weapon;

namespace OpenWorld
{
    public class GameManager : MonoBehaviour
    {
        static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameObject("GameManager").AddComponent<GameManager>();
                return instance;
            }
        }

        public string Hint { get; set; } = "";

        bool showHelp = true;
        GUIStyle hintStyle;
        GUIStyle helpStyle;
        GUIStyle speedStyle;
        GUIStyle moneyStyle;
        GUIStyle jobStyle;
        GUIStyle ammoStyle;

        string helpText =
            "УПРАВЛЕНИЕ\n" +
            "WASD — движение / руль и газ\n" +
            "Shift — бег | Space — прыжок / ручник\n" +
            "E — сесть/выйти, взять работу, банкомат\n" +
            "ЛКМ — стрелять (пешком) | R — перезарядка\n" +
            "H — подсказки | Esc — курсор\n" +
            "ЗАРАБОТОК: такси, курьер, сбор, монетки, банкоматы";

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H)) showHelp = !showHelp;

            if (JobManager.Instance != null && JobManager.Instance.HasJob)
            {
                var player = FindObjectOfType<PlayerController>();
                if (player != null && JobManager.Instance.IsNearTarget(player.transform.position, 7f))
                {
                    JobManager.Instance.CompleteJob();
                }
            }

            Hint = "";
        }

        public void SetCameraTarget(Transform t)
        {
            var cam = Camera.main;
            if (cam == null) return;
            var tc = cam.GetComponent<ThirdPersonCamera>();
            if (tc != null) tc.target = t;
        }

        void OnGUI()
        {
            EnsureStyles();

            int money = PlayerWallet.Instance != null ? PlayerWallet.Instance.Money : 0;
            GUI.Label(new Rect(Screen.width / 2f - 100f, 10f, 200f, 30f), "$ " + money, moneyStyle);

            GUI.Label(new Rect(0f, Screen.height - 95f, Screen.width, 30f), Hint, hintStyle);

            if (JobManager.Instance != null && JobManager.Instance.HasJob)
            {
                var jm = JobManager.Instance;
                var player = FindObjectOfType<PlayerController>();
                float dist = player != null ? jm.DistanceToTarget(player.transform.position) : 0f;
                string jobText = jm.Description + "  $" + jm.Reward + "  [" + dist.ToString("0") + "м]";
                GUI.Label(new Rect(Screen.width / 2f - 180f, Screen.height - 68f, 360f, 26f), jobText, jobStyle);
            }

            var pistol = FindObjectOfType<Pistol>();
            var car = CarController.ActiveCar;
            if (car != null)
            {
                GUI.Label(new Rect(Screen.width - 240f, Screen.height - 70f, 220f, 40f),
                    car.SpeedKmh.ToString("0") + " км/ч", speedStyle);
            }
            else if (pistol != null)
            {
                string ammoText = pistol.ammo + " / " + pistol.maxAmmo + (pistol.IsReloading ? "  перезарядка..." : "");
                GUI.Label(new Rect(Screen.width - 240f, Screen.height - 70f, 220f, 30f), ammoText, ammoStyle);
            }

            if (showHelp)
            {
                GUI.Box(new Rect(10f, 10f, 350f, 198f), GUIContent.none);
                GUI.Label(new Rect(22f, 20f, 330f, 182f), helpText, helpStyle);
            }
        }

        void EnsureStyles()
        {
            if (hintStyle != null) return;

            hintStyle = new GUIStyle();
            hintStyle.alignment = TextAnchor.MiddleCenter;
            hintStyle.fontSize = 20;
            hintStyle.normal.textColor = Color.white;
            var hs = new GUIStyleState(); hs.textColor = Color.white; hintStyle.normal = hs;

            helpStyle = new GUIStyle();
            helpStyle.fontSize = 13;
            helpStyle.normal.textColor = Color.white;
            helpStyle.wordWrap = true;

            speedStyle = new GUIStyle();
            speedStyle.alignment = TextAnchor.MiddleRight;
            speedStyle.fontSize = 24;
            speedStyle.normal.textColor = Color.white;

            moneyStyle = new GUIStyle();
            moneyStyle.alignment = TextAnchor.MiddleCenter;
            moneyStyle.fontSize = 26;
            moneyStyle.normal.textColor = new Color(0.2f, 1f, 0.4f);

            jobStyle = new GUIStyle();
            jobStyle.alignment = TextAnchor.MiddleCenter;
            jobStyle.fontSize = 16;
            jobStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);

            ammoStyle = new GUIStyle();
            ammoStyle.alignment = TextAnchor.MiddleRight;
            ammoStyle.fontSize = 18;
            ammoStyle.normal.textColor = Color.white;
        }
    }
}
