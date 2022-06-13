using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseCellData
{
	public int id;
	public int cellType;
	public BaseCellData(int id, int cellType)
	{
		this.id = id;
		this.cellType = cellType;
	}
}


public class BaseScrollRect<T> : ScrollRect
{
	public delegate void SettingCellData(GameObject cell, T data);

	public Vector4 padding = Vector4.zero;
	public Vector2 spacing = Vector2.zero;

	public enum eDragDir
	{
		DragUp,
		DragDown,
		DragLeft,
		DragRight,
	}

	public enum eCellAlignment
	{
		LeftUp_ToRight,
		LeftUp_ToDown,
		RightUp_ToLeft,
		RightUp_ToDown,

		LeftDown_ToRight,
		LeftDown_ToUp,
		RightDown_ToLeft,
		RightDown_ToUp,
	}

	public eCellAlignment cellAlignment = eCellAlignment.LeftUp_ToRight;


	[System.Serializable]
	public class CellPrefab
	{
		public int type;
		public GameObject prefabObj;
		public CellPrefab(int type, GameObject prefab)
		{
			this.type = type;
			this.prefabObj = prefab;
		}
	}

	//public GameObject cellPrefab;
	protected List<T> totalDataList = new List<T>();
	public T GetData(int index)
	{
		T data = default(T);
		if (index >= 0 && index < totalDataList.Count)
			data = totalDataList[index];

		return data;
	}
	public int initDataCount = 200;

	public int startIndex = 0;
	public int endIndex = 0;


	override protected void LateUpdate()
	{
		base.LateUpdate();

		if (this.verticalScrollbar)
		{
			this.verticalScrollbar.size = 0;
		}
		if (this.horizontalScrollbar)
		{
			this.horizontalScrollbar.size = 0;
		}
	}

	override public void Rebuild(CanvasUpdate executing)
	{
		base.Rebuild(executing);

		if (this.verticalScrollbar)
		{
			this.verticalScrollbar.size = 0;
		}
		if (this.horizontalScrollbar)
		{
			this.horizontalScrollbar.size = 0;
		}
	}

	/// <summary>
	/// 정렬방식에 맞게 맨 앞쪽 row에 있는 cell들을 찾아 반환.
	/// </summary>
	/// <param name="cells"></param>
	/// <param name="rowBounds"></param>
	protected void GetFrontCells(List<Transform> cells, ref Bounds rowBounds)
	{
		Vector3[] cellCorners = new Vector3[4];

		viewport.GetWorldCorners(cellCorners);
		Bounds viewBounds = InternalGetBounds(cellCorners);

		bool bCheck = false;
		int childCount = content.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			RectTransform rectTrans = content.GetChild(i) as RectTransform;
			rectTrans.GetWorldCorners(cellCorners);
			Bounds cellBounds = InternalGetBounds(cellCorners);

			switch (cellAlignment)
			{
				case eCellAlignment.LeftDown_ToRight:
				case eCellAlignment.LeftUp_ToRight:
					bCheck = cellBounds.max.x - viewBounds.max.x >= 0.0f;
					break;
				case eCellAlignment.RightDown_ToLeft:
				case eCellAlignment.RightUp_ToLeft:
					bCheck = cellBounds.min.x - viewBounds.min.x <= 0.0f;
					break;
				case eCellAlignment.LeftDown_ToUp:
				case eCellAlignment.RightDown_ToUp:
					bCheck = cellBounds.max.y - viewBounds.max.y >= 0.00f;
					break;
				case eCellAlignment.LeftUp_ToDown:
				case eCellAlignment.RightUp_ToDown:
					bCheck = cellBounds.min.y - viewBounds.min.y <= 0.0f;
					break;
			}

			ExpandBound(ref rowBounds, cellBounds);

			cells.Add(rectTrans);

			if (bCheck == true)
			{
				break;
			}
		}
	}

	/// <summary>
	/// 정렬 방식에 맞게 마지막 row에 있는 cell들을 반환.
	/// </summary>
	/// <param name="dragDir"></param>
	/// <param name="cells"></param>
	/// <param name="rowBounds"></param>
	protected void GetLastCells(eDragDir dragDir, List<Transform> cells, ref Bounds rowBounds)
	{
		Vector3[] cellCorners = new Vector3[4];

		viewport.GetWorldCorners(cellCorners);
		Bounds viewBounds = InternalGetBounds(cellCorners);

		bool bCheck = false;
		int childCount = content.childCount;
		for (int i = childCount - 1; i >= 0; --i)
		{
			RectTransform rectTrans = content.GetChild(i) as RectTransform;
			rectTrans.GetWorldCorners(cellCorners);
			Bounds cellBounds = InternalGetBounds(cellCorners);

			float diff = 0.0f;
			switch (cellAlignment)
			{
				case eCellAlignment.LeftDown_ToRight:
				case eCellAlignment.LeftUp_ToRight:
					diff = cellBounds.min.x - viewBounds.min.x;
					bCheck = diff <= 0.0f || Mathf.Abs(diff) <= 0.01f;
					break;
				case eCellAlignment.RightDown_ToLeft:
				case eCellAlignment.RightUp_ToLeft:
					diff = cellBounds.max.x - viewBounds.max.x;
					bCheck = diff >= 0 || Mathf.Abs(diff) <= 0.01f;
					break;
				case eCellAlignment.LeftDown_ToUp:
				case eCellAlignment.RightDown_ToUp:
					diff = cellBounds.min.y - viewBounds.min.y;
					bCheck = diff <= 0.0f || Mathf.Abs(diff) <= 0.01f;
					break;
				case eCellAlignment.LeftUp_ToDown:
				case eCellAlignment.RightUp_ToDown:
					diff = cellBounds.max.y - viewBounds.max.y;
					bCheck = diff >= 0.0f || Mathf.Abs(diff) <= 0.01f;
					break;
			}

			ExpandBound(ref rowBounds, cellBounds);

			cells.Add(rectTrans);

			if (bCheck == true)
			{
				break;
			}
		}
	}


	#region Add&Remove cell Func
	protected Vector2 RemoveFromFront(eDragDir dragDir)
	{
		Vector2 removeSize = Vector2.zero;

		Vector3[] viewCorners = new Vector3[4];
		viewRect.GetWorldCorners(viewCorners);
		Bounds viewBounds = InternalGetBounds(viewCorners);

		List<Transform> cells = new List<Transform>();
		Bounds rowBounds = new Bounds();
		GetFrontCells(cells, ref rowBounds);

		bool bCheck = false;
		switch (cellAlignment)
		{
			case eCellAlignment.RightUp_ToDown:
			case eCellAlignment.RightDown_ToUp:
				if (dragDir == eDragDir.DragRight)
				{
					bCheck = viewBounds.max.x < rowBounds.min.x;
				}
				break;
			case eCellAlignment.LeftUp_ToDown:
			case eCellAlignment.LeftDown_ToUp:
				if (dragDir == eDragDir.DragLeft)
				{
					bCheck = viewBounds.min.x > rowBounds.max.x;
				}
				break;
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.RightUp_ToLeft:
				if (dragDir == eDragDir.DragUp)
				{
					bCheck = viewBounds.max.y < rowBounds.min.y;
				}
				break;
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.RightDown_ToLeft:
				if (dragDir == eDragDir.DragDown)
				{
					bCheck = viewBounds.min.y > rowBounds.max.y;
				}
				break;
		}

		// ViewPort보다 컨텐트 사이즈가 작이 지는 경우 삭제 하지 않도록 한다..
		switch (cellAlignment)
		{
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.RightUp_ToLeft:
			case eCellAlignment.RightDown_ToLeft:
				if (viewRect.rect.size.y > content.rect.size.y - rowBounds.size.y)
					bCheck = false;
				break;
			case eCellAlignment.LeftUp_ToDown:
			case eCellAlignment.LeftDown_ToUp:
			case eCellAlignment.RightUp_ToDown:
			case eCellAlignment.RightDown_ToUp:
				if (viewRect.rect.size.x > content.rect.size.x - rowBounds.size.x)
					bCheck = false;
				break;
		}

		if (bCheck && cells.Count > 0)
		{
			foreach (Transform child in cells)
			{
				//DestroyImmediate(trans.gameObject);
				child.SetParent(null);
				T data = GetData(endIndex);
				int cellType = GetCellType(data);
				AddFreeCell(cellType, child.gameObject);

				startIndex++;
			}

			removeSize = rowBounds.size;
		}

		return removeSize;
	}

	protected Vector2 RemoveFromLast(eDragDir dragDir)
	{
		Vector2 removeSize = Vector2.zero;

		Vector3[] viewCorners = new Vector3[4];
		viewRect.GetWorldCorners(viewCorners);
		Bounds viewBounds = InternalGetBounds(viewCorners);

		List<Transform> cells = new List<Transform>();
		Bounds rowBounds = new Bounds();
		GetLastCells(dragDir, cells, ref rowBounds);

		bool bCheck = false;
		switch (cellAlignment)
		{
			case eCellAlignment.RightUp_ToDown:
			case eCellAlignment.RightDown_ToUp:
				if (dragDir == eDragDir.DragLeft)
				{
					bCheck = viewBounds.min.x > rowBounds.max.x;
				}
				break;
			case eCellAlignment.LeftUp_ToDown:
			case eCellAlignment.LeftDown_ToUp:
				if (dragDir == eDragDir.DragRight)
				{
					bCheck = viewBounds.max.x < rowBounds.min.x;
				}
				break;
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.RightUp_ToLeft:
				if (dragDir == eDragDir.DragDown)
				{
					bCheck = viewBounds.min.y > rowBounds.max.y;
				}
				break;
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.RightDown_ToLeft:
				if (dragDir == eDragDir.DragUp)
				{
					bCheck = viewBounds.max.y < rowBounds.min.y;
				}
				break;
		}

		//ViewPort보다 컨텐트 사이즈가 작이 지는 경우 삭제 하지 않도록 한다..
		switch (cellAlignment)
		{
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.RightUp_ToLeft:
			case eCellAlignment.RightDown_ToLeft:
				if (viewRect.rect.size.y > content.rect.size.y - rowBounds.size.y)
					bCheck = false;
				break;
			case eCellAlignment.LeftUp_ToDown:
			case eCellAlignment.LeftDown_ToUp:
			case eCellAlignment.RightUp_ToDown:
			case eCellAlignment.RightDown_ToUp:
				if (viewRect.rect.size.x > content.rect.size.x - rowBounds.size.x)
					bCheck = false;
				break;
		}

		if (bCheck && cells.Count > 0)
		{
			foreach (Transform child in cells)
			{
				//DestroyImmediate(trans.gameObject);
				child.SetParent(null);
				T data = GetData(endIndex);
				int cellType = GetCellType(data);
				AddFreeCell(cellType, child.gameObject);

				endIndex--;
			}

			removeSize = rowBounds.size;
		}

		return removeSize;
	}

	protected virtual int GetCellType(T data)
	{
		return 0;
	}

	protected Vector2 AddToFront(eDragDir dragDir)
	{
		Vector2 addSize = Vector2.zero;

		if (content.childCount == 0)
			return addSize;

		Vector3[] viewCorners = new Vector3[4];
		viewRect.GetWorldCorners(viewCorners);
		Bounds viewBounds = InternalGetBounds(viewCorners);

		List<Transform> frontCells = new List<Transform>();
		Bounds rowBounds = new Bounds();
		GetFrontCells(frontCells, ref rowBounds);

		bool bCheck = false;
		switch (cellAlignment)
		{
			case eCellAlignment.RightUp_ToDown:
			case eCellAlignment.RightDown_ToUp:
				if (dragDir == eDragDir.DragLeft)
				{
					bCheck = viewBounds.max.x > rowBounds.max.x;
				}
				break;
			case eCellAlignment.LeftUp_ToDown:
			case eCellAlignment.LeftDown_ToUp:
				if (dragDir == eDragDir.DragRight)
				{
					bCheck = viewBounds.min.x < rowBounds.min.x;
				}
				break;
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.RightUp_ToLeft:
				if (dragDir == eDragDir.DragDown)
				{
					bCheck = viewBounds.max.y > rowBounds.max.y;
				}
				break;
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.RightDown_ToLeft:
				if (dragDir == eDragDir.DragUp)
				{
					bCheck = viewBounds.min.y < rowBounds.min.y;
				}
				break;
		}

		if (bCheck == true)
		{
			addSize = AddItemToFront(viewRect);
		}

		return addSize;
	}

	protected Vector2 AddItemToFront(RectTransform itemArea)
	{
		Vector2 addSize = Vector2.zero;

		bool overArea = false;
		while (startIndex > 0 && overArea == false)
		{
			startIndex--;

			T data = GetData(startIndex);//totalDataList[startIndex];
			if (data == null)
				break;

			GameObject newCell = GetFreeCell(GetCellType(data));
			SetCellData(newCell, data);

			RectTransform newCellRect = newCell.GetComponent<RectTransform>();
			Vector2 newCellSize = newCellRect.rect.size;

			AddCellToContent(newCell, true);

			switch (cellAlignment)
			{
				case eCellAlignment.LeftUp_ToRight:
				case eCellAlignment.LeftDown_ToRight:
				case eCellAlignment.RightUp_ToLeft:
				case eCellAlignment.RightDown_ToLeft:
					//추가 되는 셀의 넓이가 ContentRect를 넘어 서는지 체크.
					addSize.x += newCellSize.x;
					overArea = addSize.x >= itemArea.rect.size.x;

					//추가 되는 셀의 넓이중 가장 큰 녀석값을 저장.
					if (addSize.y < newCellSize.y)
						addSize.y = newCellSize.y;
					break;
				case eCellAlignment.LeftUp_ToDown:
				case eCellAlignment.RightUp_ToDown:
				case eCellAlignment.LeftDown_ToUp:
				case eCellAlignment.RightDown_ToUp:
					//추가되는 셀의 높이가 ContentRect를 넘어 서는지 체크.
					addSize.y += newCellSize.y;
					overArea = addSize.y >= itemArea.rect.size.y;

					//추가 되는 셀의 넓이중 가장 큰 녀석값을 저장.
					if (addSize.x < newCellSize.x)
						addSize.x = newCellSize.x;
					break;
			}
		}

		return addSize;
	}

	protected Vector2 AddToLast(eDragDir dragDir)
	{
		Vector2 addSize = Vector2.zero;

		if (content.childCount == 0)
			return addSize;

		Vector3[] viewCorners = new Vector3[4];
		viewRect.GetWorldCorners(viewCorners);
		Bounds viewBounds = InternalGetBounds(viewCorners);

		List<Transform> cells = new List<Transform>();
		Bounds rowBounds = new Bounds();
		GetLastCells(dragDir, cells, ref rowBounds);

		bool bCheck = false;
		switch (cellAlignment)
		{
			case eCellAlignment.RightUp_ToDown:
			case eCellAlignment.RightDown_ToUp:
				if (dragDir == eDragDir.DragRight)
				{
					bCheck = viewBounds.min.x < rowBounds.min.x;
				}
				break;
			case eCellAlignment.LeftUp_ToDown:
			case eCellAlignment.LeftDown_ToUp:
				if (dragDir == eDragDir.DragLeft)
				{
					bCheck = viewBounds.max.x > rowBounds.max.x;
				}
				break;
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.RightUp_ToLeft:
				if (dragDir == eDragDir.DragUp)
				{
					bCheck = viewBounds.min.y < rowBounds.min.y;
				}
				break;
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.RightDown_ToLeft:
				if (dragDir == eDragDir.DragDown)
				{
					bCheck = viewBounds.max.y > rowBounds.max.y;
				}
				break;
		}

		if (bCheck == true)
		{
			addSize = AddItemToLast(viewRect);
		}

		return addSize;
	}

	protected Vector2 AddItemToLast(RectTransform itemArea)
	{
		Vector2 addSize = Vector2.zero;

		bool overArea = false;
		int index = 0;
		while (endIndex <= totalDataList.Count - 1 && overArea == false)
		{
			T data = GetData(endIndex);//totalDataList[endIndex];
			if (data == null)
				break;

			GameObject newCell = GetFreeCell(GetCellType(data));
			if (newCell == null)
				break;

			SetCellData(newCell, data);

			RectTransform newCellRect = newCell.GetComponent<RectTransform>();
			Vector2 newCellSize = newCellRect.rect.size;

			AddCellToContent(newCell, false);

			switch (cellAlignment)
			{
				case eCellAlignment.LeftUp_ToRight:
				case eCellAlignment.LeftDown_ToRight:
				case eCellAlignment.RightUp_ToLeft:
				case eCellAlignment.RightDown_ToLeft:
					//추가 되는 셀의 넓이가 ContentRect를 넘어 서는지 체크.
					addSize.x += newCellSize.x;
					if (index != 0)
						addSize.x += spacing.x;
					else
						addSize.x += padding.x + padding.y;

					index++;
					overArea = addSize.x >= itemArea.rect.size.x;

					//추가 되는 셀의 넓이중 가장 큰 녀석값을 저장.
					if (addSize.y < newCellSize.y)
						addSize.y = newCellSize.y;
					break;
				case eCellAlignment.LeftUp_ToDown:
				case eCellAlignment.RightUp_ToDown:
				case eCellAlignment.LeftDown_ToUp:
				case eCellAlignment.RightDown_ToUp:
					//추가되는 셀의 높이가 ContentRect를 넘어 서는지 체크.
					addSize.y += newCellSize.y;
					if (index != 0)
						addSize.y += spacing.y;
					else
						addSize.y += padding.z + padding.w;

					index++;
					overArea = addSize.y >= itemArea.rect.size.y;

					//추가 되는 셀의 넓이중 가장 큰 녀석값을 저장.
					if (addSize.x < newCellSize.x)
						addSize.x = newCellSize.x;
					break;
			}

			endIndex++;
		}

		return addSize;
	}

	#endregion

	#region Cell Cordinator Func
	protected Vector2 GetCellStartPos(eCellAlignment cellAlignment, Vector2 viewPortSize)
	{
		Vector2 offset = Vector2.zero;
		switch (cellAlignment)
		{
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.LeftUp_ToDown:
				offset.x = 0.0f + padding.x;
				offset.y = 0.0f - padding.z;
				break;
			case eCellAlignment.RightUp_ToLeft:
			case eCellAlignment.RightUp_ToDown:
				offset.x = viewPortSize.x - padding.y;
				offset.y = 0.0f + padding.z;
				break;
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.LeftDown_ToUp:
				offset.x = 0.0f + padding.x;
				offset.y = -viewPortSize.y + padding.z;
				break;
			case eCellAlignment.RightDown_ToLeft:
			case eCellAlignment.RightDown_ToUp:
				offset.x = viewPortSize.x - padding.y;
				offset.y = -viewPortSize.y + padding.z;
				break;
		}

		return offset;
	}

	protected Vector2 GetCellOffset(eCellAlignment cellAlignment, RectTransform cellRect)
	{
		Vector2 newCellSize = cellRect.rect.size;
		Vector2 cellLocalOffset = newCellSize * cellRect.pivot;
		Vector2 cellOffset = Vector2.zero;

		switch (cellAlignment)
		{
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.LeftUp_ToDown:
				cellOffset.x += cellLocalOffset.x;
				cellOffset.y -= cellLocalOffset.y;
				break;
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.LeftDown_ToUp:
				cellOffset.x += cellLocalOffset.x;
				cellOffset.y += cellLocalOffset.y;
				break;
			case eCellAlignment.RightUp_ToLeft:
			case eCellAlignment.RightUp_ToDown:
				cellOffset.x -= cellLocalOffset.x;
				cellOffset.y -= cellLocalOffset.y;
				break;
			case eCellAlignment.RightDown_ToLeft:
			case eCellAlignment.RightDown_ToUp:
				cellOffset.x -= cellLocalOffset.x;
				cellOffset.y += cellLocalOffset.y;
				break;
		}

		return cellOffset;
	}

	protected bool CheckNextCellPosition(eCellAlignment cellAlignment, ref Vector2 offset, Vector2 newCellSize, Vector2 viewportSize, Vector2 contentSize, Bounds currentBounds)
	{
		bool overArea = false;

		switch (cellAlignment)
		{
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.LeftDown_ToRight:
				//if (offset.x + newCellSize.x >= viewportSize.x)
				if (currentBounds.size.x >= viewportSize.x)
				{
					offset.y += cellAlignment == eCellAlignment.LeftUp_ToRight ? -currentBounds.size.y : currentBounds.size.y;
					offset.x = 0.0f;

					overArea = true;
				}
				else
				{
					offset.x += newCellSize.x;
				}
				break;
			case eCellAlignment.RightUp_ToLeft:
			case eCellAlignment.RightDown_ToLeft:
				//if (contentSize.x - (offset.x - newCellSize.x) >= viewportSize.x)
				if (currentBounds.size.x >= viewportSize.x)
				{
					offset.y += cellAlignment == eCellAlignment.RightUp_ToLeft ? -currentBounds.size.y : currentBounds.size.y;
					offset.x = contentSize.x;

					overArea = true;
				}
				else
				{
					offset.x -= newCellSize.x;

				}
				break;
			case eCellAlignment.LeftUp_ToDown:
			case eCellAlignment.RightUp_ToDown:
				//if (offset.y - newCellSize.y <= -viewportSize.y)
				if (currentBounds.size.y >= viewportSize.y)
				{
					offset.x += cellAlignment == eCellAlignment.LeftUp_ToDown ? currentBounds.size.x : -currentBounds.size.x;
					offset.y = 0.0f;

					overArea = true;
				}
				else
				{
					offset.y -= newCellSize.y;
				}
				break;
			case eCellAlignment.LeftDown_ToUp:
			case eCellAlignment.RightDown_ToUp:
				//if (0.0f <= offset.y + newCellSize.y)
				if (currentBounds.size.y >= viewportSize.y)
				{
					offset.x += cellAlignment == eCellAlignment.LeftDown_ToUp ? currentBounds.size.x : -currentBounds.size.x;
					offset.y = -contentSize.y;

					overArea = true;
				}
				else
				{
					offset.y += newCellSize.y;
				}
				break;
		}

		return overArea;
	}

	protected bool CheckViewPortArea(Vector2 addSize, ref Vector2 checkSize, Vector2 viewPortSize)
	{
		bool overArea = false;

		switch (cellAlignment)
		{
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.RightUp_ToLeft:
			case eCellAlignment.RightDown_ToLeft:
				checkSize.y += addSize.y;
				overArea = checkSize.y >= viewPortSize.y;

				if (checkSize.x < addSize.x)
					checkSize.x = addSize.x;
				break;
			case eCellAlignment.LeftUp_ToDown:
			case eCellAlignment.RightUp_ToDown:
			case eCellAlignment.LeftDown_ToUp:
			case eCellAlignment.RightDown_ToUp:
				checkSize.x += addSize.x;
				overArea = checkSize.x >= viewPortSize.x;

				if (checkSize.y < addSize.y)
					checkSize.y = addSize.y;
				break;
		}

		return overArea;
	}

	#endregion


	#region Bounds Func

	protected void ExpandBound(ref Bounds current, Bounds addCell)
	{
		if (current.size == Vector3.zero)
		{
			current = addCell;
		}
		else
		{
			current.Encapsulate(addCell.min);
			current.Encapsulate(addCell.max);
		}
	}

	protected Bounds InternalGetBounds(Vector3[] corners)
	{
		var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

		for (int j = 0; j < 4; j++)
		{
			Vector3 v = corners[j];
			vMin = Vector3.Min(v, vMin);
			vMax = Vector3.Max(v, vMax);
		}

		var bounds = new Bounds(vMin, Vector3.zero);
		bounds.Encapsulate(vMax);
		return bounds;
	}

	protected void GetCornerPosition(RectTransform rect, Vector3[] corners)
	{
		rect.GetLocalCorners(corners);

		corners[0] += rect.localPosition;
		corners[1] += rect.localPosition;
		corners[2] += rect.localPosition;
		corners[3] += rect.localPosition;
	}

	#endregion


	#region Init Func
	protected void InitContent()
	{
		RemoveAllChild(content);
		RemoveAllChild(freeCellRoot);

		startIndex = endIndex = 0;
	}

	protected void RemoveAllChild(Transform root)
	{
		if (root != null)
		{
			while (root.childCount > 0)
			{
				Transform child = root.GetChild(0);
				child.SetParent(null);
				DestroyImmediate(child.gameObject);
			}
		}
	}

	#endregion

	#region CellData & CellPrefab Func
	protected Dictionary<int, List<GameObject>> freeCellObjects = new Dictionary<int, List<GameObject>>();
	public List<CellPrefab> cellPrefabs = new List<CellPrefab>();
	/// <summary>
	/// 타입에 맞는 Cell Prefab을 반환.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	protected GameObject GetCellPrefab(int type)
	{
		GameObject prefab = null;

		int count = cellPrefabs.Count;
		if (count > 0)
		{
			prefab = cellPrefabs[type].prefabObj;
		}

		return prefab;
	}

	/// <summary>
	/// 재활용 가능한 셀을 타입에 맞게  반환. 재활용할 셀이 없으면 생성.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	protected GameObject GetFreeCell(int type)
	{
		GameObject obj = null;
		if (freeCellObjects.ContainsKey(type) && freeCellObjects[type].Count > 0)
		{
			obj = freeCellObjects[type][0];
			freeCellObjects[type].RemoveAt(0);
		}
		else
		{
			GameObject prefab = GetCellPrefab(type);
			if (prefab != null)
				obj = Instantiate(prefab);
		}

		return obj;
	}

	public Transform freeCellRoot;
	/// <summary>
	/// 재활용할 셀들 보관.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="obj"></param>
	protected void AddFreeCell(int type, GameObject obj)
	{
		if (obj != null)
		{
			obj.transform.SetParent(freeCellRoot);

			if (freeCellObjects.ContainsKey(type) == false)
			{
				List<GameObject> newList = new List<GameObject>();
				newList.Add(obj);

				freeCellObjects.Add(type, newList);
			}
			else
			{
				freeCellObjects[type].Add(obj);
			}
		}
	}

	protected void AddCellToContent(GameObject newCell, bool isFront)
	{
		if (newCell == null)
			return;

		newCell.transform.SetParent(content);
		newCell.transform.localPosition = Vector3.zero;
		newCell.transform.localScale = Vector3.one;

		if (isFront == true)
			newCell.transform.SetAsFirstSibling();
		else
			newCell.transform.SetAsLastSibling();
	}

	protected bool CheckCellPrefab()
	{
		bool bCheck = false;

		if (cellPrefabs.Count > 0)
		{
			bCheck = true;
			foreach (var temp in cellPrefabs)
			{
				if (temp.prefabObj == null)
				{
					bCheck = false;
					break;
				}
			}
		}

		return bCheck;
	}
	#endregion

	public SettingCellData settingCellDataFunc;
	virtual protected void SetCellData(GameObject cell, T data)
	{
		if (cell == null)
			return;

		//if (cell != null && data != null)
		//	cell.GetComponentInChildren<Text>().text = $"Name {data.id}";

		if (settingCellDataFunc != null)
			settingCellDataFunc(cell, data);
	}
	virtual public void SetData(List<T> dataList)
	{
		totalDataList = dataList;
	}
}
