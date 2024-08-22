using System.Collections;
using Econagri.Localization;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace UI
{
    public class SetBasedOnLanguage : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI text;

        [SerializeField] private Sprite englishSprite;
        [SerializeField] private Sprite hindiSprite;

        [TextArea][SerializeField] private string englishText;
        [TextArea][SerializeField] private string hindiText;

        [SerializeField] private GameObject englishObject;
        [SerializeField] private GameObject hindiObject;

        [SerializeField] private bool setupOnAwake = false;
        [SerializeField] private float hindiFontScale = 1.5f;

        private TMP_FontAsset englishFont;
        private float initFontSize;
        public bool ShouldBeUsed = false;  // we're only using this boolean as this TextMeshPro is a rare case and we don't want to disturb other elements having this script

        public void SetText(string english, string hindi)
        {
            englishText = english;
            hindiText = hindi;

            // checking if we can dynamically check the certain text is being used or not
            //if (english.Contains("last move")) Debug.Log($"Yes there is a certain text like this");

            if (this.isActiveAndEnabled)
            {
                StartCoroutine(WaitForSetup());
            }
        }

        IEnumerator WaitForSetup()
        {
            if (!ShouldBeUsed)
            {

                yield return new WaitForFixedUpdate();
                Setup();
            }
        }

        private void Awake()
        {
            if (ShouldBeUsed == false) return;
            if (text != null)
            {
                englishFont = text.font;
            }

            if (text != null)
            {
                initFontSize = text.fontSize;
            }

            Events.OnLanguageSelected.AddListener(Setup);
            if (setupOnAwake) Setup();
        }

        private void Start()
        {
            if (ShouldBeUsed == false) return;
            InvokeRepeating(nameof(Setup_2), 0.05f, 0.05f);
        }
        public void Setup_2()
        {
            var selectedLocale = LocalizationSettings.SelectedLocale;
            bool isEnglish = selectedLocale.Identifier.Code == "en";
            Debug.Log($"Checking the localeLanguage, English => {isEnglish}");

            if (isEnglish)
            {
                if (image != null)
                {
                    image.sprite = englishSprite;
                    image.SetNativeSize();
                }

                if (text != null)
                {
                    if (text.GetComponent<ChangeTextMeshHindi>() != null)
                    {
                        Destroy(text.GetComponent<ChangeTextMeshHindi>());
                    }

                    text.font = englishFont;
                    text.text = englishText;
                    text.fontSize = initFontSize;
                }

                if (englishObject != null) englishObject.SetActive(true);
                if (hindiObject != null) hindiObject.SetActive(false);
            }
            else
            {
                if (image != null)
                {
                    image.sprite = hindiSprite;
                    image.SetNativeSize();
                }

                if (text != null)
                {
                    var textComponent = text.GetComponent<ChangeTextMeshHindi>();
                    if (textComponent == null)
                    {
                        var c = text.gameObject.AddComponent<ChangeTextMeshHindi>();
                    }
                    else
                    {
                        Destroy(textComponent);
                        var c = text.gameObject.AddComponent<ChangeTextMeshHindi>();
                    }

                    text.text = hindiText;
                    text.fontSize = initFontSize * hindiFontScale;
                }

                if (englishObject != null) englishObject.SetActive(false);
                if (hindiObject != null) hindiObject.SetActive(true);
            }
        }

        public void Setup()
        {
            Debug.Log($"In the setup the current language is {GameData.language}");

            

            if (GameData.language == Language.English)
            {
                if (image != null)
                {
                    image.sprite = englishSprite;
                    image.SetNativeSize();
                }

                if (text != null)
                {
                    if (text.GetComponent<ChangeTextMeshHindi>() != null)
                    {
                        Destroy(text.GetComponent<ChangeTextMeshHindi>());
                    }

                    text.font = englishFont;
                    text.text = englishText;
                    text.fontSize = initFontSize;

                    Debug.Log($"In the setup method, the current text is {text.text}");
                }

                if (englishObject != null) englishObject.SetActive(true);
                if (hindiObject != null) hindiObject.SetActive(false);
            }
            else
            {
                if (image != null)
                {
                    image.sprite = hindiSprite;
                    image.SetNativeSize();
                }

                if (text != null)
                {
                    var textComponent = text.GetComponent<ChangeTextMeshHindi>();
                    if (textComponent == null)
                    {
                        var c = text.gameObject.AddComponent<ChangeTextMeshHindi>();
                    }
                    else
                    {
                        Destroy(textComponent);
                        var c = text.gameObject.AddComponent<ChangeTextMeshHindi>();
                    }

                    text.text = hindiText;
                    text.fontSize = initFontSize * hindiFontScale;
                }

                if (englishObject != null) englishObject.SetActive(false);
                if (hindiObject != null) hindiObject.SetActive(true);
            }
        }
    }
}