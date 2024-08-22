using System;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;

public class HindiTest : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMeshPro;
    [SerializeField] string text;

    private void OnValidate()
    {
        UpdateText();
    }

    private string FixText(string inputText)
    {
        var fixedText = new StringBuilder(inputText);

        for (var i = 1; i < fixedText.Length; i++)
        {
            if (fixedText[i] != 'ि') continue;
            (fixedText[i - 1], fixedText[i]) = (fixedText[i], fixedText[i - 1]);
            i++;
        }

        return fixedText.ToString();
    }

    void UpdateText()
    {
        string originalText = text;
        string fixedText = FixText(originalText);
        Debug.Log("Original Text: " + originalText);
        Debug.Log("Fixed Text: " + fixedText);

        textMeshPro.text = fixedText;
        // Use the fixedText for your text rendering component
    }
}