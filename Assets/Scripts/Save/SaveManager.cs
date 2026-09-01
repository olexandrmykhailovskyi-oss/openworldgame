using UnityEngine;

namespace OpenWorld.Save
{
    [System.Serializable]
    public class SaveData
    {
        public int money = 250;
        public int stars;
        public string ownedApartments = "";
        public int pistolAmmo = 18;
        public int shotgunAmmo = 8;
        public int rifleAmmo = 30;
    }

    public static class SaveManager
    {
        const string KEY = "OpenWorld_Save_v2";

        public static void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(KEY, json);
            PlayerPrefs.Save();
        }

        public static SaveData Load()
        {
            if (!PlayerPrefs.HasKey(KEY)) return new SaveData();
            string json = PlayerPrefs.GetString(KEY, "");
            if (string.IsNullOrEmpty(json)) return new SaveData();
            try { return JsonUtility.FromJson<SaveData>(json); } catch { return new SaveData(); }
        }

        public static void SaveNow()
        {
            var wallet = FindWallet();
            var ap = CollectApartments();
            var data = new SaveData();
            if (wallet != null) data.money = wallet.Money;
            data.ownedApartments = ap;
            var inv = FindInventory();
            if (inv != null)
            {
                var pistol = inv.GetComponent<Weapon.Pistol>();
                var shotgun = inv.GetComponent<Weapon.Shotgun>();
                var rifle = inv.GetComponent<Weapon.Rifle>();
                if (pistol != null) data.pistolAmmo = pistol.ammo;
                if (shotgun != null) data.shotgunAmmo = shotgun.ammo;
                if (rifle != null) data.rifleAmmo = rifle.ammo;
            }
            Save(data);
        }

        public static void LoadNow()
        {
            var data = Load();
            var wallet = FindWallet();
            if (wallet != null) wallet.SetMoney(data.money);
        }

        static Economy.PlayerWallet FindWallet()
        {
            return Object.FindObjectOfType<Economy.PlayerWallet>();
        }

        static Weapon.WeaponInventory FindInventory()
        {
            return Object.FindObjectOfType<Weapon.WeaponInventory>();
        }

        static string CollectApartments()
        {
            var apts = Object.FindObjectsOfType<World.Apartment>();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var a in apts) if (a.owned) sb.Append(a.transform.position.x.ToString("0") + "_" + a.transform.position.z.ToString("0") + ";");
            return sb.ToString();
        }
    }
}
