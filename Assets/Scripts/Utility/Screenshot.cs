using System;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class ScreenshotHandler : MonoBehaviour
{
    public Camera myCamera;
    private int resWidth = 2550;
    private int resHeight = 3300;

    public GameObject[] HideOnScreenshot;
    public GameObject[] ShowOnScreenshot;

    public GameObject screenshotCopied;
    public GameObject shareIntent;
    public GameObject winUIParent;

    private bool[] _didHide;
    private bool[] _didShow;
    
    private string currentUrl;
    private string currentQuote;

    private void Awake()
    {
        screenshotCopied.SetActive(false);
        shareIntent.SetActive(false);
        Events.OnGameOver.AddListener(OnGameOver);
    }
    
    private void OnGameOver(bool didWin, int aqi, int gdp, bool movesLeft)
    {
        shareIntent.transform.SetParent(winUIParent.transform);
        screenshotCopied.transform.SetParent(winUIParent.transform);
        
        shareIntent.transform.localPosition = Vector3.zero;
        screenshotCopied.transform.localPosition = Vector3.zero - Vector3.up * 200f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CaptureScreenshot();
        }
    }

    private void SetImageAlpha(GameObject gameObjectImage, float alpha)
    {
        // for each child of the parent object
        
        var image = gameObjectImage.GetComponent<Image>();
        if (image)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }

        foreach (var imageChild in gameObjectImage.transform.GetComponentsInChildren<Image>())
        {
            var color1 = imageChild.color;
            color1.a = alpha;
            imageChild.color = color1;
        }
        
        foreach (var textChild in gameObjectImage.transform.GetComponentsInChildren<TextMeshProUGUI>())
        {
            var color1 = textChild.color;
            color1.a = alpha;
            textChild.color = color1;
        }
    }

    private void CaptureScreenshot()
    {
        resHeight = Screen.height;
        resWidth = Screen.width;
        
        _didShow = new bool[ShowOnScreenshot.Length];
        _didHide = new bool[HideOnScreenshot.Length];
        
        for(var i = 0; i < ShowOnScreenshot.Length; i++)
        {
            if (ShowOnScreenshot[i].activeSelf)
            {
                _didShow[i] = false;
            }
            else
            {
                ShowOnScreenshot[i].SetActive(true);
                _didShow[i] = true;
                SetImageAlpha(ShowOnScreenshot[i], 1f);
            }
        }
        
        for (var i = 0; i < HideOnScreenshot.Length; i++)
        {
            if (HideOnScreenshot[i].activeSelf)
            {
                HideOnScreenshot[i].SetActive(false);
                _didHide[i] = true;
            }
            else
            {
                _didHide[i] = false;
            }
        }

        var rt = new RenderTexture(resWidth, resHeight, 24);
        myCamera.targetTexture = rt;
        var screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        myCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        myCamera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        
        for(var i = 0; i < ShowOnScreenshot.Length; i++)
        {
            if (_didShow[i])
            {
                ShowOnScreenshot[i].SetActive(false);
                SetImageAlpha(ShowOnScreenshot[i], 0f);
            }
        }

        for (var i = 0; i < HideOnScreenshot.Length; i++)
        {
            if (_didHide[i])
            {
                HideOnScreenshot[i].SetActive(true);
            }
        }

        var bytes = screenShot.EncodeToPNG();
        var filename = ScreenshotName(resWidth, resHeight);
        var finalImageUrl = "https://firebasestorage.googleapis.com/v0/b/econagar.appspot.com/o/" + filename +
                            "?alt=media";
        FirestoreManager.I.UploadScreenshot(bytes, filename,
            (url) =>
            {
                CopyToClipboard("I’m building a Clean City with Econagri! #Econagri @Wingifyearth \n" +
                                finalImageUrl);
                if (Application.platform == RuntimePlatform.Android)
                {
                    ShareTextOnAndroid("I’m building a Clean City with Econagri! #Econagri @Wingifyearth \n" +
                                       finalImageUrl);
                }
                else
                {
                    currentUrl = finalImageUrl;
                    currentQuote = "I’m building a Clean City with Econagri! #Econagri @Wingifyearth";
                }
            });
    }

    private string ScreenshotName(int width, int height)
    {
        return string.Format("screenshot_{0}x{1}_{2}.png",
            width, height,
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    private void ShareTextOnAndroid(string text)
    {
        using (var intentClass = new AndroidJavaClass("android.content.Intent"))
        using (var intentObject = new AndroidJavaObject("android.content.Intent"))
        {
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);

            using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

                // Create a chooser intent
                AndroidJavaObject jChooser =
                    intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share Via");
                currentActivity.Call("startActivity", jChooser);
            }
        }
    }

    public void TakeScreenshot()
    {
        CaptureScreenshot();
    }

    public void CopyToClipboard(string text)
    {
        var te = new TextEditor
        {
            text = text
        };
        te.SelectAll();
        te.Copy();
        Effects.I.Fade(screenshotCopied, true, 0.25f);

        if (Application.platform != RuntimePlatform.Android)
        {
            shareIntent.SetActive(true);
        }
        Invoke(nameof(HideScreenshotCopied), 1.5f);
    }

    private void HideScreenshotCopied()
    {
        Effects.I.Fade(screenshotCopied, false, 0.25f);
    }
    public void ShareToFacebook()
    {
        Application.OpenURL("https://www.facebook.com/sharer/sharer.php?u=" + Uri.EscapeDataString(currentUrl) + "&quote=" + Uri.EscapeDataString(currentQuote));
        shareIntent.SetActive(false);
    }
    
    public void ShareToTwitter()
    {
        Application.OpenURL("https://twitter.com/intent/tweet?text=" + Uri.EscapeDataString(currentUrl) + "&url=" + Uri.EscapeDataString(currentQuote));
        shareIntent.SetActive(false);
    }
    
    public void ShareToWhatsApp()
    {
        Application.OpenURL("https://api.whatsapp.com/send?text=" + Uri.EscapeDataString(currentUrl) + "%20" + Uri.EscapeDataString(currentQuote));
        shareIntent.SetActive(false);
    }
}