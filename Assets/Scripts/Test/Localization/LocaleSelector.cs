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
        public SceneChangeDetector scd;

        private void Awake()
        {
            if (Instance != this && Instance != null) Destroy(this);
            else Instance = this;
        }

        private void Start()
        {


        }


        public void ChangeLocale(int localeId)
        {
            StartCoroutine(LocaleIdCoroutine(localeId));
        }
        public void ChangeLocale()
        {
            if (LocalizationSettings.SelectedLocale.Identifier.Code == "en") StartCoroutine(LocaleIdCoroutine(1));
            else StartCoroutine(LocaleIdCoroutine(0));
        }


        IEnumerator LocaleIdCoroutine(int localId)
        {
            Debug.Log($"OnSceneStart locale is called, currently the localed id passed is {localId}");
            Debug.Log($"GameLanguage is {GameData.language} and localeId passed is {localId}");
            isActive = true;
            yield return LocalizationSettings.InitializationOperation;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localId];
            Debug.Log($"Selected locale is {LocalizationSettings.SelectedLocale}");
            var currentLocale = LocalizationSettings.SelectedLocale;
            Debug.Log($"Locale Code is {currentLocale.Identifier.Code}");
            isActive = false;

            OnChangeLanguage?.Invoke(localId);
            OnChangeLanguageSimple?.Invoke();
        }
    }
}