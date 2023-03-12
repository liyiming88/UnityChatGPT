using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
namespace MotionverseSDK.Core
{
    public static class CoroutineTween
    {
        public static IEnumerator LerpPosition(this Transform elementToLerp, Vector3 finalValue, float duration,
            [CanBeNull] AnimationCurve animationCurve = null)
        {
            if (duration > 0)
            {
                float t = 0.0f;
                Vector3 start = elementToLerp.position;
                Vector3 end = finalValue;

                while (t < duration)
                {
                    t += Time.deltaTime;
                    elementToLerp.position =
                        Vector3.LerpUnclamped(start, end, animationCurve?.Evaluate(t / duration) ?? t / duration);
                    yield return null;
                }
            }

            elementToLerp.position = finalValue;
        }

        public static IEnumerator LerpRectTransform(this RectTransform elementToLerp, Vector2 finalValue, float duration,
        [CanBeNull] AnimationCurve animationCurve = null)
        {
            if (duration > 0)
            {
                float t = 0.0f;
                Vector2 start = elementToLerp.sizeDelta;
                Vector2 end = finalValue;

                while (t < duration)
                {
                    t += Time.deltaTime;
                    elementToLerp.sizeDelta =
                        Vector2.LerpUnclamped(start, end, animationCurve?.Evaluate(t / duration) ?? t / duration);
                    yield return null;
                }
            }

            elementToLerp.sizeDelta = finalValue;
        }


        public static IEnumerator LerpScale(this Transform elementToLerp, Vector3 finalValue, float duration,
            [CanBeNull] AnimationCurve animationCurve = null)
        {
            if (duration > 0)
            {
                float t = 0.0f;
                Vector3 start = elementToLerp.localScale;
                Vector3 end = finalValue;

                while (t < duration)
                {
                    t += Time.deltaTime;
                    elementToLerp.localScale =
                        Vector3.LerpUnclamped(start, end, animationCurve?.Evaluate(t / duration) ?? t / duration);
                    yield return null;
                }
            }

            elementToLerp.localScale = finalValue;
        }

        public static IEnumerator LerpRotation(this Transform elementToLerp, Vector3 finalValue, float duration,
        [CanBeNull] AnimationCurve animationCurve = null)
        {
            if (duration > 0)
            {
                float t = 0.0f;
                Vector3 start = elementToLerp.localEulerAngles;
                Vector3 end = finalValue;

                while (t < duration)
                {
                    t += Time.deltaTime;
                    elementToLerp.localEulerAngles =
                        Vector3.Lerp(start, end, animationCurve?.Evaluate(t / duration) ?? t / duration);
                    yield return null;
                }
            }

            elementToLerp.localEulerAngles = finalValue;
        }

        public static IEnumerator LerpColor(this Image elementToLerp, Color finalValue, float duration,
            [CanBeNull] AnimationCurve animationCurve = null)
        {
            if (duration > 0)
            {
                float t = 0.0f;
                Color start = elementToLerp.color;
                Color end = finalValue;

                while (t < duration)
                {
                    t += Time.deltaTime;
                    elementToLerp.color = Color.LerpUnclamped(start, end, animationCurve?.Evaluate(t / duration) ?? t / duration);
                    yield return null;
                }
            }

            elementToLerp.color = finalValue;
        }
    }
}