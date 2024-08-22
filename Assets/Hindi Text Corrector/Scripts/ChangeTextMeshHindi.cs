using UnityEngine;
using TMPro;

public class ChangeTextMeshHindi : MonoBehaviour
{
    
    [SerializeField] TMP_FontAsset hindiFont;

    void Start()
    {
        string text = gameObject.GetComponent<TextMeshProUGUI>().text; // Getting TMPro Component

        gameObject.GetComponent<TextMeshProUGUI>().SetHindiTMPro(text); // Setting Hindi Text
    }

    public void Setup()
    {
        string text = gameObject.GetComponent<TextMeshProUGUI>().text; // Getting TMPro Component
        gameObject.GetComponent<TextMeshProUGUI>().SetHindiTMPro(text); // Setting Hindi Text
    }

    void Update()
    {

    }
}
