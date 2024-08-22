using UnityEngine;

namespace Gameplay
{
    public class GameData: MonoBehaviour
    {
        public static Language language = Language.English;
        public static GameState gameState = GameState.MainMenu;
        public static bool hasLoadedOnce = false;
    }

    public enum Language
    {
        English,
        Hindi
    }

    public enum GameState
    {
        MainMenu,
        Gameplay,
        Win,
        Lose
    }
}