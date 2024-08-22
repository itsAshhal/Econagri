using UnityEngine;
using UnityEngine.UI;

public class ChangeTextInHindi : MonoBehaviour
{

    void Start()
    {
        string text = gameObject.GetComponent<Text>().text; // accessing the text component

        gameObject.GetComponent<Text>().SetHindiText(text);
    }
}
