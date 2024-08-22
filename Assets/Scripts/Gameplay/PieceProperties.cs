using UnityEngine;

namespace Gameplay
{
    public abstract class PieceProperties : ScriptableObject
    {
        public Gameplay.PieceType type;
        public PieceProperties nextPiece;
        public bool IsBar()
        {
            return this as BarProperties;
        }
    }
}