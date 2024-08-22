using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    private Camera cam;
    private Color originalColor;
    private bool isShaking = false;

    [Tooltip("Time between each blink in seconds.")]
    public float blinkInterval = 0.1f;  // You can adjust this value in the Inspector.
    public float magnitude = 2f;
    private Vector3 m_cameraDefaultPosition;
    public float CameraResetSpeed = .5f;
    private bool m_isCameraResetting = false;
    public float CameraDistanceThreshold = .1f;
    void Start()
    {
        cam = GetComponent<Camera>();
        originalColor = cam.backgroundColor;
        m_cameraDefaultPosition = this.transform.position;
    }

    public void OnClick_ResetCamera()
    {
        m_isCameraResetting = true;
        //this.transform.position = m_cameraDefaultPosition;

    }

    private void Update()
    {
        if (!m_isCameraResetting) return;
        this.transform.position = Vector3.MoveTowards(transform.position, m_cameraDefaultPosition, CameraResetSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, m_cameraDefaultPosition) <= CameraDistanceThreshold) m_isCameraResetting = false;
    }

    public void StartCameraEffect(float duration)
    {
        if (!isShaking)  // Check to ensure we don't start another effect if one is already running.
        {
            StartCoroutine(ShakeCamera(duration));
            StartCoroutine(BlinkRedBackground(duration));
        }
    }

    private IEnumerator ShakeCamera(float duration)
    {
        isShaking = true;
        float elapsed = 0.0f;
        float magnitude = 0.1f;

        Vector3 originalPosition = transform.position;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        isShaking = false;
    }

    private IEnumerator BlinkRedBackground(float duration)
    {
        Color myColor = new Color(1.0f, 0.509f, 0.486f, 0.1f); // FF827C with full opacity
        cam.backgroundColor = myColor;

        yield return new WaitForSeconds(duration);

        cam.backgroundColor = originalColor; // Reset to original color after the duration
    }

}
