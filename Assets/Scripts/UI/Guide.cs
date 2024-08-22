using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Guide : MonoBehaviour
    {
        [SerializeField] private GameObject[] pages;
        
        [SerializeField] Button rightButton;
        [SerializeField] Button leftButton;

        private int currentPageIndex = 0;

        private void Start()
        {
            UpdateUI();
        }

        public void OnRightClick()
        {
            currentPageIndex = Mathf.Min(currentPageIndex + 1, pages.Length - 1);
            UpdateUI();
        }

        public void OnLeftClick()
        {
            currentPageIndex = Mathf.Max(currentPageIndex - 1, 0);
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (currentPageIndex == 0)
            {
                leftButton.gameObject.SetActive(false);
            }
            else
            {
                leftButton.gameObject.SetActive(true);
            }
            
            if (currentPageIndex == pages.Length - 1)
            {
                rightButton.gameObject.SetActive(false);
            }
            else
            {
                rightButton.gameObject.SetActive(true);
            }


            foreach (var page in pages)
            {
                page.SetActive( page == pages[currentPageIndex]);
            }
        }
    }
}