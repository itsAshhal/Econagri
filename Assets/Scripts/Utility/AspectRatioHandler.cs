using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utility
{
    public class AspectRatioHandler : MonoBehaviour
    {
        public float targetAspect = 16f / 9f;
        private float width;

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResizeWindow();
        }

        private void ResizeWindow()
        {
            SetCanvasSize();
            SetCameraSize();
        }

        private void Update()
        {
            if (Math.Abs(width - Screen.width) > 0.1f)
            {
                ResizeWindow();
            }
        }
        
        private void SetCanvasSize()
        {
            float currentAspect = Screen.width / (float)Screen.height;

            width = Screen.width;

            var canvasScaler = FindObjectOfType<CanvasScaler>();

            if (canvasScaler == null)
            {
                return;
            }

            if (currentAspect < targetAspect)
                canvasScaler.matchWidthOrHeight = 0;
            else
                canvasScaler.matchWidthOrHeight = 1;
        }

        private void SetCameraSize()
        {
            var _camera = Camera.main;
            if (_camera == null) return;
            
            UpdateSecondaryCamera();
            
            float currentAspect = (float)Screen.width / (float)Screen.height;
            float referenceSize = 5f; // This is your camera's default orthographic size at 16:9
            float newOrthographicSize;
            if (currentAspect >= targetAspect)
            {
                newOrthographicSize = referenceSize;
            }
            else
            {
                newOrthographicSize = referenceSize * targetAspect / currentAspect;
            }

            _camera.orthographicSize = newOrthographicSize;
        }

        private void UpdateSecondaryCamera()
        {
            var _camera = Camera.allCameras;
            if (_camera.Length < 2) return;
            var cam = _camera[0];
            var secondaryCamera = _camera[1];
            
            if (cam != Camera.main)
            {
                secondaryCamera = _camera[0];
            }
            
            var referenceSize = 8.5f; // This is your camera's default orthographic size at 16:9
            var currentAspectRatio = (float)Screen.width / Screen.height;
            var newOrthographicSize = referenceSize * targetAspect / currentAspectRatio;

            secondaryCamera.orthographicSize =  newOrthographicSize;
        }
    }
}