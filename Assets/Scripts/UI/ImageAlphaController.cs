using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Add this to work with UI elements
using Econagri.Localization;

public class ImageAlphaController : MonoBehaviour
{
    public float AlphaValue = 1f;
    void OnEnable()
    {
        Invoke(nameof(SetImageAlphaToOne), .1f);
        Debug.Log("HAHA1");
        LocaleSelector.Instance.ChangeLocale_2(0);
    }

    void Awake()
    {
        Debug.Log("HAHA2");
    }
    void Start()
    {
        Debug.Log("HAHA3");
    }

    // Function to set the alpha of an image to 1
    public void SetImageAlphaToOne()
    {
        var image = this.GetComponent<Image>();
        if (image != null)
        {
            Color color = image.color;
            color.a = AlphaValue; // Set alpha to 1
            image.color = color;
        }
    }

    void Update()
    {
        SetImageAlphaToOne();
    }
}
