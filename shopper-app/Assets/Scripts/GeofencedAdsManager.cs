using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Manages geofenced ads in the Wandur app.
/// Detects when a user enters a store's proximity and displays relevant ads.
/// </summary>
public class GeofencedAdsManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private OriientSDKManager oriientManager;

    [Header("Geofencing Configuration")]
    [SerializeField] private float checkInterval = 2.0f; // How often to check for geofence triggers (seconds)
    [SerializeField] private float defaultRadius = 10.0f; // Default radius for geofence in meters
    
    [Header("Advertisement Settings")]
    [SerializeField] private GameObject adPrefab; // Prefab for AR advertisement
    [SerializeField] private float adDisplayTime = 30.0f; // How long to display the ad (seconds)
    [SerializeField] private int maxDisplayedAds = 3; // Maximum number of concurrent ads
    
    // Stores and their associated ads
    [System.Serializable]
    public class StoreAd
    {
        public string storeId;
        public string storeName;
        public float triggerRadius = 10.0f;
        public GameObject customAdPrefab; // Override the default ad prefab if needed
        public Sprite adImage; // Ad image to display
        public string adText; // Ad text
        public bool isActive = true; // Whether this ad is active
        public float cooldownTime = 300.0f; // How long to wait before showing this ad again (seconds)
        
        [HideInInspector]
        public float lastTriggerTime; // When this ad was last triggered
    }
    
    [SerializeField] private List<StoreAd> storeAds = new List<StoreAd>();
    
    // Currently displayed ads
    private Dictionary<string, GameObject> activeAds = new Dictionary<string, GameObject>();
    
    // Event for ad triggered
    public event Action<string, string> OnAdTriggered; // storeId, adText
    
    private bool isInitialized = false;
    private bool isRunning = false;

    private void Start()
    {
        // Ensure we have a reference to the Oriient manager
        if (oriientManager == null)
        {
            oriientManager = FindObjectOfType<OriientSDKManager>();
        }
        
        if (oriientManager != null)
        {
            // Listen for SDK initialization
            oriientManager.OnSDKInitialized += OnOriientSDKInitialized;
            
            // If the SDK is already initialized, start monitoring
            if (isInitialized)
            {
                StartGeofenceMonitoring();
            }
        }
        else
        {
            Debug.LogError("GeofencedAdsManager requires OriientSDKManager but none was found");
        }
    }

    private void OnOriientSDKInitialized()
    {
        isInitialized = true;
        StartGeofenceMonitoring();
    }

    /// <summary>
    /// Begins monitoring for geofence triggers
    /// </summary>
    public void StartGeofenceMonitoring()
    {
        if (isRunning || !isInitialized)
            return;
        
        isRunning = true;
        StartCoroutine(MonitorGeofences());
        Debug.Log("Started geofence monitoring for ads");
    }

    /// <summary>
    /// Stops monitoring for geofence triggers
    /// </summary>
    public void StopGeofenceMonitoring()
    {
        isRunning = false;
        StopAllCoroutines();
        Debug.Log("Stopped geofence monitoring for ads");
    }

    /// <summary>
    /// Monitors for geofence triggers at regular intervals
    /// </summary>
    private IEnumerator MonitorGeofences()
    {
        while (isRunning)
        {
            // Get current position from Oriient SDK
            Vector3 currentPosition = oriientManager.GetCurrentPosition();
            
            // Check each geofence
            foreach (StoreAd ad in storeAds)
            {
                if (!ad.isActive)
                    continue;
                
                // Skip if on cooldown
                if (Time.time - ad.lastTriggerTime < ad.cooldownTime)
                    continue;
                
                // Get store position
                Vector3 storePosition = oriientManager.GetStoreLocation(ad.storeId);
                
                // Check if user is within the geofence
                float distance = Vector3.Distance(currentPosition, storePosition);
                float radius = ad.triggerRadius > 0 ? ad.triggerRadius : defaultRadius;
                
                if (distance <= radius)
                {
                    // User is within the geofence, trigger ad
                    TriggerAd(ad);
                }
            }
            
            // Wait for next check
            yield return new WaitForSeconds(checkInterval);
        }
    }

    /// <summary>
    /// Triggers an advertisement for a store
    /// </summary>
    private void TriggerAd(StoreAd ad)
    {
        // Update last trigger time
        ad.lastTriggerTime = Time.time;
        
        // Skip if already displaying this ad
        if (activeAds.ContainsKey(ad.storeId))
            return;
        
        // Limit the number of displayed ads
        if (activeAds.Count >= maxDisplayedAds)
        {
            // Remove the oldest ad
            RemoveOldestAd();
        }
        
        // Create the ad
        GameObject adObject = null;
        if (ad.customAdPrefab != null)
        {
            adObject = Instantiate(ad.customAdPrefab);
        }
        else if (adPrefab != null)
        {
            adObject = Instantiate(adPrefab);
        }
        else
        {
            Debug.LogWarning("No ad prefab specified");
            return;
        }
        
        // Configure the ad
        AdDisplay adDisplay = adObject.GetComponent<AdDisplay>();
        if (adDisplay != null)
        {
            // Set ad content
            adDisplay.SetAdContent(ad.storeName, ad.adText, ad.adImage);
            
            // Position the ad - in a real implementation, you would use AR placement
            // For now, place it in front of the camera
            Camera camera = Camera.main;
            if (camera != null)
            {
                adObject.transform.position = camera.transform.position + camera.transform.forward * 2f;
                adObject.transform.rotation = Quaternion.LookRotation(camera.transform.forward);
            }
        }
        
        // Store the active ad
        activeAds.Add(ad.storeId, adObject);
        
        // Trigger event
        OnAdTriggered?.Invoke(ad.storeId, ad.adText);
        
        Debug.Log($"Triggered ad for store: {ad.storeName}");
        
        // Schedule ad removal
        StartCoroutine(RemoveAdAfterDelay(ad.storeId, adDisplayTime));
    }

    /// <summary>
    /// Removes the oldest active ad
    /// </summary>
    private void RemoveOldestAd()
    {
        if (activeAds.Count == 0)
            return;
        
        // For simplicity, just remove the first ad in the dictionary
        // In a real implementation, you could track display times
        foreach (var kvp in activeAds)
        {
            RemoveAd(kvp.Key);
            break;
        }
    }

    /// <summary>
    /// Removes an ad after a delay
    /// </summary>
    private IEnumerator RemoveAdAfterDelay(string storeId, float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveAd(storeId);
    }

    /// <summary>
    /// Removes an ad for a specific store
    /// </summary>
    public void RemoveAd(string storeId)
    {
        if (activeAds.TryGetValue(storeId, out GameObject adObject))
        {
            if (adObject != null)
            {
                Destroy(adObject);
            }
            
            activeAds.Remove(storeId);
            Debug.Log($"Removed ad for store: {storeId}");
        }
    }

    /// <summary>
    /// Manually adds a new ad to the manager
    /// </summary>
    public void AddStoreAd(string storeId, string storeName, string adText, Sprite adImage, float radius = 0)
    {
        StoreAd ad = new StoreAd
        {
            storeId = storeId,
            storeName = storeName,
            adText = adText,
            adImage = adImage,
            triggerRadius = radius > 0 ? radius : defaultRadius,
            isActive = true,
            lastTriggerTime = 0
        };
        
        storeAds.Add(ad);
    }

    /// <summary>
    /// Removes all active ads
    /// </summary>
    public void ClearAllAds()
    {
        foreach (var kvp in activeAds)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        
        activeAds.Clear();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        ClearAllAds();
    }
}

/// <summary>
/// Component for displaying an ad in AR
/// </summary>
public class AdDisplay : MonoBehaviour
{
    [SerializeField] private TextMesh titleText;
    [SerializeField] private TextMesh contentText;
    [SerializeField] private SpriteRenderer imageRenderer;
    [SerializeField] private GameObject adPanel;
    
    public void SetAdContent(string title, string content, Sprite image)
    {
        if (titleText != null)
            titleText.text = title;
            
        if (contentText != null)
            contentText.text = content;
            
        if (imageRenderer != null && image != null)
            imageRenderer.sprite = image;
    }
    
    public void CloseAd()
    {
        Destroy(gameObject);
    }
    
    private void Start()
    {
        // Make the ad face the camera
        StartCoroutine(FaceCamera());
    }
    
    private IEnumerator FaceCamera()
    {
        Camera mainCamera = Camera.main;
        
        while (mainCamera != null && this != null)
        {
            // Make the ad face the camera
            transform.LookAt(mainCamera.transform);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            
            yield return null;
        }
    }
} 