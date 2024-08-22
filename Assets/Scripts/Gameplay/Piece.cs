using UnityEngine;

namespace Gameplay
{
    public abstract class Piece : MonoBehaviour
    {
        
        public PieceProperties properties; 
        public abstract void SetPosition(Vector3 position, bool final, bool snapping);
        public abstract void Init();
        public int startingLayer;
    }
}