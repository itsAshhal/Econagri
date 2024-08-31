using System.Collections;
using System.Collections.Generic;
using Econagri.Localization;
using Gameplay;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class CustomToggle : MonoBehaviour
{
    public Sprite EnglishSprite;
    public Sprite HindiSprite;
    private Image m_image;
    public enum ImageSettings
    {
        Hindi, English, None
    }
    public ImageSettings M_ImageSettings;
    [SerializeField] Vector2 HindiIncrementSettings;
    [SerializeField] Vector2 EnglishIncrementSettings;
    public bool SetImageSizeManually = false;
    void OnEnable()
    {
        Debug.Log($"Custom toggle language is {GameData.language.ToString()}");

        m_image = GetComponent<Image>();

        m_image.sprite = GameData.language == Language.English ? EnglishSprite : HindiSprite;

        InvokeRepeating(nameof(SetupManually), 0f, 0f);

        if (LocaleSelector.Instance == null) return;
        Debug.Log($"AssigningLocaleSelectorSimple");
        LocaleSelector.Instance.OnChangeLanguageSimple.AddListener(SetupManually);

        //SetupManually();
    }


    private void Update()
    {
        if (SetImageSizeManually)
        {
            var currentLocale = LocalizationSettings.SelectedLocale;
            if (currentLocale.Identifier.Code == "en") m_image.GetComponent<RectTransform>().localScale = EnglishIncrementSettings;
            else m_image.GetComponent<RectTransform>().localScale = HindiIncrementSettings;
        }
    }

    /// <summary>
    ///  In case OnEnable is not called, we can also set this up manually
    /// </summary>
    public void SetupManually()
    {
        Debug.Log($"As the language is changed, langauge is {GameData.language}");
        // m_image.sprite = GameData.language == Language.English ? EnglishSprite : HindiSprite;

        var currentLocale = LocalizationSettings.SelectedLocale;
        if (currentLocale.Identifier.Code == "en")
        {
            m_image.sprite = EnglishSprite;
            // m_image.GetComponent<RectTransform>().localScale = EnglishIncrementSettings;
        }
        else
        {
            m_image.sprite = HindiSprite;
            // m_image.GetComponent<RectTransform>().localScale = HindiIncrementSettings;
        }

        if (!SetImageSizeManually) return;

        M_ImageSettings = GameData.language == Language.Hindi ? ImageSettings.Hindi : ImageSettings.English;

        if (M_ImageSettings == ImageSettings.Hindi)
        {
            m_image.rectTransform.localScale = HindiIncrementSettings;
        }
        else if (M_ImageSettings == ImageSettings.English)
        {
            m_image.rectTransform.localScale = EnglishIncrementSettings;
        }


        if (M_ImageSettings == ImageSettings.Hindi)
        {
            // check for the default english scale
            if (m_image.sprite == EnglishSprite)
            {
                // revert back to default scale
                m_image.rectTransform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }
}
