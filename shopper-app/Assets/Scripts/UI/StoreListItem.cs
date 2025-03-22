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
    
    [Header("Visual Settings")]
    [SerializeField] private Color openColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color closedColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color selectedColor = new Color(0.9f, 0.9f, 1.0f);
    [SerializeField] private Color normalColor = new Color(1.0f, 1.0f, 1.0f);
    
    // Store data
    private StoreData storeData;
    private bool isSelected = false;
    
    // Event for click handling
    public event Action OnItemClicked;
    
    private void Awake()
    {
        // Add click listener
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(HandleClick);
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
    
    private void OnDestroy()
    {
        // Remove click listener
        if (itemButton != null)
        {
            itemButton.onClick.RemoveListener(HandleClick);
        }
    }
} 