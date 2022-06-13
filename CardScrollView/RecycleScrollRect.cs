using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecycleScrollRect<T> : BaseScrollRect<T>
{
	override protected void LateUpdate()
	{
		base.LateUpdate();

		UpdateCell();
	}

	[SerializeField] GameObject emptyPanel;
	public void ShowEmptyPanel(bool bShow)
	{
		this.SetActive(emptyPanel, bShow);
	}

	override public void SetData(List<T> dataList)
	{
		base.SetData(dataList);

		InitContent();

		if (CheckCellPrefab() == false)
			return;

		Vector2 contentSize = CalcEntireContentSize(totalDataList, viewport);
		InitContentPosition(contentSize, content, viewport);

		Vector2 checkSize = Vector2.zero;
		bool overArea = false;
		do
		{
			Vector2 addSize = AddItemToLast(viewport);

			overArea = CheckViewPortArea(addSize, ref checkSize, viewport.rect.size);

		} while (endIndex < totalDataList.Count && overArea == false);
		UpdateCellPositions();
	}


	private int rowItemCount = 0;
	private int totalRowCount = 0;
	private Vector2 cellSize = Vector2.zero;
	private RectTransform cellRect;

	/// <summary>
	/// 전체 사이즈를 계산한다.
	/// </summary>
	/// <param name="dataList"></param>
	/// <returns></returns>
	private Vector2 CalcEntireContentSize(List<T> dataList, RectTransform viewPort)
	{
		int dataCount = dataList?.Count ?? 0;

		Vector2 size = Vector2.zero;
		int cellType = 0;
		if (dataList.Count > 0)
		{
			cellType = GetCellType(dataList[0]);
		}
		GameObject cellPrefab = GetCellPrefab(cellType);
		if (cellPrefab == null)
			return size;

		cellRect = cellPrefab?.transform as RectTransform ?? null;
		cellSize = cellRect?.rect.size ?? Vector2.zero;

		Vector2 rowSize = Vector2.zero;
		rowItemCount = CalcRowSize(cellSize, viewPort, ref rowSize);
		totalRowCount = dataCount / rowItemCount;
		if (dataCount % rowItemCount != 0)
			totalRowCount++;

		size = rowSize;

		switch (cellAlignment)
		{
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.RightUp_ToLeft:
			case eCellAlignment.RightDown_ToLeft:
				size.y = padding.z + padding.w; //위/아래 여백.
				size.y += (rowSize.y * totalRowCount); //실제 사이즈.
				size.y += (spacing.y * (totalRowCount - 1)); //각 row 사이 간격.
				break;
			case eCellAlignment.LeftUp_ToDown:
			case eCellAlignment.RightUp_ToDown:
			case eCellAlignment.LeftDown_ToUp:
			case eCellAlignment.RightDown_ToUp:
				size.x = padding.x + padding.y; //좌/우 여백.
				size.x += rowSize.x * totalRowCount;
				size.x += (spacing.x * (totalRowCount - 1)); //각 row 사이 간격.
				break;
		}

		return size;
	}

	private int CalcRowSize(Vector2 cellSize, RectTransform viewPort, ref Vector2 size)
	{
		int count = 0;
		size = Vector2.zero;

		bool overArea = false;
		int index = 0;
		while (overArea == false)
		{
			switch (cellAlignment)
			{
				case eCellAlignment.LeftUp_ToRight:
				case eCellAlignment.LeftDown_ToRight:
				case eCellAlignment.RightUp_ToLeft:
				case eCellAlignment.RightDown_ToLeft:
					//추가 되는 셀의 넓이가 ContentRect를 넘어 서는지 체크.
					size.x += cellSize.x;
					if (index != 0)
						size.x += spacing.x;
					else
						size.x += padding.x + padding.y;

					index++;
					overArea = size.x >= viewPort.rect.size.x;

					//추가 되는 셀의 넓이중 가장 큰 녀석값을 저장.
					if (size.y < cellSize.y)
						size.y = cellSize.y;
					break;
				case eCellAlignment.LeftUp_ToDown:
				case eCellAlignment.RightUp_ToDown:
				case eCellAlignment.LeftDown_ToUp:
				case eCellAlignment.RightDown_ToUp:
					//추가되는 셀의 높이가 ContentRect를 넘어 서는지 체크.
					size.y += cellSize.y;
					if (index != 0)
						size.y += spacing.y;
					else
						size.y += padding.z + padding.w;

					index++;
					overArea = size.y >= viewPort.rect.size.y;

					//추가 되는 셀의 넓이중 가장 큰 녀석값을 저장.
					if (size.x < cellSize.x)
						size.x = cellSize.x;
					break;
			}

			count++;
		}

		return count;
	}

	private void InitContentPosition(Vector2 contentSize, RectTransform content, RectTransform viewport)
	{
		Vector2 sizeDelta = content.sizeDelta;
		sizeDelta = contentSize;
		content.sizeDelta = sizeDelta;

		Vector3 localPos = content.localPosition;
		switch (cellAlignment)
		{
			case eCellAlignment.RightDown_ToUp:
			case eCellAlignment.RightDown_ToLeft:
				localPos.y = sizeDelta.y - viewport.rect.size.y;
				localPos.x = -(sizeDelta.x - viewport.rect.size.x);
				break;
			case eCellAlignment.RightUp_ToDown:
			case eCellAlignment.RightUp_ToLeft:
				localPos.y = 0.0f;
				localPos.x = -(sizeDelta.x - viewport.rect.size.x);
				break;
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.LeftUp_ToDown:
				localPos = Vector3.zero;
				break;
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.LeftDown_ToUp:
				localPos.y = sizeDelta.y - viewport.rect.size.y;
				break;
		}

		content.localPosition = localPos;
		content.anchoredPosition = localPos;
	}

	#region StartIndex to EndIndex Cell
	void UpdateCell()
	{
		Vector2 newIndex = GetStartEndIndex();
		int newStartIndex = (int)newIndex.x;
		int newEndIndex = (int)newIndex.y;
		int oldStart = startIndex;
		int oldEndIndex = endIndex;


		int deleteCount = 0;
		int addCount = 0;

		bool updateFlag = false;

		//기존 영역과 겹치는 부분이 없는 경우는 전부 제거 하고, 새로운 갯수 만큼 추가
		if (endIndex < newStartIndex || newEndIndex < startIndex)
		{
			while (content.childCount > 0)
			{
				Transform child = null;
				if (content.childCount > 0)
					child = content.GetChild(0);

				if (child != null)
				{
					var baseSlot = child.GetComponent<BaseSlot>();
					int cellType = baseSlot != null ? baseSlot.GetCellType() : 0;

					child.SetParent(null);
					AddFreeCell(cellType, child.gameObject);

					deleteCount++;
				}
			}

			for (int i = newStartIndex; i < newEndIndex; ++i)
			{
				T data = GetData(i);
				if (data != null)
				{
					GameObject newCell = GetFreeCell(GetCellType(data));
					SetCellData(newCell, data);

					AddCellToContent(newCell, false);

					addCount++;
				}
			}

			updateFlag = true;
			startIndex = newStartIndex;
			endIndex = newEndIndex;
		}
		else
		{
			if (startIndex != newStartIndex)
			{
				//작은 index 에서 큰 index로 변경...(
				if (startIndex < newStartIndex)
				{
					for (int i = startIndex; i < newStartIndex; ++i)
					{
						Transform child = null;
						if (content.childCount > 0)
							child = content.GetChild(0);

						if (child != null)
						{
							child.SetParent(null);
							AddFreeCell(0, child.gameObject);
						}
					}
				}
				else if (startIndex > newStartIndex)
				{
					for (int i = startIndex - 1; i >= newStartIndex; --i)
					{
						T data = GetData(i);
						if (data != null)
						{
							GameObject newCell = GetFreeCell(GetCellType(data));
							SetCellData(newCell, data);

							AddCellToContent(newCell, true);
						}
					}
				}

				startIndex = newStartIndex;
				updateFlag = true;
			}

			if (endIndex != newEndIndex)
			{
				if (endIndex < newEndIndex)
				{
					for (int i = endIndex; i < newEndIndex; ++i)
					{
						T data = GetData(i);
						if (data != null)
						{
							GameObject newCell = GetFreeCell(GetCellType(data));
							SetCellData(newCell, data);

							AddCellToContent(newCell, false);

						}
					}
				}
				else if (endIndex > newEndIndex)
				{
					for (int i = endIndex; i > newEndIndex; --i)
					{
						Transform child = null;
						if (content.childCount > 0)
							child = content.GetChild(content.childCount - 1);

						if (child)
						{
							var baseSlot = child.GetComponent<BaseSlot>();
							int cellType = baseSlot != null ? baseSlot.GetCellType() : 0;

							child.SetParent(null);
							AddFreeCell(cellType, child.gameObject);
						}
					}
				}

				endIndex = newEndIndex;
				updateFlag = true;
			}
		}

		if (updateFlag == true)
		{
			CheckChildCount();
			UpdateCellPositions();
		}
	}

	void CheckChildCount()
	{
		var count = endIndex - startIndex;
		var childCount = content.childCount;
		if (childCount != count)
		{
			Debug.Log("Count not match!!!!");
		}
	}

	/// <summary>
	/// 현재 화면 영역에맞는 시작/끝 index계산.
	/// </summary>
	/// <returns></returns>
	Vector2 GetStartEndIndex()
	{
		Vector2 newIndex = Vector2.zero;

		Vector3[] viewCorners = new Vector3[4];

		//월드 좌표로 영역 계산 하면 월드 스케일에 의해 셀영역 계산에 오류 발생.
		viewRect.GetLocalCorners(viewCorners);
		Bounds viewBounds = InternalGetBounds(viewCorners);

		content.GetLocalCorners(viewCorners);
		GetCornerPosition(content, viewCorners);
		Bounds contentBounds = InternalGetBounds(viewCorners);

		int startRow = 0;
		int endRow = 0;
		switch (cellAlignment)
		{
			case eCellAlignment.LeftUp_ToRight:
			case eCellAlignment.RightUp_ToLeft:
				{
					float diff = Mathf.Max(0, contentBounds.max.y - viewBounds.max.y);
					diff -= padding.z;
					startRow = (int)(diff / (cellSize.y + spacing.y));
					endRow = (int)((diff + viewRect.rect.height) / (cellSize.y + spacing.y));
				}
				break;
			case eCellAlignment.LeftDown_ToRight:
			case eCellAlignment.RightDown_ToLeft:
				{
					float diff = Mathf.Max(0, viewBounds.min.y - contentBounds.min.y);
					startRow = (int)(diff / cellSize.y);
					endRow = (int)((diff + viewRect.rect.height) / cellSize.y);
				}
				break;
			case eCellAlignment.LeftUp_ToDown:
			case eCellAlignment.LeftDown_ToUp:
				{
					float diff = Mathf.Max(0, viewBounds.min.x - contentBounds.min.x);
					startRow = (int)(diff / cellSize.x);
					endRow = (int)((diff + viewRect.rect.width) / cellSize.x);
				}
				break;
			case eCellAlignment.RightUp_ToDown:
			case eCellAlignment.RightDown_ToUp:
				{
					float diff = Mathf.Max(0, contentBounds.max.x - viewBounds.max.x);
					startRow = (int)(diff / cellSize.x);
					endRow = (int)((diff + viewRect.rect.width) / cellSize.x);
				}
				break;
		}

		newIndex.x = startRow * rowItemCount;
		int endIndex = endRow * rowItemCount + rowItemCount;

		newIndex.y = Mathf.Min(endIndex, totalDataList.Count);

		return newIndex;
	}

	/// <summary>
	/// Cell좌표 계산.
	/// </summary>
	private void UpdateCellPositions()
	{
		int childCount = content.childCount;
		var count = endIndex - startIndex;
		if (childCount != count)
		{

		}
		Vector2 cellPos = Vector2.zero;

		Vector2 offset = GetCellStartPos(cellAlignment, content.rect.size);

		Vector2 tempCellSize = cellSize + spacing;
		for (int i = 0; i < childCount; ++i)
		{
			Transform child = content.GetChild(i);

			int row = (startIndex + i) / rowItemCount;
			int col = (startIndex + i) % rowItemCount;

			switch (cellAlignment)
			{
				case eCellAlignment.LeftUp_ToRight:
				case eCellAlignment.LeftDown_ToRight:
					cellPos.x = col * tempCellSize.x;
					cellPos.y = row * (cellAlignment == eCellAlignment.LeftUp_ToRight ? -tempCellSize.y : tempCellSize.y);
					break;
				case eCellAlignment.RightUp_ToLeft:
				case eCellAlignment.RightDown_ToLeft:
					cellPos.x = col * -tempCellSize.x;
					cellPos.y = row * (cellAlignment == eCellAlignment.RightUp_ToLeft ? -tempCellSize.y : tempCellSize.y);
					break;
				case eCellAlignment.LeftUp_ToDown:
				case eCellAlignment.RightUp_ToDown:
					cellPos.y = col * -tempCellSize.y;
					cellPos.x = row * (cellAlignment == eCellAlignment.LeftUp_ToDown ? tempCellSize.x : -tempCellSize.x);
					break;
				case eCellAlignment.LeftDown_ToUp:
				case eCellAlignment.RightDown_ToUp:
					cellPos.y = col * tempCellSize.y;
					cellPos.x = row * (cellAlignment == eCellAlignment.LeftDown_ToUp ? tempCellSize.x : -tempCellSize.x);
					break;
			}

			cellPos += GetCellOffset(cellAlignment, cellRect);
			child.transform.localPosition = offset + cellPos;

			child.name = $"Slot_{startIndex + i}";

		}
	}
	#endregion
}