using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Handles all UI animations for a smoother user experience
/// </summary>
public class UIAnimationController : MonoBehaviour
{
    [Header("Screen Transitions")]
    [SerializeField] private float screenTransitionDuration = 0.35f;
    [SerializeField] private AnimationCurve screenInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve screenOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Card Animations")]
    [SerializeField] private float cardAnimationDuration = 0.25f;
    [SerializeField] private float cardHoverScale = 1.05f;
    [SerializeField] private float cardSelectScale = 1.1f;
    [SerializeField] private float cardPressScale = 0.95f;
    
    [Header("Badge Animations")]
    [SerializeField] private float badgePulseDuration = 1.0f;
    [SerializeField] private float badgePulseScale = 1.15f;
    [SerializeField] private AnimationCurve badgePulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Notification Animations")]
    [SerializeField] private float notificationEnterDuration = 0.5f;
    [SerializeField] private float notificationExitDuration = 0.3f;
    [SerializeField] private Vector2 notificationStartOffset = new Vector2(0, 100);
    
    // Cached references to UI elements
    private Dictionary<GameObject, CanvasGroup> screenCanvasGroups = new Dictionary<GameObject, CanvasGroup>();
    private GameObject currentScreen = null;
    
    // Active coroutines
    private Dictionary<GameObject, Coroutine> activeAnimations = new Dictionary<GameObject, Coroutine>();
    
    private void Awake()
    {
        // Find all screens in the canvas
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                CanvasGroup canvasGroup = child.gameObject.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
                }
                screenCanvasGroups[child.gameObject] = canvasGroup;
                
                // Hide all screens initially except for the first one
                if (currentScreen == null)
                {
                    currentScreen = child.gameObject;
                    canvasGroup.alpha = 1;
                }
                else
                {
                    child.gameObject.SetActive(false);
                    canvasGroup.alpha = 0;
                }
            }
        }
    }
    
    /// <summary>
    /// Animate transition between screens
    /// </summary>
    public void TransitionToScreen(GameObject newScreen)
    {
        if (newScreen == currentScreen || !screenCanvasGroups.ContainsKey(newScreen))
            return;
        
        // Make sure the new screen is active but transparent
        newScreen.SetActive(true);
        CanvasGroup newCanvasGroup = screenCanvasGroups[newScreen];
        newCanvasGroup.alpha = 0;
        
        // Start transition animations
        if (currentScreen != null && screenCanvasGroups.ContainsKey(currentScreen))
        {
            CanvasGroup currentCanvasGroup = screenCanvasGroups[currentScreen];
            
            // Cancel any ongoing animations
            if (activeAnimations.ContainsKey(currentScreen))
            {
                StopCoroutine(activeAnimations[currentScreen]);
                activeAnimations.Remove(currentScreen);
            }
            
            if (activeAnimations.ContainsKey(newScreen))
            {
                StopCoroutine(activeAnimations[newScreen]);
                activeAnimations.Remove(newScreen);
            }
            
            // Start new animations
            activeAnimations[currentScreen] = StartCoroutine(AnimateCanvasGroup(currentCanvasGroup, 1, 0, screenTransitionDuration, screenOutCurve, 
                () => { currentScreen.SetActive(false); }));
        }
        
        activeAnimations[newScreen] = StartCoroutine(AnimateCanvasGroup(newCanvasGroup, 0, 1, screenTransitionDuration, screenInCurve));
        
        // Update current screen reference
        currentScreen = newScreen;
    }
    
    /// <summary>
    /// Animate a store card on hover
    /// </summary>
    public void AnimateCardHover(RectTransform cardRect, bool isHovering)
    {
        if (cardRect == null) return;
        
        // Cancel any existing animation
        if (activeAnimations.ContainsKey(cardRect.gameObject))
        {
            StopCoroutine(activeAnimations[cardRect.gameObject]);
            activeAnimations.Remove(cardRect.gameObject);
        }
        
        // Start new animation
        float targetScale = isHovering ? cardHoverScale : 1.0f;
        activeAnimations[cardRect.gameObject] = StartCoroutine(AnimateScale(cardRect, targetScale, cardAnimationDuration));
    }
    
    /// <summary>
    /// Animate a store card on selection
    /// </summary>
    public void AnimateCardSelect(RectTransform cardRect, bool isSelected)
    {
        if (cardRect == null) return;
        
        // Cancel any existing animation
        if (activeAnimations.ContainsKey(cardRect.gameObject))
        {
            StopCoroutine(activeAnimations[cardRect.gameObject]);
            activeAnimations.Remove(cardRect.gameObject);
        }
        
        // Start new animation
        float targetScale = isSelected ? cardSelectScale : 1.0f;
        activeAnimations[cardRect.gameObject] = StartCoroutine(AnimateScale(cardRect, targetScale, cardAnimationDuration));
    }
    
    /// <summary>
    /// Animate a store card on press
    /// </summary>
    public void AnimateCardPress(RectTransform cardRect, bool isPressed)
    {
        if (cardRect == null) return;
        
        // Cancel any existing animation
        if (activeAnimations.ContainsKey(cardRect.gameObject))
        {
            StopCoroutine(activeAnimations[cardRect.gameObject]);
            activeAnimations.Remove(cardRect.gameObject);
        }
        
        // Start new animation
        float targetScale = isPressed ? cardPressScale : 1.0f;
        activeAnimations[cardRect.gameObject] = StartCoroutine(AnimateScale(cardRect, targetScale, cardAnimationDuration * 0.5f));
    }
    
    /// <summary>
    /// Animate a badge pulsing effect
    /// </summary>
    public void AnimateBadgePulse(RectTransform badgeRect, int pulseCount = 2)
    {
        if (badgeRect == null) return;
        
        // Cancel any existing animation
        if (activeAnimations.ContainsKey(badgeRect.gameObject))
        {
            StopCoroutine(activeAnimations[badgeRect.gameObject]);
            activeAnimations.Remove(badgeRect.gameObject);
        }
        
        // Start new animation
        activeAnimations[badgeRect.gameObject] = StartCoroutine(PulseBadge(badgeRect, pulseCount));
    }
    
    /// <summary>
    /// Show a notification with animation
    /// </summary>
    public void ShowNotification(GameObject notification, float duration = 3.0f)
    {
        if (notification == null) return;
        
        // Cancel any existing animation
        if (activeAnimations.ContainsKey(notification))
        {
            StopCoroutine(activeAnimations[notification]);
            activeAnimations.Remove(notification);
        }
        
        // Ensure notification is active but with zero alpha
        notification.SetActive(true);
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notification.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;
        
        // Get RectTransform
        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Save original position
            Vector2 originalPosition = rectTransform.anchoredPosition;
            Vector2 startPosition = originalPosition + notificationStartOffset;
            rectTransform.anchoredPosition = startPosition;
            
            // Start animation
            activeAnimations[notification] = StartCoroutine(AnimateNotification(notification, canvasGroup, rectTransform, originalPosition, duration));
        }
        else
        {
            // Just fade in and out if no RectTransform
            activeAnimations[notification] = StartCoroutine(FadeNotification(notification, canvasGroup, duration));
        }
    }
    
    // Animation Coroutines
    
    private IEnumerator AnimateCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, 
        AnimationCurve curve = null, System.Action onComplete = null)
    {
        float time = 0;
        canvasGroup.alpha = startAlpha;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            if (curve != null)
                t = curve.Evaluate(t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
        
        // Call completion callback if provided
        onComplete?.Invoke();
    }
    
    private IEnumerator AnimateScale(RectTransform rectTransform, float targetScale, float duration, 
        AnimationCurve curve = null, System.Action onComplete = null)
    {
        float time = 0;
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, 1);
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            if (curve != null)
                t = curve.Evaluate(t);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        
        rectTransform.localScale = endScale;
        
        // Call completion callback if provided
        onComplete?.Invoke();
    }
    
    private IEnumerator PulseBadge(RectTransform badgeRect, int pulseCount)
    {
        Vector3 originalScale = badgeRect.localScale;
        Vector3 pulseScale = originalScale * badgePulseScale;
        
        for (int i = 0; i < pulseCount; i++)
        {
            // Pulse up
            float time = 0;
            while (time < badgePulseDuration * 0.5f)
            {
                time += Time.deltaTime;
                float t = time / (badgePulseDuration * 0.5f);
                t = badgePulseCurve.Evaluate(t);
                badgeRect.localScale = Vector3.Lerp(originalScale, pulseScale, t);
                yield return null;
            }
            
            // Pulse down
            time = 0;
            while (time < badgePulseDuration * 0.5f)
            {
                time += Time.deltaTime;
                float t = time / (badgePulseDuration * 0.5f);
                t = badgePulseCurve.Evaluate(t);
                badgeRect.localScale = Vector3.Lerp(pulseScale, originalScale, t);
                yield return null;
            }
            
            // Wait a bit between pulses if not the last one
            if (i < pulseCount - 1)
                yield return new WaitForSeconds(0.2f);
        }
        
        badgeRect.localScale = originalScale;
    }
    
    private IEnumerator AnimateNotification(GameObject notification, CanvasGroup canvasGroup, 
        RectTransform rectTransform, Vector2 originalPosition, float duration)
    {
        // Record start position
        Vector2 startPosition = rectTransform.anchoredPosition;
        
        // Fade in and move to original position
        float time = 0;
        while (time < notificationEnterDuration)
        {
            time += Time.deltaTime;
            float t = time / notificationEnterDuration;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, originalPosition, t);
            yield return null;
        }
        
        canvasGroup.alpha = 1;
        rectTransform.anchoredPosition = originalPosition;
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Fade out
        time = 0;
        while (time < notificationExitDuration)
        {
            time += Time.deltaTime;
            float t = time / notificationExitDuration;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }
        
        canvasGroup.alpha = 0;
        notification.SetActive(false);
    }
    
    private IEnumerator FadeNotification(GameObject notification, CanvasGroup canvasGroup, float duration)
    {
        // Fade in
        float time = 0;
        while (time < notificationEnterDuration)
        {
            time += Time.deltaTime;
            float t = time / notificationEnterDuration;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
        
        canvasGroup.alpha = 1;
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Fade out
        time = 0;
        while (time < notificationExitDuration)
        {
            time += Time.deltaTime;
            float t = time / notificationExitDuration;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }
        
        canvasGroup.alpha = 0;
        notification.SetActive(false);
    }
} 