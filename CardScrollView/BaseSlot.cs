using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseSlot : MonoBehaviour
{
	public virtual int GetCellType()
	{
		return 0;
	}
}
