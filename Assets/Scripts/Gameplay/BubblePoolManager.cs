using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubblePoolManager : MonoBehaviour
{
    public GameObject bubblePrefab;
    public int poolSize = 20;
    public float bubbleOffset = 0.5f; // Offset distance between bubbles
    public int bubblesPerEmission = 3; // Number of bubbles per emission
    public float offsetHorizontal = 0.5f; 
    public float riseSpeed = 5.0f;
    public float maxRiseHeight = 10.0f;
    public float startScale = 0.5f;
    
    private Queue<BubbleAnimator> bubblePool = new Queue<BubbleAnimator>();

    void Start()
    {
        // Initialize the pool
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewBubble();
        }

        // Start emitting bubbles
        StartCoroutine(EmitBubbleGroups());
    }

    private void CreateNewBubble()
    {
        GameObject bubbleObj = Instantiate(bubblePrefab, transform.position, Quaternion.identity);
        BubbleAnimator bubble = bubbleObj.GetComponent<BubbleAnimator>();
        bubble.riseSpeed = riseSpeed;
        bubble.maxRiseHeight = maxRiseHeight;
        bubble.originalScale = Vector3.one * startScale;
        bubble.gameObject.SetActive(false);
        bubblePool.Enqueue(bubble);
    }

    public BubbleAnimator GetBubbleFromPool()
    {
        if (bubblePool.Count == 0)
        {
            CreateNewBubble();
        }

        BubbleAnimator bubble = bubblePool.Dequeue();
        bubble.gameObject.SetActive(true);
        return bubble;
    }

    public void ReturnBubbleToPool(BubbleAnimator bubble)
    {
        bubble.gameObject.SetActive(false);
        bubblePool.Enqueue(bubble);
    }

    private IEnumerator EmitBubbleGroups()
    {
        while (true)
        {
            for (int i = 0; i < bubblesPerEmission; i++)
            {
                BubbleAnimator bubble = GetBubbleFromPool();
                bubble.shouldAnimate = false;
                Vector3 offsetPosition = new Vector3(Random.Range(-offsetHorizontal,offsetHorizontal),bubbleOffset * i, 0 );
                bubble.transform.position = transform.position + offsetPosition;
            }

            yield return new WaitForSeconds(2f); // Adjust emission frequency as needed
        }
    }
}