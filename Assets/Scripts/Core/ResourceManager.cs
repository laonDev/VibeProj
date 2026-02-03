using System;
using UnityEngine;

namespace AnimalKitchen
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Header("Currency")]
        [SerializeField] private int gold = 1000;
        [SerializeField] private int gems = 10;

        public int Gold => gold;
        public int Gems => gems;

        public event Action<int> OnGoldChanged;
        public event Action<int> OnGemsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0) return false;
            if (gold < amount) return false;

            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            Debug.Log($"[ResourceManager] Spent {amount} gold. Remaining: {gold}");
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            gold += amount;
            OnGoldChanged?.Invoke(gold);
            Debug.Log($"[ResourceManager] Earned {amount} gold. Total: {gold}");
        }

        public bool SpendGems(int amount)
        {
            if (amount <= 0) return false;
            if (gems < amount) return false;

            gems -= amount;
            OnGemsChanged?.Invoke(gems);
            return true;
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;

            gems += amount;
            OnGemsChanged?.Invoke(gems);
        }

        public bool HasEnoughGold(int amount) => gold >= amount;
        public bool HasEnoughGems(int amount) => gems >= amount;

        public void SetResources(int newGold, int newGems)
        {
            gold = Mathf.Max(0, newGold);
            gems = Mathf.Max(0, newGems);
            OnGoldChanged?.Invoke(gold);
            OnGemsChanged?.Invoke(gems);
        }
    }
}
