using UnityEngine;
using System;

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
} 