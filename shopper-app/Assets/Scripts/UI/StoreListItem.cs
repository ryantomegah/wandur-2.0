using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StoreListItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI storeNameText;
    [SerializeField] private TextMeshProUGUI storeCategoryText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private Image storeImage;
    [SerializeField] private Button itemButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject openStatusIndicator;
    [SerializeField] private TextMeshProUGUI openStatusText;
    [SerializeField] private GameObject favoriteIcon;
    
    [Header("Gamification Elements")]
    [SerializeField] private GameObject loyaltyBadge;
    [SerializeField] private TextMeshProUGUI loyaltyPointsText;
    [SerializeField] private GameObject achievementIcon;
    [SerializeField] private GameObject firstVisitBadge;
    
    [Header("Ad Elements")]
    [SerializeField] private GameObject adBanner;
    [SerializeField] private TextMeshProUGUI adText;
    [SerializeField] private Image adIcon;
    [SerializeField] private Button adClaimButton;
    
    [Header("Visual Settings")]
    [SerializeField] private Color openColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color closedColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color selectedColor = new Color(0.9f, 0.9f, 1.0f);
    [SerializeField] private Color normalColor = new Color(1.0f, 1.0f, 1.0f);
    [SerializeField] private Color adBackgroundColor = new Color(1.0f, 0.9f, 0.2f, 1.0f);
    [SerializeField] private Color loyaltyColor = new Color(0.4f, 0.4f, 1.0f, 1.0f);
    
    // Store data
    private StoreData storeData;
    private bool isSelected = false;
    private bool hasActiveAd = false;
    
    // Event for click handling
    public event Action OnItemClicked;
    public event Action OnAdClaimClicked;
    
    private void Awake()
    {
        // Add click listener
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(HandleClick);
        }
        
        // Add ad claim button listener
        if (adClaimButton != null)
        {
            adClaimButton.onClick.AddListener(HandleAdClaimClick);
        }
    }
    
    public void SetStoreData(StoreData data)
    {
        storeData = data;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (storeData == null) return;
        
        // Update text elements
        if (storeNameText != null) storeNameText.text = storeData.name;
        if (storeCategoryText != null) storeCategoryText.text = storeData.category;
        
        // Update distance text
        if (distanceText != null)
        {
            if (storeData.distance >= 1000f)
            {
                distanceText.text = $"{(storeData.distance / 1000f):F1} km";
            }
            else
            {
                distanceText.text = $"{Mathf.RoundToInt(storeData.distance)} m";
            }
        }
        
        // Update store image
        if (storeImage != null)
        {
            Sprite storeSprite = Resources.Load<Sprite>(storeData.imagePath);
            if (storeSprite != null)
            {
                storeImage.sprite = storeSprite;
                storeImage.preserveAspect = true;
            }
        }
        
        // Update open status
        if (openStatusIndicator != null && openStatusText != null)
        {
            bool isOpen = storeData.IsOpen();
            openStatusIndicator.SetActive(true);
            openStatusText.text = isOpen ? "OPEN" : "CLOSED";
            openStatusText.color = isOpen ? openColor : closedColor;
        }
        
        // Update favorite icon
        if (favoriteIcon != null)
        {
            favoriteIcon.SetActive(storeData.isFavorite);
        }
        
        // Update loyalty points (gamification element)
        UpdateGamificationElements();
        
        // Update ad banner if available
        UpdateAdBanner();
    }
    
    private void UpdateGamificationElements()
    {
        // Loyalty badge (simulated data for now)
        if (loyaltyBadge != null && loyaltyPointsText != null)
        {
            // Determine if we should show loyalty points
            bool showLoyaltyBadge = UnityEngine.Random.value > 0.3f; // 70% chance to show for demo
            loyaltyBadge.SetActive(showLoyaltyBadge);
            
            if (showLoyaltyBadge)
            {
                int points = UnityEngine.Random.Range(50, 200);
                loyaltyPointsText.text = $"+{points} pts";
            }
        }
        
        // First visit badge (would be based on user data)
        if (firstVisitBadge != null)
        {
            bool isFirstTimeVisit = UnityEngine.Random.value > 0.7f; // 30% chance for demo
            firstVisitBadge.SetActive(isFirstTimeVisit);
        }
        
        // Achievement icon (would be based on user achievements)
        if (achievementIcon != null)
        {
            bool hasAchievement = UnityEngine.Random.value > 0.8f; // 20% chance for demo
            achievementIcon.SetActive(hasAchievement);
        }
    }
    
    private void UpdateAdBanner()
    {
        if (adBanner != null && adText != null)
        {
            // Determine if we should show an ad (would come from geofencing system)
            hasActiveAd = UnityEngine.Random.value > 0.7f; // 30% chance to show ad for demo
            adBanner.SetActive(hasActiveAd);
            
            if (hasActiveAd)
            {
                // Example ad texts
                string[] adTexts = new string[] {
                    "20% OFF Today!",
                    "Buy 1 Get 1 Free",
                    "Flash Sale Now!",
                    "Limited Time Offer"
                };
                
                adText.text = adTexts[UnityEngine.Random.Range(0, adTexts.Length)];
            }
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }
    
    private void HandleClick()
    {
        SetSelected(true);
        OnItemClicked?.Invoke();
    }
    
    private void HandleAdClaimClick()
    {
        Debug.Log($"Ad claimed for store: {storeData.name}");
        OnAdClaimClicked?.Invoke();
        
        // Could track metrics here
        if (adBanner != null)
        {
            // Potentially update UI to show claimed state
            if (adText != null)
            {
                adText.text = "Claimed!";
            }
        }
    }
    
    // Method to directly set an ad on this item
    public void SetPromotion(string promoText, bool active = true)
    {
        if (adBanner != null && adText != null)
        {
            hasActiveAd = active;
            adBanner.SetActive(active);
            
            if (active && !string.IsNullOrEmpty(promoText))
            {
                adText.text = promoText;
            }
        }
    }
    
    // Method to set loyalty points directly
    public void SetLoyaltyPoints(int points, bool active = true)
    {
        if (loyaltyBadge != null && loyaltyPointsText != null)
        {
            loyaltyBadge.SetActive(active);
            
            if (active)
            {
                loyaltyPointsText.text = $"+{points} pts";
            }
        }
    }
    
    private void OnDestroy()
    {
        // Remove click listener
        if (itemButton != null)
        {
            itemButton.onClick.RemoveListener(HandleClick);
        }
        
        // Remove ad claim button listener
        if (adClaimButton != null)
        {
            adClaimButton.onClick.RemoveListener(HandleAdClaimClick);
        }
    }
} 