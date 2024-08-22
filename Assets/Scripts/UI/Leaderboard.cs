using System;
using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Leaderboard : MonoBehaviour
    {
        [SerializeField] public GameObject leaderboardElementPrefab;
        [SerializeField] public GameObject leaderboardElementParent;
        [SerializeField] public ScrollRect scrollRect;

        [SerializeField] public GameObject couldNotLoadLeaderboardText;
        
        private bool couldLoadLeaderboard = false;

        public void Start()
        {
            Invoke(nameof(Setup), 0.5f);
            couldNotLoadLeaderboardText.SetActive(false);
        }

        public void ScrollToTop()
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        private void Update()
        {
            if (!couldLoadLeaderboard)
            {
                FirestoreManager.I.GetTopScores();
                Invoke(nameof(Setup), 0.5f);
            }
        }

        public void Setup()
        {
            var topScores = FirestoreManager.I.topScores;
            foreach (Transform child in leaderboardElementParent.transform)
            {
                Destroy(child.gameObject);
            }

            if (topScores == null || topScores.Length == 0)
            {
                couldNotLoadLeaderboardText.SetActive(true);
                return;
            }
            
            couldLoadLeaderboard = true;
            for (int i = 0; i < topScores.Length; i++)
            {
                var element = Instantiate(leaderboardElementPrefab, leaderboardElementParent.transform);
                element.GetComponent<LeaderboardElement>()
                    .Setup(topScores[i].name, topScores[i].score.ToString(), i + 1);
            }
            couldNotLoadLeaderboardText.SetActive(false);
        }
    }
}