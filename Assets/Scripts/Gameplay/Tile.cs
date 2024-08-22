using UnityEngine;

namespace Gameplay
{
    public class Tile : Piece
    {
        public MeshRenderer meshRenderer;
        public Collider currentCollider;

        private TileProperties _tileProperties;

        public Tile[] blockTiles;

        public Mesh mergedTile;
        public Mesh singleTile;

        public GameObject assets;
        public GameObject mergeAssets;

        private int currentStackCount = 0;
        private int savedRotation = 0;
        private Vector3 startEulerAngle;

        public override void Init()
        {
            _tileProperties = (TileProperties)this.properties;
            startEulerAngle = meshRenderer.transform.eulerAngles;
            if (_tileProperties.appearAsMerged)
            {
                foreach (var blockTile in blockTiles)
                {
                    blockTile.properties = properties;
                    blockTile._tileProperties = _tileProperties;
                }

                meshRenderer.material = _tileProperties.material;
                meshRenderer.materials[1].color = _tileProperties.topColor;
                meshRenderer.materials[0].color = _tileProperties.bottomColor;
            }
            else
            {
                InitTile(this, (TileProperties)properties);
            }

            if (_tileProperties.assets != null)
            {
                var asset = Instantiate(_tileProperties.assets, transform);

                if (_tileProperties.appearAsMerged)
                {
                    asset.transform.localPosition = new Vector3(-0.137f, -0.292f, -0.13f);
                    asset.transform.localEulerAngles = new Vector3(-90f, 30f, 30f);
                }
                else
                {
                    asset.transform.localPosition = new Vector3(0f, -0.166f, 0f);
                    asset.transform.localEulerAngles = new Vector3(-90f, 30f, 0f);
                }

                asset.transform.localScale = Vector3.one * 100f;
                assets = asset;
            }
        }

        public void InitTile(Tile tile, TileProperties properties)
        {
            tile.properties = properties;
            tile._tileProperties = properties;
            tile.meshRenderer.material = _tileProperties.material;
            tile.meshRenderer.materials[1].color = properties.topColor;
            tile.meshRenderer.materials[0].color = properties.bottomColor;
        }

        public void Hide()
        {
            meshRenderer.enabled = false;
            assets.SetActive(false);
        }

        public void Show()
        {
            meshRenderer.enabled = true;
            assets.SetActive(true);
        }

        public void SetToMerged(int rotation)
        {
            assets.SetActive(false);

            meshRenderer.GetComponent<MeshFilter>().mesh = mergedTile;
            meshRenderer.transform.eulerAngles = new Vector3(0f, rotation + 240, 0f) + startEulerAngle;

            meshRenderer.material = _tileProperties.material;
            meshRenderer.materials[1].color = _tileProperties.topColor;
            meshRenderer.materials[0].color = _tileProperties.bottomColor;

            var mergeAsset = Instantiate(_tileProperties.mergeAssets, meshRenderer.transform);

            mergeAsset.transform.localPosition = new Vector3(0.0078f, 0.0071f, 0f);
            mergeAsset.transform.localEulerAngles = new Vector3(0f, 0f, 27.389f);
            mergeAsset.transform.localScale = Vector3.one * 1f;
            mergeAssets = mergeAsset;
            savedRotation = rotation;
        }

        public void AddToStack()
        {
            var stackAsset = Instantiate(_tileProperties.stackAssets[currentStackCount], meshRenderer.transform);

            stackAsset.transform.localPosition = new Vector3(0.0078f, 0.0071f, 0f);
            stackAsset.transform.localEulerAngles = new Vector3(0f, 0f, 27.389f);
            stackAsset.transform.localScale = Vector3.one * 1f;

            Destroy(mergeAssets);
            mergeAssets = stackAsset;
            currentStackCount++;
        }

        public void GoToPreviousStack()
        {
            Destroy(mergeAssets);
            currentStackCount--;
            if (currentStackCount == 0)
            {
                Destroy(mergeAssets);
                SetToMerged(savedRotation);
            }
            else
            {
                mergeAssets = Instantiate(_tileProperties.stackAssets[currentStackCount - 1], meshRenderer.transform);
                mergeAssets.transform.localPosition = new Vector3(0.0078f, 0.0071f, 0f);
                mergeAssets.transform.localEulerAngles = new Vector3(0f, 0f, 27.389f);
                mergeAssets.transform.localScale = Vector3.one * 1f;
            }
        }

        public int GetStackCount()
        {
            return currentStackCount;
        }

        public void SetToUnmerged()
        {
            assets.SetActive(true);

            meshRenderer.GetComponent<MeshFilter>().mesh = singleTile;
            meshRenderer.material = _tileProperties.material;
            meshRenderer.materials[1].color = _tileProperties.topColor;
            meshRenderer.materials[0].color = _tileProperties.bottomColor;

            Destroy(mergeAssets);
            mergeAssets = null;
        }

        public override void SetPosition(Vector3 position, bool final, bool snapping)
        {
            if (snapping && _tileProperties.appearAsMerged)
            {
                var firstTilePosition = blockTiles[0].transform.position;
                var center = transform.position;
                var offset = center - firstTilePosition;
                var snappedPosition = position + offset;
                transform.position = snappedPosition;
            }
            else
            {
                transform.position = position;
            }

            if (final)
            {
                currentCollider.enabled = false;
            }
        }

        public void Rotate(float rotateAngle)
        {
            transform.Rotate(0f, rotateAngle, 0f);
        }
    }
}