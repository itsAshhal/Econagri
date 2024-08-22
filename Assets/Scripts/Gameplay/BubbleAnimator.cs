using UnityEngine;

public class BubbleAnimator : MonoBehaviour
{
    public float riseSpeed = 5.0f;
    public float maxRiseHeight = 10.0f;
    public Vector3 direction = Vector3.up;
    private Vector3 originalPosition;
    public Vector3 originalScale;
    private Color originalColor;
    private Renderer bubbleRenderer;
    private BubblePoolManager poolManager;
    public bool shouldAnimate = false;

    void Start()
    {
        originalPosition = transform.position; // Save the original position for reuse
        bubbleRenderer = GetComponent<Renderer>();
        originalColor = bubbleRenderer.material.color; // Save the original color
        poolManager = FindObjectOfType<BubblePoolManager>(); // Reference to the pool manager
    }

    void Update()
    {
        shouldAnimate = true;
        // Increment the bubble's vertical position
        transform.position += direction * riseSpeed * Time.deltaTime;

        // Calculate the distance the bubble has traveled from its original position
        float distanceTravelled = Vector3.Distance(transform.position, originalPosition);

        // Calculate fraction of the journey completed
        float fracJourney = distanceTravelled / maxRiseHeight;
        fracJourney = Mathf.Clamp01(fracJourney); // Ensure it stays within 0 to 1

        // Fade out and scale down based on the fraction of the journey completed
        if (bubbleRenderer != null)
        {
            Color color = bubbleRenderer.material.color;
            color.a = Mathf.Lerp(originalColor.a, 0, fracJourney);
            bubbleRenderer.material.color = color;
        }
        transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, fracJourney);

        // Reset and reuse the bubble when it reaches the top
        if (fracJourney >= 1.0f)
        {
            ResetBubble();
        }
    }



    private void ResetBubble()
    {
        shouldAnimate = false;
        transform.position = originalPosition;
        transform.localScale = originalScale;
        bubbleRenderer.material.color = originalColor;
        poolManager.ReturnBubbleToPool(this);
    }
}
