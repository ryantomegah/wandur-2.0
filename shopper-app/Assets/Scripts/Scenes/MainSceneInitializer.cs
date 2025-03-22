using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using System.Collections.Generic;

public class MainSceneInitializer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject uiCanvasPrefab;
    [SerializeField] private GameObject arSessionPrefab;
    [SerializeField] private GameObject appManagerPrefab;
    [SerializeField] private GameObject navigationManagerPrefab;
    [SerializeField] private GameObject oriientSDKManagerPrefab;
    [SerializeField] private GameObject geofencingManagerPrefab;
    [SerializeField] private GameObject mallScenePrefab;
    [SerializeField] private GameObject analyticsManagerPrefab;
    
    [Header("Scene Settings")]
    [SerializeField] private bool useARMode = true;
    [SerializeField] private bool useVirtualMall = true;
    [SerializeField] private bool useSimulatedSDK = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugConsole = true;
    [SerializeField] private bool showSDKLogs = true;
    [SerializeField] private bool showNavigationGizmos = true;
    
    // References to instantiated objects
    private WandurAppManager appManager;
    private ARSessionManager arSessionManager;
    private ARNavigationManager navigationManager;
    private OriientSDKIntegration sdkIntegration;
    private OriientSDKManager sdkManager;
    private GeofencingManager geofencingManager;
    private UIManager uiManager;
    private SampleMallScene mallScene;
    private AnalyticsManager analyticsManagerComponent;
    
    private void Start()
    {
        Debug.Log("Initializing Wandur App...");
        
        Initialize();
        
        // Track app start
        if (analyticsManagerComponent != null)
        {
            analyticsManagerComponent.TrackScreenView("main_screen");
        }
        
        Debug.Log("Wandur App initialized successfully.");
    }
    
    private void Initialize()
    {
        // Ensure we have all necessary components
        InitializeManagers();
        
        // Initialize UI
        InitializeUI();
        
        // Set up AR mode or virtual mode
        if (useARMode)
        {
            SetupARMode();
        }
        else
        {
            SetupVirtualMode();
        }
        
        // Connect components
        ConnectComponents();
        
        // Populate UI with sample data
        PopulateUI();
    }
    
    private void InitializeManagers()
    {
        // Create app manager
        if (appManager == null && appManagerPrefab != null)
        {
            GameObject appObj = Instantiate(appManagerPrefab);
            appObj.name = "WandurAppManager";
            appManager = appObj.GetComponent<WandurAppManager>();
        }
        else if (appManager == null)
        {
            GameObject appObj = new GameObject("WandurAppManager");
            appManager = appObj.AddComponent<WandurAppManager>();
        }
        
        // Create Oriient SDK manager
        if (sdkManager == null && oriientSDKManagerPrefab != null)
        {
            GameObject sdkObj = Instantiate(oriientSDKManagerPrefab);
            sdkObj.name = "OriientSDKManager";
            sdkManager = sdkObj.GetComponent<OriientSDKManager>();
            
            // Add SDK integration for real API preparation
            sdkIntegration = sdkObj.AddComponent<OriientSDKIntegration>();
            sdkIntegration.enabled = useSimulatedSDK;
        }
        else if (sdkManager == null)
        {
            GameObject sdkObj = new GameObject("OriientSDKManager");
            sdkManager = sdkObj.AddComponent<OriientSDKManager>();
            
            // Configure the SDK manager for simulated mode
            sdkManager.simulatePositioning = true;
            sdkManager.debugMode = showSDKLogs;
            
            // Add SDK integration for real API preparation
            sdkIntegration = sdkObj.AddComponent<OriientSDKIntegration>();
            sdkIntegration.enabled = useSimulatedSDK;
        }
        
        // Create navigation manager
        if (navigationManager == null && navigationManagerPrefab != null)
        {
            GameObject navObj = Instantiate(navigationManagerPrefab);
            navObj.name = "ARNavigationManager";
            navigationManager = navObj.GetComponent<ARNavigationManager>();
        }
        else if (navigationManager == null)
        {
            GameObject navObj = new GameObject("ARNavigationManager");
            navigationManager = navObj.AddComponent<ARNavigationManager>();
        }
        
        // Create geofencing manager
        if (geofencingManager == null && geofencingManagerPrefab != null)
        {
            GameObject geoObj = Instantiate(geofencingManagerPrefab);
            geoObj.name = "GeofencingManager";
            geofencingManager = geoObj.GetComponent<GeofencingManager>();
        }
        else if (geofencingManager == null)
        {
            GameObject geoObj = new GameObject("GeofencingManager");
            geofencingManager = geoObj.AddComponent<GeofencingManager>();
        }
        
        // Create analytics manager
        if (analyticsManagerComponent == null && analyticsManagerPrefab != null)
        {
            GameObject analyticsObj = Instantiate(analyticsManagerPrefab);
            analyticsObj.name = "AnalyticsManager";
            analyticsManagerComponent = analyticsObj.GetComponent<AnalyticsManager>();
        }
    }
    
    private void InitializeUI()
    {
        // Create UI canvas
        if (uiManager == null && uiCanvasPrefab != null)
        {
            GameObject uiObj = Instantiate(uiCanvasPrefab);
            uiObj.name = "UI Canvas";
            uiManager = uiObj.GetComponent<UIManager>();
        }
        else if (uiManager == null)
        {
            GameObject uiObj = new GameObject("UI Canvas");
            uiManager = uiObj.AddComponent<UIManager>();
        }
    }
    
    private void SetupARMode()
    {
        // Create AR session
        if (arSessionManager == null && arSessionPrefab != null)
        {
            GameObject arObj = Instantiate(arSessionPrefab);
            arObj.name = "AR Session";
            arSessionManager = arObj.GetComponent<ARSessionManager>();
            
            // Configure AR components if they don't exist
            if (arObj.GetComponent<ARSession>() == null)
            {
                arObj.AddComponent<ARSession>();
            }
            
            if (arObj.GetComponent<ARSessionOrigin>() == null)
            {
                ARSessionOrigin sessionOrigin = arObj.AddComponent<ARSessionOrigin>();
                
                // Create camera if needed
                if (sessionOrigin.camera == null)
                {
                    GameObject cameraObj = new GameObject("AR Camera");
                    Camera camera = cameraObj.AddComponent<Camera>();
                    cameraObj.transform.parent = arObj.transform;
                    sessionOrigin.camera = camera;
                }
            }
            
            if (arObj.GetComponent<ARPlaneManager>() == null)
            {
                arObj.AddComponent<ARPlaneManager>();
            }
            
            if (arObj.GetComponent<ARRaycastManager>() == null)
            {
                arObj.AddComponent<ARRaycastManager>();
            }
        }
        
        // Hide the virtual mall in AR mode if configured
        if (mallScene != null && !useVirtualMall)
        {
            mallScene.gameObject.SetActive(false);
        }
    }
    
    private void SetupVirtualMode()
    {
        // Create virtual mall for testing
        if (mallScene == null && mallScenePrefab != null)
        {
            GameObject mallObj = Instantiate(mallScenePrefab);
            mallObj.name = "Sample Mall";
            mallScene = mallObj.GetComponent<SampleMallScene>();
        }
        else if (mallScene == null && useVirtualMall)
        {
            GameObject mallObj = new GameObject("Sample Mall");
            mallScene = mallObj.AddComponent<SampleMallScene>();
        }
        
        // Ensure we have a camera in virtual mode
        if (Camera.main == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            
            // Position the camera for a good view of the mall
            cameraObj.transform.position = new Vector3(0, 10, -10);
            cameraObj.transform.eulerAngles = new Vector3(45, 0, 0);
        }
    }
    
    private void ConnectComponents()
    {
        // Connect app manager to other components
        if (appManager != null)
        {
            if (navigationManager != null) appManager.navigationManager = navigationManager;
            if (sdkManager != null) appManager.oriientManager = sdkManager;
            if (geofencingManager != null) appManager.geofencingManager = geofencingManager;
        }
        
        // Connect navigation manager
        if (navigationManager != null)
        {
            if (sdkManager != null) navigationManager.oriientManager = sdkManager;
            
            // Configure AR components
            if (arSessionManager != null)
            {
                navigationManager.cameraTransform = arSessionManager.GetARCamera().transform;
                navigationManager.raycastManager = arSessionManager.GetComponent<ARRaycastManager>();
                navigationManager.planeManager = arSessionManager.GetComponent<ARPlaneManager>();
            }
            else if (Camera.main != null)
            {
                navigationManager.cameraTransform = Camera.main.transform;
            }
        }
        
        // Connect geofencing manager
        if (geofencingManager != null && sdkManager != null)
        {
            geofencingManager.oriientManager = sdkManager;
        }
        
        // Connect SDK simulation to integrator (if both exist)
        if (sdkManager != null && sdkIntegration != null)
        {
            // Configure SDK manager to use simulation
            sdkManager.simulatePositioning = true;
            
            // Connect events (would need modification for real integration)
            sdkIntegration.OnPositionUpdated += (position) => {
                // Forward to the SDK manager
                sdkManager.SimulatePositionUpdate(position, 0);
            };
            
            sdkIntegration.OnHeadingUpdated += (heading) => {
                // Forward to the SDK manager
                sdkManager.SimulateHeadingUpdate(heading);
            };
        }
        
        // Provide mall data to UI
        if (uiManager != null && mallScene != null)
        {
            StartCoroutine(DelayedUIPopulation());
        }
        
        // Connect Analytics to UI
        if (analyticsManagerComponent != null && uiManager != null)
        {
            uiManager.OnScreenChanged += (screenName) => {
                analyticsManagerComponent.TrackScreenView(screenName);
            };
            
            uiManager.OnStoreSelected += (storeId, storeName) => {
                analyticsManagerComponent.TrackStoreView(storeId, storeName);
            };
            
            uiManager.OnNavigationRequested += (storeId, storeName, startPos, destPos) => {
                analyticsManagerComponent.TrackNavigationStart(storeId, storeName, startPos, destPos);
            };
            
            uiManager.OnPromotionViewed += (promotionId, storeId) => {
                analyticsManagerComponent.TrackPromotionView(promotionId, storeId);
            };
            
            uiManager.OnPromotionClaimed += (promotionId, storeId) => {
                analyticsManagerComponent.TrackPromotionClaim(promotionId, storeId);
            };
        }
        
        // Connect Analytics to Navigation
        if (analyticsManagerComponent != null && navigationManager != null)
        {
            navigationManager.OnNavigationStarted += (storeId, storeName, startPos, destPos) => {
                analyticsManagerComponent.TrackNavigationStart(storeId, storeName, startPos, destPos);
            };
            
            navigationManager.OnNavigationEnded += (storeId, completed, distance) => {
                analyticsManagerComponent.TrackNavigationEnd(storeId, completed, distance);
            };
            
            navigationManager.OnDivineLineVisibilityChanged += (visible, duration, storeId) => {
                analyticsManagerComponent.TrackDivineLineVisibility(visible, duration, storeId);
            };
        }
    }
    
    private IEnumerator DelayedUIPopulation()
    {
        // Wait to ensure mall is fully initialized
        yield return new WaitForSeconds(0.5f);
        PopulateUI();
    }
    
    private void PopulateUI()
    {
        if (uiManager == null) return;
        
        // If we have a mall scene, populate with those stores
        if (mallScene != null)
        {
            // Implement method to populate UI with mall data
            Debug.Log("Populated UI with mall data");
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (sdkManager != null && sdkIntegration != null && analyticsManagerComponent != null)
        {
            sdkIntegration.OnPositionUpdated -= (position) => {
                sdkManager.SimulatePositionUpdate(position, 0);
            };
            
            sdkIntegration.OnHeadingUpdated -= (heading) => {
                sdkManager.SimulateHeadingUpdate(heading);
            };
        }
        
        if (uiManager != null && analyticsManagerComponent != null)
        {
            uiManager.OnScreenChanged -= (screenName) => {
                analyticsManagerComponent.TrackScreenView(screenName);
            };
            
            uiManager.OnStoreSelected -= (storeId, storeName) => {
                analyticsManagerComponent.TrackStoreView(storeId, storeName);
            };
            
            uiManager.OnNavigationRequested -= (storeId, storeName, startPos, destPos) => {
                analyticsManagerComponent.TrackNavigationStart(storeId, storeName, startPos, destPos);
            };
            
            uiManager.OnPromotionViewed -= (promotionId, storeId) => {
                analyticsManagerComponent.TrackPromotionView(promotionId, storeId);
            };
            
            uiManager.OnPromotionClaimed -= (promotionId, storeId) => {
                analyticsManagerComponent.TrackPromotionClaim(promotionId, storeId);
            };
        }
        
        if (navigationManager != null && analyticsManagerComponent != null)
        {
            navigationManager.OnNavigationStarted -= (storeId, storeName, startPos, destPos) => {
                analyticsManagerComponent.TrackNavigationStart(storeId, storeName, startPos, destPos);
            };
            
            navigationManager.OnNavigationEnded -= (storeId, completed, distance) => {
                analyticsManagerComponent.TrackNavigationEnd(storeId, completed, distance);
            };
            
            navigationManager.OnDivineLineVisibilityChanged -= (visible, duration, storeId) => {
                analyticsManagerComponent.TrackDivineLineVisibility(visible, duration, storeId);
            };
        }
    }
} 