using UnityEngine;

namespace AnimalKitchen
{
    /// <summary>
    /// 씬에 배치하면 게임에 필요한 모든 매니저와 오브젝트를 자동 생성합니다.
    /// </summary>
    public class GameSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool setupOnAwake = true;

        [Header("References (Auto-created if null)")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Restaurant restaurant;
        [SerializeField] private CustomerSpawner customerSpawner;

        private void Awake()
        {
            if (setupOnAwake)
            {
                SetupGame();
            }
        }

        [ContextMenu("Setup Game")]
        public void SetupGame()
        {
            // SetupManagers();
            // SetupUI();
            SetupRestaurant();
            SetupCustomerSpawner();
            // SetupCamera();

            Debug.Log("[GameSetup] Game setup complete!");
        }

        [ContextMenu("Rebuild Kitchen")]
        public void RebuildKitchen()
        {
            // Find and delete existing kitchen
            var existingKitchen = FindObjectOfType<Kitchen>();
            if (existingKitchen != null)
            {
                DestroyImmediate(existingKitchen.gameObject);
                Debug.Log("[GameSetup] Removed existing kitchen");
            }

            // Find restaurant
            var restaurant = FindObjectOfType<Restaurant>();
            if (restaurant != null)
            {
                // Entrance
                CreateEntrance(restaurant.transform);

                // Exit
                CreateExit(restaurant.transform);
                CreateKitchen(restaurant.transform);
                Debug.Log("[GameSetup] Kitchen rebuilt successfully!");
            }
            else
            {
                Debug.LogError("[GameSetup] No restaurant found! Create restaurant first.");
            }
        }

        private void SetupManagers()
        {
            // GameManager
            if (gameManager == null)
            {
                var go = new GameObject("GameManager");
                gameManager = go.AddComponent<GameManager>();
            }

            // ResourceManager
            if (resourceManager == null)
            {
                var go = new GameObject("ResourceManager");
                resourceManager = go.AddComponent<ResourceManager>();
            }

            // SaveManager
            if (saveManager == null)
            {
                var go = new GameObject("SaveManager");
                saveManager = go.AddComponent<SaveManager>();
            }

            Debug.Log("[GameSetup] Managers created");
        }

        private void SetupUI()
        {
            // Create EventSystem if not exists
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystem = eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Find or create Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var canvasScaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);

                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Create UIManager
            if (uiManager == null)
            {
                var uiGO = new GameObject("UIManager");
                uiGO.transform.SetParent(canvas.transform, false);
                uiManager = uiGO.AddComponent<UIManager>();
            }

            // Create HUD
            CreateHUD(canvas.transform);

            // Create bottom menu buttons
            CreateMenuButtons(canvas.transform);

            // Create test panels for now
            CreateTestPanels(canvas.transform);

            Debug.Log("[GameSetup] UI created");
        }

        private void CreateTestPanels(Transform parent)
        {
            var defaultFont = GetDefaultTMPFont();

            // Create simple panels using reflection to set UIManager fields
            var expansionPanel = SimplePanelTest.CreateSimplePanel("Restaurant Expansion", parent, defaultFont);
            var recipePanel = SimplePanelTest.CreateSimplePanel("Recipe Unlock", parent, defaultFont);
            var staffPanel = SimplePanelTest.CreateSimplePanel("Staff Hiring", parent, defaultFont);
            var collectionPanel = SimplePanelTest.CreateSimplePanel("Staff Collection", parent, defaultFont);

            // Assign panels to UIManager via reflection
            if (uiManager != null)
            {
                var type = uiManager.GetType();

                var expansionField = type.GetField("expansionPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (expansionField != null) expansionField.SetValue(uiManager, expansionPanel);

                var recipeField = type.GetField("recipePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (recipeField != null) recipeField.SetValue(uiManager, recipePanel);

                var staffField = type.GetField("staffPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (staffField != null) staffField.SetValue(uiManager, staffPanel);

                var collectionField = type.GetField("collectionPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (collectionField != null) collectionField.SetValue(uiManager, collectionPanel);

                Debug.Log("[GameSetup] Test panels created and assigned");
            }
        }

        private void CreateHUD(Transform parent)
        {
            var hudGO = new GameObject("HUD");
            var hudRect = hudGO.AddComponent<RectTransform>();
            hudRect.SetParent(parent, false);
            hudRect.anchorMin = new Vector2(0, 1);
            hudRect.anchorMax = new Vector2(0, 1);
            hudRect.pivot = new Vector2(0, 1);
            hudRect.anchoredPosition = new Vector2(20, -20);
            hudRect.sizeDelta = new Vector2(400, 100);

            // Load default TMP font
            var defaultFont = GetDefaultTMPFont();

            // Gold display
            var goldGO = new GameObject("GoldDisplay");
            var goldRect = goldGO.AddComponent<RectTransform>();
            goldRect.SetParent(hudRect, false);
            goldRect.anchorMin = new Vector2(0, 1);
            goldRect.anchorMax = new Vector2(0, 1);
            goldRect.pivot = new Vector2(0, 1);
            goldRect.anchoredPosition = new Vector2(0, 0);
            goldRect.sizeDelta = new Vector2(200, 40);

            var goldText = goldGO.AddComponent<TMPro.TextMeshProUGUI>();
            goldText.text = "Gold: 0";
            goldText.fontSize = 32;
            goldText.color = Color.yellow;
            goldText.font = defaultFont;

            // Gems display
            var gemsGO = new GameObject("GemsDisplay");
            var gemsRect = gemsGO.AddComponent<RectTransform>();
            gemsRect.SetParent(hudRect, false);
            gemsRect.anchorMin = new Vector2(0, 1);
            gemsRect.anchorMax = new Vector2(0, 1);
            gemsRect.pivot = new Vector2(0, 1);
            gemsRect.anchoredPosition = new Vector2(0, -50);
            gemsRect.sizeDelta = new Vector2(200, 40);

            var gemsText = gemsGO.AddComponent<TMPro.TextMeshProUGUI>();
            gemsText.text = "Gems: 0";
            gemsText.fontSize = 32;
            gemsText.color = Color.cyan;
            gemsText.font = defaultFont;

            Debug.Log("[GameSetup] HUD created");
        }

        private void CreateMenuButtons(Transform parent)
        {
            var menuGO = new GameObject("BottomMenu");
            var menuRect = menuGO.AddComponent<RectTransform>();
            menuRect.SetParent(parent, false);
            menuRect.anchorMin = new Vector2(0, 0);
            menuRect.anchorMax = new Vector2(1, 0);
            menuRect.pivot = new Vector2(0.5f, 0);
            menuRect.anchoredPosition = Vector2.zero;
            menuRect.sizeDelta = new Vector2(0, 120);

            var bg = menuGO.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Load default TMP font
            var defaultFont = GetDefaultTMPFont();

            // Button positions
            string[] buttonNames = { "Expansion", "Recipes", "Staff", "Collection" };
            float buttonWidth = 180f;
            float spacing = 20f;
            float totalWidth = (buttonWidth * buttonNames.Length) + (spacing * (buttonNames.Length - 1));
            float startX = -totalWidth / 2 + buttonWidth / 2;

            for (int i = 0; i < buttonNames.Length; i++)
            {
                var buttonGO = new GameObject($"{buttonNames[i]}Button");
                var buttonRect = buttonGO.AddComponent<RectTransform>();
                buttonRect.SetParent(menuRect, false);
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.pivot = new Vector2(0.5f, 0.5f);
                buttonRect.anchoredPosition = new Vector2(startX + (buttonWidth + spacing) * i, 0);
                buttonRect.sizeDelta = new Vector2(buttonWidth, 80);

                var button = buttonGO.AddComponent<UnityEngine.UI.Button>();
                var buttonImage = buttonGO.AddComponent<UnityEngine.UI.Image>();
                buttonImage.color = new Color(0.2f, 0.6f, 0.8f);

                // Add onClick event based on button name
                int index = i; // Capture for closure
                switch (buttonNames[index])
                {
                    case "Expansion":
                        button.onClick.AddListener(() => UIManager.Instance?.OpenExpansionPanel());
                        break;
                    case "Recipes":
                        button.onClick.AddListener(() => UIManager.Instance?.OpenRecipePanel());
                        break;
                    case "Staff":
                        button.onClick.AddListener(() => UIManager.Instance?.OpenStaffPanel());
                        break;
                    case "Collection":
                        button.onClick.AddListener(() => UIManager.Instance?.OpenCollectionPanel());
                        break;
                }

                // Button text
                var textGO = new GameObject("Text");
                var textRect = textGO.AddComponent<RectTransform>();
                textRect.SetParent(buttonRect, false);
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;

                var buttonText = textGO.AddComponent<TMPro.TextMeshProUGUI>();
                buttonText.text = buttonNames[i];
                buttonText.fontSize = 24;
                buttonText.alignment = TMPro.TextAlignmentOptions.Center;
                buttonText.color = Color.white;
                buttonText.font = defaultFont;
            }

            Debug.Log("[GameSetup] Menu buttons created");
        }

        private TMPro.TMP_FontAsset GetDefaultTMPFont()
        {
            // Try to get TMP default font
            var font = TMPro.TMP_Settings.defaultFontAsset;

            if (font == null)
            {
                // Try to load from Resources
                font = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }

            if (font == null)
            {
                Debug.LogWarning("[GameSetup] No TMP font found. Please import TextMeshPro Essentials via Window > TextMeshPro > Import TMP Essential Resources");
            }

            return font;
        }

        private void SetupRestaurant()
        {
            Debug.Log($"[GameSetup] SetupRestaurant - restaurant is null: {restaurant == null}");

            if (restaurant == null)
            {
                Debug.Log("[GameSetup] Creating new Restaurant GameObject");
                var restaurantGO = new GameObject("Restaurant");
                restaurant = restaurantGO.AddComponent<Restaurant>();

                // Entrance
                CreateEntrance(restaurantGO.transform);

                // Exit
                CreateExit(restaurantGO.transform);

                // Kitchen
                CreateKitchen(restaurantGO.transform);

                // Create default tables
                CreateDefaultTables(restaurantGO.transform);
            }
            else
            {
                Debug.Log("[GameSetup] Restaurant already exists, skipping creation");
            }

            // Always create default staff (whether restaurant is new or existing)
            if (restaurant != null)
            {
                CreateDefaultStaff(restaurant.transform);
            }

            Debug.Log("[GameSetup] Restaurant setup complete");
        }

        private void CreateKitchen(Transform parent)
        {
            var kitchenGO = new GameObject("Kitchen");
            kitchenGO.transform.SetParent(parent);
            kitchenGO.transform.localPosition = new Vector3(0, 3, 0);

            // Add Kitchen component
            var kitchen = kitchenGO.AddComponent<Kitchen>();

            // Create visual background
            var backgroundGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backgroundGO.name = "KitchenBackground";
            backgroundGO.transform.SetParent(kitchenGO.transform);
            backgroundGO.transform.localPosition = Vector3.zero;
            backgroundGO.transform.localScale = new Vector3(5, 2, 1);

            var bgRenderer = backgroundGO.GetComponent<Renderer>();
            if (bgRenderer != null)
            {
                bgRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);
            }

            // Remove collider
            var bgCollider = backgroundGO.GetComponent<Collider>();
            if (bgCollider != null) DestroyImmediate(bgCollider);

            // Create cooking stations
            Transform[] cookingStations = new Transform[3];
            Vector3[] stationPositions = new Vector3[]
            {
                new Vector3(-1.5f, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(1.5f, 0, 0)
            };

            for (int i = 0; i < 3; i++)
            {
                var stationGO = new GameObject($"CookingStation_{i}");
                stationGO.transform.SetParent(kitchenGO.transform);
                stationGO.transform.localPosition = stationPositions[i];

                // Station visual (small cube for stove)
                var stoveVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stoveVisual.name = "Stove";
                stoveVisual.transform.SetParent(stationGO.transform);
                stoveVisual.transform.localPosition = Vector3.zero;
                stoveVisual.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);

                var stoveRenderer = stoveVisual.GetComponent<Renderer>();
                if (stoveRenderer != null)
                {
                    stoveRenderer.material.color = new Color(0.5f, 0.5f, 0.5f);
                }

                // Remove collider
                var stoveCollider = stoveVisual.GetComponent<Collider>();
                if (stoveCollider != null) DestroyImmediate(stoveCollider);

                cookingStations[i] = stationGO.transform;
            }

            // Assign stations to Kitchen using reflection
            var kitchenType = kitchen.GetType();
            var stationsField = kitchenType.GetField("cookingStations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (stationsField != null)
            {
                stationsField.SetValue(kitchen, cookingStations);
            }

            // Add KitchenUI component
            var kitchenUI = kitchenGO.AddComponent<KitchenUI>();

            Debug.Log("[GameSetup] Kitchen created with 3 cooking stations");
        }

        private void CreateEntrance(Transform parent)
        {
            var entranceGO = new GameObject("Entrance");
            entranceGO.transform.SetParent(parent);
            entranceGO.transform.localPosition = new Vector3(-5, 0, 0);

            // Create door visual (Quad)
            var doorGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            doorGO.name = "EntranceDoor";
            doorGO.transform.SetParent(entranceGO.transform);
            doorGO.transform.localPosition = Vector3.zero;
            doorGO.transform.localScale = new Vector3(1.5f, 2f, 1f);

            var doorRenderer = doorGO.GetComponent<Renderer>();
            if (doorRenderer != null)
            {
                doorRenderer.material.color = new Color(0.3f, 0.8f, 0.3f, 0.8f); // Green for entrance
            }

            // Remove collider
            var doorCollider = doorGO.GetComponent<Collider>();
            if (doorCollider != null) DestroyImmediate(doorCollider);

            // Create arrow visual (pointing inward)
            var arrowGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrowGO.name = "EntranceArrow";
            arrowGO.transform.SetParent(entranceGO.transform);
            arrowGO.transform.localPosition = new Vector3(0.3f, 0, 0);
            arrowGO.transform.localScale = new Vector3(0.3f, 0.5f, 0.1f);
            arrowGO.transform.localRotation = Quaternion.Euler(0, 0, -90f); // Point right (into restaurant)

            var arrowRenderer = arrowGO.GetComponent<Renderer>();
            if (arrowRenderer != null)
            {
                arrowRenderer.material.color = new Color(0.2f, 1f, 0.2f); // Bright green
            }

            var arrowCollider = arrowGO.GetComponent<Collider>();
            if (arrowCollider != null) DestroyImmediate(arrowCollider);

            // Create text label
            var textGO = new GameObject("EntranceText");
            textGO.transform.SetParent(entranceGO.transform);
            textGO.transform.localPosition = new Vector3(0, -1.2f, 0);
            textGO.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            var textMesh = textGO.AddComponent<TextMesh>();
            textMesh.text = "ENTRANCE";
            textMesh.fontSize = 20;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = new Color(0.2f, 1f, 0.2f);

            Debug.Log("[GameSetup] Entrance created with visual elements");
        }

        private void CreateExit(Transform parent)
        {
            var exitGO = new GameObject("Exit");
            exitGO.transform.SetParent(parent);
            exitGO.transform.localPosition = new Vector3(5, 0, 0);

            // Create door visual (Quad)
            var doorGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            doorGO.name = "ExitDoor";
            doorGO.transform.SetParent(exitGO.transform);
            doorGO.transform.localPosition = Vector3.zero;
            doorGO.transform.localScale = new Vector3(1.5f, 2f, 1f);

            var doorRenderer = doorGO.GetComponent<Renderer>();
            if (doorRenderer != null)
            {
                doorRenderer.material.color = new Color(0.8f, 0.3f, 0.3f, 0.8f); // Red for exit
            }

            // Remove collider
            var doorCollider = doorGO.GetComponent<Collider>();
            if (doorCollider != null) DestroyImmediate(doorCollider);

            // Create arrow visual (pointing outward)
            var arrowGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrowGO.name = "ExitArrow";
            arrowGO.transform.SetParent(exitGO.transform);
            arrowGO.transform.localPosition = new Vector3(-0.3f, 0, 0);
            arrowGO.transform.localScale = new Vector3(0.3f, 0.5f, 0.1f);
            arrowGO.transform.localRotation = Quaternion.Euler(0, 0, 90f); // Point left (out of restaurant)

            var arrowRenderer = arrowGO.GetComponent<Renderer>();
            if (arrowRenderer != null)
            {
                arrowRenderer.material.color = new Color(1f, 0.2f, 0.2f); // Bright red
            }

            var arrowCollider = arrowGO.GetComponent<Collider>();
            if (arrowCollider != null) DestroyImmediate(arrowCollider);

            // Create text label
            var textGO = new GameObject("ExitText");
            textGO.transform.SetParent(exitGO.transform);
            textGO.transform.localPosition = new Vector3(0, -1.2f, 0);
            textGO.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            var textMesh = textGO.AddComponent<TextMesh>();
            textMesh.text = "EXIT";
            textMesh.fontSize = 20;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = new Color(1f, 0.2f, 0.2f);

            Debug.Log("[GameSetup] Exit created with visual elements");
        }

        private void CreateDefaultTables(Transform parent)
        {
            var tablesParent = new GameObject("Tables");
            tablesParent.transform.SetParent(parent);

            Vector3[] tablePositions = new Vector3[]
            {
                new Vector3(-2, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(-2, -2, 0),
                new Vector3(0, -2, 0),
                new Vector3(2, -2, 0)
            };

            for (int i = 0; i < tablePositions.Length; i++)
            {
                var tableGO = new GameObject($"Table_{i}");
                tableGO.transform.SetParent(tablesParent.transform);
                tableGO.transform.localPosition = tablePositions[i];

                // Seat position (먼저 생성해야 Table.Awake에서 찾을 수 있음)
                var seat = new GameObject("SeatPosition");
                seat.transform.SetParent(tableGO.transform);
                seat.transform.localPosition = new Vector3(0, -0.3f, 0);

                // Food position
                var food = new GameObject("FoodPosition");
                food.transform.SetParent(tableGO.transform);
                food.transform.localPosition = new Vector3(0, 0.2f, 0);

                // Table 컴포넌트 추가 (자식이 먼저 있어야 Awake에서 찾음)
                var table = tableGO.AddComponent<Table>();
                table.SetPositions(seat.transform, food.transform);

                // Visual placeholder
                var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.name = "TableVisual";
                visual.transform.SetParent(tableGO.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);

                var renderer = visual.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.6f, 0.4f, 0.2f);
                }

                // Remove collider from visual
                var collider = visual.GetComponent<Collider>();
                if (collider != null) DestroyImmediate(collider);
            }

            Debug.Log($"[GameSetup] Created {tablePositions.Length} tables");
        }

        private void SetupCustomerSpawner()
        {
            if (customerSpawner == null)
            {
                var go = new GameObject("CustomerSpawner");
                customerSpawner = go.AddComponent<CustomerSpawner>();

                // Spawn point at entrance
                var spawnPoint = new GameObject("SpawnPoint");
                spawnPoint.transform.SetParent(go.transform);
                spawnPoint.transform.localPosition = new Vector3(-6, 0, 0);
            }

            Debug.Log("[GameSetup] CustomerSpawner created");
        }

        private void CreateDefaultStaff(Transform parent)
        {
            Debug.Log("[GameSetup] CreateDefaultStaff - Starting staff creation...");

            var restaurant = parent.GetComponent<Restaurant>();
            if (restaurant == null)
            {
                Debug.LogError("[GameSetup] Cannot create staff without Restaurant component!");
                return;
            }
            Debug.Log("[GameSetup] Restaurant component found");

            var staffParent = new GameObject("Staff");
            staffParent.transform.SetParent(parent);
            Debug.Log($"[GameSetup] Staff parent created at position: {staffParent.transform.position}");

            // ===== CREATE CHEF =====
            Debug.Log("[GameSetup] Creating Chef...");

            var chefGO = new GameObject("Chef");
            chefGO.transform.SetParent(staffParent.transform);
            chefGO.transform.localPosition = new Vector3(-1, 2, 0); // Near kitchen
            Debug.Log($"[GameSetup] Chef GameObject created at local position: {chefGO.transform.localPosition}, world position: {chefGO.transform.position}");

            // Chef visual (red sphere)
            var chefVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            chefVisual.name = "ChefVisual";
            chefVisual.transform.SetParent(chefGO.transform);
            chefVisual.transform.localPosition = Vector3.zero;
            chefVisual.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            var chefRenderer = chefVisual.GetComponent<Renderer>();
            if (chefRenderer != null)
            {
                chefRenderer.material.color = new Color(1f, 0.3f, 0.3f); // Red for chef
                Debug.Log("[GameSetup] Chef visual created with red color");
            }
            else
            {
                Debug.LogWarning("[GameSetup] Chef renderer is null!");
            }

            // Remove collider
            var chefCollider = chefVisual.GetComponent<Collider>();
            if (chefCollider != null) DestroyImmediate(chefCollider);

            // Add Chef component
            var chef = chefGO.AddComponent<Chef>();
            Debug.Log($"[GameSetup] Chef component added. GameObject active: {chefGO.activeSelf}");

            // Add SpriteRenderer (even though we're using 3D for now)
            var chefSpriteRenderer = chefGO.AddComponent<SpriteRenderer>();
            chefSpriteRenderer.enabled = false; // Disable for now since we're using 3D sphere

            // Try to load chef data from Resources, or create temporary data
            var chefData = Resources.Load<StaffData>("Staff/Chef_Default");
            if (chefData == null)
            {
                Debug.Log("[GameSetup] Chef data not found in Resources, creating runtime instance");
                // Create temporary StaffData at runtime
                chefData = ScriptableObject.CreateInstance<StaffData>();
                chefData.staffName = "Default Chef";
                chefData.animalType = AnimalType.Cat;
                chefData.staffType = StaffType.Chef;
                chefData.rarity = Rarity.Common;
                chefData.baseSpeed = 2f;
                chefData.baseEfficiency = 1f;
                chefData.speedPerLevel = 0.05f;
                chefData.efficiencyPerLevel = 0.05f;
                chefData.hireCost = 0; // Free default staff
                chefData.levelUpBaseCost = 50;
            }
            else
            {
                Debug.Log("[GameSetup] Chef data loaded from Resources");
            }

            // Initialize with StaffData using reflection to set private field
            var staffType = typeof(Staff);
            var dataField = staffType.GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dataField != null)
            {
                dataField.SetValue(chef, chefData);
                Debug.Log($"[GameSetup] Chef data assigned via reflection: {chefData.staffName}");
            }
            else
            {
                Debug.LogError("[GameSetup] Could not find 'data' field via reflection!");
            }

            // Add to restaurant's hired staff
            bool chefHired = restaurant.HireStaff(chef);
            Debug.Log($"[GameSetup] Chef hired: {chefHired}, Restaurant hired staff count: {restaurant.HiredStaff.Count}");

            // ===== CREATE WAITER =====
            Debug.Log("[GameSetup] Creating Waiter...");

            var waiterGO = new GameObject("Waiter");
            waiterGO.transform.SetParent(staffParent.transform);
            waiterGO.transform.localPosition = new Vector3(1, 0, 0); // Near tables
            Debug.Log($"[GameSetup] Waiter GameObject created at local position: {waiterGO.transform.localPosition}, world position: {waiterGO.transform.position}");

            // Waiter visual (blue sphere)
            var waiterVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            waiterVisual.name = "WaiterVisual";
            waiterVisual.transform.SetParent(waiterGO.transform);
            waiterVisual.transform.localPosition = Vector3.zero;
            waiterVisual.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            var waiterRenderer = waiterVisual.GetComponent<Renderer>();
            if (waiterRenderer != null)
            {
                waiterRenderer.material.color = new Color(0.3f, 0.5f, 1f); // Blue for waiter
                Debug.Log("[GameSetup] Waiter visual created with blue color");
            }
            else
            {
                Debug.LogWarning("[GameSetup] Waiter renderer is null!");
            }

            // Remove collider
            var waiterCollider = waiterVisual.GetComponent<Collider>();
            if (waiterCollider != null) DestroyImmediate(waiterCollider);

            // Add Waiter component
            var waiter = waiterGO.AddComponent<Waiter>();
            Debug.Log($"[GameSetup] Waiter component added. GameObject active: {waiterGO.activeSelf}");

            // Add SpriteRenderer
            var waiterSpriteRenderer = waiterGO.AddComponent<SpriteRenderer>();
            waiterSpriteRenderer.enabled = false; // Disable for now since we're using 3D sphere

            // Try to load waiter data from Resources, or create temporary data
            var waiterData = Resources.Load<StaffData>("Staff/Waiter_Default");
            if (waiterData == null)
            {
                Debug.Log("[GameSetup] Waiter data not found in Resources, creating runtime instance");
                // Create temporary StaffData at runtime
                waiterData = ScriptableObject.CreateInstance<StaffData>();
                waiterData.staffName = "Default Waiter";
                waiterData.animalType = AnimalType.Rabbit;
                waiterData.staffType = StaffType.Waiter;
                waiterData.rarity = Rarity.Common;
                waiterData.baseSpeed = 3f;
                waiterData.baseEfficiency = 1f;
                waiterData.speedPerLevel = 0.05f;
                waiterData.efficiencyPerLevel = 0.05f;
                waiterData.hireCost = 0; // Free default staff
                waiterData.levelUpBaseCost = 50;
            }
            else
            {
                Debug.Log("[GameSetup] Waiter data loaded from Resources");
            }

            // Initialize with StaffData using reflection to set private field
            if (dataField != null)
            {
                dataField.SetValue(waiter, waiterData);
                Debug.Log($"[GameSetup] Waiter data assigned via reflection: {waiterData.staffName}");
            }
            else
            {
                Debug.LogError("[GameSetup] Could not find 'data' field via reflection!");
            }

            // Add to restaurant's hired staff
            bool waiterHired = restaurant.HireStaff(waiter);
            Debug.Log($"[GameSetup] Waiter hired: {waiterHired}, Restaurant hired staff count: {restaurant.HiredStaff.Count}");

            // ===== CREATE CASHIER =====
            Debug.Log("[GameSetup] Creating Cashier...");

            var cashierGO = new GameObject("Cashier");
            cashierGO.transform.SetParent(staffParent.transform);
            cashierGO.transform.localPosition = new Vector3(4, 0, 0); // Near exit/cashier area
            Debug.Log($"[GameSetup] Cashier GameObject created at local position: {cashierGO.transform.localPosition}, world position: {cashierGO.transform.position}");

            // Cashier visual (green sphere)
            var cashierVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cashierVisual.name = "CashierVisual";
            cashierVisual.transform.SetParent(cashierGO.transform);
            cashierVisual.transform.localPosition = Vector3.zero;
            cashierVisual.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            var cashierRenderer = cashierVisual.GetComponent<Renderer>();
            if (cashierRenderer != null)
            {
                cashierRenderer.material.color = new Color(0.3f, 1f, 0.3f); // Green for cashier
                Debug.Log("[GameSetup] Cashier visual created with green color");
            }
            else
            {
                Debug.LogWarning("[GameSetup] Cashier renderer is null!");
            }

            // Remove collider
            var cashierCollider = cashierVisual.GetComponent<Collider>();
            if (cashierCollider != null) DestroyImmediate(cashierCollider);

            // Add Cashier component
            var cashier = cashierGO.AddComponent<Cashier>();
            Debug.Log($"[GameSetup] Cashier component added. GameObject active: {cashierGO.activeSelf}");

            // Add SpriteRenderer
            var cashierSpriteRenderer = cashierGO.AddComponent<SpriteRenderer>();
            cashierSpriteRenderer.enabled = false; // Disable for now since we're using 3D sphere

            // Try to load cashier data from Resources, or create temporary data
            var cashierData = Resources.Load<StaffData>("Staff/Cashier_Default");
            if (cashierData == null)
            {
                Debug.Log("[GameSetup] Cashier data not found in Resources, creating runtime instance");
                // Create temporary StaffData at runtime
                cashierData = ScriptableObject.CreateInstance<StaffData>();
                cashierData.staffName = "Default Cashier";
                cashierData.animalType = AnimalType.Raccoon;
                cashierData.staffType = StaffType.Cashier;
                cashierData.rarity = Rarity.Common;
                cashierData.baseSpeed = 2f;
                cashierData.baseEfficiency = 1.2f; // Cashier is more efficient at payments
                cashierData.speedPerLevel = 0.05f;
                cashierData.efficiencyPerLevel = 0.05f;
                cashierData.hireCost = 0; // Free default staff
                cashierData.levelUpBaseCost = 50;
            }
            else
            {
                Debug.Log("[GameSetup] Cashier data loaded from Resources");
            }

            // Initialize with StaffData using reflection to set private field
            if (dataField != null)
            {
                dataField.SetValue(cashier, cashierData);
                Debug.Log($"[GameSetup] Cashier data assigned via reflection: {cashierData.staffName}");
            }
            else
            {
                Debug.LogError("[GameSetup] Could not find 'data' field via reflection!");
            }

            // Add to restaurant's hired staff
            bool cashierHired = restaurant.HireStaff(cashier);
            Debug.Log($"[GameSetup] Cashier hired: {cashierHired}, Restaurant hired staff count: {restaurant.HiredStaff.Count}");

            Debug.Log($"[GameSetup] ===== STAFF CREATION COMPLETE =====");
            Debug.Log($"[GameSetup] Chef position: {chefGO.transform.position}, active: {chefGO.activeSelf}");
            Debug.Log($"[GameSetup] Waiter position: {waiterGO.transform.position}, active: {waiterGO.activeSelf}");
            Debug.Log($"[GameSetup] Cashier position: {cashierGO.transform.position}, active: {cashierGO.activeSelf}");
            Debug.Log($"[GameSetup] Total hired staff: {restaurant.HiredStaff.Count}");
        }

        private void SetupCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Setup for 2D isometric view
                mainCamera.transform.position = new Vector3(0, 0, -10);
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 5;
                mainCamera.backgroundColor = new Color(0.2f, 0.3f, 0.4f);

                Debug.Log("[GameSetup] Camera configured for 2D isometric view");
            }
        }
    }
}
