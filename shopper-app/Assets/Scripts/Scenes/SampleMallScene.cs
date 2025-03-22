using UnityEngine;
using System.Collections.Generic;

public class SampleMallScene : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private Transform mallContainer;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject storePrefab;
    
    [Header("Store Points")]
    [SerializeField] private List<StorePoint> storePoints = new List<StorePoint>();
    
    [Header("Scene Settings")]
    [SerializeField] private float floorSize = 50f;
    [SerializeField] private float wallHeight = 3f;
    [SerializeField] private float corridorWidth = 5f;
    [SerializeField] private bool generateAtStart = true;
    
    private Dictionary<string, GameObject> storeObjects = new Dictionary<string, GameObject>();
    
    [System.Serializable]
    public class StorePoint
    {
        public string id;
        public string name;
        public string category;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 size = new Vector3(10, 3, 10);
        public Color color = Color.white;
    }
    
    private void Start()
    {
        if (generateAtStart)
        {
            GenerateSampleMall();
        }
    }
    
    public void GenerateSampleMall()
    {
        ClearExistingMall();
        
        if (mallContainer == null)
        {
            GameObject container = new GameObject("Mall Container");
            mallContainer = container.transform;
        }
        
        // Create floor
        CreateFloor();
        
        // Create sample stores if none defined
        if (storePoints.Count == 0)
        {
            CreateDefaultStorePoints();
        }
        
        // Create store objects
        CreateStores();
        
        // Create corridors or walls between stores
        CreateCorridors();
    }
    
    private void ClearExistingMall()
    {
        if (mallContainer != null)
        {
            foreach (Transform child in mallContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        storeObjects.Clear();
    }
    
    private void CreateFloor()
    {
        if (floorPrefab != null)
        {
            GameObject floor = Instantiate(floorPrefab, mallContainer);
            floor.name = "Mall Floor";
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(floorSize, 0.1f, floorSize);
        }
        else
        {
            // Create a simple floor if no prefab is provided
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Mall Floor";
            floor.transform.parent = mallContainer;
            floor.transform.localPosition = new Vector3(0, -0.05f, 0);
            floor.transform.localScale = new Vector3(floorSize, 0.1f, floorSize);
            
            // Set material
            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.9f, 0.9f, 0.9f);
            }
        }
    }
    
    private void CreateDefaultStorePoints()
    {
        // Create a sample mall layout with stores
        
        // Fashion stores on north wing
        storePoints.Add(new StorePoint
        {
            id = "store1",
            name = "Fashion Boutique",
            category = "Fashion",
            position = new Vector3(-15, 0, 20),
            rotation = new Vector3(0, 180, 0),
            color = new Color(0.8f, 0.2f, 0.8f)
        });
        
        storePoints.Add(new StorePoint
        {
            id = "store4",
            name = "Sportswear Elite",
            category = "Fashion",
            position = new Vector3(15, 0, 20),
            rotation = new Vector3(0, 180, 0),
            color = new Color(0.2f, 0.6f, 0.8f)
        });
        
        // Food court on east wing
        storePoints.Add(new StorePoint
        {
            id = "store2",
            name = "Gourmet Café",
            category = "Food",
            position = new Vector3(20, 0, 5),
            rotation = new Vector3(0, -90, 0),
            color = new Color(0.8f, 0.6f, 0.2f)
        });
        
        storePoints.Add(new StorePoint
        {
            id = "store5",
            name = "Burger Junction",
            category = "Food",
            position = new Vector3(20, 0, -15),
            rotation = new Vector3(0, -90, 0),
            color = new Color(0.8f, 0.4f, 0.2f)
        });
        
        // Tech store on west wing
        storePoints.Add(new StorePoint
        {
            id = "store3",
            name = "Tech Haven",
            category = "Electronics",
            position = new Vector3(-20, 0, -10),
            rotation = new Vector3(0, 90, 0),
            color = new Color(0.3f, 0.8f, 0.3f)
        });
    }
    
    private void CreateStores()
    {
        foreach (StorePoint storePoint in storePoints)
        {
            GameObject storeObject;
            
            if (storePrefab != null)
            {
                storeObject = Instantiate(storePrefab, mallContainer);
            }
            else
            {
                // Create a simple store if no prefab is provided
                storeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                storeObject.transform.parent = mallContainer;
            }
            
            storeObject.name = $"Store_{storePoint.name}";
            storeObject.transform.localPosition = storePoint.position;
            storeObject.transform.localEulerAngles = storePoint.rotation;
            storeObject.transform.localScale = storePoint.size;
            
            // Set material color
            Renderer renderer = storeObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = storePoint.color;
            }
            
            // Add store name text
            GameObject textObject = new GameObject("StoreName");
            textObject.transform.parent = storeObject.transform;
            textObject.transform.localPosition = new Vector3(0, storePoint.size.y / 2 + 0.5f, 0);
            textObject.transform.localEulerAngles = new Vector3(0, 180, 0);
            
            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = storePoint.name;
            textMesh.fontSize = 50;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.color = Color.black;
            textMesh.characterSize = 0.1f;
            
            // Store reference
            storeObjects[storePoint.id] = storeObject;
        }
    }
    
    private void CreateCorridors()
    {
        // For a simple mall, we'll just create some walls to define corridors
        if (wallPrefab == null)
        {
            return;
        }
        
        // Example: North-South corridor
        CreateWall(new Vector3(0, 0, 0), new Vector3(corridorWidth, wallHeight, floorSize * 0.8f));
        
        // Example: East-West corridor
        CreateWall(new Vector3(0, 0, 0), new Vector3(floorSize * 0.8f, wallHeight, corridorWidth));
    }
    
    private void CreateWall(Vector3 position, Vector3 size)
    {
        if (wallPrefab != null)
        {
            GameObject wallObject = Instantiate(wallPrefab, mallContainer);
            wallObject.name = "Corridor";
            wallObject.transform.localPosition = position;
            wallObject.transform.localScale = size;
        }
    }
    
    // Get a store GameObject by ID
    public GameObject GetStoreById(string storeId)
    {
        if (storeObjects.TryGetValue(storeId, out GameObject storeObject))
        {
            return storeObject;
        }
        
        return null;
    }
    
    // Get the position of a store by ID
    public Vector3 GetStorePosition(string storeId)
    {
        GameObject store = GetStoreById(storeId);
        if (store != null)
        {
            return store.transform.position;
        }
        
        // Try to find the store point if object doesn't exist
        StorePoint storePoint = storePoints.Find(sp => sp.id == storeId);
        if (storePoint != null)
        {
            return storePoint.position;
        }
        
        return Vector3.zero;
    }
    
    // Get all store IDs
    public List<string> GetAllStoreIds()
    {
        List<string> ids = new List<string>();
        foreach (StorePoint store in storePoints)
        {
            ids.Add(store.id);
        }
        return ids;
    }
    
    // Get all StoreData for UI population
    public List<StoreData> GetAllStoreData()
    {
        List<StoreData> stores = new List<StoreData>();
        
        foreach (StorePoint storePoint in storePoints)
        {
            StoreData storeData = new StoreData(
                storePoint.id,
                storePoint.name,
                storePoint.category,
                storePoint.position
            );
            
            // Add some fake distance (would normally be calculated from user position)
            storeData.distance = Random.Range(20f, 200f);
            
            stores.Add(storeData);
        }
        
        return stores;
    }
} 