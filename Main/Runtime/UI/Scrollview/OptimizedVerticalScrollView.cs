﻿/**
 * Author HNB-RaBear - 2017
 **/

#if DOTWEEN
using DG.Tweening;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RCore.Inspector;
using System;
using System.Collections;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace RCore.UI
{
	public class OptimizedVerticalScrollView : MonoBehaviour
	{
		public Action onContentUpdated;
		public ScrollRect scrollView;
		public RectTransform container;
		public OptimizedScrollItem prefab;
		public int total = 1;
		public float spacing;
		public int totalCellOnRow = 1;
		public RectTransform content => scrollView.content;

		private int m_totalVisible;
		private int m_totalBuffer = 2;
		private float m_halfSizeContainer;
		private float m_cellSizeY;
		private float m_prefabSizeX;

		private List<RectTransform> m_itemsRect = new List<RectTransform>();
		private List<OptimizedScrollItem> m_itemsScrolled = new List<OptimizedScrollItem>();
		private int m_optimizedTotal;
		private Vector3 m_startPos;
		private Vector3 m_offsetVec;
		private Vector2 m_pivot;

		//Advance settings, in case the height of View is flexible
		[Separator("Advanced Settings")]
		public bool autoMatchHeight;
		public float minViewHeight;
		public float maxViewHeight;

		private void Start()
		{
			scrollView.onValueChanged.AddListener(ScrollBarChanged);

		}

		public void Init(OptimizedScrollItem pPrefab, int pTotalItems, bool pForce, int startIndex)
		{
			prefab = pPrefab;
			m_itemsScrolled.Free();
			for (int i = 0; i < m_itemsScrolled.Count; i++)
				m_itemsScrolled[i].Refresh();

			Init(pTotalItems, pForce, startIndex);
		}

		private void LateUpdate()
		{
			for (int i = 0; i < m_itemsScrolled.Count; i++)
				m_itemsScrolled[i].ManualUpdate();
		}

		public void Init(int pTotalItems, bool pForce, int startIndex = 0)
		{
			if (pTotalItems == total && !pForce)
				return;

			m_totalBuffer = 2;
			m_itemsRect = new List<RectTransform>();

			if (m_itemsScrolled == null || m_itemsScrolled.Count == 0)
			{
				m_itemsScrolled = new List<OptimizedScrollItem>();
				m_itemsScrolled.Prepare(prefab, container.parent, 5);
			}
			else
				m_itemsScrolled.Free(container);

			total = pTotalItems;

			container.anchoredPosition3D = new Vector3(0, 0, 0);

			var rectZero = m_itemsScrolled[0].GetComponent<RectTransform>();
			var prefabScale = rectZero.rect.size;
			m_cellSizeY = prefabScale.y + spacing;
			if (totalCellOnRow > 1)
				m_prefabSizeX = prefabScale.x + spacing;
			else
				m_prefabSizeX = prefabScale.x;
			m_pivot = rectZero.pivot;

			container.sizeDelta = new Vector2(m_prefabSizeX * totalCellOnRow, m_cellSizeY * Mathf.CeilToInt(total * 1f / totalCellOnRow));
			m_halfSizeContainer = container.rect.size.y * 0.5f;

			var scrollRect = scrollView.transform as RectTransform;

			// Re Update min-max view size
			if (autoMatchHeight)
			{
				float preferHeight = container.rect.size.y + spacing * 2;
				if (maxViewHeight > 0 && preferHeight > maxViewHeight)
					preferHeight = maxViewHeight;
				else if (minViewHeight > 0 && preferHeight < minViewHeight)
					preferHeight = minViewHeight;

				var size = scrollRect.rect.size;
				size.y = preferHeight;
				scrollRect.sizeDelta = size;
			}

			var viewport = scrollView.viewport;
			m_totalVisible = Mathf.CeilToInt(viewport.rect.size.y / m_cellSizeY) * totalCellOnRow;
			m_totalBuffer *= totalCellOnRow;

			m_offsetVec = Vector3.down;
			m_startPos = container.anchoredPosition3D - m_offsetVec * m_halfSizeContainer + m_offsetVec * (prefabScale.y * 0.5f);
			m_optimizedTotal = Mathf.Min(total, m_totalVisible + m_totalBuffer);
			for (int i = 0; i < m_optimizedTotal; i++)
			{
				var cellIndex = (i) % totalCellOnRow;
				var rowIndex = Mathf.FloorToInt((i) * 1f / totalCellOnRow);

				var item = m_itemsScrolled.Obtain(container);
				var rt = item.transform as RectTransform;
				rt.anchoredPosition3D = m_startPos + m_offsetVec * (rowIndex * m_cellSizeY);
				rt.anchoredPosition3D = new Vector3(-container.rect.size.x / 2 + cellIndex * m_prefabSizeX + m_prefabSizeX * 0.5f,
					rt.anchoredPosition3D.y,
					rt.anchoredPosition3D.z);
				m_itemsRect.Add(rt);

				item.gameObject.SetActive(true);
				item.UpdateContent(i, true);
			}

			prefab.gameObject.SetActive(false);
			if (startIndex <= 0)
				container.anchoredPosition3D += m_offsetVec * (m_halfSizeContainer - viewport.rect.size.y * 0.5f) + new Vector3(0, m_cellSizeY, 0);
			else
				ScrollToIndex(startIndex);
		}

#if ODIN_INSPECTOR
		[Button]
#endif
		public void ScrollToTop(bool tween = false)
		{
			scrollView.StopMovement();
			if (tween)
			{
#if DOTWEEN
				float fromY = scrollView.normalizedPosition.y;
				float toY = 1f;
				if (fromY != toY)
				{
					float time = Mathf.Abs(toY - fromY);
					if (time < 0.1f && time > 0)
						time = 0.1f;
					float val = fromY;
					DOTween.To(() => val, x => val = x, toY, time)
						.OnUpdate(() => scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x, val));
				}
#else
				scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x, 1);
#endif
			}
			else
			{
				scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x, 1);
			}
		}

#if ODIN_INSPECTOR
		[Button]
#endif
		public void ScrollToBot(bool tween = false)
		{
			scrollView.StopMovement();
			if (tween)
			{
#if DOTWEEN
				float fromY = scrollView.normalizedPosition.y;
				float toY = 0f;
				if (fromY != toY)
				{
					float time = Mathf.Abs(toY - fromY);
					if (time < 0.1f && time > 0)
						time = 0.1f;
					float val = fromY;
					DOTween.To(() => val, x => val = x, toY, time)
						.OnUpdate(() => scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x, val));
				}
#else
				scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x, 0);
#endif
			}
			else
			{
				scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x, 0);
			}
		}
		private void ScrollBarChanged(Vector2 pNormPos)
		{
			if (m_optimizedTotal <= 0)
				return;

			if (totalCellOnRow > 1)
				pNormPos.y = 1f - pNormPos.y + 0.06f;
			else
				pNormPos.y = 1f - pNormPos.y;
			if (pNormPos.y > 1)
				pNormPos.y = 1;

			// Calculate the viewport bounds in world space
			var viewport = scrollView.viewport;
			var viewportCorners = new Vector3[4];
			viewport.GetWorldCorners(viewportCorners);
			var viewportRect = new Rect(viewportCorners[0], viewportCorners[2] - viewportCorners[0]);

			int numOutOfView = Mathf.CeilToInt(pNormPos.y * (total - m_totalVisible)); //number of elements beyond the left boundary (or top)
			int firstIndex = Mathf.Max(0, numOutOfView - m_totalBuffer); //index of first element beyond the left boundary (or top)
			int originalIndex = firstIndex % m_optimizedTotal;

			int newIndex = firstIndex;
			for (int i = originalIndex; i < m_optimizedTotal; i++)
			{
				MoveItemByIndex(m_itemsRect[i], newIndex);
				m_itemsScrolled[i].UpdateContent(newIndex);
				m_itemsScrolled[i].visible = IsItemVisible(viewportRect, i);
				newIndex++;
			}
			for (int i = 0; i < originalIndex; i++)
			{
				MoveItemByIndex(m_itemsRect[i], newIndex);
				m_itemsScrolled[i].UpdateContent(newIndex);
				m_itemsScrolled[i].visible = IsItemVisible(viewportRect, i);
				newIndex++;
			}
			onContentUpdated?.Invoke();
		}

		private void CheckItemsInViewPort()
		{
			var viewport = scrollView.viewport;

			// Calculate the viewport bounds in world space
			var viewportCorners = new Vector3[4];
			viewport.GetWorldCorners(viewportCorners);
			var viewportRect = new Rect(viewportCorners[0], viewportCorners[2] - viewportCorners[0]);

			for (var i = 0; i < m_itemsRect.Count; i++)
				m_itemsScrolled[i].visible = IsItemVisible(viewportRect, i);
		}

		private bool IsItemVisible(Rect viewportRect, int index)
		{
			// Calculate item bounds in world space
			var itemCorners = new Vector3[4];
			m_itemsRect[index].GetWorldCorners(itemCorners);
			var itemRect = new Rect(itemCorners[0], itemCorners[2] - itemCorners[0]);

			// Check if the item's bounds overlap the viewport's bounds
			return viewportRect.Overlaps(itemRect);
		}

		private void MoveItemByIndex(RectTransform item, int index)
		{
			int cellIndex = index % totalCellOnRow;
			int rowIndex = Mathf.FloorToInt(index * 1f / totalCellOnRow);
			item.anchoredPosition3D = m_startPos + m_offsetVec * rowIndex * m_cellSizeY;
			item.anchoredPosition3D = new Vector3(-container.rect.size.x / 2 + cellIndex * m_prefabSizeX + m_prefabSizeX * 0.5f,
				item.anchoredPosition3D.y,
				item.anchoredPosition3D.z);
		}

		public List<OptimizedScrollItem> GetListItem() => m_itemsScrolled;

#if ODIN_INSPECTOR
		[Button]
#endif
		public void ScrollToIndex(int pIndex, bool pTween = false, Action pOnComplete = null)
		{
			pIndex = Mathf.Clamp(pIndex, 0, total - 1);
			int rowIndex = Mathf.FloorToInt(pIndex * 1f / totalCellOnRow);
			float toY = rowIndex * m_cellSizeY / (container.rect.size.y - scrollView.viewport.rect.size.y);

			toY = Mathf.Clamp01(toY);

			if (pTween)
			{
#if DOTWEEN
				float fromY = 1 - scrollView.normalizedPosition.y;
				if (toY != fromY)
				{
					float time = Mathf.Abs(toY - fromY) * 2;
					if (time < 0.1f && time > 0)
						time = 0.1f;
					float val = fromY;
					DOTween.To(() => val, x => val = x, toY, time)
						.OnUpdate(() =>
						{
							scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x, 1f - val);
						})
						.OnComplete(() =>
						{
							pOnComplete?.Invoke();
						});
				}
#else
				scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x, 1f - toY);
				pOnComplete?.Invoke();
				ScrollBarChanged(scrollView.normalizedPosition);
#endif
			}
			else
			{
				scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x, 1f - toY);
				pOnComplete?.Invoke();
				ScrollBarChanged(scrollView.normalizedPosition);
			}
		}
#if ODIN_INSPECTOR
		[Button]
#endif
		public void TestMoveItemToIndex(int a, int b)
		{
			StartCoroutine(MoveItemToIndex(a, b));
		}

		public IEnumerator MoveItemToIndex(int a, int b)
		{
			if (a < 0 || a >= total || b < 0 || b >= total || a == b)
				yield break;

			bool wait = true;
			ScrollToIndex(a, true, () => wait = false);
			yield return new WaitUntil(() => !wait);
			ScrollToIndex(b, true);
		}
	}
}