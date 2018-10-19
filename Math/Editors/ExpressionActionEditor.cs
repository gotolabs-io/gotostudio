using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GOTO.Logic.Actions
{
	
	[CustomActionEditor(typeof(ExpressionAction))]
	public class ExpressionActionEditor : ActionEditor 
	{
		const float padding = 6f;
		const float removeIconWidth = 16f;
		public ExpressionAction action { get { return this.Target as ExpressionAction; } }

		public override void OnInspectorGUI(Rect rect)
		{
			float y = rect.yMin;
			float lineHeight = EditorGUIUtility.singleLineHeight;
			
			for(int i=0; i<action.inputs.Length; i++)
			{
				Rect r = new Rect(rect.xMin, y, rect.width - removeIconWidth, lineHeight);
				SerializedProperty sp = SerializedObject.FindProperty("inputs").GetArrayElementAtIndex(i);
				GUIContent label = new GUIContent(ExpressionAction.alphabet[i]);

				EditorGUI.PropertyField(r, sp, label);
				if(GUI.Button(new Rect(rect.xMax - removeIconWidth + 4f, y, removeIconWidth, lineHeight), removeIcon, GUIStyle.none))
				{
					List<Value> tmp = new List<Value>(action.inputs);
					tmp.RemoveAt(i);
					action.inputs = tmp.ToArray();
				}

				y += lineHeight;
			}
			if(GUI.Button(new Rect(rect.xMin + EditorGUIUtility.labelWidth, y, rect.width - EditorGUIUtility.labelWidth, lineHeight), "Add New"))
			{
				List<Value> tmp = new List<Value>(action.inputs);
				tmp.Add(new Value());
				action.inputs = tmp.ToArray();
			}

			y += lineHeight + padding;
			EditorGUI.PropertyField(new Rect(rect.xMin, y, rect.width, lineHeight), SerializedObject.FindProperty("expression"));
			y += lineHeight;
			EditorGUI.PropertyField(new Rect(rect.xMin, y, rect.width, lineHeight), SerializedObject.FindProperty("output"));
		}

		public override float GetInspectorHeight()
		{
			return EditorGUIUtility.singleLineHeight * (4 + (action.inputs != null ? action.inputs.Length : 0)) + padding;
		} 

		private static Texture2D s_RemoveIcon;
		public static Texture2D removeIcon
		{
			get
			{
				if (s_RemoveIcon == null)
					s_RemoveIcon = Base64ToTexture ("iVBORw0KGgoAAAANSUhEUgAAAAkAAAAMCAYAAACwXJejAAAAT0lEQVQYlWNgGHyAcY2RUR0DA0MjHjX1DGuMjP7jM2WNkdF/JmQOTAMym4GBgQGuKOTcOUaYAmQ+iiJ8AMU6bCYyYHBwOJyFgYGhnoDCegCL2SIhQX4/FwAAAABJRU5ErkJggg==");

				return s_RemoveIcon;
			}
		}

		private static Texture2D Base64ToTexture (string base64)
		{
			Texture2D t = new Texture2D (1, 1);
			t.hideFlags = HideFlags.HideAndDontSave;
			t.LoadImage (System.Convert.FromBase64String (base64));
			return t;
		}

	}
}
