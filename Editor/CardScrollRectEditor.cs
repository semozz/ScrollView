using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardScrollRect))]
public class CardScrollRectEditor : Editor
{
	CardScrollRect scrollRect = null;

	private void OnEnable()
	{
		scrollRect = (CardScrollRect)target;
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();

		base.OnInspectorGUI();


	}
}