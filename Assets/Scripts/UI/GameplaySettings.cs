using System.Collections;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class GameplaySettings : MonoBehaviour
    {
        [SerializeField] private GameObject gameplaySettingsPanel;
        [SerializeField] private GameObject inGameSettingsPanel;

        [SerializeField] private TextMeshProUGUI gdpScore;
        [SerializeField] private TextMeshProUGUI aqiScore;

        [SerializeField] private GameObject credits;
        [SerializeField] private GameObject guide;
        [SerializeField] private GameObject leaderboard;
        [SerializeField] private Leaderboard leaderboardScript;
        
        [SerializeField] private Camera extraCamera;
        [SerializeField] private CameraController primaryCamera;
        [SerializeField] private Dragging dragging;
        [SerializeField] private PieceSpawner pieceSpawner;

        [SerializeField] private float speedToShow = 2f;
        [SerializeField] private TMP_InputField leaderboardName;
    
        [SerializeField] private TextMeshProUGUI failTip;
        [SerializeField] private Button saveScoreButton;
    
        ///  Toggle Slider
        [SerializeField] private TextMeshProUGUI toggleText;
        [SerializeField] private Slider toggle;
        
        public void OnToggleSliderChange()
        {
            var value = (int) toggle.value;
            toggleText.text = value == 0 ? "ON" : "OFF";
            Tooltip.I.ShouldShowTooltip(value == 0);
            if (value != 0)
            {
                Tooltip.I.HideTooltip();
            }
        }

        public void Undo()
        {
            Events.OnUndo.Invoke();    
        }

        private void Awake()
        {
            gameplaySettingsPanel.SetActive(false);
            credits.SetActive(false);
            guide.SetActive(false);
            leaderboard.SetActive(false);
        }
        
        public void ShowLeaderboard()
        {
            AudioPlayer.PlayButtonClick();
            Effects.I.Fade(leaderboard, true, 0.25f);
            leaderboardScript.ScrollToTop(); 
            Effects.I.Fade(gameplaySettingsPanel, false, 0.25f);
        }
        
        public void HideLeaderboard()
        {
            AudioPlayer.PlayButtonClick();
            Effects.I.Fade(leaderboard, false, 0.25f);
            Effects.I.Fade(gameplaySettingsPanel, true, 0.25f);
        }

        public void ToggleLanguage()
        {
            var language = GameData.language == Language.English ? Language.Hindi : Language.English;
            GameData.language = language;
            Events.OnLanguageSelected.Invoke();
        }
        
        public void OnGuideButtonClick()
        {
            AudioPlayer.PlayButtonClick();
            Effects.I.Fade(guide, true, 0.25f);
            Effects.I.Fade(gameplaySettingsPanel, false, 0.25f);
        }

        public void StopGuide()
        {
            AudioPlayer.PlayButtonClick();
            Effects.I.Fade(guide, false, 0.25f);
            Effects.I.Fade(gameplaySettingsPanel, true, 0.25f);
        }
        

        public void OnCreditsButtonClick()
        {
            AudioPlayer.PlayButtonClick();
            Effects.I.Fade(credits, true, 0.25f);
            Effects.I.Fade(gameplaySettingsPanel, false, 0.25f);
        }

        public void StopCredits()
        {
            AudioPlayer.PlayButtonClick();
            Effects.I.Fade(credits, false, 0.25f);
            Effects.I.Fade(gameplaySettingsPanel, true, 0.25f);
        }

        public void OnRestartButtonClick()
        {
            AudioPlayer.PlayButtonClick();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        public void BackToMainMenu()
        {
            AudioPlayer.PlayButtonClick();
            SceneManager.LoadScene(1);
        }

        public void HideGameplaySettings()
        {
            AudioPlayer.PlayButtonClick();
            Effects.I.Fade(gameplaySettingsPanel, false, 0.25f);
            Effects.I.Fade(inGameSettingsPanel, true, 0.25f);
            extraCamera.gameObject.SetActive(true);
            primaryCamera.canMove = true;
            dragging.canDrag = true;
            Invoke(nameof(UpdateCommIndu), 0.27f);
        }

        public void ShowGameplaySettings()
        {
            AudioPlayer.PlayButtonClick();
            gdpScore.text = GameManager.Instance.GDP.ToString();
            aqiScore.text = GameManager.Instance.AQI.ToString();

            Effects.I.Fade(gameplaySettingsPanel, true, 0.25f);
            Effects.I.Fade(inGameSettingsPanel, false, 0.25f);
            extraCamera.gameObject.SetActive(false);
            primaryCamera.canMove = false;
            dragging.canDrag = false;
        }

        private void UpdateCommIndu()
        {
            pieceSpawner.UpdateComInduMergeMarkerText();
        }

        public void ShowRandomFailTip()
        {
            var tipsEnglish = new[]
            {
                "Certain blocks can have more tiles of the same kind stacked on top of it, for a bigger GDP boost.",
                "Place different types of tiles next to each together to get an extra +1 GDP.",
                "Metro lines can replace Traffic Jams, removing their effect on the AQI by a great amount.",
                "Careful planning early on can help your city later."
            };

            var tipsHindi = new[]
            {
                "अपने GDP को जल्दी बढ़ाने के लिए ब्लॉक और स्टैक बनाएं।",
                "अतिरिक्त +1 GDP प्राप्त करने के लिए प्रत्येक टाइल के बगल में अलग अलग प्रकार की टाइलें रखें। ",
                "मेट्रो लाइनें ट्रैफिक जाम की जगह ले सकती हैं, जिससे AQI पर उनका प्रभाव काफी कम हो सकता है।",
                "शुरुआत में सोच समझ के टाइल रखने से आपके शहर को बाद में मदद मिल सकती है।"
            };
        
            var tips = GameData.language == Language.English ? tipsEnglish : tipsHindi;
            var randomTip = tips[UnityEngine.Random.Range(0, tips.Length)];
        
            failTip.text = randomTip;
            if (GameData.language == Language.Hindi)
            {
                failTip.gameObject.AddComponent<ChangeTextMeshHindi>();
            }
        }

        public void ToggleVisibility(Image image)
        {
            if (image.fillAmount > 0)
            {
                StartCoroutine(HideImage(image));
            }
            else
            {
                StartCoroutine(ShowImage(image));
            }
        }

        public void SaveScore()
        {
            var nameToSave = leaderboardName.text;
            if (string.IsNullOrEmpty(nameToSave))
            {
                return;
            }
            var scoreToSave = GameManager.Instance.GDP;
            FirestoreManager.I.AddScore(nameToSave, (int)scoreToSave, s =>
            {
                saveScoreButton.interactable = false;
                saveScoreButton.gameObject.SetActive(false);
            });
        }

        IEnumerator ShowImage(Image image)
        {
            image.fillAmount = 0;
            while (image.fillAmount < 1)
            {
                image.fillAmount += Time.deltaTime * speedToShow;
                yield return null;
            }
        }
    
        IEnumerator HideImage(Image image)
        {
            image.fillAmount = 1;
            while (image.fillAmount > 0)
            {
                image.fillAmount -= Time.deltaTime * speedToShow;
                yield return null;
            }
        }
    }
}