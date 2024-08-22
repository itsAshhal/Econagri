using TMPro;
using UnityEngine;

namespace UI
{
    public class Credits : MonoBehaviour
    {
        public TextMeshProUGUI textMeshPro;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) && Input.touchCount <= 0) return;
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, null);
            if (linkIndex == -1) return;
            var linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }

        public void OpenURL(string url)
        {
            Application.OpenURL(url);
        }
    }
}