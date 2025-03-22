using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Main manager for the Wandur shopping app.
/// Coordinates between different subsystems like AR navigation, geofencing, and social features.
/// </summary>
public class WandurAppManager : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private ARNavigationManager navigationManager;
    [SerializeField] private OriientSDKManager oriientManager;
    [SerializeField] private GeofencedAdsManager adsManager;
    
    [Header("App Settings")]
    [SerializeField] private bool debugMode = true;
    
    // Singleton instance
    public static WandurAppManager Instance { get; private set; }
    
    // App state
    private bool isInitialized = false;
    
    // Current store or destination
    private string currentDestinationId;
    
    // Events
    public event Action OnAppInitialized;
    public event Action<string> OnDestinationSelected; // storeId
    public event Action<string> OnDestinationReached; // storeId
    
    private void Awake()
    {
        // Singleton pattern
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
        Debug.Log("Wandur App starting...");
        
        // Find required components if not set
        if (navigationManager == null)
            navigationManager = FindObjectOfType<ARNavigationManager>();
            
        if (oriientManager == null)
            oriientManager = FindObjectOfType<OriientSDKManager>();
            
        if (adsManager == null)
            adsManager = FindObjectOfType<GeofencedAdsManager>();
            
        // Initialize the app
        StartCoroutine(InitializeApp());
    }

    /// <summary>
    /// Initializes all required components of the app
    /// </summary>
    private IEnumerator InitializeApp()
    {
        Debug.Log("Initializing Wandur App...");
        
        // Wait for Oriient SDK to initialize (if it's not already)
        if (oriientManager != null)
        {
            oriientManager.OnSDKInitialized += OnOriientSDKInitialized;
            
            // Wait for SDK initialization if needed
            yield return new WaitUntil(() => oriientManager.GetCurrentPosition() != Vector3.zero);
        }
        else
        {
            Debug.LogError("OriientSDKManager not found");
        }
        
        // Wait for AR to initialize
        if (navigationManager != null)
        {
            // AR is typically initialized in its own Start method
            yield return new WaitForSeconds(1.0f); // Give AR time to initialize
        }
        else
        {
            Debug.LogError("ARNavigationManager not found");
        }
        
        // Initialize ad system
        if (adsManager != null)
        {
            // Register for ad events
            adsManager.OnAdTriggered += OnAdTriggered;
        }
        else
        {
            Debug.LogWarning("GeofencedAdsManager not found");
        }
        
        // App is now initialized
        isInitialized = true;
        Debug.Log("Wandur App initialized successfully");
        
        // Notify listeners
        OnAppInitialized?.Invoke();
    }

    /// <summary>
    /// Called when the Oriient SDK is initialized
    /// </summary>
    private void OnOriientSDKInitialized()
    {
        Debug.Log("Oriient SDK initialized");
    }

    /// <summary>
    /// Called when a geofenced ad is triggered
    /// </summary>
    private void OnAdTriggered(string storeId, string adText)
    {
        Debug.Log($"Ad triggered - Store: {storeId}, Ad: {adText}");
        
        // Here you might want to update UI, play a sound, etc.
    }

    /// <summary>
    /// Starts navigation to a specific store
    /// </summary>
    public void NavigateToStore(string storeId)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Cannot navigate - app not fully initialized");
            return;
        }
        
        // Store current destination
        currentDestinationId = storeId;
        
        // Notify listeners
        OnDestinationSelected?.Invoke(storeId);
        
        if (navigationManager != null)
        {
            // Use the navigation manager to start navigation
            navigationManager.NavigateToStore(storeId);
            Debug.Log($"Starting navigation to store: {storeId}");
        }
    }

    /// <summary>
    /// Stops the current navigation
    /// </summary>
    public void StopNavigation()
    {
        if (navigationManager != null)
        {
            navigationManager.StopNavigation();
            Debug.Log("Navigation stopped");
        }
        
        currentDestinationId = null;
    }

    /// <summary>
    /// Called when the user reaches their destination
    /// </summary>
    public void DestinationReached(string storeId)
    {
        // Notify listeners
        OnDestinationReached?.Invoke(storeId);
        
        Debug.Log($"Destination reached: {storeId}");
        
        // Here you would typically show a store details UI,
        // offer rewards, etc.
    }

    /// <summary>
    /// Gets nearby stores based on current location
    /// </summary>
    public List<string> GetNearbyStores(float radius = 10f)
    {
        if (oriientManager != null)
        {
            return oriientManager.GetNearbyStores(radius);
        }
        
        return new List<string>();
    }

    /// <summary>
    /// Show a specific advertisement immediately (for testing or manual triggering)
    /// </summary>
    public void ShowAd(string storeId)
    {
        if (adsManager != null)
        {
            // This assumes the ad manager has a method to manually trigger an ad
            // You would need to implement this in the GeofencedAdsManager class
            Debug.Log($"Manually showing ad for store: {storeId}");
        }
    }

    #region UI Callbacks
    
    // These methods would be called from UI elements
    
    /// <summary>
    /// Called when a store is selected from the UI
    /// </summary>
    public void OnStoreSelected(string storeId)
    {
        NavigateToStore(storeId);
    }
    
    /// <summary>
    /// Called when the user wants to cancel navigation
    /// </summary>
    public void OnCancelNavigation()
    {
        StopNavigation();
    }
    
    #endregion

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (oriientManager != null)
        {
            oriientManager.OnSDKInitialized -= OnOriientSDKInitialized;
        }
        
        if (adsManager != null)
        {
            adsManager.OnAdTriggered -= OnAdTriggered;
        }
    }
} 