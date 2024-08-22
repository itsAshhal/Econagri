using Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Events;


namespace Econagri.Localization
{
    public class LocaleSelector : MonoBehaviour
    {
        bool isActive = false;
        public UnityEvent<int> OnChangeLanguage;
        public UnityEvent OnChangeLanguageSimple;
        public bool ShouldRunOnStart = false;

        public static LocaleSelector Instance;

        private void Awake()
        {
            if (Instance != this && Instance != null) Destroy(this);
            else Instance = this;
        }

        private void Start()
        {
            if (ShouldRunOnStart == false) return;
            DefaultLocale();
            Debug.Log($"GameLanguage is {GameData.language}");

        }

        void DefaultLocale()
        {
            StartCoroutine(LocaleIdCoroutine(0));
        }

        public void ChangeLocale()
        {
            if (isActive) return;
            Debug.Log($"ChangeLocale called");
            Debug.Log($"Method called from {this.name}");
            Debug.Log($"GameLanguage is {GameData.language}");
            StartCoroutine(LocaleIdCoroutine(GameData.language == Language.English ? 0 : 1));
        }
        public void ChangeLocale_2(int id)
        {
            StartCoroutine(LocaleIdCoroutine(id));
        }

        IEnumerator LocaleIdCoroutine(int localId)
        {
            Debug.Log($"GameLanguage is {GameData.language} and localeId passed is {localId}");
            isActive = true;
            yield return LocalizationSettings.InitializationOperation;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localId];
            Debug.Log($"Selected locale is {LocalizationSettings.SelectedLocale}");
            var currentLocale = LocalizationSettings.SelectedLocale;
            Debug.Log($"Locale Code is {currentLocale.Identifier.Code}");
            //yield return new WaitForSeconds(.75f);

            // make a callback
            OnChangeLanguage?.Invoke(localId);
            // hi-IN
            // en


            // change the language as well
            GameData.language = GameData.language == Language.English ? Language.Hindi : Language.English;

            OnChangeLanguageSimple?.Invoke();

            isActive = false;
        }
    }
}