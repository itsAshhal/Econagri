using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utility
{
    public class Effects : MonoBehaviour
    {
        public static Effects I;
        private void Awake()
        {
            I = this;
        }

        public void Fade(GameObject screenToShow, bool toShow, float time = 1f)
        {
            if (toShow)
            {
                screenToShow.SetActive(true);
            }

            var images = screenToShow.GetComponentsInChildren<Image>();
            var texts = screenToShow.GetComponentsInChildren<TextMeshProUGUI>();

            StartCoroutine(FadeCoroutine(screenToShow, toShow, time, images, texts));
        }

        private static IEnumerator FadeCoroutine(GameObject screenToShow, bool toShow, float time, Image[] images, TextMeshProUGUI[] texts)
        {
            float elapsedTime = 0f;
            float from = toShow ? 0f : 1f;
            float to = toShow ? 1f : 0f;

            while (elapsedTime < time)
            {
                float t = elapsedTime / time;
                float easedT = CubicEaseInOut(t);
                float alpha = Mathf.Lerp(from, to, easedT);

                foreach (var image in images)
                {
                    Color c = image.color;
                    c.a = alpha;
                    image.color = c;
                }

                foreach (var text in texts)
                {
                    Color c = text.color;
                    c.a = alpha;
                    text.color = c;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Set final alpha
            foreach (var image in images)
            {
                Color c = image.color;
                c.a = to;
                image.color = c;
            }

            foreach (var text in texts)
            {
                Color c = text.color;
                c.a = to;
                text.color = c;
            }

            if (!toShow)
            {
                screenToShow.SetActive(false);
            }
        }

        private static float CubicEaseInOut(float t)
        {
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }
    }
}
