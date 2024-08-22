using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Econagri.Localization
{
    [CreateAssetMenu(fileName = "NewUsedFont", menuName = "Scriptables/NewUsedFont")]
    public class UsedFontScriptable : ScriptableObject
    {
        public TMP_FontAsset EnglishFont;
        public TMP_FontAsset HindiFont;
    }
}
