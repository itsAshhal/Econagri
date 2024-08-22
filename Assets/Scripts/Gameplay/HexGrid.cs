using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using Utility;

namespace Gameplay
{
    public class HexGrid : MonoBehaviour
    {
        [SerializeField] private float hexRadius = 1f;
        [SerializeField] private Vector2Int gridSize = new(10, 10);
        [SerializeField] private Tile centerTile;
        [SerializeField] private GameObject ghostTilePrefab;
        [SerializeField] private GameObject ghostBarPrefab;
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private GameObject roadTrianglePrefab;

        [SerializeField] private Mesh roadTriangleThreeSides;
        [SerializeField] private Mesh roadTriangleTwoSides;

        [SerializeField] private Mesh metroTriangleThreeSides;
        [SerializeField] private Mesh metroTriangleTwoSides;

        [SerializeField] private GameObject metroTrianglePrefab;
        [SerializeField] private PieceSpawner pieceSpawner;
        [SerializeField] private Dragging dragging;
        [SerializeField] private BarProperties wasteBarProperties;

        private Grid _grid;
        private Vector2Int _center;

        private Dictionary<Vector2Int, GameObject> _availableHexes;
        private Dictionary<Vector2Int, GameObject> _stackedAvailableHexes;

        private Dictionary<(Vector2Int, Vector2Int), GameObject> _availableBars;

        private List<(Vector2Int, Vector2Int, Vector2Int)> _currentMergedHexes;
        private List<(Vector2Int, Vector2Int, Vector2Int)> _currentStackedMergedHexes;

        private Dictionary<Vector2Int, Tile> _filledTileProperties;
        private Dictionary<Vector2Int, Tile> _stackedTiles;
        private Dictionary<(Vector2Int, Vector2Int), Bar> _filledBarProperties;

        private Dictionary<(Vector2Int, Vector2Int), GameObject> _filledRoads;
        private Dictionary<(Vector2Int, Vector2Int, Vector2Int), GameObject> _filledRoadTriangles;

        private Dictionary<(Vector2Int, Vector2Int, Vector2Int), GameObject> _metroAddedTriangles;

        public Tile[] currentTilesShowing;

        [SerializeField] private Material metroMaterial1;
        [SerializeField] private Material metroMaterial2;

        // Undo variables
        private List<(Vector2Int, Tile)> _undoFilledTiles;
        private List<(Vector2Int, Tile)> _undoStackedTiles;

        private GameObject _blockTileAdded;

        private List<((Vector2Int, Vector2Int), Bar)> _undoFilledBarsDeleted;
        private List<((Vector2Int, Vector2Int), Bar)> _undoFilledBarsAdded;

        private List<(Vector2Int, Vector2Int, Vector2Int)> _undoMergedHexes;
        private List<(Vector2Int, Vector2Int, Vector2Int)> _undoStackedMergedHexes;

        private List<(Vector2Int, GameObject)> _undoAvailableHexesAdded;
        private List<(Vector2Int, GameObject)> _undoAvailableHexesDeleted;

        private List<(Vector2Int, GameObject)> _undoStackedAvailableHexesAdded;
        private List<(Vector2Int, GameObject)> _undoStackedAvailableHexesDeleted;

        private Dictionary<(Vector2Int, Vector2Int), GameObject> _undoAvailableBarsAdded;
        private Dictionary<(Vector2Int, Vector2Int), GameObject> _undoAvailableBarsDelete;

        private Dictionary<(Vector2Int, Vector2Int), GameObject> _undoRoadsAdded;
        private Dictionary<(Vector2Int, Vector2Int), GameObject> _undoRoadsDeleted;

        private Dictionary<(Vector2Int, Vector2Int, Vector2Int), GameObject> _undoMetroAddedTriangles;

        private Dictionary<(Vector2Int, Vector2Int, Vector2Int), GameObject> _undoRoadTrianglesAdded;
        private Dictionary<(Vector2Int, Vector2Int, Vector2Int), GameObject> _undoRoadTrianglesDeleted;

        private bool _undoDidActivateZoningLaw = false;
        private bool _undoActivateGreenLaw = false;

        private void Awake()
        {
            _grid = new Grid(transform.position, gridSize, hexRadius);
            _filledTileProperties = new Dictionary<Vector2Int, Tile>();
            _stackedTiles = new Dictionary<Vector2Int, Tile>();

            _filledBarProperties = new Dictionary<(Vector2Int, Vector2Int), Bar>();

            _center = new Vector2Int(gridSize.x / 2, gridSize.y / 2);
            _availableHexes = new Dictionary<Vector2Int, GameObject>();
            _stackedAvailableHexes = new Dictionary<Vector2Int, GameObject>();
            _availableBars = new Dictionary<(Vector2Int, Vector2Int), GameObject>();

            _currentMergedHexes = new List<(Vector2Int, Vector2Int, Vector2Int)>();
            _currentStackedMergedHexes = new List<(Vector2Int, Vector2Int, Vector2Int)>();

            _filledRoads = new Dictionary<(Vector2Int, Vector2Int), GameObject>();
            _filledRoadTriangles = new Dictionary<(Vector2Int, Vector2Int, Vector2Int), GameObject>();

            _metroAddedTriangles = new Dictionary<(Vector2Int, Vector2Int, Vector2Int), GameObject>();

            ResetUndoVariables(false);
            FillHex(_center, centerTile, -1, new Vector2Int[] { }, false);
            ResetUndoVariables(false);
            centerTile.Init();
            centerTile.SetPosition(_grid.GetWorldPosition(_center), true, true);

            Events.OnPieceNotSnapped.AddListener(OnHoverStop);
            Events.OnUndo.AddListener(OnUndo);
        }

        private void ResetUndoVariables(bool withDelete)
        {
            if (withDelete)
            {
                if (_undoFilledBarsDeleted != null)
                {
                    foreach (var bar in _undoFilledBarsDeleted)
                    {
                        Destroy(bar.Item2.gameObject);
                    }
                }

                if (_undoAvailableHexesDeleted != null)
                {
                    foreach (var hex in _undoAvailableHexesDeleted)
                    {
                        Destroy(hex.Item2);
                    }
                }

                if (_undoStackedAvailableHexesDeleted != null)
                {
                    foreach (var hex in _undoStackedAvailableHexesDeleted)
                    {
                        Destroy(hex.Item2);
                    }
                }

                if (_undoAvailableBarsDelete != null)
                {
                    foreach (var bar in _undoAvailableBarsDelete)
                    {
                        Destroy(bar.Value);
                    }
                }
            }

            _blockTileAdded = null;
            _undoFilledTiles = new List<(Vector2Int, Tile)>();
            _undoStackedTiles = new List<(Vector2Int, Tile)>();

            _undoFilledBarsDeleted = new List<((Vector2Int, Vector2Int), Bar)>();
            _undoFilledBarsAdded = new List<((Vector2Int, Vector2Int), Bar)>();

            _undoMergedHexes = new List<(Vector2Int, Vector2Int, Vector2Int)>();
            _undoStackedMergedHexes = new List<(Vector2Int, Vector2Int, Vector2Int)>();

            _undoAvailableHexesAdded = new List<(Vector2Int, GameObject)>();
            _undoAvailableHexesDeleted = new List<(Vector2Int, GameObject)>();

            _undoStackedAvailableHexesAdded = new List<(Vector2Int, GameObject)>();
            _undoStackedAvailableHexesDeleted = new List<(Vector2Int, GameObject)>();

            _undoAvailableBarsAdded = new Dictionary<(Vector2Int, Vector2Int), GameObject>();
            _undoAvailableBarsDelete = new Dictionary<(Vector2Int, Vector2Int), GameObject>();

            _undoRoadsAdded = new Dictionary<(Vector2Int, Vector2Int), GameObject>();
            _undoRoadsDeleted = new Dictionary<(Vector2Int, Vector2Int), GameObject>();

            _undoRoadTrianglesAdded = new Dictionary<(Vector2Int, Vector2Int, Vector2Int), GameObject>();
            _undoRoadTrianglesDeleted = new Dictionary<(Vector2Int, Vector2Int, Vector2Int), GameObject>();

            _undoDidActivateZoningLaw = false;
            _undoActivateGreenLaw = false;

            _undoMetroAddedTriangles = new Dictionary<(Vector2Int, Vector2Int, Vector2Int), GameObject>();
        }

        private void OnUndo()
        {
            if (_undoMergedHexes.Count != 0)
            {
                foreach (var undoMergedHex in _undoMergedHexes)
                {
                    var tile1 = _filledTileProperties[undoMergedHex.Item1];
                    var tile2 = _filledTileProperties[undoMergedHex.Item2];
                    var tile3 = _filledTileProperties[undoMergedHex.Item3];

                    tile1.SetToUnmerged();
                    tile2.Show();
                    tile3.Show();

                    _currentMergedHexes.Remove(undoMergedHex);
                }
            }

            if (_blockTileAdded != null)
            {
                Destroy(_blockTileAdded);
            }

            if (_undoMetroAddedTriangles.Count != 0)
            {
                foreach (var undoMetroAddedTriangle in _undoMetroAddedTriangles)
                {
                    _metroAddedTriangles.Remove(undoMetroAddedTriangle.Key);
                    Destroy(undoMetroAddedTriangle.Value);
                }
            }

            if (_undoStackedMergedHexes.Count != 0)
            {
                foreach (var undoStackedMergedHex in _undoStackedMergedHexes)
                {
                    _currentStackedMergedHexes.Remove(undoStackedMergedHex);
                }
            }

            if (_undoFilledTiles.Count != 0)
            {
                foreach (var tile in _undoFilledTiles)
                {
                    _filledTileProperties.Remove(tile.Item1);
                    Destroy(tile.Item2.gameObject);
                }
            }

            if (_undoStackedTiles.Count != 0)
            {
                foreach (var tile in _undoStackedTiles)
                {
                    var mergedHexes = _currentMergedHexes.FirstOrDefault(x =>
                        x.Item1 == tile.Item1 || x.Item2 == tile.Item1 || x.Item3 == tile.Item1);

                    var tile1 = _filledTileProperties[mergedHexes.Item1];

                    tile1.GoToPreviousStack();
                    tile.Item2.Show();
                    _stackedTiles.Remove(tile.Item1);
                    Destroy(tile.Item2.gameObject);
                }
            }


            if (_undoFilledBarsAdded.Count != 0)
            {
                foreach (var bar in _undoFilledBarsAdded)
                {
                    _filledBarProperties.Remove(bar.Item1);
                    Destroy(bar.Item2.gameObject);
                }
            }

            if (_undoFilledBarsDeleted.Count != 0)
            {
                foreach (var bar in _undoFilledBarsDeleted)
                {
                    _filledBarProperties.Add(bar.Item1, bar.Item2);
                    bar.Item2.gameObject.SetActive(true);
                }
            }

            if (_undoAvailableHexesAdded.Count != 0)
            {
                foreach (var undoAvailableHex in _undoAvailableHexesAdded)
                {
                    _availableHexes.Remove(undoAvailableHex.Item1);
                    Destroy(undoAvailableHex.Item2);
                }
            }

            if (_undoAvailableHexesDeleted.Count != 0)
            {
                foreach (var undoAvailableHex in _undoAvailableHexesDeleted)
                {
                    _availableHexes.Add(undoAvailableHex.Item1, undoAvailableHex.Item2);
                    undoAvailableHex.Item2.SetActive(true);
                }
            }

            if (_undoStackedAvailableHexesAdded.Count != 0)
            {
                foreach (var undoStackedAvailableHex in _undoStackedAvailableHexesAdded)
                {
                    _stackedAvailableHexes.Remove(undoStackedAvailableHex.Item1);
                    Destroy(undoStackedAvailableHex.Item2);
                }
            }

            if (_undoStackedAvailableHexesDeleted.Count != 0)
            {
                foreach (var undoStackedAvailableHex in _undoStackedAvailableHexesDeleted)
                {
                    _stackedAvailableHexes.Add(undoStackedAvailableHex.Item1, undoStackedAvailableHex.Item2);
                    undoStackedAvailableHex.Item2.SetActive(true);
                }
            }

            if (_undoAvailableBarsAdded.Count != 0)
            {
                foreach (var undoAvailableBar in _undoAvailableBarsAdded)
                {
                    _availableBars.Remove(undoAvailableBar.Key);
                    Destroy(undoAvailableBar.Value);
                }
            }

            if (_undoAvailableBarsDelete.Count != 0)
            {
                foreach (var undoAvailableBar in _undoAvailableBarsDelete)
                {
                    _availableBars.Add(undoAvailableBar.Key, undoAvailableBar.Value);
                    undoAvailableBar.Value.SetActive(true);
                }
            }

            if (_undoRoadsAdded.Count != 0)
            {
                foreach (var undoRoad in _undoRoadsAdded)
                {
                    _filledRoads.Remove(undoRoad.Key);
                    Destroy(undoRoad.Value);
                }
            }

            if (_undoRoadsDeleted.Count != 0)
            {
                foreach (var undoRoad in _undoRoadsDeleted)
                {
                    _filledRoads.Add(undoRoad.Key, undoRoad.Value);
                    undoRoad.Value.SetActive(true);
                }
            }

            if (_undoRoadTrianglesAdded.Count != 0)
            {
                foreach (var undoRoadTriangle in _undoRoadTrianglesAdded)
                {
                    _filledRoadTriangles.Remove(undoRoadTriangle.Key);
                    Destroy(undoRoadTriangle.Value);
                }
            }

            if (_undoRoadTrianglesDeleted.Count != 0)
            {
                foreach (var undoRoadTriangle in _undoRoadTrianglesDeleted)
                {
                    _filledRoadTriangles.Add(undoRoadTriangle.Key, undoRoadTriangle.Value);
                    undoRoadTriangle.Value.SetActive(true);
                }
            }

            if (_undoDidActivateZoningLaw)
            {
                GameManager.Instance.DisableZoningLaw();
            }

            if (_undoActivateGreenLaw)
            {
                GameManager.Instance.DisableGreenTech();
            }

            ResetUndoVariables(false);
            HidePossibleHexes();
            HidePossibleBars();
        }

        public float GetAdjacencyScore(Vector2Int hexCoord, PieceType type)
        {
            return _grid.GetAdjacencyScore(hexCoord, type, _filledTileProperties);
        }

        public (int, int) GetAdjacentTileNumbersForMerge(int mergeIndex)
        {
            return Grid.GetAdjacentTileNumbersForMerge(mergeIndex, _currentMergedHexes, _filledTileProperties);
        }

        public (Vector2Int, Vector2Int) GetBarPosition(Bar bar)
        {
            return _grid.GetBarPosition(bar, _filledBarProperties);
        }
        ////////////////////////////////////////// Hexes /////////////////////////////////////////////////////

        /// Ghost Hexes
        public void HidePossibleHexes()
        {
            foreach (var availableHex in _availableHexes)
            {
                availableHex.Value.SetActive(false);
            }

            foreach (var stackedAvailableHex in _stackedAvailableHexes)
            {
                stackedAvailableHex.Value.SetActive(false);
            }
        }

        private void CreateGhostTiles(Vector2Int hexCoord, Vector2Int[] toIgnore, bool isStacked)
        {
            var offsets = Grid.GetNeighborOffsets(hexCoord.x);
            foreach (var offset in offsets)
            {
                var neighbor = hexCoord + offset;

                if (!_filledTileProperties.ContainsKey(neighbor) && _grid.InGridBounds(neighbor) &&
                    !_availableHexes.ContainsKey(neighbor) && !toIgnore.Contains(neighbor))
                {
                    var ghostTile = SpawnGhostTile(neighbor);
                    _availableHexes.Add(neighbor, ghostTile);
                    _undoAvailableHexesAdded.Add((neighbor, ghostTile));
                }
            }

            HidePossibleHexes();
        }

        private GameObject SpawnGhostTile(Vector2Int coord)
        {
            var ghostTile = Instantiate(ghostTilePrefab, _grid.GetWorldPosition(coord), Quaternion.identity);
            return ghostTile;
        }

        public bool IsNextToSameTile(Tile tile)
        {
            var hexCoord = _grid.WorldToGrid(tile.transform.position);
            return Grid.IsNextToTileType(hexCoord, tile.properties.type, _filledTileProperties);
        }

        public bool IsStackingTile(Tile tile)
        {
            var hexCoord = _grid.WorldToGrid(tile.transform.position);
            return _stackedAvailableHexes.ContainsKey(hexCoord);
        }

        public void ShowPossibleHexes(TileProperties tileProperties)
        {
            var currentTileType = tileProperties.type;
            HidePossibleHexes();
            var stackedTilesToShow = GetStackedGhostTilesToShow(currentTileType);
            var tilesToShow = GetGhostTilesToShow(tileProperties);
            foreach (var tile in tilesToShow)
            {
                tile.SetActive(true);
            }

            foreach (var tile in stackedTilesToShow)
            {
                tile.SetActive(true);
            }
        }

        private List<GameObject> GetGhostTilesToShow(TileProperties tileProperties)
        {
            var currentTileType = tileProperties.type;
            var tilesToShow = new List<GameObject>();

            if (GameManager.Instance.IsZoningLawActive())
            {
                if (currentTileType is PieceType.Agricultural or PieceType.Residential)
                {
                    foreach (var availableHex in _availableHexes)
                    {
                        if (!Grid.IsNextToTileType(availableHex.Key, PieceType.Industrial, _filledTileProperties) &&
                            !Grid.IsNextToTileType(availableHex.Key, PieceType.Waste, _filledTileProperties))
                        {
                            tilesToShow.Add(availableHex.Value);
                        }
                    }

                    return tilesToShow;
                }

                if (currentTileType == PieceType.Industrial || currentTileType == PieceType.Waste)
                {
                    foreach (var availableHex in _availableHexes)
                    {
                        if (!Grid.IsNextToTileType(availableHex.Key, PieceType.Agricultural, _filledTileProperties) &&
                            !Grid.IsNextToTileType(availableHex.Key, PieceType.Residential, _filledTileProperties))
                        {
                            tilesToShow.Add(availableHex.Value);
                        }
                    }

                    return tilesToShow;
                }
            }

            foreach (var availableHex in _availableHexes)
            {
                tilesToShow.Add(availableHex.Value);
                availableHex.Value.SetActive(true);
            }

            return tilesToShow;
        }

        private List<GameObject> GetStackedGhostTilesToShow(PieceType tileType)
        {
            var tilesToShow = new List<GameObject>();

            if (tileType != PieceType.Agricultural
                && tileType != PieceType.Residential
                && tileType != PieceType.Commercial
                && tileType != PieceType.Industrial
               ) return tilesToShow;

            foreach (var stackedAvailableHex in _stackedAvailableHexes)
            {
                var stackedTileType = _filledTileProperties[stackedAvailableHex.Key].properties.type;
                if (stackedTileType == tileType)
                {
                    if (ShouldHaveStackingAvailable(stackedAvailableHex.Key, tileType))
                    {
                        tilesToShow.Add(stackedAvailableHex.Value);
                    }
                }
            }

            return tilesToShow;
        }

        public void GhostPiecePlacement(Vector3 worldPosition, Piece piece)
        {
            var (aqiChange, gdpChange) = (0f, 0f);
            if (piece.properties.IsBar())
            {
                (aqiChange, gdpChange) = GhostBarPlacement(worldPosition, piece as Bar);
            }
            else
            {
                (aqiChange, gdpChange) = GhostTilePlacement(worldPosition, piece as Tile);
            }

            GameManager.Instance.UpdateGhostScore(aqiChange, gdpChange);
        }

        private (float, float) GhostTilePlacement(Vector3 worldPosition, Tile tile)
        {
            OnHoverStop();
            var hexCoord = _grid.WorldToGrid(worldPosition);

            if (_filledTileProperties.ContainsKey(hexCoord))
            {
                var merge = _grid.CheckForMergedTiles(hexCoord, tile, _stackedTiles, _filledBarProperties,
                    _currentStackedMergedHexes);

                var index = merge.Item1 ? 0 : -1;
                var (stackAqiChange, stackGdpChange) =
                    GameManager.Instance.GetNetScoreChange(tile, index, hexCoord, true);
                return (stackAqiChange, stackGdpChange);
            }

            var mergeResult = _grid.CheckForMergedTiles(hexCoord, tile, _filledTileProperties, _filledBarProperties,
                _currentMergedHexes);
            var mergeIndex = -1;

            var tileProperties = tile.properties as TileProperties;
            if (mergeResult.Item1 && tileProperties.canMerge)
            {
                _currentMergedHexes.Add((mergeResult.Item2, mergeResult.Item3, mergeResult.Item4));
                mergeIndex = _currentMergedHexes.Count - 1;
                _filledTileProperties.Add(hexCoord, tile);
            }

            var (aqiChange, gdpChange) = GameManager.Instance.GetNetScoreChange(tile, mergeIndex, hexCoord, false);

            if (mergeResult.Item1 && tileProperties.canMerge)
            {
                _currentMergedHexes.RemoveAt(_currentMergedHexes.Count - 1);
                _filledTileProperties.Remove(hexCoord);
            }

            return (aqiChange, gdpChange);
        }

        public bool IsValidMoveLeftTile(Tile tile)
        {
            var tileProperties = tile.properties as TileProperties;
            if (tileProperties == null) return false;

            var stackedGhostTilesToShow = GetStackedGhostTilesToShow(tileProperties.type);
            var ghostTilesToShow = GetGhostTilesToShow(tileProperties);

            return ghostTilesToShow.Count != 0 || stackedGhostTilesToShow.Count != 0;
        }

        private bool ShouldHaveStackingAvailable(Vector2Int hexCoord, PieceType pieceType)
        {
            if (pieceType != PieceType.Agricultural
                && pieceType != PieceType.Residential
                && pieceType != PieceType.Commercial
                && pieceType != PieceType.Industrial
               ) return false;


            if (!_filledTileProperties.ContainsKey(hexCoord))
            {
                return false;
            }

            var canMerge = _grid.CheckForMergedTiles(hexCoord, _filledTileProperties[hexCoord], _stackedTiles,
                _filledBarProperties, _currentStackedMergedHexes).Item1;


            var hexes = _currentMergedHexes.FirstOrDefault(x =>
                x.Item1 == hexCoord || x.Item2 == hexCoord || x.Item3 == hexCoord);

            if (hexes == default) return false;
            if (!canMerge) return true;

            var hexesCoords = new List<Vector2Int>
            {
                hexes.Item1,
                hexes.Item2,
                hexes.Item3
            };

            if (pieceType == PieceType.Agricultural)
            {
                foreach (var coord in hexesCoords)
                {
                    if (_grid.DoesHavePieceTypeAdjacent(coord, PieceType.Commercial, _filledTileProperties)
                        || _grid.DoesHavePieceTypeAdjacent(coord, PieceType.Industrial, _filledTileProperties))
                    {
                        Tooltip.I.ShowTooltip(TooltipType.NoStackingAgricultural);
                        return false;
                    }
                }

                return true;
            }

            if (pieceType == PieceType.Residential)
            {
                foreach (var coord in hexesCoords)
                {
                    if (_grid.HasMergedStackedNearBy(coord, _currentStackedMergedHexes))
                    {
                        Tooltip.I.ShowTooltip(TooltipType.NoStackingResidential);
                        return false;
                    }
                }

                return true;
            }

            if (pieceType == PieceType.Commercial)
            {
                foreach (var coord in hexesCoords)
                {
                    if (_grid.DoesHavePieceTypeAdjacent(coord, PieceType.Residential, _filledTileProperties)
                        || _grid.DoesHavePieceTypeAdjacent(coord, PieceType.Commercial, _filledTileProperties,
                            hexesCoords)
                        || _grid.DoesHavePieceTypeAdjacent(coord, PieceType.Unique, _filledTileProperties))
                    {
                        return true;
                    }
                }

                Tooltip.I.ShowTooltip(TooltipType.NoStackingCommercial);
                return false;
            }

            if (pieceType == PieceType.Industrial)
            {
                foreach (var coord in hexesCoords)
                {
                    if (_grid.DoesHavePieceTypeAdjacent(coord, PieceType.Residential, _filledTileProperties)
                        || _grid.DoesHavePieceTypeAdjacent(coord, PieceType.Agricultural, _filledTileProperties)
                        || _grid.DoesHavePieceTypeAdjacent(coord, PieceType.Nature, _filledTileProperties))
                    {
                        Tooltip.I.ShowTooltip(TooltipType.NoStackingIndustrial);
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private void MergeHexes(Vector2Int hexCoord1, Vector2Int hexCoord2, Vector2Int hexCoord3)
        {
            _currentMergedHexes.Add((hexCoord1, hexCoord2, hexCoord3));
            _undoMergedHexes.Add((hexCoord1, hexCoord2, hexCoord3));
            var angleToRotate = CalculateRotationAngle(hexCoord1, hexCoord2, hexCoord3);

            var tile1 = _filledTileProperties[hexCoord1];
            var tile2 = _filledTileProperties[hexCoord2];
            var tile3 = _filledTileProperties[hexCoord3];


            tile1.SetToMerged(angleToRotate);
            tile2.Hide();
            tile3.Hide();
        }

        private static int CalculateRotationAngle(Vector2Int hexCoord1, Vector2Int hexCoord2, Vector2Int hexCoord3)
        {
            var neighbors = Grid.GetNeighborOffsets(hexCoord1.x);
            var index2 = Array.IndexOf(neighbors, hexCoord2 - hexCoord1);
            var index3 = Array.IndexOf(neighbors, hexCoord3 - hexCoord1);
            var angle2 = index2 * 60;
            return angle2;
        }

        ///// Actual Hexes
        private void FillHex(Vector2Int hexCoord, Tile tile, int selectedSlotIndex, Vector2Int[] neighborsToIgnore,
            bool isStacked = false)
        {
            if (_availableHexes.ContainsKey(hexCoord))
            {
                _undoAvailableHexesDeleted.Add((hexCoord, _availableHexes[hexCoord]));
                _availableHexes[hexCoord].SetActive(false);
                _availableHexes.Remove(hexCoord);
            }
            else
            {
                Debug.Log("Hex not available");
            }

            if (_filledTileProperties.ContainsKey(hexCoord))
            {
                var tileToRemove = _filledTileProperties[hexCoord];
                Debug.Log("Hex already filled" + tileToRemove);
                _filledTileProperties.Remove(hexCoord);
            }
            _filledTileProperties.Add(hexCoord, tile);
            _undoFilledTiles.Add((hexCoord, tile));

            CreateGhostTiles(hexCoord, neighborsToIgnore, false);
            CreateGhostBars(hexCoord);
            if (selectedSlotIndex == -1)
            {
                return;
            }

            var mergeIndex = -1;
            var tileProperties = tile.properties as TileProperties;
            if (tileProperties.canMerge)
            {
                var checkResult = _grid.CheckForMergedTiles(hexCoord, tile, _filledTileProperties, _filledBarProperties,
                    _currentMergedHexes);
                if (checkResult.Item1)
                {
                    MergeHexes(checkResult.Item2, checkResult.Item3, checkResult.Item4);
                    mergeIndex = _currentMergedHexes.Count - 1;
                    RemoveExtraAvailableBars(mergeIndex);
                    RemoveExtraRoadTriangles(mergeIndex);
                    SpawnStackedTiles(mergeIndex, tile.properties.type);

                    switch (tileProperties.type)
                    {
                        case PieceType.Municipal:
                            if (!GameManager.Instance.IsZoningLawActive())
                            {
                                GameManager.Instance.EnableZoningLaw();
                                _undoDidActivateZoningLaw = true;
                            }

                            break;

                        case PieceType.Research:
                            if (!GameManager.Instance.IsGreenTechActive())
                            {
                                GameManager.Instance.EnableGreenTech();
                                _undoActivateGreenLaw = true;
                            }

                            break;
                    }
                }
            }

            pieceSpawner.SpawnNextPiece(selectedSlotIndex, mergeIndex, tile, false);
            Events.OnPiecePlaced.Invoke(tile, mergeIndex, hexCoord, false);
        }

        public void FillHex(Vector3 worldPosition, Tile tile, int selectedSlotIndex)
        {
            ResetUndoVariables(false);
            var hexCoord = _grid.WorldToGrid(worldPosition);

            var tileProperties = tile.properties as TileProperties;
            if (tileProperties.appearAsMerged)
            {
                var (firstOffset, secondOffset) =
                    Grid.GetMergeBlockTileOffsets((int)tile.transform.eulerAngles.y, hexCoord.x);
                var firstNeighbor = hexCoord + firstOffset;
                var secondNeighbor = hexCoord + secondOffset;

                var tile1 = tile.blockTiles[0];
                var tile2 = tile.blockTiles[1];
                var tile3 = tile.blockTiles[2];

                _blockTileAdded = tile1.gameObject.transform.parent.gameObject;

                FillHex(firstNeighbor, tile2, -1, new[] { secondNeighbor, hexCoord }, false);
                FillHex(secondNeighbor, tile3, -1, new[] { firstNeighbor, hexCoord }, false);
                FillHex(hexCoord, tile1, selectedSlotIndex, new[] { firstNeighbor, secondNeighbor }, false);

                RemoveExtraBarsInsideBlock(hexCoord, firstNeighbor, secondNeighbor);

                return;
            }

            if (_filledTileProperties.ContainsKey(hexCoord))
            {
                FillStackedHex(hexCoord, tile, selectedSlotIndex);
                return;
            }

            FillHex(hexCoord, tile, selectedSlotIndex, new Vector2Int[] { }, false);
        }

        private void FillStackedHex(Vector2Int hexCoord, Tile tile, int selectedSlotIndex)
        {
            if (_stackedAvailableHexes.ContainsKey(hexCoord))
            {
                _undoStackedAvailableHexesDeleted.Add((hexCoord, _stackedAvailableHexes[hexCoord]));
                _stackedAvailableHexes[hexCoord].SetActive(false);
                _stackedAvailableHexes.Remove(hexCoord);
            }

            var currentMergeIndex = _currentMergedHexes.FindIndex(x =>
                x.Item1 == hexCoord || x.Item2 == hexCoord || x.Item3 == hexCoord);

            if (currentMergeIndex == -1) return;

            var tileAtFirst = _filledTileProperties[_currentMergedHexes[currentMergeIndex].Item1];
            tileAtFirst.AddToStack();

            _stackedTiles.Add(hexCoord, tile);
            _undoStackedTiles.Add((hexCoord, tile));
            tile.Hide();

            var checkResults = _grid.CheckForMergedTiles(hexCoord, tile, _stackedTiles, _filledBarProperties,
                _currentStackedMergedHexes);

            if (checkResults.Item1)
            {
                _currentStackedMergedHexes.Add((checkResults.Item2, checkResults.Item3, checkResults.Item4));
                _undoStackedMergedHexes.Add((checkResults.Item2, checkResults.Item3, checkResults.Item4));
            }

            pieceSpawner.SpawnNextPiece(selectedSlotIndex, -1, tile, checkResults.Item1);
            Events.OnPiecePlaced.Invoke(tile, -1, hexCoord, true);
        }

        private void SpawnStackedTiles(int mergeIndex, PieceType pieceType)
        {
            if (mergeIndex == -1) return;
            if (pieceType != PieceType.Agricultural
                && pieceType != PieceType.Residential
                && pieceType != PieceType.Commercial
                && pieceType != PieceType.Industrial
               ) return;

            var hexes = _currentMergedHexes[mergeIndex];
            var positions = new List<Vector2Int>
            {
                hexes.Item1,
                hexes.Item2,
                hexes.Item3
            };

            var offsetForHeight = new Vector3(0f, 0.2f, 0f);
            foreach (var position in positions)
            {
                var ghostTile = SpawnGhostTile(position);
                ghostTile.transform.position += offsetForHeight;

                _stackedAvailableHexes.Add(position, ghostTile);
                _undoStackedAvailableHexesAdded.Add((position, ghostTile));
            }

            HidePossibleHexes();
        }

        public bool CanMergeBlockSnap(Vector3 worldPosition, int angle)
        {
            return _grid.CanMergeBlockSnap(worldPosition, angle, _availableHexes, _filledTileProperties);
        }

        private void OnHoverStop()
        {
            if (currentTilesShowing != null)
            {
                currentTilesShowing = null;
            }
        }

        ////////////////////////////////////////// BAR /////////////////////////////////////////////////////

        ///// Actual Bars
        private void RemoveExtraAvailableBars(int mergeIndex)
        {
            var hex1 = _currentMergedHexes[mergeIndex].Item1;
            var hex2 = _currentMergedHexes[mergeIndex].Item2;
            var hex3 = _currentMergedHexes[mergeIndex].Item3;

            var barsToRemove = new List<(Vector2Int, Vector2Int)>
            {
                (hex1, hex2),
                (hex2, hex3),
                (hex3, hex1),
                (hex2, hex1),
                (hex3, hex2),
                (hex1, hex3)
            };

            foreach (var barToRemove in barsToRemove)
            {
                if (!_availableBars.ContainsKey(barToRemove)) continue;


                if (_undoRoadsAdded.ContainsKey(barToRemove))
                {
                    _undoRoadsAdded.Remove(barToRemove);
                    Destroy(_filledRoads[barToRemove]);
                    _filledRoads.Remove(barToRemove);
                }
                else
                {
                    if (_filledRoads.ContainsKey(barToRemove))
                    {
                        _undoRoadsDeleted.Add(barToRemove, _filledRoads[barToRemove]);
                        _filledRoads[barToRemove].SetActive(false);
                        _filledRoads.Remove(barToRemove);
                    }
                }

                if (_undoAvailableBarsAdded.ContainsKey(barToRemove))
                {
                    _undoAvailableBarsAdded.Remove(barToRemove);
                    Destroy(_availableBars[barToRemove]);
                    _availableBars.Remove(barToRemove);
                }
                else
                {
                    _undoAvailableBarsDelete.Add(barToRemove, _availableBars[barToRemove]);
                    _availableBars[barToRemove].SetActive(false);
                    _availableBars.Remove(barToRemove);
                }
            }
        }

        private void RemoveExtraBarsInsideBlock(Vector2Int hexCoord, Vector2Int firstNeighbor,
            Vector2Int secondNeighbor)
        {
            var barsToRemove = new List<(Vector2Int, Vector2Int)>
            {
                (hexCoord, firstNeighbor),
                (firstNeighbor, hexCoord),

                (hexCoord, secondNeighbor),
                (secondNeighbor, hexCoord),

                (firstNeighbor, secondNeighbor),
                (secondNeighbor, firstNeighbor)
            };

            foreach (var barToRemove in barsToRemove)
            {
                if (!_availableBars.ContainsKey(barToRemove)) continue;
                var bar = _availableBars[barToRemove];
                _availableBars.Remove(barToRemove);
                Destroy(bar);
            }
        }

        public void AddBar(Bar bar, (Vector2Int, Vector2Int) barIndex)
        {
            _filledBarProperties.Add(barIndex, bar);
        }

        public void FillBar(GameObject ghostBar, Bar bar, int selectedSlotIndex, int numberOfBarsLeft)
        {
            var barProperties = bar.properties as BarProperties;
            if (numberOfBarsLeft == barProperties.numberOfBars)
            {
                ResetUndoVariables(false);
            }

            var getIndexForBar = _availableBars.FirstOrDefault(x => x.Value == ghostBar).Key;

            if (getIndexForBar != default && _availableBars.ContainsKey(getIndexForBar))
            {
                _undoAvailableBarsDelete.Add(getIndexForBar, _availableBars[getIndexForBar]);
                _availableBars[getIndexForBar].SetActive(false);
                _availableBars.Remove(getIndexForBar);
            }
            else
            {
                var hitWasteBar = ghostBar.GetComponent<Bar>();
                getIndexForBar = _filledBarProperties.FirstOrDefault(x => x.Value == hitWasteBar).Key;

                if (_filledBarProperties.ContainsKey(getIndexForBar))
                {
                    _filledBarProperties.Remove(getIndexForBar);
                    _undoFilledBarsDeleted.Add((getIndexForBar, hitWasteBar));
                }
            }

            _filledBarProperties.Add(getIndexForBar, bar);
            _undoFilledBarsAdded.Add((getIndexForBar, bar));

            if (selectedSlotIndex == -1)
            {
                return;
            }

            if (numberOfBarsLeft <= 1)
            {
                Events.OnPiecePlaced.Invoke(bar, -1, default, false);
                pieceSpawner.SpawnNextPiece(selectedSlotIndex, -1, bar, false);
                HidePossibleHexes();
            }

            if (barProperties.type == PieceType.Solution)
            {
                CheckAndAddMetroTriangles(getIndexForBar);
                UpdateMetroTriangleDirectionAnMesh();
            }
        }

        ///// Ghost Bars
        private (float, float) GhostBarPlacement(Vector3 worldPosition, Bar bar)
        {
            OnHoverStop();
            var numberOfBarsPlaced = dragging.GetNumberOfBarsPut() + 1;
            var numberOfBarsReplaced = dragging.GetWasteBarsDeleted();

            var isOverWaterBar = dragging.IsOverWasteBar();
            if (isOverWaterBar)
            {
                numberOfBarsReplaced++;
            }

            var barProperties = bar.properties as BarProperties;
            if (barProperties == null) return (0, 0);

            var aqiChange = barProperties.AQIChangesPerBar * numberOfBarsPlaced;
            var gdpChange = barProperties.GDPChangesPerBar * numberOfBarsPlaced;

            if (barProperties.type == PieceType.Waste)
            {
                return (aqiChange, gdpChange);
            }

            if (barProperties.type == PieceType.Solution)
            {
                aqiChange -= wasteBarProperties.AQIChangesPerBar * numberOfBarsReplaced;
                gdpChange -= wasteBarProperties.GDPChangesPerBar * numberOfBarsReplaced;

                return (aqiChange, gdpChange);
            }

            return (0, 0);
        }

        public bool IsValidMoveLeftBar(Bar bar)
        {
            var barProperties = bar.properties as BarProperties;
            if (barProperties == null) return false;

            if (barProperties.type == PieceType.Waste)
            {
                return _grid.CanPlaceNBars(barProperties.numberOfBars, _availableBars);
            }

            // Combine available bars and filled bars
            var allBars = new Dictionary<(Vector2Int, Vector2Int), GameObject>();
            foreach (var availableBar in _availableBars)
            {
                if (allBars.ContainsKey(availableBar.Key)) continue;
                allBars.Add(availableBar.Key, availableBar.Value);
            }

            foreach (var filledBar in _filledBarProperties)
            {
                if (allBars.ContainsKey(filledBar.Key)) continue;
                allBars.Add(filledBar.Key, filledBar.Value.gameObject);
            }

            return _grid.CanPlaceNBars(barProperties.numberOfBars, allBars);
        }

        public void ShowPossibleBars(PieceType pieceType, List<Bar> currentBars)
        {
            HidePossibleBars();
            foreach (var availableBar in _availableBars)
            {
                if (currentBars.Count > 0)
                {
                    foreach (var bar in currentBars)
                    {
                        var bar1 = _filledBarProperties.FirstOrDefault(x => x.Value == bar).Key;
                        if (bar1 == default) continue;
                        if (Grid.IsEdgeConnected(bar1, availableBar.Key))
                        {
                            availableBar.Value.SetActive(true);
                        }
                    }
                }
                else
                {
                    // Show all bars for the first time
                    availableBar.Value.SetActive(true);
                }
            }

            if (pieceType is PieceType.Solution)
            {
                foreach (var filledBar in _filledBarProperties)
                {
                    if (filledBar.Value.properties.type == PieceType.Waste)
                    {
                        if (currentBars.Count > 0)
                        {
                            foreach (var bar in currentBars)
                            {
                                var bar1 = _filledBarProperties.FirstOrDefault(x => x.Value == bar).Key;
                                if (bar1 == default) continue;
                                if (Grid.IsEdgeConnected(bar1, filledBar.Key))
                                {
                                    filledBar.Value.ShowPlaceable();
                                }
                            }
                        }
                        else
                        {
                            filledBar.Value.ShowPlaceable();
                        }
                    }
                }
            }
        }

        public void HidePossibleBars()
        {
            foreach (var availableBar in _availableBars)
            {
                availableBar.Value.SetActive(false);
            }

            foreach (var filledBar in _filledBarProperties)
            {
                if (filledBar.Value.properties.type == PieceType.Waste)
                {
                    filledBar.Value.HidePlaceable();
                }
            }
        }

        public void RemoveBar(Bar barToRemove)
        {
            if (_filledBarProperties.ContainsValue(barToRemove))
            {
                var barToRemoveIndex = _filledBarProperties.FirstOrDefault(x => x.Value == barToRemove).Key;
                _filledBarProperties.Remove(barToRemoveIndex);

                var _newAvailableBar = Instantiate(ghostBarPrefab, barToRemove.transform.position, Quaternion.identity);
                _newAvailableBar.transform.eulerAngles = barToRemove.transform.eulerAngles;
                if (_availableBars.ContainsKey(barToRemoveIndex))
                {
                    _availableBars.Remove(barToRemoveIndex);
                }
                _availableBars.Add(barToRemoveIndex, _newAvailableBar);
                Destroy(barToRemove.gameObject);
            }
        }

        private void CreateGhostBars(Vector2Int hexCoord)
        {
            var offsets = Grid.GetNeighborOffsets(hexCoord.x);
            foreach (var offset in offsets)
            {
                var neighbor = hexCoord + offset;

                if (_availableBars.ContainsKey((neighbor, hexCoord))) continue;
                if (_availableBars.ContainsKey((hexCoord, neighbor))) continue;

                if (_filledBarProperties.ContainsKey((neighbor, hexCoord))) continue;
                if (_filledBarProperties.ContainsKey((hexCoord, neighbor))) continue;

                var barPosition = (_grid.GetWorldPosition(hexCoord) + _grid.GetWorldPosition(neighbor)) / 2f;
                barPosition += Vector3.up * 0.2f;

                var directionToNeighbor = _grid.GetWorldPosition(neighbor) - _grid.GetWorldPosition(hexCoord);
                var barAngle = Vector3.SignedAngle(Vector3.forward, directionToNeighbor.normalized, Vector3.up);

                var ghostBar = Instantiate(ghostBarPrefab, barPosition, Quaternion.identity);
                ghostBar.transform.eulerAngles = new Vector3(0f, barAngle + 90f, 0f);

                if (_availableBars.ContainsKey((neighbor, hexCoord)))
                {
                    _availableBars.Remove((neighbor, hexCoord));
                }
                _availableBars.Add((neighbor, hexCoord), ghostBar);

                if (_undoAvailableBarsAdded.ContainsKey((neighbor, hexCoord)))
                {
                    _undoAvailableBarsAdded.Remove((neighbor, hexCoord));
                }
                _undoAvailableBarsAdded.Add((neighbor, hexCoord), ghostBar);
            }

            CreateRoads(hexCoord);
            HidePossibleBars();
        }

        private void RemoveExtraRoadTriangles(int mergeIndex)
        {
            var hex1 = _currentMergedHexes[mergeIndex].Item1;
            var hex2 = _currentMergedHexes[mergeIndex].Item2;
            var hex3 = _currentMergedHexes[mergeIndex].Item3;

            var trianglesToRemove = new List<(Vector2Int, Vector2Int, Vector2Int)>
            {
                (hex1, hex2, hex3),
                (hex2, hex3, hex1),
                (hex3, hex1, hex2),
                (hex2, hex1, hex3),
                (hex3, hex2, hex1),
                (hex1, hex3, hex2)
            };

            foreach (var triangleToRemove in trianglesToRemove)
            {
                if (_filledRoadTriangles.ContainsKey(triangleToRemove))
                {
                    if (_undoRoadTrianglesAdded.ContainsKey(triangleToRemove))
                    {
                        _undoRoadTrianglesAdded.Remove(triangleToRemove);
                        Destroy(_filledRoadTriangles[triangleToRemove]);
                        _filledRoadTriangles.Remove(triangleToRemove);
                    }
                    else
                    {
                        _undoRoadTrianglesDeleted.Add(triangleToRemove, _filledRoadTriangles[triangleToRemove]);
                        _filledRoadTriangles[triangleToRemove].SetActive(false);
                        _filledRoadTriangles.Remove(triangleToRemove);
                    }
                }
            }
        }


        private void CreateRoads(Vector2Int hexCoord)
        {
            var offsets = Grid.GetNeighborOffsets(hexCoord.x);
            var didCreateRoad = new bool[6];

            var i = 0;
            foreach (var offset in offsets)
            {
                var neighbor = hexCoord + offset;
                if (!_filledRoads.ContainsKey((neighbor, hexCoord)) && !_filledRoads.ContainsKey((hexCoord, neighbor)))
                {
                    didCreateRoad[i] = true;
                    var barPosition = (_grid.GetWorldPosition(hexCoord) + _grid.GetWorldPosition(neighbor)) / 2f;
                    barPosition += Vector3.up * 0.141f;

                    var directionToNeighbor = _grid.GetWorldPosition(neighbor) - _grid.GetWorldPosition(hexCoord);
                    var barAngle = Vector3.SignedAngle(Vector3.forward, directionToNeighbor.normalized, Vector3.up);

                    var road = Instantiate(roadPrefab, barPosition, Quaternion.identity);
                    //Debug.Log($"RoadPrefab Instantiated, the piece name is {PieceAnimationController.Instance.ActivePiece.name}");
                    road.transform.eulerAngles = new Vector3(0f, barAngle + 90f, 0f);

                    if (_filledRoads.ContainsKey((neighbor, hexCoord)))
                    {
                        _filledRoads.Remove((neighbor, hexCoord));
                    }
                    _filledRoads.Add((neighbor, hexCoord), road);
                    if (_undoRoadsAdded.ContainsKey((neighbor, hexCoord)))
                    {
                        _undoRoadsAdded.Remove((neighbor, hexCoord));
                    }
                    _undoRoadsAdded.Add((neighbor, hexCoord), road);
                }

                i++;
            }

            // if two roads are next to each other, create a triangle

            for (int j = 0; j < 6; j++)
            {
                if (didCreateRoad[j] && didCreateRoad[(j + 1) % 6])
                {
                    var neighbor1 = hexCoord + offsets[j];
                    var neighbor2 = hexCoord + offsets[(j + 1) % 6];

                    var triangleCenter = (_grid.GetWorldPosition(hexCoord) + _grid.GetWorldPosition(neighbor1) +
                                          _grid.GetWorldPosition(neighbor2)) / 3f;
                    triangleCenter += Vector3.up * 0.141f;

                    var triangle = Instantiate(roadTrianglePrefab, triangleCenter, Quaternion.identity);
                    triangle.transform.eulerAngles = new Vector3(0f, 60f * j + 120, 0f);

                    if (_filledRoadTriangles.ContainsKey((hexCoord, neighbor1, neighbor2)))
                    {
                        _filledRoadTriangles.Remove((hexCoord, neighbor1, neighbor2));
                    }
                    _filledRoadTriangles.Add((hexCoord, neighbor1, neighbor2), triangle);
                    if (_undoRoadTrianglesAdded.ContainsKey((hexCoord, neighbor1, neighbor2)))
                    {
                        _undoRoadTrianglesAdded.Remove((hexCoord, neighbor1, neighbor2));
                    }
                    _undoRoadTrianglesAdded.Add((hexCoord, neighbor1, neighbor2), triangle);
                }
            }

            CheckAndCorrectTriangleMesh();
        }

        private void CheckAndCorrectTriangleMesh()
        {
            foreach (var triangle in _filledRoadTriangles)
            {
                var containsHex1 = _filledTileProperties.ContainsKey(triangle.Key.Item1);
                var containsHex2 = _filledTileProperties.ContainsKey(triangle.Key.Item2);
                var containsHex3 = _filledTileProperties.ContainsKey(triangle.Key.Item3);


                if (containsHex1 && containsHex2 || containsHex2 && containsHex3 || containsHex3 && containsHex1)
                {
                    triangle.Value.GetComponentInChildren<MeshFilter>().mesh = roadTriangleThreeSides;
                    continue;
                }

                triangle.Value.GetComponentInChildren<MeshFilter>().mesh = roadTriangleTwoSides;
            }
        }

        private void CheckAndAddMetroTriangles((Vector2Int, Vector2Int) index)
        {
            // with the index find the other two Vector2Int, which will be the other two hexes
            var hex1 = index.Item1;
            var hex2 = index.Item2;

            var neighborsIndices1 = Grid.GetNeighborOffsets(hex1.x);
            var neighborsIndices2 = Grid.GetNeighborOffsets(hex2.x);

            var neighbors1 = neighborsIndices1.Select(neighborIndex => hex1 + neighborIndex).ToList();
            var neighbors2 = neighborsIndices2.Select(neighborIndex => hex2 + neighborIndex).ToList();

            var neighbors = neighbors1.Intersect(neighbors2).ToList();

            var barsWithNeighbor1 = new List<(Vector2Int, Vector2Int)>
            {
                (neighbors[0], hex1),
                (neighbors[0], hex2),
                (hex1, neighbors[0]),
                (hex2, neighbors[0]),
            };

            barsWithNeighbor1 = barsWithNeighbor1.Where(_filledBarProperties.ContainsKey).ToList();
            barsWithNeighbor1 = barsWithNeighbor1
                .Where(x => _filledBarProperties[x].properties.type == PieceType.Solution).ToList();

            if (barsWithNeighbor1.Count == 1)
            {
                AddMetroTriangle(hex1, neighbors[0], hex2);
            }

            var barsWithNeighbor2 = new List<(Vector2Int, Vector2Int)>
            {
                (neighbors[1], hex1),
                (neighbors[1], hex2),
                (hex1, neighbors[1]),
                (hex2, neighbors[1]),
            };

            barsWithNeighbor2 = barsWithNeighbor2.Where(_filledBarProperties.ContainsKey).ToList();
            barsWithNeighbor2 = barsWithNeighbor2
                .Where(x => _filledBarProperties[x].properties.type == PieceType.Solution).ToList();

            if (barsWithNeighbor2.Count == 1)
            {
                AddMetroTriangle(hex1, neighbors[1], hex2);
            }
        }

        private void AddMetroTriangle(Vector2Int hexCoord, Vector2Int neighbor1, Vector2Int neighbor2)
        {
            var triangleCenter = (_grid.GetWorldPosition(hexCoord) + _grid.GetWorldPosition(neighbor1) +
                                  _grid.GetWorldPosition(neighbor2)) / 3f;
            triangleCenter += Vector3.up * 1.2206f;
            var triangle = Instantiate(metroTrianglePrefab, triangleCenter, Quaternion.identity);


            var potentialTriangles = new List<(Vector2Int, Vector2Int, Vector2Int)>
            {
                (hexCoord, neighbor1, neighbor2),
                (hexCoord, neighbor2, neighbor1),
                (neighbor1, hexCoord, neighbor2),
                (neighbor1, neighbor2, hexCoord),
                (neighbor2, hexCoord, neighbor1),
                (neighbor2, neighbor1, hexCoord),
            };

            var triangleIndex = potentialTriangles.FirstOrDefault(x => _filledRoadTriangles.ContainsKey(x));
            var roadTriangle = _filledRoadTriangles[triangleIndex];
            var angle = roadTriangle.transform.eulerAngles.y;

            // triangle.transform.eulerAngles = new Vector3(0, angle, 0);
            _metroAddedTriangles.Add((hexCoord, neighbor1, neighbor2), triangle);
            _undoMetroAddedTriangles.Add((hexCoord, neighbor1, neighbor2), triangle);
        }

        private void SetMeshForMetroTriangle(GameObject triangle, bool toThreeSides)
        {
            var meshFilter = triangle.GetComponentInChildren<MeshFilter>();
            meshFilter.mesh = toThreeSides ? metroTriangleThreeSides : metroTriangleTwoSides;
            StartCoroutine(SetMaterialMetroTriangle(triangle, toThreeSides));
        }

        IEnumerator SetMaterialMetroTriangle(GameObject g, bool toThreeSides)
        {
            yield return new WaitForEndOfFrame();
            var meshRenderer = g.GetComponentInChildren<MeshRenderer>();
            meshRenderer.materials = toThreeSides ? new[] { metroMaterial2, metroMaterial1 } : new[] { metroMaterial1, metroMaterial2 };
        }

        public void UpdateTrackToMetroStationIfNeeded(Bar bar)
        {
            if (!IsNotCornerTrack(bar))
            {
                bar.ConvertToMetroStation();
            }
        }

        private bool IsNotCornerTrack(Bar bar)
        {
            // has a track connected to each side

            var barProperties = bar.properties as BarProperties;
            if (barProperties == null) return false;

            var location = GetBarPosition(bar);


            var metros = _filledBarProperties.Where(x => x.Value.properties.type == PieceType.Solution).ToList();

            var neighbors1Offset = Grid.GetNeighborOffsets(location.Item1.x);
            var neighbors2Offset = Grid.GetNeighborOffsets(location.Item2.x);

            var neighbors1 = neighbors1Offset.Select(neighborIndex => location.Item1 + neighborIndex).ToList();
            var neighbors2 = neighbors2Offset.Select(neighborIndex => location.Item2 + neighborIndex).ToList();

            var commonNeighbors = neighbors1.Intersect(neighbors2).ToList();

            var tracksPossibleN1 = new List<(Vector2Int, Vector2Int)>
            {
                (location.Item1, commonNeighbors[0]),
                (commonNeighbors[0], location.Item1),
                (location.Item2, commonNeighbors[0]),
                (commonNeighbors[0], location.Item2),
            };

            var tracksPossibleN2 = new List<(Vector2Int, Vector2Int)>
            {
                (location.Item1, commonNeighbors[1]),
                (commonNeighbors[1], location.Item1),
                (location.Item2, commonNeighbors[1]),
                (commonNeighbors[1], location.Item2),
            };

            var hasTrackN1 = tracksPossibleN1.Any(x => metros.Any(y => y.Key == x));
            var hasTrackN2 = tracksPossibleN2.Any(x => metros.Any(y => y.Key == x));

            return hasTrackN1 && hasTrackN2;
        }

        private void UpdateMetroTriangleDirectionAnMesh()
        {
            foreach (var triangle in _metroAddedTriangles)
            {
                // three potential metros
                var potentialMetro1 = new List<(Vector2Int, Vector2Int)>
                {
                    (triangle.Key.Item1, triangle.Key.Item2),
                    (triangle.Key.Item2, triangle.Key.Item1),
                };

                var potentialMetro2 = new List<(Vector2Int, Vector2Int)>
                {
                    (triangle.Key.Item1, triangle.Key.Item3),
                    (triangle.Key.Item3, triangle.Key.Item1),
                };

                var potentialMetro3 = new List<(Vector2Int, Vector2Int)>
                {
                    (triangle.Key.Item2, triangle.Key.Item3),
                    (triangle.Key.Item3, triangle.Key.Item2),
                };

                var metro1 = potentialMetro1.FirstOrDefault(x => _filledBarProperties.ContainsKey(x));
                var metro2 = potentialMetro2.FirstOrDefault(x => _filledBarProperties.ContainsKey(x));
                var metro3 = potentialMetro3.FirstOrDefault(x => _filledBarProperties.ContainsKey(x));

                // if all three metros are filled, use three sides mesh

                if (metro1 != default && metro2 != default && metro3 != default)
                {
                    SetMeshForMetroTriangle(triangle.Value, true);
                    continue;
                }


                SetMeshForMetroTriangle(triangle.Value, false);

                // current active metros
                var activeMetros = new List<(Vector2Int, Vector2Int)>
                {
                    metro1,
                    metro2,
                    metro3
                };

                activeMetros = activeMetros.Where(x => x != default).ToList();

                if (activeMetros.Count != 2)
                {
                    continue;
                }

                var commonTile = new Vector2Int();

                if (activeMetros[0].Item1 == activeMetros[1].Item1)
                {
                    commonTile = activeMetros[0].Item1;
                }
                else if (activeMetros[0].Item1 == activeMetros[1].Item2)
                {
                    commonTile = activeMetros[0].Item1;
                }
                else if (activeMetros[0].Item2 == activeMetros[1].Item1)
                {
                    commonTile = activeMetros[0].Item2;
                }
                else if (activeMetros[0].Item2 == activeMetros[1].Item2)
                {
                    commonTile = activeMetros[0].Item2;
                }

                // loop through the common tile neighbors
                var commonTileNeighbors = Grid.GetNeighborOffsets(commonTile.x);

                var finalAngle = 0f;

                for (int j = 0; j < 6; j++)
                {
                    var neighbor = commonTile + commonTileNeighbors[j];
                    var neighbor2 = commonTile + commonTileNeighbors[(j + 1) % 6];

                    var possibleActiveMetro1 = new List<(Vector2Int, Vector2Int)>
                    {
                        (commonTile, neighbor),
                        (neighbor, commonTile),
                    };

                    var possibleActiveMetro2 = new List<(Vector2Int, Vector2Int)>
                    {
                        (commonTile, neighbor2),
                        (neighbor2, commonTile),
                    };

                    if (activeMetros.Contains(possibleActiveMetro1[0]) ||
                        activeMetros.Contains(possibleActiveMetro1[1]))
                    {
                        if (activeMetros.Contains(possibleActiveMetro2[0]) ||
                            activeMetros.Contains(possibleActiveMetro2[1]))
                        {
                            finalAngle = j * 60f;
                            break;
                        }
                    }
                }

                triangle.Value.transform.eulerAngles = new Vector3(0, finalAngle, 0);
            }
        }

        public void OnCancel()
        {
            if (_undoMetroAddedTriangles.Count > 0)
            {
                foreach (var triangle in _undoMetroAddedTriangles)
                {
                    _metroAddedTriangles.Remove(triangle.Key);
                    Destroy(triangle.Value);
                }
                _undoMetroAddedTriangles.Clear();
            }
            ResetUndoVariables(false);
        }
    }
}