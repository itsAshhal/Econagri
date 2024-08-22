using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "Tile", menuName = "New Tile", order = 0)]
    public class TileProperties : PieceProperties
    {
        public bool canMerge = true;
        public bool appearAsMerged = false;
        
        public float AQIChanges;
        public float GDPChanges;

        public float AQIChangesStack;
        public float GDPChangesStack;
        public GameObject assets;
        public GameObject mergeAssets;

        public GameObject[] stackAssets;

        public float GDPStackMergeBonus;
        
        public float AQIChangesInCaseOfMerge;
        public float GDPChangesInCaseOfMerge;
        
        public float AQIChangeForAdjacentTiles;
        public float AQIChangeForAdjacentMergedTiles;
    
        public Material material;
        public Color topColor;
        public Color bottomColor;
        public PieceProperties nextPieceInCaseOfMerge;
        public PieceProperties nextPieceInCaseOfStackMerge;
    }
}