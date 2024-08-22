using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

namespace Gameplay
{
    public class GameOver : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverBackground;
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject gamePanel;
    
        [SerializeField] private GameObject winScreen;
        [SerializeField] private GameObject loseScreen;

    
        [SerializeField] private TextMeshProUGUI gdpScoreWin;
        [SerializeField] private TextMeshProUGUI gdpScoreLose;

        [SerializeField] private GameObject headingMovesLeftWin;
        [SerializeField] private GameObject headingMovesLeftLose;
        
        [SerializeField] private GameObject headingNoMovesLeftWin;
        [SerializeField] private GameObject headingNoMovesLeftLose;
        
        [SerializeField] GameObject cameraToStop;

        private void Awake()
        {
            gameOverScreen.SetActive(false);
            gameOverBackground.SetActive(false);
            Events.OnGameOver.AddListener(ShowGameOverScreen);
        }

        private void ShowGameOverScreen(bool hasWon, int aqi, int gdp, bool movesLeft)
        {
            gameOverScreen.SetActive(true);
            gameOverBackground.SetActive(true);
            gamePanel.SetActive(false);
            cameraToStop.SetActive(false);
            
            if (hasWon)
            {
                winScreen.SetActive(true);
                loseScreen.SetActive(false);
            
                gdpScoreWin.text = gdp.ToString()  + " GDP";
                
                headingMovesLeftWin.SetActive(movesLeft);
                headingNoMovesLeftWin.SetActive(!movesLeft);
            }
            else
            {
                winScreen.SetActive(false);
                loseScreen.SetActive(true);
            
                var gameplaySettings = FindObjectOfType<GameplaySettings>();
                gameplaySettings.ShowRandomFailTip();
            
                gdpScoreLose.text = gdp.ToString()  + " GDP";
                
                headingMovesLeftLose.SetActive(movesLeft);
                headingNoMovesLeftLose.SetActive(!movesLeft);
            }
        }
    
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
