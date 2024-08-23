using Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.Localization;


namespace Econagri.Localization
{
    public class LocalizeText : MonoBehaviour
    {
        public UsedFontScriptable _Font;
        bool isTranslating = false;
        private void Start()
        {
            // SetTranslation();
            // subscribe the function
            LocaleSelector.Instance.OnChangeLanguage.AddListener(this.OnChangeLanguageMethod);

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
            // change the fonts for each language as well
            TMP_Text textComponent = GetComponent<TMP_Text>();
            string text = textComponent.text;
            //textComponent.text = "";
            GetComponent<Animator>().CrossFade("Disappear", .1f);
            yield return new WaitForSeconds(.5f);
            GetComponent<TMP_Text>().font = localeId == 0 ? _Font.EnglishFont : _Font.HindiFont;
            //textComponent.text = text;
            GetComponent<Animator>().CrossFade("Appear", .1f);
        }

    }


}
