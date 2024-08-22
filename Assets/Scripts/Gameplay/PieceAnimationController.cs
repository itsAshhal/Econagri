using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Econagri.Singleton;
using DG.Tweening;
using Utility;
using TMPro;
using System.Collections.Specialized;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gameplay
{

    /// <summary>
    /// So when we drag a new piece into the world, we need to enable the user
    /// to rotate it as well based on his touch type
    /// </summary>
    public enum CameraDraggingState
    {
        Active, NotActive
    }


    public class PieceAnimationController : Singleton<PieceAnimationController>
    {
        [Tooltip("This is the active piece (Tile) in the gameplay scene which we need to rotate")]
        public Piece ActivePiece;

        public float rotationSpeed = 5f; // Adjust the rotation speed as needed


        [SerializeField] float SizeAnimationDuration = .5f;
        [SerializeField] float SizeScaleFactor = 1.25f;
        [SerializeField] TMP_Text m_aqiText;
        public TMP_Text AQIText { get { return m_aqiText; } }
        public TMP_Text GDPText { get { return m_gdpText; } }
        [SerializeField] TMP_Text m_gdpText;

        [SerializeField] GameObject[] CloudPrefabs;
        [SerializeField] float Random_Y_Min;
        [SerializeField] float Random_Y_Max;
        [SerializeField] float CloudInstantiateTime = 1f;  // cloud will spawn after every this second
        public float DestroyAfter = 5f;  // 5 seconds
        private float m_timer = 0f;
        [SerializeField] Image AqiProgressImage;
        [SerializeField] Image GdpProgressImage;
        [SerializeField] Color[] AQIColors;
        [Tooltip("so we can add certain icons for specific text elements with conditions")]
        [SerializeField] TMP_Text ToolTipText;
        public bool EmojiAdded = false;


        private Vector3 m_cloudStartingPosition;


        public CameraDraggingState M_CameraDraggingState;
        public enum TextAnimationBase
        {
            AQI, GDP
        }
        public TextAnimationBase M_TextAnimationBase;


        private void Start()
        {
            m_cloudStartingPosition = CloudPrefabs[0].transform.position;
        }

        private void Update()
        {
            AnimateClouds();
            FillProgressBars();

            if (ActivePiece == null) return;

            switch (M_TextAnimationBase)
            {
                case TextAnimationBase.GDP: SetNeedle(int.Parse(m_gdpText.text)); break;
                case TextAnimationBase.AQI: SetNeedle(int.Parse(m_aqiText.text)); break;
            }



            return;
            // checking for the tooltip text
            //if (EmojiAdded == true) return;
            string tip = ToolTipText.text;
            if (tip.Contains("last move"))
            {
                // Append the emoji (sprite) to the tooltip text
                Debug.Log($"Icon has been added");
                tip += "<sprite name=\"undoIcon\">";  // Ensure the sprite name is correctly referenced
                ToolTipText.text = tip;
                EmojiAdded = true;
            }
            else EmojiAdded = false;


        }

        void FillProgressBars()
        {
            // since the fill amount max is 1//
            // we have aqi and gdp max 100
            // so the value we need is 0.01
            if (float.Parse(AQIText.text) <= 0.0f) AqiProgressImage.fillAmount = 0f;
            else AqiProgressImage.fillAmount = Mathf.Abs(float.Parse(AQIText.text) / 100.0f);
            GdpProgressImage.fillAmount = Mathf.Abs(float.Parse(GDPText.text) / 100.0f);

            Debug.Log($"Division values we have are {float.Parse(AQIText.text) / 100.0f} and {float.Parse(GDPText.text) / 100.0f}");
        }

        void AnimateClouds()
        {
            m_timer += Time.deltaTime;
            if (m_timer >= CloudInstantiateTime)
            {
                Debug.Log($"Timer is {m_timer}, instantiating a cloud");
                var newPosition = new Vector3(m_cloudStartingPosition.x,
               m_cloudStartingPosition.y,
                m_cloudStartingPosition.z);
                Destroy(Instantiate(CloudPrefabs[Random.Range(0, CloudPrefabs.Length)], newPosition, Quaternion.identity), DestroyAfter);

                m_timer = 0f;
            }

        }


        public void AnimatePiece(Piece piece)
        {
            GameObject obj = piece.gameObject;



            // Store the original scale of the object
            Vector3 originalScale = obj.transform.localScale;

            // Create a sequence
            Sequence sequence = DOTween.Sequence();

            // Add a scaling up tween to the sequence
            sequence.Append(obj.transform.DOScale(originalScale * SizeScaleFactor, SizeAnimationDuration / 2));

            // Add a scaling down tween to the sequence
            sequence.Append(obj.transform.DOScale(originalScale, SizeAnimationDuration / 2));

            // Start the sequence
            sequence.Play();
        }

        public float TextAnimationDuration = .25f;
        public float TextScalar = 1.2f;
        public void AnimateText(TextMeshProUGUI t)
        {
            t.GetComponent<Animator>().CrossFade("OnChange", .1f);
        }

        [SerializeField] Animator NeedleContainerAnim;

        public void SetNeedle(int gdpScore)
        {
            Debug.Log($"GDP score is {gdpScore}");
            if (gdpScore <= 20)
            {
                AqiProgressImage.transform.GetChild(0).GetComponent<Image>().color = AQIColors[0];
                NeedleContainerAnim.CrossFade("0", .1f);
            }
            else if (gdpScore > 20 && gdpScore <= 40)
            {
                AqiProgressImage.transform.GetChild(0).GetComponent<Image>().color = AQIColors[1];
                NeedleContainerAnim.CrossFade("20", .1f);
            }
            else if (gdpScore > 40 && gdpScore <= 60)
            {
                AqiProgressImage.transform.GetChild(0).GetComponent<Image>().color = AQIColors[2];
                NeedleContainerAnim.CrossFade("40", .1f);
            }
            else if (gdpScore > 60 && gdpScore <= 80)
            {
                AqiProgressImage.transform.GetChild(0).GetComponent<Image>().color = AQIColors[3];
                NeedleContainerAnim.CrossFade("60", .1f);
            }
            else
            {
                AqiProgressImage.transform.GetChild(0).GetComponent<Image>().color = AQIColors[4];
                NeedleContainerAnim.CrossFade("80", .1f);
            }
        }
    }
}
