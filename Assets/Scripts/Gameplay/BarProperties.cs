using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "Bar", menuName = "New Bar", order = 0)]
    public class BarProperties : PieceProperties
    {
        public int numberOfBars = 4; 
        
        public float AQIChangesPerBar;
        public float GDPChangesPerBar;
        
        public Material material;
    }
}