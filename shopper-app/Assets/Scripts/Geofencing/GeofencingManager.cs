using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GeofenceZone
{
    public string id;
    public string name;
    public Vector3 center;
    public float radius;
    public string[] tags;
    public string adPrefabPath;
    public float cooldownTime = 300f; // 5 minutes default cooldown
    public bool isActive = true;
}

[System.Serializable]
public class GeofenceEvent
{
    public string zoneId;
    public DateTime timestamp;
    public bool isEntry;
}

public class GeofencingManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private OriientSDKManager oriientManager;
    
    [Header("Geofence Settings")]
    [SerializeField] private List<GeofenceZone> geofenceZones = new List<GeofenceZone>();
    [SerializeField] private float checkInterval = 1f;
    [SerializeField] private float defaultRadius = 3f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    [SerializeField] private Color activeZoneColor = Color.green;
    [SerializeField] private Color inactiveZoneColor = Color.red;
    
    // Events
    public event Action<GeofenceZone> OnZoneEntered;
    public event Action<GeofenceZone> OnZoneExited;
    public event Action<GeofenceZone> OnAdTriggered;
    
    private Dictionary<string, bool> zoneStates = new Dictionary<string, bool>();
    private Dictionary<string, DateTime> lastTriggerTimes = new Dictionary<string, DateTime>();
    private Dictionary<string, GameObject> activeAds = new Dictionary<string, GameObject>();
    private float lastCheckTime;
    
    private void Start()
    {
        if (!oriientManager)
        {
            oriientManager = FindObjectOfType<OriientSDKManager>();
        }
        
        if (oriientManager)
        {
            oriientManager.OnPositionUpdated += CheckGeofences;
        }
        else
        {
            Debug.LogError("GeofencingManager requires OriientSDKManager to function!");
        }
        
        // Initialize zone states
        foreach (var zone in geofenceZones)
        {
            zoneStates[zone.id] = false;
            lastTriggerTimes[zone.id] = DateTime.MinValue;
        }
    }
    
    private void CheckGeofences(Vector3 position)
    {
        if (Time.time - lastCheckTime < checkInterval) return;
        lastCheckTime = Time.time;
        
        foreach (var zone in geofenceZones)
        {
            if (!zone.isActive) continue;
            
            bool wasInside = zoneStates[zone.id];
            bool isInside = IsPositionInZone(position, zone);
            
            if (isInside != wasInside)
            {
                zoneStates[zone.id] = isInside;
                
                if (isInside)
                {
                    HandleZoneEntry(zone);
                }
                else
                {
                    HandleZoneExit(zone);
                }
            }
        }
    }
    
    private bool IsPositionInZone(Vector3 position, GeofenceZone zone)
    {
        return Vector3.Distance(position, zone.center) <= zone.radius;
    }
    
    private void HandleZoneEntry(GeofenceZone zone)
    {
        OnZoneEntered?.Invoke(zone);
        
        // Check cooldown
        if (DateTime.Now - lastTriggerTimes[zone.id] > TimeSpan.FromSeconds(zone.cooldownTime))
        {
            TriggerAd(zone);
            lastTriggerTimes[zone.id] = DateTime.Now;
        }
    }
    
    private void HandleZoneExit(GeofenceZone zone)
    {
        OnZoneExited?.Invoke(zone);
        
        // Clean up active ad if exists
        if (activeAds.TryGetValue(zone.id, out GameObject adObject))
        {
            Destroy(adObject);
            activeAds.Remove(zone.id);
        }
    }
    
    private void TriggerAd(GeofenceZone zone)
    {
        if (string.IsNullOrEmpty(zone.adPrefabPath)) return;
        
        // Load and instantiate ad prefab
        GameObject adPrefab = Resources.Load<GameObject>(zone.adPrefabPath);
        if (adPrefab != null)
        {
            GameObject adInstance = Instantiate(adPrefab, zone.center, Quaternion.identity);
            
            // Clean up previous ad if exists
            if (activeAds.TryGetValue(zone.id, out GameObject previousAd))
            {
                Destroy(previousAd);
            }
            
            activeAds[zone.id] = adInstance;
            OnAdTriggered?.Invoke(zone);
        }
        else
        {
            Debug.LogWarning($"Ad prefab not found at path: {zone.adPrefabPath}");
        }
    }
    
    // Public methods for runtime management
    public void AddGeofenceZone(Vector3 center, float radius = -1f, string[] tags = null)
    {
        GeofenceZone newZone = new GeofenceZone
        {
            id = System.Guid.NewGuid().ToString(),
            name = $"Zone_{geofenceZones.Count + 1}",
            center = center,
            radius = radius < 0 ? defaultRadius : radius,
            tags = tags ?? new string[0],
            isActive = true
        };
        
        geofenceZones.Add(newZone);
        zoneStates[newZone.id] = false;
        lastTriggerTimes[newZone.id] = DateTime.MinValue;
    }
    
    public void RemoveGeofenceZone(string zoneId)
    {
        var zone = geofenceZones.FirstOrDefault(z => z.id == zoneId);
        if (zone != null)
        {
            geofenceZones.Remove(zone);
            zoneStates.Remove(zoneId);
            lastTriggerTimes.Remove(zoneId);
            
            if (activeAds.TryGetValue(zoneId, out GameObject adObject))
            {
                Destroy(adObject);
                activeAds.Remove(zoneId);
            }
        }
    }
    
    public void SetZoneActive(string zoneId, bool active)
    {
        var zone = geofenceZones.FirstOrDefault(z => z.id == zoneId);
        if (zone != null)
        {
            zone.isActive = active;
            if (!active && activeAds.TryGetValue(zoneId, out GameObject adObject))
            {
                Destroy(adObject);
                activeAds.Remove(zoneId);
            }
        }
    }
    
    public List<GeofenceZone> GetNearbyZones(Vector3 position, float maxDistance)
    {
        return geofenceZones
            .Where(zone => zone.isActive && Vector3.Distance(position, zone.center) <= maxDistance)
            .ToList();
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        foreach (var zone in geofenceZones)
        {
            Gizmos.color = zone.isActive ? activeZoneColor : inactiveZoneColor;
            Gizmos.DrawWireSphere(zone.center, zone.radius);
            
            // Draw zone name
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(zone.center + Vector3.up * zone.radius, zone.name);
            #endif
        }
    }
    
    private void OnDestroy()
    {
        if (oriientManager)
        {
            oriientManager.OnPositionUpdated -= CheckGeofences;
        }
        
        // Clean up active ads
        foreach (var adObject in activeAds.Values)
        {
            if (adObject)
            {
                Destroy(adObject);
            }
        }
        activeAds.Clear();
    }
} 