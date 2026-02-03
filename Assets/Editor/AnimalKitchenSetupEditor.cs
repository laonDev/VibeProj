using UnityEngine;
using UnityEditor;
using System.IO;
using AnimalKitchen;

public class AnimalKitchenSetupEditor : Editor
{
    private const string MENU_PATH = "Animal Kitchen/";

    [MenuItem(MENU_PATH + "Setup Scene", false, 0)]
    public static void SetupScene()
    {
        // Find or create GameSetup
        var gameSetup = FindObjectOfType<GameSetup>();
        if (gameSetup == null)
        {
            var go = new GameObject("GameSetup");
            gameSetup = go.AddComponent<GameSetup>();
        }

        gameSetup.SetupGame();
        Selection.activeGameObject = gameSetup.gameObject;

        Debug.Log("[AnimalKitchen] Scene setup complete! Press Play to test.");
    }

    [MenuItem(MENU_PATH + "Create Default Data", false, 1)]
    public static void CreateDefaultData()
    {
        CreateDefaultRecipes();
        CreateDefaultStaff();
        CreateDefaultCustomers();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[AnimalKitchen] Default data created in Assets/ScriptableObjects/");
    }

    [MenuItem(MENU_PATH + "Create Prefabs", false, 2)]
    public static void CreatePrefabs()
    {
        CreateCustomerPrefab();
        CreateStaffPrefabs();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[AnimalKitchen] Prefabs created in Assets/Prefabs/");
    }

    private static void CreateDefaultRecipes()
    {
        string path = "Assets/ScriptableObjects/Recipes";
        EnsureDirectory(path);

        var recipes = new (string name, FoodCategory category, float cookTime, int price, int unlockCost)[]
        {
            ("Hamburger", FoodCategory.MainDish, 5f, 50, 0),
            ("Pizza", FoodCategory.MainDish, 8f, 80, 500),
            ("Salad", FoodCategory.Appetizer, 3f, 30, 200),
            ("Steak", FoodCategory.MainDish, 10f, 150, 1000),
            ("Ice Cream", FoodCategory.Dessert, 2f, 25, 300),
            ("Coffee", FoodCategory.Beverage, 2f, 20, 0),
            ("Orange Juice", FoodCategory.Beverage, 1f, 15, 100),
            ("Pasta", FoodCategory.MainDish, 7f, 70, 700),
            ("Soup", FoodCategory.Appetizer, 4f, 35, 400),
            ("Cake", FoodCategory.Dessert, 6f, 60, 600)
        };

        foreach (var (name, category, cookTime, price, unlockCost) in recipes)
        {
            string assetPath = $"{path}/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<RecipeData>(assetPath) != null) continue;

            var recipe = ScriptableObject.CreateInstance<RecipeData>();
            recipe.recipeName = name;
            recipe.category = category;
            recipe.cookTime = cookTime;
            recipe.sellPrice = price;
            recipe.unlockCost = unlockCost;
            recipe.unlockLevel = unlockCost > 0 ? (unlockCost / 500) + 1 : 1;
            recipe.popularity = 50;
            recipe.satisfactionBonus = 3;

            AssetDatabase.CreateAsset(recipe, assetPath);
        }

        Debug.Log($"[AnimalKitchen] Created {recipes.Length} recipes");
    }

    private static void CreateDefaultStaff()
    {
        string path = "Assets/ScriptableObjects/Staff";
        EnsureDirectory(path);

        var staffList = new (string name, AnimalType animal, StaffType type, Rarity rarity, int cost)[]
        {
            // Chefs
            ("Chef Cat", AnimalType.Cat, StaffType.Chef, Rarity.Common, 100),
            ("Chef Pig", AnimalType.Pig, StaffType.Chef, Rarity.Rare, 300),
            ("Chef Bear", AnimalType.Bear, StaffType.Chef, Rarity.Epic, 800),

            // Waiters
            ("Waiter Rabbit", AnimalType.Rabbit, StaffType.Waiter, Rarity.Common, 100),
            ("Waiter Dog", AnimalType.Dog, StaffType.Waiter, Rarity.Rare, 300),
            ("Waiter Fox", AnimalType.Fox, StaffType.Waiter, Rarity.Epic, 800),

            // Cashiers
            ("Cashier Raccoon", AnimalType.Raccoon, StaffType.Cashier, Rarity.Common, 100),
            ("Cashier Hamster", AnimalType.Hamster, StaffType.Cashier, Rarity.Rare, 300)
        };

        foreach (var (name, animal, type, rarity, cost) in staffList)
        {
            string assetPath = $"{path}/{name.Replace(" ", "_")}.asset";
            if (AssetDatabase.LoadAssetAtPath<StaffData>(assetPath) != null) continue;

            var staff = ScriptableObject.CreateInstance<StaffData>();
            staff.staffName = name;
            staff.animalType = animal;
            staff.staffType = type;
            staff.rarity = rarity;
            staff.hireCost = cost;
            staff.baseSpeed = 1f + ((int)rarity * 0.1f);
            staff.baseEfficiency = 1f + ((int)rarity * 0.15f);
            staff.speedPerLevel = 0.05f;
            staff.efficiencyPerLevel = 0.05f;
            staff.levelUpBaseCost = 50;

            AssetDatabase.CreateAsset(staff, assetPath);
        }

        Debug.Log($"[AnimalKitchen] Created {staffList.Length} staff data");
    }

    private static void CreateDefaultCustomers()
    {
        string path = "Assets/ScriptableObjects/Customers";
        EnsureDirectory(path);

        var customers = new (string name, AnimalType animal, float patience, float speed)[]
        {
            ("Customer Cat", AnimalType.Cat, 60f, 2f),
            ("Customer Dog", AnimalType.Dog, 50f, 2.5f),
            ("Customer Rabbit", AnimalType.Rabbit, 45f, 3f),
            ("Customer Bear", AnimalType.Bear, 80f, 1.5f),
            ("Customer Fox", AnimalType.Fox, 55f, 2.2f),
            ("Customer Raccoon", AnimalType.Raccoon, 70f, 1.8f)
        };

        foreach (var (name, animal, patience, speed) in customers)
        {
            string assetPath = $"{path}/{name.Replace(" ", "_")}.asset";
            if (AssetDatabase.LoadAssetAtPath<CustomerData>(assetPath) != null) continue;

            var customer = ScriptableObject.CreateInstance<CustomerData>();
            customer.customerName = name;
            customer.animalType = animal;
            customer.patience = patience;
            customer.moveSpeed = speed;
            customer.eatDuration = 10f;
            customer.baseTipRate = 0.1f;
            customer.maxBonusTipRate = 0.2f;

            AssetDatabase.CreateAsset(customer, assetPath);
        }

        Debug.Log($"[AnimalKitchen] Created {customers.Length} customer data");
    }

    private static void CreateCustomerPrefab()
    {
        string path = "Assets/Prefabs/Characters/Customer.prefab";
        EnsureDirectory("Assets/Prefabs/Characters");

        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("Customer");

        // Add sprite renderer
        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 10;

        // Create placeholder sprite
        var texture = new Texture2D(64, 64);
        var colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(0.8f, 0.6f, 0.4f);
        }
        texture.SetPixels(colors);
        texture.Apply();

        var sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        spriteRenderer.sprite = sprite;

        // Add Customer component
        var customer = go.AddComponent<Customer>();

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);

        Debug.Log("[AnimalKitchen] Created Customer prefab");
    }

    private static void CreateStaffPrefabs()
    {
        EnsureDirectory("Assets/Prefabs/Characters");

        CreateStaffPrefab("Chef", new Color(1f, 0.8f, 0.6f));
        CreateStaffPrefab("Waiter", new Color(0.6f, 0.8f, 1f));
    }

    private static void CreateStaffPrefab(string staffType, Color color)
    {
        string path = $"Assets/Prefabs/Characters/{staffType}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject(staffType);

        // Add sprite renderer
        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = 10;

        // Create placeholder sprite
        var texture = new Texture2D(64, 64);
        var colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }
        texture.SetPixels(colors);
        texture.Apply();

        var sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        spriteRenderer.sprite = sprite;

        // Add Staff component
        if (staffType == "Chef")
            go.AddComponent<Chef>();
        else if (staffType == "Waiter")
            go.AddComponent<Waiter>();

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);

        Debug.Log($"[AnimalKitchen] Created {staffType} prefab");
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
