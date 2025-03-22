using UnityEngine;
using System;
using System.Collections.Generic;

public class AnalyticsManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool enableAnalytics = true;
    [SerializeField] private bool logEventsToConsole = true;
    
    [Header("References")]
    [SerializeField] private GameObject arNavigationManager;
    [SerializeField] private GameObject oriientSDKManager;
    [SerializeField] private GameObject uiManager;
    
    // Singleton instance
    private static AnalyticsManager _instance;
    public static AnalyticsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AnalyticsManager>();
                if (_instance == null)
                {
                    GameObject analyticsManagerObj = new GameObject("AnalyticsManager");
                    _instance = analyticsManagerObj.AddComponent<AnalyticsManager>();
                }
            }
            return _instance;
        }
    }
    
    // Tracking data
    private Dictionary<string, int> screenViewCounts = new Dictionary<string, int>();
    private Dictionary<string, float> screenTimeSpent = new Dictionary<string, float>();
    private Dictionary<string, int> storeViewCounts = new Dictionary<string, int>();
    private Dictionary<string, int> storeNavigationCounts = new Dictionary<string, int>();
    private Dictionary<string, float> navigationAccuracy = new Dictionary<string, float>();
    private Dictionary<string, float> navigationCompletionTimes = new Dictionary<string, float>();
    private Dictionary<string, int> promotionViewCounts = new Dictionary<string, int>();
    private Dictionary<string, int> promotionClaimCounts = new Dictionary<string, int>();
    
    // Session data
    private DateTime sessionStartTime;
    private string currentScreen = "";
    private DateTime currentScreenStartTime;
    private string currentNavigationTarget = "";
    private DateTime navigationStartTime;
    
    // Performance metrics
    private float avgFrameRate = 0f;
    private float minFrameRate = float.MaxValue;
    private int frameRateDropCount = 0;
    private float avgPositioningAccuracy = 0f;
    private int positioningSampleCount = 0;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize session
        sessionStartTime = DateTime.Now;
        LogEvent("session_start", new Dictionary<string, object> { { "timestamp", sessionStartTime } });
    }
    
    private void Update()
    {
        if (!enableAnalytics) return;
        
        // Track frame rate
        float currentFrameRate = 1.0f / Time.deltaTime;
        avgFrameRate = Mathf.Lerp(avgFrameRate, currentFrameRate, 0.03f);
        
        if (currentFrameRate < minFrameRate)
        {
            minFrameRate = currentFrameRate;
        }
        
        if (currentFrameRate < 24) // Consider this a frame drop
        {
            frameRateDropCount++;
        }
        
        // Update screen time if on a screen
        if (!string.IsNullOrEmpty(currentScreen))
        {
            float additionalTime = Time.deltaTime;
            if (screenTimeSpent.ContainsKey(currentScreen))
            {
                screenTimeSpent[currentScreen] += additionalTime;
            }
            else
            {
                screenTimeSpent[currentScreen] = additionalTime;
            }
        }
    }
    
    private void OnDestroy()
    {
        // End session and send any final analytics
        LogEvent("session_end", new Dictionary<string, object> 
        { 
            { "duration", (DateTime.Now - sessionStartTime).TotalSeconds },
            { "avg_frame_rate", avgFrameRate },
            { "min_frame_rate", minFrameRate },
            { "frame_drops", frameRateDropCount }
        });
    }
    
    #region Public Tracking Methods
    
    public void TrackScreenView(string screenName)
    {
        if (!enableAnalytics) return;
        
        // End previous screen view if there was one
        if (!string.IsNullOrEmpty(currentScreen))
        {
            float timeSpent = (float)(DateTime.Now - currentScreenStartTime).TotalSeconds;
            LogEvent("screen_exit", new Dictionary<string, object> 
            { 
                { "screen_name", currentScreen },
                { "time_spent", timeSpent }
            });
        }
        
        // Start new screen view
        currentScreen = screenName;
        currentScreenStartTime = DateTime.Now;
        
        // Increment screen view count
        if (screenViewCounts.ContainsKey(screenName))
        {
            screenViewCounts[screenName]++;
        }
        else
        {
            screenViewCounts[screenName] = 1;
        }
        
        LogEvent("screen_view", new Dictionary<string, object> { { "screen_name", screenName } });
    }
    
    public void TrackStoreView(string storeId, string storeName)
    {
        if (!enableAnalytics) return;
        
        if (storeViewCounts.ContainsKey(storeId))
        {
            storeViewCounts[storeId]++;
        }
        else
        {
            storeViewCounts[storeId] = 1;
        }
        
        LogEvent("store_view", new Dictionary<string, object> 
        {
            { "store_id", storeId },
            { "store_name", storeName }
        });
    }
    
    public void TrackNavigationStart(string storeId, string storeName, Vector3 startPosition, Vector3 destinationPosition)
    {
        if (!enableAnalytics) return;
        
        currentNavigationTarget = storeId;
        navigationStartTime = DateTime.Now;
        
        if (storeNavigationCounts.ContainsKey(storeId))
        {
            storeNavigationCounts[storeId]++;
        }
        else
        {
            storeNavigationCounts[storeId] = 1;
        }
        
        LogEvent("navigation_start", new Dictionary<string, object> 
        {
            { "store_id", storeId },
            { "store_name", storeName },
            { "start_x", startPosition.x },
            { "start_y", startPosition.y },
            { "start_z", startPosition.z },
            { "dest_x", destinationPosition.x },
            { "dest_y", destinationPosition.y },
            { "dest_z", destinationPosition.z }
        });
    }
    
    public void TrackNavigationEnd(string storeId, bool completed, float distanceToTarget)
    {
        if (!enableAnalytics) return;
        
        float duration = (float)(DateTime.Now - navigationStartTime).TotalSeconds;
        
        LogEvent("navigation_end", new Dictionary<string, object> 
        {
            { "store_id", storeId },
            { "completed", completed },
            { "duration", duration },
            { "distance_to_target", distanceToTarget }
        });
        
        if (completed)
        {
            if (navigationCompletionTimes.ContainsKey(storeId))
            {
                navigationCompletionTimes[storeId] = (navigationCompletionTimes[storeId] + duration) / 2f; // Average
            }
            else
            {
                navigationCompletionTimes[storeId] = duration;
            }
        }
        
        currentNavigationTarget = "";
    }
    
    public void TrackPositioningAccuracy(float actualAccuracyInMeters)
    {
        if (!enableAnalytics) return;
        
        positioningSampleCount++;
        avgPositioningAccuracy = ((avgPositioningAccuracy * (positioningSampleCount - 1)) + actualAccuracyInMeters) / positioningSampleCount;
        
        LogEvent("positioning_accuracy", new Dictionary<string, object> 
        {
            { "accuracy", actualAccuracyInMeters },
            { "avg_accuracy", avgPositioningAccuracy }
        });
    }
    
    public void TrackPromotionView(string promotionId, string storeId)
    {
        if (!enableAnalytics) return;
        
        if (promotionViewCounts.ContainsKey(promotionId))
        {
            promotionViewCounts[promotionId]++;
        }
        else
        {
            promotionViewCounts[promotionId] = 1;
        }
        
        LogEvent("promotion_view", new Dictionary<string, object> 
        {
            { "promotion_id", promotionId },
            { "store_id", storeId }
        });
    }
    
    public void TrackPromotionClaim(string promotionId, string storeId)
    {
        if (!enableAnalytics) return;
        
        if (promotionClaimCounts.ContainsKey(promotionId))
        {
            promotionClaimCounts[promotionId]++;
        }
        else
        {
            promotionClaimCounts[promotionId] = 1;
        }
        
        LogEvent("promotion_claim", new Dictionary<string, object> 
        {
            { "promotion_id", promotionId },
            { "store_id", storeId }
        });
    }
    
    public void TrackDivineLineVisibility(bool wasVisible, float visibilityDuration, string storeId)
    {
        if (!enableAnalytics) return;
        
        LogEvent("divine_line_visibility", new Dictionary<string, object> 
        {
            { "was_visible", wasVisible },
            { "duration", visibilityDuration },
            { "store_id", storeId }
        });
    }
    
    public void TrackError(string errorType, string errorMessage, string context)
    {
        if (!enableAnalytics) return;
        
        LogEvent("app_error", new Dictionary<string, object> 
        {
            { "error_type", errorType },
            { "error_message", errorMessage },
            { "context", context }
        });
    }
    
    #endregion
    
    #region Analytics Helpers
    
    private void LogEvent(string eventName, Dictionary<string, object> parameters)
    {
        if (!enableAnalytics) return;
        
        // In a real implementation, this would send the event to Firebase, Amplitude, etc.
        // For now, we'll just log to console if enabled
        
        if (logEventsToConsole)
        {
            string paramString = "";
            foreach (var param in parameters)
            {
                paramString += $"{param.Key}:{param.Value}, ";
            }
            
            if (paramString.Length > 2)
            {
                paramString = paramString.Substring(0, paramString.Length - 2);
            }
            
            Debug.Log($"[ANALYTICS] {eventName}: {paramString}");
        }
        
        // TODO: Implement actual analytics service integration here
        // For Firebase Analytics:
        // Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameters.Select(p => new Firebase.Analytics.Parameter(p.Key, p.Value)).ToArray());
    }
    
    public Dictionary<string, object> GetAnalyticsSummary()
    {
        Dictionary<string, object> summary = new Dictionary<string, object>();
        
        // Session data
        summary["session_duration"] = (DateTime.Now - sessionStartTime).TotalMinutes;
        summary["total_screens_viewed"] = screenViewCounts.Sum(kv => kv.Value);
        summary["total_stores_viewed"] = storeViewCounts.Sum(kv => kv.Value);
        summary["total_navigations"] = storeNavigationCounts.Sum(kv => kv.Value);
        summary["total_promotions_viewed"] = promotionViewCounts.Sum(kv => kv.Value);
        summary["total_promotions_claimed"] = promotionClaimCounts.Sum(kv => kv.Value);
        
        // Performance
        summary["avg_frame_rate"] = avgFrameRate;
        summary["min_frame_rate"] = minFrameRate;
        summary["frame_drops"] = frameRateDropCount;
        summary["avg_positioning_accuracy"] = avgPositioningAccuracy;
        
        // Most viewed screens
        var mostViewedScreen = screenViewCounts.OrderByDescending(kv => kv.Value).FirstOrDefault();
        if (mostViewedScreen.Key != null)
        {
            summary["most_viewed_screen"] = mostViewedScreen.Key;
            summary["most_viewed_screen_count"] = mostViewedScreen.Value;
        }
        
        // Most viewed stores
        var mostViewedStore = storeViewCounts.OrderByDescending(kv => kv.Value).FirstOrDefault();
        if (mostViewedStore.Key != null)
        {
            summary["most_viewed_store"] = mostViewedStore.Key;
            summary["most_viewed_store_count"] = mostViewedStore.Value;
        }
        
        return summary;
    }
    
    #endregion
} 