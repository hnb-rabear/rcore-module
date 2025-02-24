using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

namespace RCore.UI
{
	public class ScreenSafeArea : MonoBehaviour
	{
		public static float topOffset;
		public static float bottomOffset;
		public Canvas canvas;
		public RectTransform[] safeRects;
		public bool fixedTop;
		public bool fixedBottom;
		private ScreenOrientation m_CurrentOrientation;
		private Rect m_CurrentSafeArea;

		private void Start()
		{
			m_CurrentOrientation = Screen.orientation;
			m_CurrentSafeArea = Screen.safeArea;
			CheckSafeArea();
		}

		// private void OnValidate()
		// {
		// 	if (Application.isPlaying)
		// 		CheckSafeArea();
		// }

		[Button]
		public void Log()
		{
			var safeArea = Screen.safeArea;
			var sWidth = Screen.currentResolution.width;
			var sHeight = Screen.currentResolution.height;
			var oWidthTop = (Screen.currentResolution.width - safeArea.width - safeArea.x) / 2f;
			var oHeightTop = (Screen.currentResolution.height - safeArea.height - safeArea.y) / 2f;
			var oWidthBot = -safeArea.x / 2f;
			var oHeightBot = -safeArea.y / 2f;
			Debug.Log($"Screen size: (width:{sWidth}, height:{sHeight})"
				+ $"\nSafe area: {safeArea}"
				+ $"\nOffset Top: (width:{oWidthTop}, height:{oHeightTop})"
				+ $"\nOffset Bottom: (width:{oWidthBot}, height:{oHeightBot})");
		}

		private void CheckSafeArea()
		{
			var safeArea = Screen.safeArea;
			safeArea.height -= topOffset;
			var anchorMin = safeArea.position;
			var anchorMax = safeArea.position + safeArea.size;
			var sizeDelta = ((RectTransform)canvas.transform).sizeDelta;
			sizeDelta.y = -bottomOffset;
			((RectTransform)canvas.transform).sizeDelta = sizeDelta;
			var position = ((RectTransform)canvas.transform).anchoredPosition;
			position.y = bottomOffset / 2;
			((RectTransform)canvas.transform).anchoredPosition = position;

			anchorMin.x /= canvas.pixelRect.width;
			anchorMin.y /= canvas.pixelRect.height;
			anchorMax.x /= canvas.pixelRect.width;
			anchorMax.y /= canvas.pixelRect.height;

			foreach (var rect in safeRects)
			{
				if (!fixedBottom)
					rect.anchorMin = anchorMin;
				else
					rect.anchorMin = Vector2.zero;
				if (!fixedTop)
					rect.anchorMax = anchorMax;
				else
					rect.anchorMax = Vector2.one;
			}

			m_CurrentOrientation = Screen.orientation;
			m_CurrentSafeArea = Screen.safeArea;
		}

		public static void SetTopOffsetForBannerAd(float pBannerHeight, bool pPlaceInSafeArea = true)
		{
			float offset = 0;
			var safeAreaHeightOffer = Screen.height - Screen.safeArea.height;
			if (!pPlaceInSafeArea)
			{
				if (pBannerHeight <= safeAreaHeightOffer)
					offset = 0;
				else
					offset = pBannerHeight - safeAreaHeightOffer;
			}
			else
			{
				offset = pBannerHeight;
			}

			if (topOffset == offset)
				return;

			topOffset = offset;

			var ScreenSafeAreas = FindObjectsOfType<ScreenSafeArea>();
			foreach (var component in ScreenSafeAreas)
				component.StartCoroutine(component.IECheckSafeArea());
		}

		public static void SetBottomOffsetForBannerAd(float pBannerHeight, bool pPlaceInSafeArea = true)
		{
			float offset = 0;
			var safeAreaHeightOffer = Screen.height - Screen.safeArea.height;
			if (!pPlaceInSafeArea)
			{
				if (pBannerHeight <= safeAreaHeightOffer)
					offset = 0;
				else
					offset = pBannerHeight - safeAreaHeightOffer;
			}
			else
			{
				offset = pBannerHeight;
			}

			if (bottomOffset == offset)
				return;

			bottomOffset = offset;

			var screenSafeAreas = FindObjectsOfType<ScreenSafeArea>();
			foreach (var component in screenSafeAreas)
				component.StartCoroutine(component.IECheckSafeArea());
		}

		private IEnumerator IECheckSafeArea()
		{
			float time = 0.5f;
			while (time > 0)
			{
				CheckSafeArea();
				time -= Time.deltaTime;
				yield return null;
			}
		}

		[Button]
		private void TestTopOffsetForBannerAd(int height) => SetTopOffsetForBannerAd(height);

		[Button]
		private void TestBottomOffsetForBannerAd(int height) => SetBottomOffsetForBannerAd(height);
	}
}