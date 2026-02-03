using System;
using UnityEngine;

namespace AnimalKitchen
{
    [Serializable]
    public class SaveData
    {
        public int gold;
        public int gems;
        public int restaurantLevel;
        public string lastPlayTime;

        // Restaurant data
        public int tableCount;
        public int kitchenSlots;
        public string[] unlockedRecipeNames;

        // Staff data
        public StaffSaveData[] hiredStaff;
    }

    [Serializable]
    public class StaffSaveData
    {
        public string staffName;
        public int level;
        public string staffType;
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_KEY = "AnimalKitchenSave";

        [Header("Auto Save")]
        [SerializeField] private float autoSaveInterval = 60f; // Save every 60 seconds
        private float autoSaveTimer;

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

        private void Start()
        {
            LoadGame();
            autoSaveTimer = autoSaveInterval;
        }

        private void Update()
        {
            // Auto-save timer
            autoSaveTimer -= Time.deltaTime;
            if (autoSaveTimer <= 0)
            {
                SaveGame();
                autoSaveTimer = autoSaveInterval;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Save when app goes to background
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            // Save when app quits
            SaveGame();
        }

        public void SaveGame()
        {
            var restaurant = GameManager.Instance?.CurrentRestaurant;

            var saveData = new SaveData
            {
                gold = ResourceManager.Instance?.Gold ?? 0,
                gems = ResourceManager.Instance?.Gems ?? 0,
                restaurantLevel = restaurant?.Level ?? 1,
                lastPlayTime = DateTime.Now.ToString("o"),
                tableCount = restaurant?.Tables.Count ?? 0,
                kitchenSlots = restaurant?.Kitchen?.AvailableSlots ?? 3
            };

            // Save unlocked recipes
            if (restaurant != null && restaurant.UnlockedRecipes.Count > 0)
            {
                saveData.unlockedRecipeNames = new string[restaurant.UnlockedRecipes.Count];
                for (int i = 0; i < restaurant.UnlockedRecipes.Count; i++)
                {
                    saveData.unlockedRecipeNames[i] = restaurant.UnlockedRecipes[i].recipeName;
                }
            }

            // Save hired staff
            if (restaurant != null && restaurant.HiredStaff.Count > 0)
            {
                var validStaff = new System.Collections.Generic.List<StaffSaveData>();
                for (int i = 0; i < restaurant.HiredStaff.Count; i++)
                {
                    var staff = restaurant.HiredStaff[i];
                    if (staff != null && staff.Data != null)
                    {
                        validStaff.Add(new StaffSaveData
                        {
                            staffName = staff.Data.staffName,
                            level = staff.Level,
                            staffType = staff.Data.staffType.ToString()
                        });
                    }
                    else
                    {
                        Debug.LogWarning($"[SaveManager] Staff at index {i} or its Data is null, skipping save");
                    }
                }
                saveData.hiredStaff = validStaff.ToArray();
            }

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            Debug.Log("[SaveManager] Game saved");
        }

        public void LoadGame()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                Debug.Log("[SaveManager] No save data found, starting fresh");
                return;
            }

            string json = PlayerPrefs.GetString(SAVE_KEY);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData != null)
            {
                // Load resources
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.SetResources(saveData.gold, saveData.gems);
                }

                Debug.Log($"[SaveManager] Game loaded - Gold: {saveData.gold}, Gems: {saveData.gems}");

                // Calculate offline earnings
                CalculateOfflineEarnings(saveData.lastPlayTime);

                // Note: Restaurant data (recipes, staff, tables) will be loaded
                // when the restaurant is initialized in the scene
                // This is handled by LoadRestaurantData() which should be called by Restaurant.Start()
            }
        }

        public SaveData GetSaveData()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                return null;
            }

            string json = PlayerPrefs.GetString(SAVE_KEY);
            return JsonUtility.FromJson<SaveData>(json);
        }

        private void CalculateOfflineEarnings(string lastPlayTimeStr)
        {
            if (string.IsNullOrEmpty(lastPlayTimeStr)) return;

            if (DateTime.TryParse(lastPlayTimeStr, out DateTime lastPlayTime))
            {
                TimeSpan offlineTime = DateTime.Now - lastPlayTime;
                if (offlineTime.TotalMinutes > 1)
                {
                    int offlineGold = Mathf.Min((int)(offlineTime.TotalMinutes * 10), 10000);
                    ResourceManager.Instance?.AddGold(offlineGold);
                    Debug.Log($"[SaveManager] Offline earnings: {offlineGold} gold for {offlineTime.TotalMinutes:F0} minutes");
                }
            }
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            Debug.Log("[SaveManager] Save data deleted");
        }
    }
}
