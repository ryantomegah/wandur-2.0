using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class StoreData
{
    public string id;
    public string name;
    public string description;
    public string category;
    public string location;
    public float distance;
    public string imagePath;
    public Vector3 position;
    public string[] tags;
    public bool isFavorite;
    public float rating;
    public DateTime openTime;
    public DateTime closeTime;
    
    // Gamification elements
    public int availableLoyaltyPoints;
    public bool hasVisited;
    public int visitCount;
    public string[] achievements;
    
    // Promotion elements
    public StorePromotion activePromotion;
    public List<StorePromotion> availablePromotions;
    
    // Constructor with essential fields
    public StoreData(string id, string name, string category, Vector3 position)
    {
        this.id = id;
        this.name = name;
        this.category = category;
        this.position = position;
        this.description = "";
        this.location = "";
        this.distance = 0f;
        this.imagePath = "";
        this.tags = new string[0];
        this.isFavorite = false;
        this.rating = 0f;
        this.openTime = DateTime.Today.AddHours(9); // Default 9 AM
        this.closeTime = DateTime.Today.AddHours(21); // Default 9 PM
        
        // Initialize gamification elements
        this.availableLoyaltyPoints = 0;
        this.hasVisited = false;
        this.visitCount = 0;
        this.achievements = new string[0];
        
        // Initialize promotion elements
        this.activePromotion = null;
        this.availablePromotions = new List<StorePromotion>();
    }
    
    // Empty constructor for initialization
    public StoreData() { }
    
    // Check if the store is currently open
    public bool IsOpen()
    {
        DateTime now = DateTime.Now;
        TimeSpan currentTime = new TimeSpan(now.Hour, now.Minute, now.Second);
        TimeSpan openTimeSpan = new TimeSpan(openTime.Hour, openTime.Minute, openTime.Second);
        TimeSpan closeTimeSpan = new TimeSpan(closeTime.Hour, closeTime.Minute, closeTime.Second);
        
        return currentTime >= openTimeSpan && currentTime <= closeTimeSpan;
    }
    
    // Get a formatted time string for display
    public string GetOpenHoursString()
    {
        return $"{openTime.ToString("h:mm tt")} - {closeTime.ToString("h:mm tt")}";
    }
    
    // Add a promotion to the store
    public void AddPromotion(StorePromotion promotion)
    {
        if (promotion != null)
        {
            availablePromotions.Add(promotion);
            
            // Automatically set as active if there's no active promotion
            if (activePromotion == null)
            {
                activePromotion = promotion;
            }
        }
    }
    
    // Check if the store has an active promotion
    public bool HasActivePromotion()
    {
        return activePromotion != null && activePromotion.isActive && DateTime.Now < activePromotion.expirationDate;
    }
    
    // Update active promotion based on current time
    public void UpdateActivePromotion()
    {
        // First check if current active promotion is expired
        if (activePromotion != null && DateTime.Now >= activePromotion.expirationDate)
        {
            activePromotion = null;
        }
        
        // If no active promotion, find a new one
        if (activePromotion == null)
        {
            foreach (var promo in availablePromotions)
            {
                if (promo.isActive && DateTime.Now < promo.expirationDate)
                {
                    activePromotion = promo;
                    break;
                }
            }
        }
    }
    
    // Generate random loyalty points (for demo/testing only)
    public int GenerateRandomLoyaltyPoints()
    {
        availableLoyaltyPoints = UnityEngine.Random.Range(50, 500);
        return availableLoyaltyPoints;
    }
}

[System.Serializable]
public class StorePromotion
{
    public string id;
    public string title;
    public string description;
    public PromotionType type;
    public float discountAmount; // in percent or absolute value depending on type
    public string couponCode;
    public DateTime startDate;
    public DateTime expirationDate;
    public bool requiresCheckIn;
    public bool isActive;
    public string[] targetUserSegments;
    
    public enum PromotionType
    {
        PercentDiscount,
        FlatDiscount,
        BuyOneGetOne,
        FreeGift,
        LoyaltyPoints,
        Custom
    }
    
    // Constructor
    public StorePromotion(string title, PromotionType type, float discountAmount, int durationInHours)
    {
        this.id = System.Guid.NewGuid().ToString();
        this.title = title;
        this.type = type;
        this.discountAmount = discountAmount;
        this.startDate = DateTime.Now;
        this.expirationDate = DateTime.Now.AddHours(durationInHours);
        this.isActive = true;
        this.requiresCheckIn = false;
        this.targetUserSegments = new string[0];
    }
} 