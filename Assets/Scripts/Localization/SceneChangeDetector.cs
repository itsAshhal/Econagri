using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SceneChangeDetector", menuName = "Scriptables/New Scene Change Detector")]
public class SceneChangeDetector : ScriptableObject
{
    /// <summary>
    /// So when the game loades for the first time everything works and after that
    /// we won't have any interference for changing languages on different scene changes
    /// </summary>
    public bool IsChangedAlready = false;
}
