using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimalKitchen
{
    public class Restaurant : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private string restaurantName = "My Restaurant";
        [SerializeField] private int level = 1;

        [Header("Layout")]
        [SerializeField] private Transform entrance;
        [SerializeField] private Transform exit;
        [SerializeField] private Kitchen kitchen;
        [SerializeField] private List<Table> tables = new List<Table>();

        [Header("Menu")]
        [SerializeField] private List<RecipeData> unlockedRecipes = new List<RecipeData>();

        [Header("Staff")]
        [SerializeField] private List<Staff> hiredStaff = new List<Staff>();

        public string RestaurantName => restaurantName;
        public int Level => level;
        public Kitchen Kitchen => kitchen;
        public Transform Entrance => entrance;
        public Transform Exit => exit;
        public IReadOnlyList<Table> Tables => tables;
        public IReadOnlyList<RecipeData> UnlockedRecipes => unlockedRecipes;
        public IReadOnlyList<Staff> HiredStaff => hiredStaff;

        public event Action<int> OnLevelUp;
        public event Action<RecipeData> OnRecipeUnlocked;
        public event Action<Staff> OnStaffHired;

        private void Awake()
        {
            AutoFindReferences();
            RefreshTableIds();
        }

        private void Start()
        {
            GameManager.Instance?.SetRestaurant(this);

            // 기본 레시피가 없으면 자동으로 로드
            if (unlockedRecipes.Count == 0)
            {
                LoadDefaultRecipes();
            }
        }

        private void AutoFindReferences()
        {
            // 자식에서 자동으로 컴포넌트 찾기
            if (entrance == null)
                entrance = transform.Find("Entrance");
            if (exit == null)
                exit = transform.Find("Exit");
            if (kitchen == null)
                kitchen = GetComponentInChildren<Kitchen>();
            if (tables.Count == 0)
            {
                var foundTables = GetComponentsInChildren<Table>();
                tables.AddRange(foundTables);
            }
        }

        private void LoadDefaultRecipes()
        {
            // Resources 폴더 또는 ScriptableObjects에서 기본 레시피 로드
            var recipes = Resources.LoadAll<RecipeData>("Recipes");
            if (recipes.Length > 0)
            {
                foreach (var recipe in recipes)
                {
                    if (recipe.unlockCost == 0) // 무료 레시피만 기본 해금
                    {
                        unlockedRecipes.Add(recipe);
                    }
                }
            }

            // Resources에 없으면 ScriptableObjects 폴더에서 찾기
            #if UNITY_EDITOR
            if (unlockedRecipes.Count == 0)
            {
                Debug.LogWarning("[Restaurant] No default recipes found. Create recipes in ScriptableObjects/Recipes/");
            }
            #endif
        }

        private void RefreshTableIds()
        {
            for (int i = 0; i < tables.Count; i++)
            {
                tables[i].SetTableId(i);
            }
        }

        public Table GetAvailableTable()
        {
            foreach (var table in tables)
            {
                if (!table.IsOccupied)
                    return table;
            }
            return null;
        }

        public int GetOccupiedTableCount()
        {
            int count = 0;
            foreach (var table in tables)
            {
                if (table.IsOccupied) count++;
            }
            return count;
        }

        public void AddTable(Table table)
        {
            if (!tables.Contains(table))
            {
                tables.Add(table);
                table.SetTableId(tables.Count - 1);
            }
        }

        public void RemoveTable(Table table)
        {
            if (tables.Contains(table))
            {
                tables.Remove(table);
                RefreshTableIds();
            }
        }

        public bool UnlockRecipe(RecipeData recipe)
        {
            if (unlockedRecipes.Contains(recipe)) return false;

            if (ResourceManager.Instance.SpendGold(recipe.unlockCost))
            {
                unlockedRecipes.Add(recipe);
                OnRecipeUnlocked?.Invoke(recipe);
                Debug.Log($"[Restaurant] Unlocked recipe: {recipe.recipeName}");
                return true;
            }
            return false;
        }

        public bool HireStaff(Staff staff)
        {
            if (hiredStaff.Contains(staff)) return false;

            hiredStaff.Add(staff);
            OnStaffHired?.Invoke(staff);
            Debug.Log($"[Restaurant] Hired staff: {staff.Data.staffName}");
            return true;
        }

        public void LevelUp()
        {
            level++;
            OnLevelUp?.Invoke(level);
            Debug.Log($"[Restaurant] Leveled up to {level}");
        }

        public RecipeData GetRandomRecipe()
        {
            if (unlockedRecipes.Count == 0) return null;
            return unlockedRecipes[UnityEngine.Random.Range(0, unlockedRecipes.Count)];
        }

        public Staff GetAvailableStaff(StaffType type)
        {
            foreach (var staff in hiredStaff)
            {
                if (staff.Data.staffType == type && staff.CurrentState == StaffState.Idle)
                    return staff;
            }
            return null;
        }
    }
}
