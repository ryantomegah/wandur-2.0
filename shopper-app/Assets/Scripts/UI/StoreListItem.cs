using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class StoreListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
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
    [SerializeField] private Sprite defaultStoreImage;
    
    [Header("Animation")]
    [SerializeField] private bool useAnimations = true;
    [SerializeField] private float loadInDelay = 0.05f;
    [SerializeField] private float animationDuration = 0.2f;
    
    // Store data
    private StoreData storeData;
    private bool isSelected = false;
    private bool hasActiveAd = false;
    private RectTransform rectTransform;
    private UIAnimationController animationController;
    private CanvasGroup canvasGroup;
    
    // Event for click handling
    public event Action OnItemClicked;
    public event Action OnAdClaimClicked;
    
    private void Awake()
    {
        // Get references
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Find animation controller
        animationController = FindObjectOfType<UIAnimationController>();
        
        // Add click listeners
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(HandleClick);
        }
        
        if (adClaimButton != null)
        {
            adClaimButton.onClick.AddListener(HandleAdClaimClick);
        }
        
        // Initialize animations
        if (useAnimations)
        {
            // Start invisible
            canvasGroup.alpha = 0;
            // Slightly scaled down
            rectTransform.localScale = new Vector3(0.95f, 0.95f, 1f);
            // Animate in after delay based on hierarchy index
            int siblingIndex = transform.GetSiblingIndex();
            float delay = loadInDelay * siblingIndex;
            StartCoroutine(AnimateIn(delay));
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
            }
            else if (defaultStoreImage != null)
            {
                storeImage.sprite = defaultStoreImage;
            }
            storeImage.preserveAspect = true;
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
        // Loyalty badge
        if (loyaltyBadge != null && loyaltyPointsText != null)
        {
            bool showLoyaltyBadge = storeData.availableLoyaltyPoints > 0;
            loyaltyBadge.SetActive(showLoyaltyBadge);
            
            if (showLoyaltyBadge)
            {
                loyaltyPointsText.text = $"+{storeData.availableLoyaltyPoints} pts";
                
                // Animate loyalty badge if it's significant
                if (useAnimations && animationController != null && storeData.availableLoyaltyPoints > 100)
                {
                    RectTransform badgeRect = loyaltyBadge.GetComponent<RectTransform>();
                    if (badgeRect != null)
                    {
                        animationController.AnimateBadgePulse(badgeRect);
                    }
                }
            }
        }
        
        // First visit badge
        if (firstVisitBadge != null)
        {
            bool isFirstTimeVisit = !storeData.hasVisited && storeData.visitCount == 0;
            firstVisitBadge.SetActive(isFirstTimeVisit);
            
            // Animate first visit badge
            if (useAnimations && animationController != null && isFirstTimeVisit)
            {
                RectTransform badgeRect = firstVisitBadge.GetComponent<RectTransform>();
                if (badgeRect != null)
                {
                    animationController.AnimateBadgePulse(badgeRect);
                }
            }
        }
        
        // Achievement icon (if store has achievements)
        if (achievementIcon != null)
        {
            bool hasAchievement = storeData.achievements != null && storeData.achievements.Length > 0;
            achievementIcon.SetActive(hasAchievement);
        }
    }
    
    private void UpdateAdBanner()
    {
        if (adBanner != null && adText != null)
        {
            // Show ad banner if store has active promotion
            hasActiveAd = storeData.HasActivePromotion();
            adBanner.SetActive(hasActiveAd);
            
            if (hasActiveAd && storeData.activePromotion != null)
            {
                adText.text = storeData.activePromotion.title;
                
                // Animate ad banner
                if (useAnimations && animationController != null)
                {
                    RectTransform bannerRect = adBanner.GetComponent<RectTransform>();
                    if (bannerRect != null)
                    {
                        animationController.AnimateBadgePulse(bannerRect, 1);
                    }
                }
            }
        }
    }
    
    public void SetSelected(bool selected)
    {
        if (selected == isSelected) return;
        
        isSelected = selected;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
        
        // Animate selection if animation controller is available
        if (useAnimations && animationController != null)
        {
            animationController.AnimateCardSelect(rectTransform, isSelected);
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
        
        // Animate ad claim
        if (adBanner != null && adText != null)
        {
            adText.text = "Claimed!";
            
            if (useAnimations && animationController != null)
            {
                RectTransform bannerRect = adBanner.GetComponent<RectTransform>();
                if (bannerRect != null)
                {
                    animationController.AnimateBadgePulse(bannerRect, 2);
                }
            }
        }
    }
    
    // IPointerEnterHandler implementation
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected && useAnimations && animationController != null)
        {
            animationController.AnimateCardHover(rectTransform, true);
        }
    }
    
    // IPointerExitHandler implementation
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected && useAnimations && animationController != null)
        {
            animationController.AnimateCardHover(rectTransform, false);
        }
    }
    
    // IPointerDownHandler implementation
    public void OnPointerDown(PointerEventData eventData)
    {
        if (useAnimations && animationController != null)
        {
            animationController.AnimateCardPress(rectTransform, true);
        }
    }
    
    // IPointerUpHandler implementation
    public void OnPointerUp(PointerEventData eventData)
    {
        if (useAnimations && animationController != null)
        {
            animationController.AnimateCardPress(rectTransform, false);
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
    
    // Animation coroutine for card appearance
    private System.Collections.IEnumerator AnimateIn(float delay)
    {
        // Wait for delay
        yield return new WaitForSeconds(delay);
        
        // Animate in
        float time = 0;
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = time / animationDuration;
            
            // Ease in quad
            float easeT = t * t;
            
            canvasGroup.alpha = easeT;
            rectTransform.localScale = Vector3.Lerp(
                new Vector3(0.95f, 0.95f, 1f),
                Vector3.one,
                easeT
            );
            
            yield return null;
        }
        
        // Ensure final state
        canvasGroup.alpha = 1;
        rectTransform.localScale = Vector3.one;
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