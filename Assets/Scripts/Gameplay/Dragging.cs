using System.Collections.Generic;
using UI;
using UnityEngine;
using Utility;

namespace Gameplay
{
    public class Dragging : MonoBehaviour
    {
        public HexGrid hexGrid;
        public PieceSpawner pieceSpawner;
        public Camera mainCamera;
        public Camera pieceCamera;
        public GameManager gameManger;

        [SerializeField] private GameObject cancelButton;
        [SerializeField] private GameObject undoButton;
        [SerializeField] private float rotateAngle;
        private CameraController _cameraController;
        private Piece _selectedPiece;
        private int _selectedSlotIndex = -1;
        private bool _isDragging;
        private bool _isFirstTimeDragging = true;

        private int _numberOfBarsLeft = 0;
        private bool _isOverWasteBar = false;
        private List<Bar> _barsPutCurrently;
        private List<(Bar, (Vector2Int, Vector2Int))> _wasteBarDeleted;

        public bool canDrag = true;
        private bool didShowRotateTooltip = false;

        private void Awake()
        {
            _barsPutCurrently = new List<Bar>();
            _wasteBarDeleted = new List<(Bar, (Vector2Int, Vector2Int))>();
            cancelButton.SetActive(false);
            _cameraController = mainCamera.GetComponent<CameraController>();
            undoButton.SetActive(false);
            Events.OnUndo.AddListener(OnUndo);
        }

        private void OnUndo()
        {
            undoButton.SetActive(false);
        }

        public int GetNumberOfBarsPut()
        {
            return _barsPutCurrently.Count;
        }

        public int GetWasteBarsDeleted()
        {
            return _wasteBarDeleted.Count;
        }

        public bool IsOverWasteBar()
        {
            return _isOverWasteBar;
        }

        private void Update()
        {
            if (!canDrag) return;

            if (!didShowRotateTooltip)
            {
                var tile = GetTileUnderMouse();
                if (tile != null)
                {
                    Tooltip.I.ShowTooltip(TooltipType.Rotate);
                    didShowRotateTooltip = true;
                }
            }


            var isMouseDown = Input.GetMouseButtonDown(0) ||
                              (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
            var isMouseUp = Input.GetMouseButtonUp(0) ||
                            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
            var isMouseHold = Input.GetMouseButton(0) ||
                              (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved);

            if (!isMouseDown && !isMouseUp && !isMouseHold)
            {
                if (Input.GetMouseButtonDown(1) ||
                    (Input.touchCount > 1 && Input.GetTouch(1).phase == TouchPhase.Began))
                {
                    var tile = GetTileUnderMouse();
                    if (tile != null)
                    {
                        tile.Rotate(rotateAngle);
                    }
                }
            }

            if (isMouseDown && _numberOfBarsLeft < 1)
            {
                TryPickUpPiece();
            }

            if (_isDragging && isMouseHold || _isDragging && _numberOfBarsLeft > 0)
            {
                DragPiece();

                if (Input.GetMouseButtonDown(1) ||
                    (Input.touchCount > 1 && Input.GetTouch(1).phase == TouchPhase.Began))
                {
                    RotatePiece();
                }
            }

            if (isMouseUp && _isDragging)
            {
                TryDropPiece();
            }
        }


        private void RotatePiece()
        {
            var tile = _selectedPiece as Tile;
            if (tile == null) return;
            tile.Rotate(rotateAngle);
        }

        public int NumberOfWasteBars()
        {
            return _wasteBarDeleted.Count;
        }

        private void OnStopDragBar()
        {
            _isDragging = false;
            _selectedSlotIndex = -1;
            _numberOfBarsLeft = 0;
            _barsPutCurrently.Clear();
            undoButton.SetActive(true);
            cancelButton.SetActive(false);
            hexGrid.HidePossibleBars();
            gameManger.UpdateGhostScore(0, 0);
            _wasteBarDeleted.Clear();
            Tooltip.I.HideTooltip();
        }

        public void OnCancelButton()
        {
            foreach (var bar in _barsPutCurrently)
            {
                hexGrid.RemoveBar(bar);
            }

            foreach (var (bar, position) in _wasteBarDeleted)
            {
                hexGrid.AddBar(bar, position);
                bar.Show();
            }

            pieceSpawner.ReturnPiece(_selectedSlotIndex, _selectedPiece);
            hexGrid.OnCancel();
            OnStopDragBar(); 
            GameManager.Instance.SetButtonVisibility(true); 
            undoButton.SetActive(false);
        }

        private Tile GetTileUnderMouse()
        {
            var ray = pieceCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit) && hit.collider.GetComponent<Tile>() != null)
            {
                Debug.Log($"Got the tile {hit.collider.name}");
                return hit.collider.GetComponent<Tile>();
            }

            return null;
        }

        private void TryPickUpPiece()
        {
            if (_isDragging) return;
            if (_numberOfBarsLeft > 0) return;

            var ray = pieceCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit) || hit.collider.GetComponent<Piece>() == null) return;

            AudioPlayer.PlaySound("Picked");
            GameManager.Instance.SetButtonVisibility(false);
            _cameraController.canMove = false;

            _selectedPiece = hit.collider.GetComponent<Piece>();
            _selectedSlotIndex = pieceSpawner.GetSlotIndex(_selectedPiece);
            _isDragging = true;
            pieceSpawner.PickPiece(_selectedSlotIndex);
            Tooltip.I.ShowToolTipForTile(_selectedPiece.properties.type, _selectedPiece.properties.IsBar());

            if (_selectedPiece.properties.IsBar())
            {
                hexGrid.ShowPossibleBars(_selectedPiece.properties.type, _barsPutCurrently);

                var barProperties = _selectedPiece.properties as BarProperties;
                _numberOfBarsLeft = barProperties.numberOfBars;
                var bar = _selectedPiece as Bar;
                bar.SetOriginalScale();
                undoButton.SetActive(false);
                cancelButton.SetActive(true);
            }
            else
            {
                hexGrid.ShowPossibleHexes(_selectedPiece.properties as TileProperties);
            }
        }

        #region Drag

        private void DragPiece()
        {
            if (_selectedPiece.properties.IsBar())
            {
                DragBar();
            }
            else
            {
                DragTile();
            }
        }

        private void DragTile()
        {
            var (canPlace, hit) = CanPlaceTile();
            if (canPlace)
            {
                var position = hit.transform.position;
                _selectedPiece.SetPosition(position, false, true);
                hexGrid.GhostPiecePlacement(position, _selectedPiece);

                if (_isFirstTimeDragging)
                {
                    Tooltip.I.ShowTooltip(TooltipType.HoverOverTile);
                }
                else
                {
                    if (hexGrid.IsNextToSameTile(_selectedPiece as Tile))
                    {
                        Tooltip.I.ShowTooltip(TooltipType.SameTile);
                    }
                    else
                    {
                        Tooltip.I.ShowTooltip(TooltipType.DifferentTile);
                    }

                    if (hexGrid.IsStackingTile(_selectedPiece as Tile))
                    {
                        Tooltip.I.ShowTooltip(TooltipType.Stacking);
                    }
                }
            }
            else
            {
                Events.OnPieceNotSnapped.Invoke();
                FreeDrag();
            }
        }

        private void DragBar()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f, LayerMask.GetMask("GhostBar")))
            {
                _selectedPiece.SetPosition(hit.transform.position, false, true);

                var wasteBar = hit.transform.gameObject.GetComponent<Bar>();
                if (wasteBar != null)
                {
                    _isOverWasteBar = true;
                }
                else
                {
                    _isOverWasteBar = false;
                }

                var bar = _selectedPiece as Bar;
                bar.SetAngle(hit.transform.eulerAngles);
                hexGrid.GhostPiecePlacement(hit.transform.position, _selectedPiece);
            }
            else
            {
                var bar = _selectedPiece as Bar;
                _isOverWasteBar = false;
                bar.ResetAngle();
                FreeDrag();
            }
        }

        private void FreeDrag()
        {
            var mousePosition = Input.mousePosition;
            mousePosition.z = 10f;
            var worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            _selectedPiece.SetPosition(worldPosition, false, false);
            gameManger.UpdateGhostScore(0, 0);
        }

        #endregion

        private void TryDropPiece()
        {
            Events.OnPieceNotSnapped.Invoke();
            AudioPlayer.PlaySound("Placed");
            if (_selectedPiece.properties.IsBar())
            {
                var didDropBar = TryDropBar();
                if (didDropBar)
                {
                    _numberOfBarsLeft--;
                    if (_numberOfBarsLeft < 1)
                    {
                        undoButton.SetActive(true);
                        _cameraController.canMove = true;
                        
                        if (_selectedPiece.properties.type == PieceType.Solution)
                        {
                            foreach (var bar in _barsPutCurrently)
                            {
                                hexGrid.UpdateTrackToMetroStationIfNeeded(bar);
                            }
                            hexGrid.UpdateTrackToMetroStationIfNeeded(_selectedPiece as Bar);
                        }
                        OnStopDragBar();
                        GameManager.Instance.SetButtonVisibility(true); 
                    }
                    else
                    {
                        _barsPutCurrently.Add(_selectedPiece as Bar);
                        hexGrid.ShowPossibleBars(_selectedPiece.properties.type, _barsPutCurrently);
                        _selectedPiece = pieceSpawner.SpawnPiece(_selectedSlotIndex, _selectedPiece.properties);
                        PieceSpawner.PieceSetLayer(_selectedPiece.gameObject, _selectedPiece.startingLayer);
                        (_selectedPiece as Bar).SetOriginalScale();
                    }
                }
                else
                {
                    var barProperties = _selectedPiece.properties as BarProperties;
                    if (_numberOfBarsLeft == barProperties.numberOfBars)
                    {
                        _cameraController.canMove = true;
                        pieceSpawner.ReturnPiece(_selectedSlotIndex, _selectedPiece);
                        var bar = _selectedPiece as Bar;
                        bar.SetBigScale();
                        OnStopDragBar();
                        GameManager.Instance.SetButtonVisibility(true); 
                    }
                }
            }
            else
            {
                TryDropTile();
                GameManager.Instance.SetButtonVisibility(true); 
                gameManger.UpdateGhostScore(0, 0);
                _isDragging = false;
                _selectedSlotIndex = -1;
                _cameraController.canMove = true;
            }
        }


        private void TryDropTile()
        {
            var (canPlace, hit) = CanPlaceTile();
            if (canPlace)
            {
                Debug.Log("Dropping tile");
                undoButton.SetActive(true);
                var position = hit.transform.position;
                hexGrid.FillHex(position, _selectedPiece as Tile, _selectedSlotIndex);
                _selectedPiece.SetPosition(position, true, true);

                if (!Tooltip.I.ShowTooltip(TooltipType.Undo))
                {
                    Tooltip.I.HideTooltip();
                }
            }
            else
            {
                Tooltip.I.HideTooltip();
                pieceSpawner.ReturnPiece(_selectedSlotIndex, _selectedPiece);
            }

            _isFirstTimeDragging = false;
            hexGrid.HidePossibleHexes();
        }

        private bool TryDropBar()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, 1000f, LayerMask.GetMask("GhostBar")))
            {
                var position = hit.transform.position;
                var wasteBar = hit.transform.gameObject.GetComponent<Bar>();
                if (wasteBar != null)
                {
                    _wasteBarDeleted.Add((wasteBar, hexGrid.GetBarPosition(wasteBar)));
                    wasteBar.Hide();
                }

                hexGrid.FillBar(hit.transform.gameObject, _selectedPiece as Bar, _selectedSlotIndex, _numberOfBarsLeft);
                _selectedPiece.SetPosition(position, true, true);
                return true;
            }

            return false;
        }

        private (bool, RaycastHit) CanPlaceTile()
        {
            var tileProperties = _selectedPiece.properties as TileProperties;
            var mousePosition = Input.mousePosition;

            RaycastHit hit = new RaycastHit(); // Initialize 'hit' before using it
            bool canPlace = false;

            if (tileProperties && tileProperties.appearAsMerged)
            {
                var tile = _selectedPiece as Tile;
                if (tile == null) return (false, hit);

                var tiles = tile.blockTiles;
                var screenPositionForTiles = new List<Vector3>();

                var centerScreenPosition = mainCamera.WorldToScreenPoint(tile.transform.position);
                foreach (var mergeTile in tiles)
                {
                    var position = mergeTile.transform.position;
                    var screenPosition = mainCamera.WorldToScreenPoint(position);
                    screenPosition -= centerScreenPosition;
                    screenPosition += mousePosition;
                    screenPositionForTiles.Add(screenPosition);
                }

                foreach (var screenPositionForTile in screenPositionForTiles)
                {
                    var ray = mainCamera.ScreenPointToRay(screenPositionForTile);
                    if (Physics.Raycast(ray, out var hit1, 1000f, LayerMask.GetMask("GhostTile")))
                    {
                        hit = hit1;
                        canPlace = true;
                        break;
                    }
                }

                if (canPlace)
                {
                    var transform1 = _selectedPiece.transform;
                    var angle = Mathf.RoundToInt(transform1.eulerAngles.y);
                    canPlace = hexGrid.CanMergeBlockSnap(hit.transform.position, angle);
                }

                return (canPlace, hit);
            }
            else
            {
                var ray = mainCamera.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("GhostTile")))
                {
                    return (true, hit);
                }
            }

            return (false, hit); // Use the initialized 'hit' variable
        }
    }
}