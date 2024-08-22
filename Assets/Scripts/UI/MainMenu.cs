using Econagri.Localization;
using Gameplay;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject languageScreen;
        [SerializeField] private GameObject splashScreen;
        [SerializeField] private GameObject splashScreenHindi;
        [SerializeField] private GameObject mainMenuScreen;
        [SerializeField] private GameObject creditScreen;
        [SerializeField] private GameObject leaderboardScreen;
        [SerializeField] private GameObject guideScreen;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite languageScreenBackground;
        [SerializeField] private Sprite splashScreenBackground;
        [SerializeField] private Sprite mainMenuBackground;
        [SerializeField] private Sprite guideScreenBackground;
        [SerializeField] private GameObject hexes;

        private void Awake()
        {
            splashScreen.SetActive(true);
            splashScreenHindi.SetActive(false);
            languageScreen.SetActive(false);
            mainMenuScreen.SetActive(false);
            creditScreen.SetActive(false);
            leaderboardScreen.SetActive(false);
            guideScreen.SetActive(false);
            hexes.SetActive(false);

            if (GameData.hasLoadedOnce)
            {
                ShowMainMenuScreen();
            }
            else
            {
                GameData.hasLoadedOnce = true;
                ShowSplashScreen();
            }
        }

        private void Start()
        {
            LocaleSelector.Instance.ChangeLocale();
            Debug.Log($"MainMenuHasStarted");
        }
        private void OnEnable()
        {
            Debug.Log($"MainMenuOnEnabledStarted");
        }

        private void SetImage(Sprite sprite, bool showHexes = false)
        {
            backgroundImage.sprite = sprite;
            hexes.SetActive(showHexes);
        }

        public void OnGuideButtonClick()
        {
            AudioPlayer.PlayButtonClick();
            SetImage(guideScreenBackground);
            Effects.I.Fade(guideScreen, true, 0.25f);
            Effects.I.Fade(mainMenuScreen, false, 0.25f);
        }

        public void OnLeaderboardButtonClick()
        {
            AudioPlayer.PlayButtonClick();
            SetImage(mainMenuBackground, true);
            Effects.I.Fade(leaderboardScreen, true, 0.25f);
            FindObjectOfType<Leaderboard>().ScrollToTop();
            Effects.I.Fade(mainMenuScreen, false, 0.25f);
        }

        public void OnCreditButtonClick()
        {
            AudioPlayer.PlayButtonClick();
            SetImage(guideScreenBackground, true);
            Effects.I.Fade(creditScreen, true, 0.25f);
            Effects.I.Fade(mainMenuScreen, false, 0.25f);
        }

        public void OnBackButtonClick()
        {
            AudioPlayer.PlayButtonClick();
            Effects.I.Fade(creditScreen, false, 0.25f);
            Effects.I.Fade(leaderboardScreen, false, 0.25f);
            Effects.I.Fade(mainMenuScreen, true, 0.25f);
            Effects.I.Fade(guideScreen, false, 0.25f);

            // since it contains the issue lets solve it manually

            var currentLocale = LocalizationSettings.SelectedLocale;
            string code = currentLocale.Identifier.Code;
            int index = 1;
            if (code == "en") index = 0;
            else index = 1;

            LocaleSelector.Instance.ChangeLocale_2(index);
        }

        public void OnLanguageButtonClick(bool isEnglish)
        {
            AudioPlayer.PlayButtonClick();
            GameData.language = isEnglish ? Language.English : Language.Hindi;
            Events.OnLanguageSelected.Invoke();
            ShowMainMenuScreen();

            // change the screen from the localeSelector as well
            Invoke(nameof(ChangeLocaleSafely), 1f);
        }
        void ChangeLocaleSafely()
        {
            LocaleSelector.Instance.ChangeLocale_2(GameData.language == Language.English == true ? 0 : 1);
        }

        public void ShowLanguageScreen()
        {
            SetImage(languageScreenBackground);
            Effects.I.Fade(languageScreen, true, 0.5f);
            Effects.I.Fade(splashScreenHindi, false, 0.5f);
        }

        public void ShowSplashScreen()
        {
            splashScreen.SetActive(true);
            SetImage(splashScreenBackground);
            Invoke(nameof(ShowSplashScreenHindi), 1.5f);
        }

        public void ShowSplashScreenHindi()
        {
            splashScreenHindi.SetActive(true);
            Effects.I.Fade(splashScreenHindi, true, 0.5f);
            Effects.I.Fade(splashScreen, false, 0.5f);
            Invoke(nameof(ShowLanguageScreen), 1.5f);
        }

        public void OnToggleLanguageButtonClick()
        {
            AudioPlayer.PlayButtonClick();
            GameData.language = GameData.language == Language.English ? Language.Hindi : Language.English;
            Events.OnLanguageSelected.Invoke();
        }

        public void ShowMainMenuScreen()
        {
            AudioPlayer.PlayButtonClick();
            SetImage(mainMenuBackground);
            Effects.I.Fade(languageScreen, false, 0.25f);
            Effects.I.Fade(splashScreen, false, 0.25f);
            Effects.I.Fade(mainMenuScreen, true, 0.25f);
        }

        public void PlayGame()
        {
            AudioPlayer.PlayButtonClick();
            SceneManager.LoadScene("Game");
        }
    }
}