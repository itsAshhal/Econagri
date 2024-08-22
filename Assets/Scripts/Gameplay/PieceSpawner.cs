using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace Gameplay
{
    public class PieceSpawner : MonoBehaviour
    {
        public Tile tilePrefab;
        public Tile blockPrefab;
        public Bar barPrefab;

        public Transform[] spawnPoints;
        public PieceProperties[] initialPieceProperties;
        [SerializeField] private TileProperties wasteTile;
        [SerializeField] private HexGrid hexGrid;

        [SerializeField] private Image commercialImage;
        [SerializeField] private Image industrialImage;

        [SerializeField] private float alphaForCommercial;

        private Piece[] _spawnedPieces;
        private PieceType? _commercialIndustrialMergeMarker = null;

        private PieceProperties _undoPiece = null;
        private int _undoPieceIndex = 0;
        private PieceType? _undoCommercialIndustrialMergeMarker = null;

        private void Awake()
        {
            Events.OnUndo.AddListener(OnUndo);
        }

        private void Start()
        {
            _spawnedPieces = new Piece[spawnPoints.Length];
            UpdateComInduMergeMarkerText();

            for (var i = 0; i < spawnPoints.Length; i++)
            {
                SpawnPiece(i, initialPieceProperties[i]);
            }
        }

        private void OnUndo()
        {
            if (_undoPiece != null)
            {
                Destroy(_spawnedPieces[_undoPieceIndex].gameObject);
                SpawnPiece(_undoPieceIndex, _undoPiece);
                _undoPiece = null;
            }

            _commercialIndustrialMergeMarker = _undoCommercialIndustrialMergeMarker;
            UpdateComInduMergeMarkerText();
        }

        public void UpdateComInduMergeMarkerText()
        {
            if (_commercialIndustrialMergeMarker == null)
            {
                commercialImage.color = new Color(commercialImage.color.r, commercialImage.color.g,
                    commercialImage.color.b, alphaForCommercial);
                industrialImage.color = new Color(industrialImage.color.r, industrialImage.color.g,
                    industrialImage.color.b, alphaForCommercial);
            }

            if (_commercialIndustrialMergeMarker == PieceType.Commercial)
            {
                commercialImage.color = new Color(commercialImage.color.r, commercialImage.color.g,
                    commercialImage.color.b, 1);
                industrialImage.color = new Color(industrialImage.color.r, industrialImage.color.g,
                    industrialImage.color.b, alphaForCommercial);
            }
            else
            {
                if (_commercialIndustrialMergeMarker == PieceType.Industrial)
                {
                    commercialImage.color = new Color(commercialImage.color.r, commercialImage.color.g,
                        commercialImage.color.b, alphaForCommercial);
                    industrialImage.color = new Color(industrialImage.color.r, industrialImage.color.g,
                        industrialImage.color.b, 1);
                }
            }
        }

        public Piece SpawnPiece(int slotIndex, PieceProperties properties)
        {
            Piece piecePrefab = properties.IsBar() ? barPrefab : tilePrefab;
            if (!properties.IsBar())
            {
                var tileProperties = (TileProperties)properties;
                if (tileProperties.appearAsMerged)
                {
                    piecePrefab = blockPrefab;
                }
            }

            var piece = Instantiate(piecePrefab, spawnPoints[slotIndex].position, Quaternion.identity);
            piece.properties = properties;
            piece.Init();
            
            PieceSetLayer(piece.gameObject, 8);
            _spawnedPieces[slotIndex] = piece;
            return piece;
        }

        public void PickPiece(int slotIndex)
        {
            PieceSetLayer(_spawnedPieces[slotIndex].gameObject, _spawnedPieces[slotIndex].startingLayer);
            _spawnedPieces[slotIndex] = null;
        }

        public static void PieceSetLayer(GameObject piece, int layer)
        {
            piece.gameObject.layer = layer;
            foreach (Transform child in piece.transform)
            {
                child.gameObject.layer = layer;

                var hasChildren = child.GetComponentInChildren<Transform>();
                if (hasChildren != null)
                    PieceSetLayer(child.gameObject, layer);
            }
        }


        public void ReturnPiece(int slotIndex, Piece piece)
        {
            PieceSetLayer(piece.gameObject, 8);
            piece.SetPosition(spawnPoints[slotIndex].position, false, false);
            if (piece is Bar bar)
            {
                bar.SetBigScale();
                bar.SetOffsetPosition();
            }

            _spawnedPieces[slotIndex] = piece;
        }

        public int GetSlotIndex(Piece tile)
        {
            for (var i = 0; i < _spawnedPieces.Length; i++)
            {
                if (_spawnedPieces[i] == tile)
                    return i;
            }

            return -1;
        }

        public void SpawnNextPiece(int slotIndex, int mergeIndex, Piece currentPiece, bool isStackComplete)
        {
            var isMerged = mergeIndex != -1;
            _undoCommercialIndustrialMergeMarker = _commercialIndustrialMergeMarker;
            var nextPiece = GetNextPiece(currentPiece.properties, isMerged, isStackComplete);

            _undoPieceIndex = slotIndex;
            _undoPiece = currentPiece.properties;

            SpawnPiece(slotIndex, nextPiece);
            if (!IsValidMoveLeft())
            {
                GameManager.Instance.CheckForGameOver(false);
            }
        }

        public bool IsValidMoveLeft()
        {
            foreach (var piece in _spawnedPieces)
            {
                var tile = piece as Tile;
                if (tile != null)
                {
                    if (hexGrid.IsValidMoveLeftTile(tile))
                    {
                        return true;
                    }
                }
                else
                {
                    var bar = piece as Bar;
                    if (hexGrid.IsValidMoveLeftBar(bar))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private PieceProperties GetNextPiece(PieceProperties currentPiece, bool isMerged, bool isStackComplete)
        {
            if (isStackComplete)
            {
                if (currentPiece.type == PieceType.Residential || currentPiece.type == PieceType.Agricultural
                                                               || currentPiece.type == PieceType.Industrial ||
                                                               currentPiece.type == PieceType.Commercial)
                {
                    var tile = currentPiece as TileProperties;
                    return tile.nextPieceInCaseOfStackMerge;
                }
            }

            if (!isMerged) return currentPiece.nextPiece;
            var tileProperties = currentPiece as TileProperties;

            if (currentPiece.type is PieceType.Commercial or PieceType.Industrial)
            {
                if (_commercialIndustrialMergeMarker == null)
                {
                    _commercialIndustrialMergeMarker = currentPiece.type;
                    UpdateComInduMergeMarkerText();

                    return currentPiece.type == PieceType.Industrial ? wasteTile : currentPiece.nextPiece;
                }

                if (_commercialIndustrialMergeMarker == currentPiece.type)
                {
                    return currentPiece.type == PieceType.Industrial ? wasteTile : currentPiece.nextPiece;
                }

                _commercialIndustrialMergeMarker = null;
                UpdateComInduMergeMarkerText();
                return tileProperties.nextPieceInCaseOfMerge;
            }

            return tileProperties.nextPieceInCaseOfMerge;
        }
    }
}