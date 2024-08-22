using System.Globalization;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Econagri.Singleton;
using System.Xml.Xsl;

namespace Gameplay
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private float aqiToLose = 30;
        [SerializeField] private float gdpToWin = 30;
        [SerializeField] private float adjacencyScoreMultiplier = 2;


        [SerializeField] private bool isDebug = false;
        [SerializeField] private HexGrid hexGrid;
        [SerializeField] private BarProperties wasteBarProperties;
        [SerializeField] private Dragging dragging;

        [SerializeField] private float aqiChangeForGreenTechSingleHex = -1;
        [SerializeField] private float aqiChangeForGreenTechMergedHex = -2;

        private float _aqi;
        private float _gdp;

        [SerializeField] private GameObject[] buttonsToHide;

        [SerializeField] private TextMeshProUGUI aqiText;
        [SerializeField] private TextMeshProUGUI gdpText;

        [SerializeField] private TextMeshProUGUI aqiGhostText;
        [SerializeField] private TextMeshProUGUI gdpGhostText;

        [SerializeField] private GameObject greenTechObject;
        [SerializeField] private Image greenTechImage1;
        [SerializeField] private Image greenTechImage2;
        [SerializeField] private Sprite greenTechActiveSprite1;
        [SerializeField] private Sprite greenTechActiveSprite2;
        [SerializeField] private Sprite greenTechInactiveSprite1;
        [SerializeField] private Sprite greenTechInactiveSprite2;

        [SerializeField] private GameObject zoningLawObject;
        [SerializeField] private Image zoningLawImage1;
        [SerializeField] private Image zoningLawImage2;
        [SerializeField] private Sprite zoningLawActiveSprite1;
        [SerializeField] private Sprite zoningLawActiveSprite2;
        [SerializeField] private Sprite zoningLawInactiveSprite1;
        [SerializeField] private Sprite zoningLawInactiveSprite2;

        private bool zoningLawActive = false;
        public bool greenTechActive = false;

        public static GameManager Instance { get; private set; }
        public float AQI => _aqi;
        public float GDP => _gdp;

        private float _undoAQI;
        private float _undoGDP;

        public void EnableZoningLaw()
        {
            zoningLawObject.SetActive(true);
            zoningLawActive = true;
            zoningLawImage1.sprite = zoningLawActiveSprite1;
            zoningLawImage2.sprite = zoningLawActiveSprite2;
        }

        public void EnableGreenTech()
        {
            greenTechObject.SetActive(true);
            greenTechActive = true;
            greenTechImage1.sprite = greenTechActiveSprite1;
            greenTechImage2.sprite = greenTechActiveSprite2;
        }

        public void DisableZoningLaw()
        {
            zoningLawObject.SetActive(false);
            zoningLawActive = false;
            zoningLawImage1.sprite = zoningLawInactiveSprite1;
            zoningLawImage2.sprite = zoningLawInactiveSprite2;
        }

        public void DisableGreenTech()
        {
            greenTechObject.SetActive(false);
            greenTechActive = false;
            greenTechImage1.sprite = greenTechInactiveSprite1;
            greenTechImage2.sprite = greenTechInactiveSprite2;
        }

        public bool IsZoningLawActive()
        {
            return zoningLawActive;
        }

        public bool IsGreenTechActive()
        {
            Debug.Log($"GreenTech active");
            return greenTechActive;
        }

        private void OnEnable()
        {
            Events.OnPiecePlaced.AddListener(OnPiecePlaced);
        }

        private void OnDisable()
        {
            Events.OnPiecePlaced.RemoveListener(OnPiecePlaced);
        }

        private void Awake()
        {
            aqiText.text = _aqi.ToString(CultureInfo.InvariantCulture);
            gdpText.text = _gdp.ToString(CultureInfo.InvariantCulture);
            Instance = this;
            GameData.gameState = GameState.Gameplay;
            Events.OnUndo.AddListener(Undo);

            DisableGreenTech();
            DisableZoningLaw();
        }

        public void SetButtonVisibility(bool visibility)
        {
            foreach (var button in buttonsToHide)
            {
                button.SetActive(visibility);
            }
        }

        public void Undo()
        {
            _aqi -= _undoAQI;
            _gdp -= _undoGDP;
            UpdateUI();
        }

        private void Start()
        {
            Tooltip.I.ShowTooltip(TooltipType.StartGame);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                // isDebug = !isDebug;
            }
        }

        private void OnPiecePlaced(Piece piece, int mergeIndex, Vector2Int hexCoord, bool didStack)
        {
            Debug.Log($"Piece has been placed");
            Debug.Log($"Piece name is {piece.name}");

            // Animate the piece as well
            PieceAnimationController.Instance.ActivePiece = piece;
            PieceAnimationController.Instance.AnimatePiece(piece);

            // set the camera drag state as well
            PieceAnimationController.Instance.M_CameraDraggingState = CameraDraggingState.NotActive;
            PieceAnimationController.Instance.AnimateText(aqiText);
            PieceAnimationController.Instance.AnimateText(gdpText);
            //PieceAnimationController.Instance.SetNeedle(int.Parse(gdpText.text));


            UpdateScore(piece, mergeIndex, hexCoord, didStack);
            CheckForGameOver(true);
            UpdateUI();
        }

        public void CheckForGameOver(bool isMovesLeft)
        {
            if (isDebug) return;

            if (_aqi >= aqiToLose || !isMovesLeft)
            {
                if (_gdp >= gdpToWin)
                {
                    GameData.gameState = GameState.Win;
                    Events.OnGameOver.Invoke(true, (int)_aqi, (int)_gdp, isMovesLeft);
                }
                else
                {
                    GameData.gameState = GameState.Lose;
                    Events.OnGameOver.Invoke(false, (int)_aqi, (int)_gdp, isMovesLeft);
                }
            }
        }

        private void UpdateUI()
        {
            aqiText.text = _aqi.ToString(CultureInfo.InvariantCulture);
            gdpText.text = _gdp.ToString(CultureInfo.InvariantCulture);

            //PieceAnimationController.Instance.SetNeedle(int.Parse(gdpText.text));
        }

        private void UpdateScore(Piece piece, int mergeIndex, Vector2Int hexCoord, bool didStack)
        {

            var (aqiChange, gdpChange) = GetNetScoreChange(piece, mergeIndex, hexCoord, didStack);
            _aqi += aqiChange;
            _gdp += gdpChange;

            _undoAQI = aqiChange;
            _undoGDP = gdpChange;

            
        }

        public (float, float) GetNetScoreChange(Piece piece, int mergeIndex, Vector2Int hexCoord, bool didStack)
        {
            var didMerge = mergeIndex != -1 && !didStack;
            var aqiChange = 0f;
            var gdpChange = 0f;

            if (piece.properties.IsBar())
            {
                var barProperties = (BarProperties)piece.properties;
                if (piece.properties.type == PieceType.Waste)
                {
                    aqiChange += barProperties.AQIChangesPerBar * barProperties.numberOfBars;
                    gdpChange += barProperties.GDPChangesPerBar * barProperties.numberOfBars;
                    return (aqiChange, gdpChange);
                }

                var numberOfWasteBarsReplaced = dragging.NumberOfWasteBars();

                aqiChange += barProperties.AQIChangesPerBar * barProperties.numberOfBars;
                gdpChange += barProperties.GDPChangesPerBar * barProperties.numberOfBars;

                aqiChange -= wasteBarProperties.AQIChangesPerBar * numberOfWasteBarsReplaced;
                gdpChange -= wasteBarProperties.GDPChangesPerBar * numberOfWasteBarsReplaced;
                return (aqiChange, gdpChange);
            }

            var tileProperties = (TileProperties)piece.properties;

            if (didMerge)
            {
                aqiChange += tileProperties.AQIChangesInCaseOfMerge;
                gdpChange += tileProperties.GDPChangesInCaseOfMerge;


                if (piece.properties.type is PieceType.Agricultural or PieceType.Nature)
                {
                    var (adjacentTile, adjacentMerged) = hexGrid.GetAdjacentTileNumbersForMerge(mergeIndex);

                    aqiChange += tileProperties.AQIChangeForAdjacentTiles * adjacentTile;
                    aqiChange += tileProperties.AQIChangeForAdjacentMergedTiles * adjacentMerged;
                }

                if (piece.properties.type is PieceType.Commercial or PieceType.Industrial)
                {
                    if (greenTechActive)
                    {
                        aqiChange += aqiChangeForGreenTechMergedHex;
                    }
                }

            }
            else
            {
                if (didStack)
                {
                    aqiChange += tileProperties.AQIChangesStack;
                    gdpChange += tileProperties.GDPChangesStack;

                    if (mergeIndex != -1)
                    {
                        gdpChange += tileProperties.GDPStackMergeBonus;
                    }
                }
                else
                {
                    aqiChange += tileProperties.AQIChanges;
                    gdpChange += tileProperties.GDPChanges;

                    if (piece.properties.type is PieceType.Commercial or PieceType.Industrial)
                    {
                        if (greenTechActive)
                        {
                            aqiChange += aqiChangeForGreenTechSingleHex;
                        }
                    }
                }
            }

            var adjacencyScore = hexGrid.GetAdjacencyScore(hexCoord, tileProperties.type);
            gdpChange += adjacencyScore * adjacencyScoreMultiplier;
            return (aqiChange, gdpChange);
        }

        public void UpdateGhostScore(float aqiChange, float gdpChange)
        {
            aqiGhostText.text = aqiChange switch
            {
                > 0 => "+" + aqiChange,
                0 => "",
                _ => aqiChange.ToString(CultureInfo.InvariantCulture)
            };

            gdpGhostText.text = gdpChange switch
            {
                > 0 => "+" + gdpChange,
                0 => "",
                _ => gdpChange.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}