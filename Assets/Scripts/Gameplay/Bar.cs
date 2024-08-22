using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class Bar : Piece
    {
        // [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Collider currentCollider;
        [SerializeField] private GameObject assetParent;

        [SerializeField] private GameObject[] trafficAsset;

        [SerializeField] private GameObject[] metroPillar;
        [SerializeField] private GameObject metroStation;
        [SerializeField] private GameObject ghostRoad;

        private BarProperties _barProperties;
        private GameObject currentAsset;
        [SerializeField] float ShowScale = 1f;
        [SerializeField] float BigScale = 2.5f;
        [SerializeField] float BigScaleMetro = 2f;

        public override void Init()
        {
            _barProperties = (BarProperties)this.properties;
            SetMesh();
            SetBigScale();
            SetOffsetPosition();
            ghostRoad.SetActive(false);
        }

        public void SetOffsetPosition()
        {
            if (_barProperties.type == PieceType.Solution)
            {
                var offset = new Vector3(0f, -0.6f, 0f);
                this.transform.position += offset;
            }
        }

        private void SetMesh()
        {
            if (properties.type == PieceType.Waste)
            {
                var random = Random.Range(0, trafficAsset.Length);
                currentAsset = Instantiate(trafficAsset[random], assetParent.transform);
                currentAsset.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
                currentAsset.transform.localScale = Vector3.one * 100f;
                currentAsset.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
            else
            {
                var pillar = metroPillar[0];
                currentAsset = Instantiate(pillar, assetParent.transform);
                currentAsset.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                currentAsset.transform.localScale = Vector3.one * 1f;
                currentAsset.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
        }
        
        public void ConvertToMetroStation()
        {
            if (properties.type != PieceType.Waste)
            {
                Destroy(currentAsset);
                currentAsset = Instantiate(metroStation, assetParent.transform);
                currentAsset.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                currentAsset.transform.localScale = Vector3.one * 1f;
                currentAsset.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
        }

        public override void SetPosition(Vector3 position, bool final, bool snapping)
        {
            transform.position = position;

            if (!final) return;
            currentCollider.enabled = false;
        }

        public void SetBigScale()
        {
            if (properties.type == PieceType.Waste)
            {
                gameObject.transform.localScale = Vector3.one * BigScale;
            }
            else
            {
                gameObject.transform.localScale = Vector3.one * BigScaleMetro;
            }
        }

        public void SetOriginalScale()
        {
            gameObject.transform.localScale = Vector3.one * ShowScale;
        }


        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void ResetAngle()
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }

        public void SetAngle(Vector3 angle)
        {
            transform.eulerAngles = angle;
        }

        public void ShowPlaceable()
        {
            currentCollider.enabled = true;
            gameObject.layer = 7;
            ghostRoad.SetActive(true);
        }

        public void HidePlaceable()
        {
            currentCollider.enabled = false;
            gameObject.layer = 0;
            ghostRoad.SetActive(false);
        }
    }
}