using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class DotweenButton : MonoBehaviour
{
    private Button button;
    private Image buttonImage;
    public UnityEvent ClickEvents;

    // Fields to set scale amount and animation duration in the Unity Inspector
    public Vector2 scaleAmount = new Vector2(1.2f, 1.2f);
    public float animationDuration = 0.2f;

    private Vector3 originalScale; // Variable to store the original scale

    void OnEnable()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        originalScale = buttonImage.transform.localScale; // Store the original scale

        button.onClick.AddListener(OnPointerClick);
    }

    public void OnPointerClick()
    {
        Debug.Log("Animation started");
        // Disable the button to prevent multiple clicks during the animation
        button.interactable = false;
        // Play animation to scale up, then scale back down, and then invoke ClickEvents
        buttonImage.transform.DOScale(scaleAmount, animationDuration)
            .OnComplete(() =>
            {
                buttonImage.transform.DOScale(originalScale, animationDuration).OnComplete(() =>
                {
                    // Invoke button actions
                    ClickEvents?.Invoke();

                    // Re-enable button after the actions
                    button.interactable = true;

                    Debug.Log("Animation ended and button re-enabled.");
                });
            });
    }
}
