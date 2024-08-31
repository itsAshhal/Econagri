using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


[RequireComponent(typeof(TMP_Text))]
public class LinkHandlerForTMPText : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text _tmpTextBox;



    private void Start()
    {
        _tmpTextBox = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        string code = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (code == "en")
        {
            //   <link=https://www.youtube.com/watch?v=zRsX5S9ndvI&t=166s>YOUTUBE</link>
            // we're in English version
            _tmpTextBox.text = "Econagri – Clean City Tycoon is an initiative of <link=https://wingify.earth/><b>WINGIFY EARTH</b></link> to spread awareness about air pollution. Find out more about  <link=https://wingify.earth/><b>WINGIFY EARTH</b></link> on our <link=https://www.youtube.com/channel/UCO9q3KLbJEtGe6CgbW1uT_w><b>YOUTUBE</b></link> and <link=https://www.instagram.com/wingifyearth/><b>INSTAGRAM</b></link> channels. Econagari is built by <link=https://bakarmax.com/><b>Bakarmax</b></link>. Find us on <link=https://www.instagram.com/bakarmax/><b>INSTAGRAM</b></link>.";


        }
        else
        {
            // we're in Hindi version
            _tmpTextBox.text = "इकोनगरी - क्लीन  िसटी टाइकून, वायु पॉलुशन के बारे में जागरुकता फैलाने के  िलए <link=https://wingify.earth/><b>Wingify Earth</b></link> की एक पहल है। <link=https://wingify.earth/><b>Wingify Earth</b></link> के बारे में और जानकारी हमारे <link=https://www.youtube.com/channel/UCO9q3KLbJEtGe6CgbW1uT_w><b>Youtube</b></link> और <link=https://www.instagram.com/wingifyearth/><b>Instagram</b></link> चैनल पर पाएं। इकोनगरी का  िवकास <link=https://bakarmax.com/><b>Bakarmax</b></link> द्वारा  िकया गया है। हमें <link=https://www.instagram.com/bakarmax/><b>Instagram</b></link> पर पाए।";

        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int index = TMP_TextUtilities.FindIntersectingLink(_tmpTextBox, Input.mousePosition, null);
            if (index > -1)
            {
                Application.OpenURL(_tmpTextBox.textInfo.linkInfo[index].GetLinkID());
            }
        }
    }
}