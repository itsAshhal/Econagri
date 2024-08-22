using Gameplay;
using UnityEngine;

namespace Utility
{
    public static class Events
    {
        public class PieceEvent : UnityEngine.Events.UnityEvent<Piece, int, Vector2Int, bool>
        {
        }

        public class GameOverEvent : UnityEngine.Events.UnityEvent<bool, int, int, bool>
        {
        }
        
        public class FloatEvent : UnityEngine.Events.UnityEvent<float>
        {
        }

        public static readonly GameOverEvent OnGameOver = new();
        public static readonly PieceEvent OnPiecePlaced = new();
        public static readonly UnityEngine.Events.UnityEvent OnPieceNotSnapped = new();
        public static readonly UnityEngine.Events.UnityEvent OnLanguageSelected = new();
        public static readonly FloatEvent OnVolumeChanged = new();
        public static readonly UnityEngine.Events.UnityEvent OnUndo = new();
    }
}