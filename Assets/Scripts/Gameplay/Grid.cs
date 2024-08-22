using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay
{
    public class Grid
    {
        private readonly Vector3 _origin;
        private Vector2Int _gridSize;
        private readonly float _hexRadius;

        private readonly float _hexWidth;
        private readonly float _hexHeight;

        public Grid(Vector3 origin, Vector2Int gridSize, float hexRadius)
        {
            _origin = origin;
            _gridSize = gridSize;
            _hexRadius = hexRadius;

            _hexWidth = _hexRadius * 2f;
            _hexHeight = Mathf.Sqrt(3f) * _hexRadius;
        }

        public static Vector2Int[] GetNeighborOffsets(int xCoord)
        {
            if (xCoord % 2 == 0)
            {
                return new Vector2Int[]
                {
                    new(0, 1), new(1, 0), new(1, -1),
                    new(0, -1), new(-1, -1), new(-1, 0)
                };
            }

            return new Vector2Int[]
            {
                new(0, 1), new(1, 1), new(1, 0),
                new(0, -1), new(-1, 0), new(-1, 1)
            };
        }

        public (Vector2Int, Vector2Int) GetBarPosition(Bar bar, Dictionary<(Vector2Int, Vector2Int), Bar> allBars)
        {
            var barIndex = allBars.FirstOrDefault(x => x.Value == bar).Key;
            return barIndex;
        }

        public static bool AnyBarInBetweenHexes(Vector2Int hexCoord1, Vector2Int hexCoord2, Vector2Int hexCoord3,
            Dictionary<(Vector2Int, Vector2Int), Bar> bars)
        {
            var bar1 = (hexCoord1, hexCoord2);
            var bar2 = (hexCoord2, hexCoord3);
            var bar3 = (hexCoord3, hexCoord1);

            var bar1MergeGroup = bars.FirstOrDefault(x => x.Key == bar1).Value;
            var bar2MergeGroup = bars.FirstOrDefault(x => x.Key == bar2).Value;
            var bar3MergeGroup = bars.FirstOrDefault(x => x.Key == bar3).Value;

            if (bar1MergeGroup != default || bar2MergeGroup != default || bar3MergeGroup != default)
            {
                return true;
            }

            // check for reverse bar as well
            bar1 = (hexCoord2, hexCoord1);
            bar2 = (hexCoord3, hexCoord2);
            bar3 = (hexCoord1, hexCoord3);

            bar1MergeGroup = bars.FirstOrDefault(x => x.Key == bar1).Value;
            bar2MergeGroup = bars.FirstOrDefault(x => x.Key == bar2).Value;
            bar3MergeGroup = bars.FirstOrDefault(x => x.Key == bar3).Value;

            if (bar1MergeGroup != default || bar2MergeGroup != default || bar3MergeGroup != default)
            {
                return true;
            }

            return false;
        }

        public static (Vector2Int, Vector2Int) GetMergeBlockTileOffsets(int angle, int startingHexCoord)
        {
            var neighbors = GetNeighborOffsets(startingHexCoord);

            var index = angle / 60;
            if (index is < 0 or > 5)
            {
                return (new Vector2Int(), new Vector2Int()); // Default return for invalid angles
            }

            var firstOffset = neighbors[index];
            var secondOffset = neighbors[(index + 1) % 6]; // Use modulo to wrap around to the beginning of the array

            return (firstOffset, secondOffset);
        }

        public bool InGridBounds(Vector2Int hexCoord)
        {
            return hexCoord.x >= 0 && hexCoord.x < _gridSize.x && hexCoord.y >= 0 && hexCoord.y < _gridSize.y;
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            var position = _origin;
            var x = Mathf.RoundToInt((worldPosition.x - position.x) / (_hexWidth * 0.75f));
            var y = Mathf.RoundToInt((worldPosition.z - position.z - (x % 2) * (_hexHeight * 0.5f)) / _hexHeight);
            return new Vector2Int(x, y);
        }

        public Vector3 GetWorldPosition(Vector2Int hexCoord)
        {
            var position = _origin;
            var xPos = position.x + hexCoord.x * _hexWidth * 0.75f;
            var zPos = position.z + hexCoord.y * _hexHeight + (hexCoord.x % 2) * (_hexHeight * 0.5f);
            return new Vector3(xPos, 0f, zPos);
        }

        public static bool IsNextToTileType(Vector2Int position, PieceType type, Dictionary<Vector2Int, Tile> tiles)
        {
            var neighborOffsets = Grid.GetNeighborOffsets(position.x);
            foreach (var offset in neighborOffsets)
            {
                var neighbor = position + offset;
                if (!tiles.ContainsKey(neighbor)) continue;

                if (tiles[neighbor].properties.type == type)
                {
                    return true;
                }
            }

            return false;
        }


        public (bool, Vector2Int, Vector2Int, Vector2Int) CheckForMergedTiles(Vector2Int hexCoord, Tile tile,
            Dictionary<Vector2Int, Tile> tilesToCheck, Dictionary<(Vector2Int, Vector2Int), Bar> possibleBars,
            List<(Vector2Int, Vector2Int, Vector2Int)> mergedHexes)
        {
            var offsets = GetNeighborOffsets(hexCoord.x);
            var filledNeighbors = new List<int>();

            for (var i = 0; i < offsets.Length; i++)
            {
                if (tilesToCheck.ContainsKey(hexCoord + offsets[i]))
                {
                    if (tilesToCheck[hexCoord + offsets[i]].properties == tile.properties)
                    {
                        if (!IsPartOfMergedHexes(hexCoord + offsets[i], mergedHexes))
                        {
                            filledNeighbors.Add(i);
                        }
                    }
                }
            }

            if (filledNeighbors.Count >= 2)
            {
                var firstNeighbor = -1;
                var secondNeighbor = -1;
                for (var i = 0; i < 6; i++)
                {
                    if (filledNeighbors.Contains(i) && filledNeighbors.Contains((i + 1) % 6))
                    {
                        firstNeighbor = i;
                        secondNeighbor = (i + 1) % 6;

                        if (Grid.AnyBarInBetweenHexes(hexCoord, hexCoord + offsets[firstNeighbor],
                                hexCoord + offsets[secondNeighbor], possibleBars))
                        {
                            continue;
                        }

                        return (true, hexCoord, hexCoord + offsets[firstNeighbor], hexCoord + offsets[secondNeighbor]);
                    }
                }
            }

            return (false, default, default, default);
        }

        private bool IsPartOfMergedHexes(Vector2Int tileToCheck,
            List<(Vector2Int, Vector2Int, Vector2Int)> mergedHexes)
        {
            foreach (var currentMergedHex in mergedHexes)
            {
                if (currentMergedHex.Item1 == tileToCheck || currentMergedHex.Item2 == tileToCheck ||
                    currentMergedHex.Item3 == tileToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DoesContainTwoJoiningTiles(Vector2Int hexCoord, Dictionary<Vector2Int, GameObject> list)
        {
            var neighborOffsets = Grid.GetNeighborOffsets(hexCoord.x);
            var emptyTiles = new List<int>();

            for (var i = 0; i < neighborOffsets.Length; i++)
            {
                if (!list.ContainsKey(hexCoord + neighborOffsets[i])) continue;
                emptyTiles.Add(i);
            }

            if (emptyTiles.Count < 2) return false;

            for (var i = 0; i < 6; i++)
            {
                if (emptyTiles.Contains(i) && emptyTiles.Contains((i + 1) % 6))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEdgeConnected((Vector2Int, Vector2Int) bar1, (Vector2Int, Vector2Int) bar2)
        {
            var bar1Hexes = new List<Vector2Int>
            {
                bar1.Item1,
                bar1.Item2
            };

            var bar2Hexes = new List<Vector2Int>
            {
                bar2.Item1,
                bar2.Item2
            };

            var commonHex = bar1Hexes.FirstOrDefault(x => bar2Hexes.Contains(x));
            if (commonHex == default)
            {
                return false;
            }

            var bar1OtherHex = bar1Hexes.FirstOrDefault(x => x != commonHex);
            var bar2OtherHex = bar2Hexes.FirstOrDefault(x => x != commonHex);

            var offsets = GetNeighborOffsets(bar1OtherHex.x);
            return offsets.Any(offset => bar1OtherHex + offset == bar2OtherHex);
        }

        private void DFS(Vector2Int hexCoord, HashSet<Vector2Int> visited, PieceType type,
            Dictionary<Vector2Int, Tile> tiles)
        {
            if (visited.Contains(hexCoord))
                return;
            visited.Add(hexCoord);
            var neighboringOffsets = Grid.GetNeighborOffsets(hexCoord.x);

            foreach (var offset in neighboringOffsets)
            {
                var neighboringTileCoord = hexCoord + offset;

                if (tiles.TryGetValue(neighboringTileCoord, out var neighboringTile)
                    && neighboringTile.properties.type == type)
                {
                    DFS(neighboringTileCoord, visited, type, tiles);
                }
            }
        }

        public bool DoesHavePieceTypeTileAdjacentRecursive(Vector2Int hexCoord, PieceType placedPieceType,
            PieceType currentPieceType, List<Vector2Int> ignoreHex, Dictionary<Vector2Int, Tile> tiles)
        {
            var offsets = Grid.GetNeighborOffsets(hexCoord.x);
            ignoreHex.Add(hexCoord);

            foreach (var offset in offsets)
            {
                var neighbor = hexCoord + offset;
                if (ignoreHex.Contains(neighbor)) continue;
                if (tiles.TryGetValue(neighbor, out var tile))
                {
                    if (tile.properties.type == placedPieceType)
                    {
                        return true;
                    }

                    if (tile.properties.type == currentPieceType)
                    {
                        return DoesHavePieceTypeTileAdjacentRecursive(neighbor, placedPieceType, currentPieceType, ignoreHex,
                            tiles);
                    }
                }
            }
            return false;
        }

        public bool DoesHavePieceTypeAdjacent(Vector2Int hexCoord, PieceType adjacentPieceType,
            Dictionary<Vector2Int, Tile> tiles, List<Vector2Int> tilesToIgnore = null)
        {
            var offsets = Grid.GetNeighborOffsets(hexCoord.x);
            
            foreach (var offset in offsets)
            {
                var neighbor = hexCoord + offset;
                
                if (tilesToIgnore != null && tilesToIgnore.Contains(neighbor)) continue;
                if (tiles.TryGetValue(neighbor, out var tile))
                {
                    if (tile.properties.type == adjacentPieceType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasMergedStackedNearBy(Vector2Int hexCoord, List<(Vector2Int, Vector2Int, Vector2Int)> mergedHex)
        {
            var offsets = Grid.GetNeighborOffsets(hexCoord.x);

            foreach (var offset in offsets)
            {
                var neighbor = hexCoord + offset;
                if (IsPartOfMergedHexes(neighbor, mergedHex))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CanMergeBlockSnap(Vector3 worldPosition, int angle, Dictionary<Vector2Int, GameObject> availableHexes, Dictionary<Vector2Int, Tile> addedHexes)
        {
            var hexCoord = WorldToGrid(worldPosition);
            if (!availableHexes.ContainsKey(hexCoord)) return false;

            var (mergeBlockNeighbor1, mergeBlockNeighbor2) = Grid.GetMergeBlockTileOffsets(angle, hexCoord.x);

            var mergeBlockTile1 = hexCoord + mergeBlockNeighbor1;
            var mergeBlockTile2 = hexCoord + mergeBlockNeighbor2;

            if (addedHexes.ContainsKey(mergeBlockTile1) || addedHexes.ContainsKey(mergeBlockTile2))
            {
                return false;
            }
            return true;
        }
        
        public int GetAdjacencyScore(Vector2Int hexCoord, PieceType pieceType, Dictionary<Vector2Int, Tile> tiles)
        {
            var offsets = GetNeighborOffsets(hexCoord.x);
            var adjacentTiles = new List<Tile>();

            foreach (var offset in offsets)
            {
                var neighbor = hexCoord + offset;
                if (tiles.TryGetValue(neighbor, out var tile))
                {
                    if (tile.properties.type == pieceType) continue;
                    if (!DoesHavePieceTypeTileAdjacentRecursive(neighbor, pieceType, tile.properties.type,
                            new List<Vector2Int>{hexCoord}, tiles))
                    {
                        adjacentTiles.Add(tile);
                    }
                }
            }

            var visited = new HashSet<Vector2Int>();
            int groups = 0;

            foreach (var startTile in adjacentTiles)
            {
                var hexCoordOfStartTile = tiles.FirstOrDefault(x => x.Value == startTile).Key;
                if (!visited.Contains(hexCoordOfStartTile))
                {
                    DFS(hexCoordOfStartTile, visited, startTile.properties.type, tiles);
                    groups++;
                }
            }

            return groups;
        }
        
         public static (int, int) GetAdjacentTileNumbersForMerge(int mergeIndex, List<(Vector2Int, Vector2Int, Vector2Int)> mergedHexes, 
            Dictionary<Vector2Int, Tile> filledTiles)
        {
            var numberOfAdjacentTiles = new List<Vector2Int>();
            var numberOfAdjacentMergedTiles = new List<Vector2Int>();

            var tileCoords = new List<Vector2Int>
            {
                mergedHexes[mergeIndex].Item1,
                mergedHexes[mergeIndex].Item2,
                mergedHexes[mergeIndex].Item3
            };

            if (tileCoords.Any(coord => coord.Equals(default(Vector2Int))))
            {
                return (-1, -1);
            }

            var mergedHexCoords = new HashSet<Vector2Int>();
            foreach (var mergedHex in mergedHexes)
            {
                mergedHexCoords.Add(mergedHex.Item1);
                mergedHexCoords.Add(mergedHex.Item2);
                mergedHexCoords.Add(mergedHex.Item3);
            }

            foreach (var hexCoord in tileCoords)
            {
                var offsets = Grid.GetNeighborOffsets(hexCoord.x);

                foreach (var offset in offsets)
                {
                    var neighbor = hexCoord + offset;
                    if (tileCoords.Contains(neighbor))
                    {
                        continue;
                    }

                    if (mergedHexCoords.Contains(neighbor))
                    {
                        if (!numberOfAdjacentMergedTiles.Contains(neighbor))
                        {
                            numberOfAdjacentMergedTiles.Add(neighbor);
                        }

                        continue;
                    }

                    if (filledTiles.ContainsKey(neighbor))
                    {
                        if (!numberOfAdjacentTiles.Contains(neighbor))
                        {
                            numberOfAdjacentTiles.Add(neighbor);
                        }
                    }
                }
            }
            return (numberOfAdjacentTiles.Count, numberOfAdjacentMergedTiles.Count);
        }
         
         public bool CanPlaceNBars(int n, Dictionary<(Vector2Int, Vector2Int), GameObject> availableBars) 
         {
             if (n > availableBars.Count) 
             {
                 return false;
             }
    
             foreach (var bar in availableBars.Keys) 
             {
                 var visited = new HashSet<(Vector2Int, Vector2Int)>();
                 if (DepthFirstSearch(bar, availableBars,visited) >= n) 
                 {
                     return true;
                 }
             }
    
             return false;
         }
         

         private int DepthFirstSearch((Vector2Int, Vector2Int) bar, Dictionary<(Vector2Int, Vector2Int), GameObject> availableBars, HashSet<(Vector2Int, Vector2Int)> visited) 
         {
             if (visited.Contains(bar)) 
             {
                 return 0;
             }
    
             visited.Add(bar);
             int count = 1;
    
             foreach (var otherBar in availableBars.Keys) 
             {
                 if (IsEdgeConnected(bar, otherBar)) 
                 {
                     count += DepthFirstSearch(otherBar, availableBars, visited);
                 }
             }
    
             return count;
         }
         
    }
}