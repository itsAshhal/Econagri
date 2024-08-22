using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageCarousel : MonoBehaviour
{
    public Image[] images; // Array to hold the 8 images
    private int currentIndex = 0; // Index of the currently displayed image

    void Start()
    {
        ShowImage(currentIndex); // Display the default image at index 0
        foreach (var img in images) img.GetComponent<CustomToggle>().SetupManually();
    }

    // Function to show the image at the specified index and hide all others
    private void ShowImage(int index)
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].gameObject.SetActive(i == index);
        }
    }

    private void OnEnable()
    {
        foreach (var img in images) img.GetComponent<CustomToggle>().SetupManually();
    }

    // Function called when the right button is clicked
    public void onClickRight()
    {
        currentIndex++;
        if (currentIndex >= images.Length)
        {
            currentIndex = 0; // Wrap around to the first image
        }
        ShowImage(currentIndex);
    }

    // Function called when the left button is clicked
    public void onClickLeft()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = images.Length - 1; // Wrap around to the last image
        }
        ShowImage(currentIndex);
    }
}

