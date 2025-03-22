using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject welcomeScreen;
    [SerializeField] private GameObject storeDirectoryScreen;
    [SerializeField] private GameObject storeDetailScreen;
    [SerializeField] private GameObject navigationScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject rewardsScreen;
    
    [Header("Store Directory")]
    [SerializeField] private RectTransform storeListContainer;
    [SerializeField] private GameObject storeItemPrefab;
    [SerializeField] private TMP_InputField searchInput;
    [SerializeField] private Button categoryAllButton;
    [SerializeField] private Button categoryFashionButton;
    [SerializeField] private Button categoryFoodButton;
    [SerializeField] private Button categoryElectronicsButton;
    
    [Header("Store Detail")]
    [SerializeField] private Image storeImage;
    [SerializeField] private TextMeshProUGUI storeNameText;
    [SerializeField] private TextMeshProUGUI storeDescriptionText;
    [SerializeField] private TextMeshProUGUI storeLocationText;
    [SerializeField] private Button navigateButton;
    [SerializeField] private Button backToDirectoryButton;
    
    [Header("Navigation")]
    [SerializeField] private TextMeshProUGUI destinationText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI estimatedTimeText;
    [SerializeField] private Button stopNavigationButton;
    [SerializeField] private GameObject directionalArrow;
    [SerializeField] private Image compassBar;
    
    [Header("AR Elements")]
    [SerializeField] private GameObject arPromptScreen;
    [SerializeField] private Button arStartButton;
    
    [Header("Settings")]
    [SerializeField] private Toggle locationPermissionToggle;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Toggle notificationsToggle;
    
    [Header("Gamification UI")]
    [SerializeField] private GameObject achievementNotification;
    [SerializeField] private Text achievementText;
    [SerializeField] private GameObject pointsEarnedNotification;
    [SerializeField] private Text pointsEarnedText;
    [SerializeField] private GameObject progressBar;
    [SerializeField] private Image progressFill;
    [SerializeField] private Text progressText;
    
    [Header("Promotion UI")]
    [SerializeField] private GameObject promotionPanel;
    [SerializeField] private Text promotionTitleText;
    [SerializeField] private Text promotionDescriptionText;
    [SerializeField] private Button claimPromotionButton;
    
    // References to other managers
    private WandurAppManager appManager;
    private ARNavigationManager navigationManager;
    
    // Events
    public event Action<string> OnStoreSelected;
    public event Action OnNavigationStarted;
    public event Action OnNavigationStopped;
    public event Action<string> OnScreenChanged;
    public event Action<string, string> OnPromotionViewed;
    public event Action<string, string> OnPromotionClaimed;
    public event Action<int> OnPointsEarned;
    public event Action<string> OnAchievementUnlocked;
    
    // Store data
    private List<StoreData> allStores = new List<StoreData>();
    private List<StoreData> filteredStores = new List<StoreData>();
    private StoreData selectedStore;
    private string currentCategory = "All";
    private string searchQuery = "";
    private Coroutine navigationUpdateCoroutine;
    private int userPoints = 0;
    private int userLevel = 1;
    private float navigationStartTime;
    
    private void Awake()
    {
        appManager = FindObjectOfType<WandurAppManager>();
        navigationManager = FindObjectOfType<ARNavigationManager>();
        
        // Initialize UI elements
        InitializeUI();
    }
    
    private void Start()
    {
        // Show welcome screen initially
        ShowScreen(welcomeScreen);
        
        // Load sample data
        LoadSampleStoreData();
        
        // Subscribe to events
        if (appManager != null)
        {
            appManager.OnAppInitialized += HandleAppInitialized;
            appManager.OnNavigationStarted += HandleNavigationStarted;
            appManager.OnNavigationStopped += HandleNavigationStopped;
            appManager.OnDestinationReached += HandleDestinationReached;
        }
        
        if (navigationManager != null)
        {
            navigationManager.OnPositionUpdated += HandlePositionUpdated;
        }
    }
    
    private void InitializeUI()
    {
        // Connect button listeners
        if (searchInput != null)
            searchInput.onValueChanged.AddListener(HandleSearchInputChanged);
            
        if (categoryAllButton != null)
            categoryAllButton.onClick.AddListener(() => FilterByCategory("All"));
            
        if (categoryFashionButton != null)
            categoryFashionButton.onClick.AddListener(() => FilterByCategory("Fashion"));
            
        if (categoryFoodButton != null)
            categoryFoodButton.onClick.AddListener(() => FilterByCategory("Food"));
            
        if (categoryElectronicsButton != null)
            categoryElectronicsButton.onClick.AddListener(() => FilterByCategory("Electronics"));
            
        if (navigateButton != null)
            navigateButton.onClick.AddListener(StartNavigation);
            
        if (backToDirectoryButton != null)
            backToDirectoryButton.onClick.AddListener(() => ShowScreen(storeDirectoryScreen));
            
        if (stopNavigationButton != null)
            stopNavigationButton.onClick.AddListener(StopNavigation);
            
        if (arStartButton != null)
            arStartButton.onClick.AddListener(StartARNavigation);
        
        if (claimPromotionButton != null)
            claimPromotionButton.onClick.AddListener(OnClaimPromotionButtonClicked);
    }
    
    private void LoadSampleStoreData()
    {
        // Add sample store data
        allStores.Add(new StoreData {
            id = "store1",
            name = "Fashion Boutique",
            description = "Trendy clothing and accessories for all ages.",
            category = "Fashion",
            location = "Level 1, North Wing",
            distance = 50f,
            imagePath = "StoreImages/fashion_store"
        });
        
        allStores.Add(new StoreData {
            id = "store2",
            name = "Gourmet Café",
            description = "Specialty coffees and pastries.",
            category = "Food",
            location = "Level 2, Food Court",
            distance = 120f,
            imagePath = "StoreImages/cafe"
        });
        
        allStores.Add(new StoreData {
            id = "store3",
            name = "Tech Haven",
            description = "Latest gadgets and electronics.",
            category = "Electronics",
            location = "Level 3, South Wing",
            distance = 200f,
            imagePath = "StoreImages/tech_store"
        });
        
        allStores.Add(new StoreData {
            id = "store4",
            name = "Sportswear Elite",
            description = "Athletic apparel and equipment.",
            category = "Fashion",
            location = "Level 1, East Wing",
            distance = 80f,
            imagePath = "StoreImages/sports_store"
        });
        
        allStores.Add(new StoreData {
            id = "store5",
            name = "Burger Junction",
            description = "Gourmet burgers and shakes.",
            category = "Food",
            location = "Level 2, Food Court",
            distance = 130f,
            imagePath = "StoreImages/burger_store"
        });
        
        // Initialize filtered stores
        filteredStores = new List<StoreData>(allStores);
        
        // Populate store list
        UpdateStoreList();
    }
    
    private void UpdateStoreList()
    {
        // Clear existing items
        foreach (Transform child in storeListContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Filter stores based on category and search query
        filteredStores = allStores.FindAll(store => 
            (currentCategory == "All" || store.category == currentCategory) &&
            (string.IsNullOrEmpty(searchQuery) || 
             store.name.ToLower().Contains(searchQuery.ToLower()) ||
             store.description.ToLower().Contains(searchQuery.ToLower()))
        );
        
        // Sort by distance
        filteredStores.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        // Instantiate store items
        foreach (var store in filteredStores)
        {
            GameObject storeItem = Instantiate(storeItemPrefab, storeListContainer);
            StoreListItem listItem = storeItem.GetComponent<StoreListItem>();
            
            if (listItem != null)
            {
                listItem.SetStoreData(store);
                listItem.OnItemClicked += () => SelectStore(store);
            }
        }
    }
    
    private void FilterByCategory(string category)
    {
        currentCategory = category;
        UpdateStoreList();
        
        // Update category button states
        categoryAllButton.interactable = category != "All";
        categoryFashionButton.interactable = category != "Fashion";
        categoryFoodButton.interactable = category != "Food";
        categoryElectronicsButton.interactable = category != "Electronics";
    }
    
    private void HandleSearchInputChanged(string value)
    {
        searchQuery = value;
        UpdateStoreList();
    }
    
    private void SelectStore(StoreData store)
    {
        selectedStore = store;
        
        // Update store detail UI
        storeNameText.text = store.name;
        storeDescriptionText.text = store.description;
        storeLocationText.text = store.location;
        
        // Load store image
        Sprite storeSprite = Resources.Load<Sprite>(store.imagePath);
        if (storeSprite != null)
        {
            storeImage.sprite = storeSprite;
        }
        
        // Show store detail screen
        ShowScreen(storeDetailScreen);
        
        // Trigger event
        OnStoreSelected?.Invoke(store.id);
    }
    
    private void StartNavigation()
    {
        if (selectedStore != null)
        {
            // Show AR prompt
            ShowScreen(arPromptScreen);
        }
    }
    
    private void StartARNavigation()
    {
        if (selectedStore != null && appManager != null)
        {
            // Start navigation to the selected store
            appManager.StartNavigation(selectedStore.id);
            
            // Show navigation UI
            ShowScreen(navigationScreen);
            
            // Update navigation UI
            destinationText.text = selectedStore.name;
            UpdateNavigationInfo(selectedStore.distance, selectedStore.distance / 1.2f); // Estimated time based on walking speed
            
            // Trigger event
            OnNavigationStarted?.Invoke();
        }
    }
    
    private void StopNavigation()
    {
        if (appManager != null)
        {
            appManager.StopNavigation();
        }
        
        // Return to store directory
        ShowScreen(storeDirectoryScreen);
        
        // Trigger event
        OnNavigationStopped?.Invoke();
    }
    
    private void HandleAppInitialized()
    {
        // Show store directory when app is initialized
        ShowScreen(storeDirectoryScreen);
    }
    
    private void HandleNavigationStarted(string destinationId)
    {
        // Find the store data for the destination
        StoreData destination = allStores.Find(store => store.id == destinationId);
        if (destination != null)
        {
            selectedStore = destination;
            destinationText.text = destination.name;
        }
        
        // Show navigation screen
        ShowScreen(navigationScreen);
    }
    
    private void HandleNavigationStopped()
    {
        // Return to store directory
        ShowScreen(storeDirectoryScreen);
    }
    
    private void HandleDestinationReached(string destinationId)
    {
        // Show a destination reached message or UI
        // For now, simply stop navigation after a delay
        Invoke(nameof(StopNavigation), 3f);
    }
    
    private void HandlePositionUpdated(Vector3 position, float heading)
    {
        if (selectedStore != null && navigationManager != null)
        {
            // Calculate remaining distance
            float distance = navigationManager.GetRemainingDistance();
            
            // Calculate estimated time based on walking speed (1.2 m/s)
            float estimatedTime = distance / 1.2f;
            
            // Update navigation UI
            UpdateNavigationInfo(distance, estimatedTime);
            
            // Update directional arrow
            if (directionalArrow != null)
            {
                Vector3 direction = navigationManager.GetDirectionToNextWaypoint();
                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                directionalArrow.transform.rotation = Quaternion.Euler(0, angle, 0);
            }
            
            // Update compass bar
            if (compassBar != null)
            {
                // Map heading (0-360) to compass bar fill (0-1)
                compassBar.fillAmount = heading / 360f;
            }
        }
    }
    
    private void UpdateNavigationInfo(float distance, float estimatedTime)
    {
        // Update distance text
        if (distanceText != null)
        {
            if (distance >= 1000f)
            {
                distanceText.text = $"{(distance / 1000f):F1} km";
            }
            else
            {
                distanceText.text = $"{Mathf.RoundToInt(distance)} m";
            }
        }
        
        // Update estimated time text
        if (estimatedTimeText != null)
        {
            if (estimatedTime >= 60f)
            {
                float minutes = estimatedTime / 60f;
                estimatedTimeText.text = $"{Mathf.CeilToInt(minutes)} min";
            }
            else
            {
                estimatedTimeText.text = $"{Mathf.CeilToInt(estimatedTime)} sec";
            }
        }
    }
    
    private void ShowScreen(GameObject screen)
    {
        // Hide all screens
        if (welcomeScreen != null) welcomeScreen.SetActive(false);
        if (storeDirectoryScreen != null) storeDirectoryScreen.SetActive(false);
        if (storeDetailScreen != null) storeDetailScreen.SetActive(false);
        if (navigationScreen != null) navigationScreen.SetActive(false);
        if (settingsScreen != null) settingsScreen.SetActive(false);
        if (rewardsScreen != null) rewardsScreen.SetActive(false);
        if (arPromptScreen != null) arPromptScreen.SetActive(false);
        
        // Show the requested screen
        if (screen != null)
        {
            screen.SetActive(true);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (appManager != null)
        {
            appManager.OnAppInitialized -= HandleAppInitialized;
            appManager.OnNavigationStarted -= HandleNavigationStarted;
            appManager.OnNavigationStopped -= HandleNavigationStopped;
            appManager.OnDestinationReached -= HandleDestinationReached;
        }
        
        if (navigationManager != null)
        {
            navigationManager.OnPositionUpdated -= HandlePositionUpdated;
        }
        
        // Unsubscribe button listeners
        if (searchInput != null)
            searchInput.onValueChanged.RemoveListener(HandleSearchInputChanged);
            
        if (categoryAllButton != null)
            categoryAllButton.onClick.RemoveAllListeners();
            
        if (categoryFashionButton != null)
            categoryFashionButton.onClick.RemoveAllListeners();
            
        if (categoryFoodButton != null)
            categoryFoodButton.onClick.RemoveAllListeners();
            
        if (categoryElectronicsButton != null)
            categoryElectronicsButton.onClick.RemoveAllListeners();
            
        if (navigateButton != null)
            navigateButton.onClick.RemoveAllListeners();
            
        if (backToDirectoryButton != null)
            backToDirectoryButton.onClick.RemoveAllListeners();
            
        if (stopNavigationButton != null)
            stopNavigationButton.onClick.RemoveAllListeners();
            
        if (arStartButton != null)
            arStartButton.onClick.RemoveAllListeners();
        
        if (claimPromotionButton != null)
            claimPromotionButton.onClick.RemoveAllListeners();
    }
    
    private void ShowPromotion(StoreData store)
    {
        if (promotionPanel == null || store.activePromotion == null) return;
        
        promotionPanel.SetActive(true);
        
        if (promotionTitleText != null) 
        {
            promotionTitleText.text = store.activePromotion.title;
        }
        
        if (promotionDescriptionText != null)
        {
            promotionDescriptionText.text = store.activePromotion.description;
        }
        
        // Trigger promotion viewed event for analytics
        OnPromotionViewed?.Invoke(store.activePromotion.id, store.id);
    }
    
    private void HidePromotion()
    {
        if (promotionPanel == null) return;
        
        promotionPanel.SetActive(false);
    }
    
    private void OnClaimPromotionButtonClicked()
    {
        if (selectedStore == null || selectedStore.activePromotion == null) return;
        
        // Handle promotion claim
        ShowAchievement($"Claimed: {selectedStore.activePromotion.title}");
        
        // Award bonus points for claiming promotion
        int bonusPoints = 25;
        userPoints += bonusPoints;
        UpdateUserProfile();
        ShowPointsEarned(bonusPoints);
        
        // Hide promotion after claiming
        HidePromotion();
        
        // Trigger promotion claimed event for analytics
        OnPromotionClaimed?.Invoke(selectedStore.activePromotion.id, selectedStore.id);
    }
    
    private void ShowAchievement(string achievementName)
    {
        if (achievementNotification == null || achievementText == null) return;
        
        achievementText.text = achievementName;
        achievementNotification.SetActive(true);
        
        // Hide after delay
        StartCoroutine(HideNotificationAfterDelay(achievementNotification, 3.0f));
        
        // Trigger achievement event for analytics
        OnAchievementUnlocked?.Invoke(achievementName);
    }
    
    private IEnumerator HideNotificationAfterDelay(GameObject notification, float delay)
    {
        yield return new WaitForSeconds(delay);
        notification.SetActive(false);
    }
    
    private void UpdateUserProfile()
    {
        // Calculate level based on points
        userLevel = 1 + (userPoints / 500);
        
        // Update UI
        if (progressText != null) progressText.text = $"{userPoints}/{userLevel * 500}";
        
        // Update progress bar
        if (progressBar != null && progressFill != null)
        {
            float progress = (float)userPoints / (userLevel * 500);
            progressFill.fillAmount = progress;
        }
    }
    
    private void ShowPointsEarned(int points)
    {
        if (pointsEarnedNotification == null || pointsEarnedText == null) return;
        
        pointsEarnedText.text = $"+{points} points";
        pointsEarnedNotification.SetActive(true);
        
        // Hide after delay
        StartCoroutine(HideNotificationAfterDelay(pointsEarnedNotification, 3.0f));
        
        // Trigger points earned event for analytics
        OnPointsEarned?.Invoke(points);
    }
} 