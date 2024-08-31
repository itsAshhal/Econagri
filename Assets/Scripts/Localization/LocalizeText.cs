using Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;


namespace Econagri.Localization
{
    public class LocalizeText : MonoBehaviour
    {
        public UsedFontScriptable _Font;
        bool isTranslating = false;
        [Tooltip("For custom controlling over translation and texts")]
        public bool ShouldBeAutomated = false;
        private void Start()
        {
            // SetTranslation();
            // subscribe the function
            LocaleSelector.Instance.OnChangeLanguage.AddListener(this.OnChangeLanguageMethod);

        }


        private void Update()
        {

        }

        private void OnEnable()
        {
            var com = GetComponent<TMP_Text>();
            com.enabled = true;
            if (LocalizationSettings.SelectedLocale.Identifier.Code == "en") StartCoroutine(ChangeTextCoroutine(0));
            else StartCoroutine(ChangeTextCoroutine(1));

            // settings for custom automated
            if (ShouldBeAutomated)
            {
                if (LocalizationSettings.SelectedLocale.Identifier.Code == "en") com.font = _Font.EnglishFont;
                else com.font = _Font.HindiFont;
            }


        }
        public void SetTranslation()
        {
            if (isTranslating) return;

            //GameData.language = GameData.language == Language.English ? Language.Hindi : Language.English;
        }

        public void OnChangeLanguageMethod(int localeId)
        {
            Debug.Log($"Translation called");
            StartCoroutine(ChangeTextCoroutine(localeId));

        }

        IEnumerator ChangeTextCoroutine(int localeId)
        {

            if (SceneManager.GetActiveScene().name == "MainMenu" && !ShouldBeAutomated)
            {
                Debug.Log($"Changing Text Components");
                // change the fonts for each language as well
                TMP_Text textComponent = GetComponent<TMP_Text>();
                textComponent.enabled = false;
                string text = textComponent.text;
                //textComponent.text = "";
                GetComponent<Animator>().CrossFade("Disappear", .1f);
                yield return new WaitForSeconds(.5f);
                GetComponent<TMP_Text>().font = localeId == 0 ? _Font.EnglishFont : _Font.HindiFont;
                //textComponent.text = text;
                textComponent.enabled = true;
                GetComponent<Animator>().CrossFade("Appear", .1f);
            }
            else if (SceneManager.GetActiveScene().name == "Game" || ShouldBeAutomated)
            {
                Debug.Log($"Changing Text Components");
                // change the fonts for each language as well
                TMP_Text textComponent = GetComponent<TMP_Text>();
                //textComponent.enabled = false;
                string text = textComponent.text;
                //textComponent.text = "";
                GetComponent<Animator>().CrossFade("Disappear", .1f);
                yield return new WaitForSeconds(.5f);
                GetComponent<TMP_Text>().font = localeId == 0 ? _Font.EnglishFont : _Font.HindiFont;
                //textComponent.text = text;
                //textComponent.enabled = true;
                GetComponent<Animator>().CrossFade("Appear", .1f);
            }
        }

    }


}
