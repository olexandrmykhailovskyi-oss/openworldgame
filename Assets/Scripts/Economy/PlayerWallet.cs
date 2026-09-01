using UnityEngine;

namespace OpenWorld.Economy
{
    public class PlayerWallet : MonoBehaviour
    {
        public static PlayerWallet Instance { get; private set; }

        public int Money { get; private set; }
        public int TotalEarned { get; private set; }

        public System.Action<int> OnMoneyChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            try
            {
                var data = Save.SaveManager.Load();
                Money = data.money;
                if (Money < 0) Money = PlayerPrefs.GetInt("Money", 250);
            }
            catch { Money = PlayerPrefs.GetInt("Money", 250); }
            PlayerPrefs.SetInt("Money", Money);
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0) return;
            Money += amount;
            TotalEarned += amount;
            PlayerPrefs.SetInt("Money", Money);
            OnMoneyChanged?.Invoke(Money);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0) return true;
            if (Money < amount) return false;
            Money -= amount;
            PlayerPrefs.SetInt("Money", Money);
            OnMoneyChanged?.Invoke(Money);
            return true;
        }

        public void SetMoney(int amount)
        {
            Money = Mathf.Max(0, amount);
            PlayerPrefs.SetInt("Money", Money);
            OnMoneyChanged?.Invoke(Money);
        }
    }
}
