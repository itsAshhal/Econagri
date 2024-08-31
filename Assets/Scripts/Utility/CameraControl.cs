using Gameplay;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Utility
{
    public class CameraController : MonoBehaviour
    {
        public float panSpeed = 20f;
        public float zoomSpeed = 5f;
        public float minZoom = 5f;
        public float maxZoom = 200f;

        private Vector3 _touchStart;
        private Camera _cam;
        public bool canMove;

        public int GdpToAqiDifferenceForSkyBackground = 10;
        public int AqiThresholdForSmokyRedColor = 25;
        [SerializeField] string[] SkyPaletteHexColors;
        [SerializeField] string[] SmokyRedPaletteColors;
        [SerializeField] Animator MotivationTextImageAnim;
        [SerializeField] Animator MotivationConfirmAnim;
        [Tooltip("So once the GDP is good, this motivation message will keep on appearing after this seconds")]
        [SerializeField] float RepititionTimeForMotivationOnceGdpIsGood;
        [SerializeField] ParticleSystem ConfettiParticle;
        [SerializeField] Animator AlertTextAnim;
        [SerializeField] Animator AlertConfirmAnim;
        bool canShowAlert = true;
        [SerializeField] float AlertDuration = 1.5f;
        [SerializeField] TMP_Text m_alarmingText;
        public bool fiftyPartDone = false;
        public bool eightyPartDone = false;


        public enum SkyState
        {
            Red, Blue, None
        }
        public SkyState M_SkyState;

        private void Start()
        {
            _cam = GetComponent<Camera>();
            canMove = true;
            GetRandomSkyColor(SkyPaletteHexColors);

            M_SkyState = SkyState.None;
            ConfettiParticle.gameObject.SetActive(false);


        }





        void GetRandomSkyColor(string[] SkyPaletteHexColors)
        {
            // Check if there are any colors defined
            if (SkyPaletteHexColors.Length == 0)
            {
                Debug.LogError("No colors defined in the SkyPaletteHexColors array.");
                return;
            }

            // Pick a random color from the array
            string randomHex = SkyPaletteHexColors[Random.Range(0, SkyPaletteHexColors.Length)];

            // Convert hex to Unity Color and assign it to the camera's background color
            Camera.main.backgroundColor = HexToColor(randomHex);
        }

        // Helper function to convert hex color to Unity Color object
        Color HexToColor(string hex)
        {
            // In case the hex code is formatted with a hash prefix
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            // Convert hex to RGBA components
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = 255; // Default full opacity
            if (hex.Length == 8) // Check if alpha is specified
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }

            // Return the Unity Color object
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        public void DisableInvokeRepeating()
        {
            CancelInvoke(nameof(DisplayMotivation));
            //CancelInvoke(nameof(DisplayMotivationConfirm));
        }

        void DisplayMotivation()
        {
            Debug.Log($"Motivation displayed");
            ConfettiParticle.gameObject.SetActive(true);
            MotivationTextImageAnim.CrossFade("Appear", .1f);
            float value = Random.Range(1.5f, 2f);
            Invoke(nameof(UnDisplayMotivation), value);
            Invoke(nameof(UnDisplayMotivationConfirm), value);
            Invoke(nameof(HideConfetti), 4f);
        }

        void HideConfetti()
        {
            ConfettiParticle.gameObject.SetActive(false);
        }

        void UnDisplayMotivation()
        {
            MotivationTextImageAnim.CrossFade("Disappear", .1f);
        }

        void UnDisplayMotivationConfirm()
        {
            MotivationConfirmAnim.CrossFade("Disappear", .1f);
        }
        void DisplayMotivationConfirm()
        {
            MotivationConfirmAnim.CrossFade("Appear", .1f);
        }

        void DisplayAlert()
        {
            AlertTextAnim.CrossFade("Appear", .1f);
            Invoke(nameof(HideAlert), AlertDuration);
        }
        void HideAlert()
        {
            AlertTextAnim.CrossFade("Disappear", .1f);
        }

        void CalculateThresholds()
        {
            int gdp = int.Parse(PieceAnimationController.Instance.GDPText.text);
            int aqi = int.Parse(PieceAnimationController.Instance.AQIText.text);
            bool smokyCondition = aqi >= AqiThresholdForSmokyRedColor;
            Debug.Log($"SmokyCondition {smokyCondition}");


            if ((gdp - aqi) >= GdpToAqiDifferenceForSkyBackground)
            {

                if (M_SkyState != SkyState.Blue)
                {
                    DisplayMotivation();
                    DisplayMotivationConfirm();
                    M_SkyState = SkyState.Blue;
                }

                // else if (aqi >= AqiThresholdForSmokyRedColor)
                // {
                //     if (M_SkyState != SkyState.Red)
                //     {
                //         DisplayAlert();
                //         M_SkyState = SkyState.Red;
                //     }
                // }
            }

            if ((aqi >= 50 && aqi < 52) || (aqi > 80 && aqi < 82))
            {
                Debug.Log($"Reached the red area");

                if (aqi >= 50 && aqi < 52)
                {
                    Debug.Log($"AQI 50 started");
                    if (fiftyPartDone == true) return;
                    DisplayAlert();
                    m_alarmingText.text = "AQI is increasing!";
                    //Invoke(nameof(UnCheckRed), 1f);
                    //M_SkyState = SkyState.Red;
                    fiftyPartDone = true;

                    Debug.Log($"Simple increasing");
                }
                else if (aqi > 80 && aqi < 82)
                {
                    Debug.Log($"AQI 80 started");
                    if (eightyPartDone == true) return;
                    DisplayAlert();
                    m_alarmingText.text = "AQI is critical!";
                    //Invoke(nameof(UnCheckRed), 1f);
                    //M_SkyState = SkyState.Red;
                    eightyPartDone = true;

                    Debug.Log($"Critical increasing");
                }
            }




        }

        void UnCheckRed()
        {
            M_SkyState = SkyState.None;
        }



        // if (!smokyCondition)
        // {
        //     if ((gdp - aqi) >= GdpToAqiDifferenceForSkyBackground)
        //     {
        //         if (M_SkyState == SkyState.Blue) return;
        //         Debug.Log($"Logic, sky");
        //         // it means we need  a sky bavckground
        //         GetRandomSkyColor(SkyPaletteHexColors);


        //         // display the motivation as well
        //         // Invoke(nameof(DisplayMotivation), RepititionTimeForMotivationOnceGdpIsGood);
        //         // Invoke(nameof(DisplayMotivationConfirm), RepititionTimeForMotivationOnceGdpIsGood + 1f);
        //         DisplayMotivation();
        //         DisplayMotivationConfirm();

        //         M_SkyState = SkyState.Blue;
        //     }
        // }

        // if (smokyCondition)
        // {
        //     if (M_SkyState == SkyState.Red || !canShowAlert) return;
        //     canShowAlert = false;
        //     Debug.Log($"Logic, smoky");
        //     // it means we need a smoky red background
        //     /*GetRandomSkyColor(SmokyRedPaletteColors);
        //     */

        //     M_SkyState = SkyState.Red;
        //     //GetComponent<CameraEffects>().StartCameraEffect(.5f);

        //     Invoke(nameof(DisplayAlert), 1f);

        //     // CancelInvoke(nameof(DisplayMotivation));
        //     // CancelInvoke(nameof(DisplayMotivationConfirm));
        // }


        void GDPCheck()
        {
            var gdpText = PieceAnimationController.Instance.GDPText;
            //gdpText.text = float.Parse(gdpText.text) < 0 ? gdpText.text = "0" : gdpText.text = gdpText.text;
        }

        private void Update()
        {
            CalculateThresholds();
            GDPCheck();

            if (!canMove) return;

            if (Input.GetMouseButtonDown(0))
            {
                _touchStart = GetWorldPosition();
            }

            if (Input.GetMouseButton(0))
            {
                var direction = _touchStart - GetWorldPosition();
                _cam.transform.position += direction;
            }

            // Mouse Zoom
            var scrollData = Input.GetAxis("Mouse ScrollWheel");
            Zoom(scrollData);

            // Touch Pan
            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _touchStart = GetWorldPosition(touch.position);
                        break;
                    case TouchPhase.Moved:
                        {
                            var direction = _touchStart - GetWorldPosition(touch.position);
                            _cam.transform.position += direction;
                            break;
                        }
                }
            }

            // Touch Zoom
            if (Input.touchCount == 2)
            {
                var touchZero = Input.GetTouch(0);
                var touchOne = Input.GetTouch(1);

                var touchPrevDist = (touchZero.position - touchOne.position).magnitude;
                var touchCurDist = ((touchZero.position - touchZero.deltaPosition) - (touchOne.position - touchOne.deltaPosition)).magnitude;

                var difference = touchPrevDist - touchCurDist;

                Zoom(difference * 0.01f);
            }
        }

        private void Zoom(float increment)
        {
            var newZoom = Mathf.Clamp(_cam.orthographicSize - increment * zoomSpeed, minZoom, maxZoom);
            _cam.orthographicSize = newZoom;
        }

        private Vector3 GetWorldPosition(Vector2 touchPosition = default)
        {
            var mousePos = Input.mousePosition;
            if (touchPosition != default)
            {
                mousePos = new Vector3(touchPosition.x, touchPosition.y, mousePos.z);
            }
            return _cam.ScreenToWorldPoint(mousePos);
        }
    }
}
