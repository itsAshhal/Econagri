using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LanguageButton : MonoBehaviour
    {
        [SerializeField] private Image baseImage;
    
        [SerializeField] private Sprite hoverSprite;
        [SerializeField] private Sprite baseSprite;
    
        public void OnHover()
        {
            baseImage.sprite = hoverSprite;
        }
    
        public void OnExit()
        {
            baseImage.sprite = baseSprite;
        }
    
        public void OnClick()
        {
            Debug.Log("Language button clicked");
        }
    }
}
