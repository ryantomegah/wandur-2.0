using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class GeofencedAd : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Canvas adCanvas;
    [SerializeField] private RectTransform adPanel;
    [SerializeField] private Image adImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button actionButton;
    [SerializeField] private Button closeButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float appearDuration = 0.5f;
    [SerializeField] private float disappearDuration = 0.3f;
    [SerializeField] private AnimationCurve appearCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 offsetPosition = new Vector3(0, 1.5f, 0);
    
    [Header("Billboard Settings")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private float rotationSpeed = 5f;
    
    // Events
    public event Action OnAdClosed;
    public event Action OnAdClicked;
    
    private Camera mainCamera;
    private CanvasGroup canvasGroup;
    private Vector3 targetPosition;
    private bool isAnimating;
    private float animationTime;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        canvasGroup = adCanvas.GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = adCanvas.gameObject.AddComponent<CanvasGroup>();
        
        // Initialize UI
        if (closeButton)
        {
            closeButton.onClick.AddListener(CloseAd);
        }
        
        if (actionButton)
        {
            actionButton.onClick.AddListener(HandleAdClick);
        }
        
        // Set initial state
        targetPosition = transform.position + offsetPosition;
        canvasGroup.alpha = 0f;
        
        // Ensure the ad faces the camera initially
        if (faceCamera && mainCamera)
        {
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            directionToCamera.y = 0f;
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }
    
    private void Update()
    {
        // Handle billboard effect
        if (faceCamera && mainCamera)
        {
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            directionToCamera.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Handle animation
        if (isAnimating)
        {
            animationTime += Time.deltaTime;
            float progress = animationTime / (canvasGroup.alpha < 1f ? appearDuration : disappearDuration);
            
            if (progress >= 1f)
            {
                isAnimating = false;
                progress = 1f;
                
                // If we were disappearing, destroy the ad
                if (canvasGroup.alpha < 1f)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            
            float curveValue = appearCurve.Evaluate(progress);
            
            if (canvasGroup.alpha < 1f)
            {
                // Appearing
                canvasGroup.alpha = curveValue;
                transform.position = Vector3.Lerp(transform.position, targetPosition, curveValue);
            }
            else
            {
                // Disappearing
                canvasGroup.alpha = 1f - curveValue;
            }
        }
    }
    
    public void SetAdContent(string title, string description, Sprite adSprite, string buttonText = "Learn More")
    {
        if (titleText) titleText.text = title;
        if (descriptionText) descriptionText.text = description;
        if (adImage) adImage.sprite = adSprite;
        if (actionButton && actionButton.GetComponentInChildren<TextMeshProUGUI>())
        {
            actionButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
        }
        
        // Start appear animation
        isAnimating = true;
        animationTime = 0f;
    }
    
    public void CloseAd()
    {
        // Start disappear animation
        isAnimating = true;
        animationTime = 0f;
        OnAdClosed?.Invoke();
    }
    
    private void HandleAdClick()
    {
        OnAdClicked?.Invoke();
    }
    
    private void OnDestroy()
    {
        // Clean up button listeners
        if (closeButton)
        {
            closeButton.onClick.RemoveListener(CloseAd);
        }
        
        if (actionButton)
        {
            actionButton.onClick.RemoveListener(HandleAdClick);
        }
    }
} 