using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;

[InitializeOnLoad]
public static class AutoApplyPlayModeChanges {
	private const string PREV_KEY = "AutoApplyPlayModeChanges_Enabled";
	public static bool Enabled {
		get => EditorPrefs.GetBool(PREV_KEY, true);
		set => EditorPrefs.SetBool(PREV_KEY, value);
	}

  private static readonly Dictionary<int, Dictionary<string, object>> savedValues = new Dictionary<int, Dictionary<string, object>>();
  static AutoApplyPlayModeChanges() {
      EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
  }
  private static void OnPlayModeStateChanged(PlayModeStateChange state) {
    
		if (state == PlayModeStateChange.EnteredPlayMode) {
		  savedValues.Clear();
		} else if (state == PlayModeStateChange.ExitingPlayMode) {
			if (Enabled) SaveModified();
		} else if (state == PlayModeStateChange.EnteredEditMode) {
			if (Enabled) ApplyModified();
		}
  }

	private static void SaveModified() {
		
		savedValues.Clear();
		
		var comps = Object.FindObjectsOfType<Component>(true);
		
		foreach (var comp in comps) {
		
			if (comp == null) continue;

			if (comp is Transform) continue;
			if (comp is MeshFilter) continue;
			if (comp is MeshRenderer) continue;
			if (comp is SkinnedMeshRenderer) continue;
			if (comp is VRCAvatarDescriptor) continue;

			if (PrefabUtility.IsPartOfPrefabAsset(comp)) continue;
			if (PrefabUtility.IsPartOfNonAssetPrefabInstance(comp)) continue;

			SerializedObject so = new SerializedObject(comp);
			SerializedProperty prop = so.GetIterator();

			Dictionary<string, object> values = new();

			if (prop.NextVisible(true)) {
				do {
					if (prop.propertyType == SerializedPropertyType.Generic) continue;

					values[prop.propertyPath] = GetValue(prop);
				} while (prop.NextVisible(false));
			}

			savedValues[comp.GetInstanceID()] = values;
		}
		Debug.Log($"[AutoApply] Saved changes for {savedValues.Count} objects.");
	}

	private static void ApplyModified() {
		var comps = Object.FindObjectsOfType<Component>(true);
		
		foreach (var comp in comps) {
			
			if (comp == null) continue;
			if (comp is Transform) continue;
			
			int id = comp.GetInstanceID();
			if (!savedValues.ContainsKey(id)) continue;
			
			var values = savedValues[id];

			SerializedObject so = new SerializedObject(comp);
			SerializedProperty prop = so.GetIterator();

			if (prop.NextVisible(true)) {
				do {
					if (!values.ContainsKey(prop.propertyPath)) continue;
					SetValue(prop, values[prop.propertyPath]);
				} while (prop.NextVisible(false));
			}
			so.ApplyModifiedProperties();
		}
		Debug.Log($"[AutoApply] Applied changes for {savedValues.Count} objects.");
	}

	private static object GetValue(SerializedProperty prop) {
		return prop.propertyType switch {
			SerializedPropertyType.Integer => prop.intValue,
			SerializedPropertyType.Boolean => prop.boolValue,
			SerializedPropertyType.Float => prop.floatValue,
			SerializedPropertyType.String => prop.stringValue,
			SerializedPropertyType.Color => prop.colorValue,
			SerializedPropertyType.ObjectReference => prop.objectReferenceValue,
			SerializedPropertyType.Enum => prop.enumValueIndex,
			SerializedPropertyType.Vector2 => prop.vector2Value,
			SerializedPropertyType.Vector3 => prop.vector3Value,
			SerializedPropertyType.Vector4 => prop.vector4Value,
			SerializedPropertyType.Vector2Int => prop.vector2IntValue,
			SerializedPropertyType.Vector3Int => prop.vector3IntValue,
			SerializedPropertyType.Rect => prop.rectValue,
			SerializedPropertyType.RectInt => prop.rectIntValue,
			SerializedPropertyType.Bounds => prop.boundsValue,
			SerializedPropertyType.BoundsInt => prop.boundsIntValue,
			SerializedPropertyType.Quaternion => prop.quaternionValue,
			_ => null,
		};
	}

	private static void SetValue(SerializedProperty prop, object value) {
		if (value == null) return;

		switch (prop.propertyType) {
			case SerializedPropertyType.Integer:
				prop.intValue = (int)value;
				break;
			case SerializedPropertyType.Boolean:
				prop.boolValue = (bool)value;
				break;
			case SerializedPropertyType.Float:
				prop.floatValue = (float)value;
				break;
			case SerializedPropertyType.String:
				prop.stringValue = (string)value;
				break;
			case SerializedPropertyType.Color:
				prop.colorValue = (Color)value;
				break;
			case SerializedPropertyType.ObjectReference:
				prop.objectReferenceValue = (Object)value;
				break;
			case SerializedPropertyType.Enum:
				prop.enumValueIndex = (int)value;
				break;
			case SerializedPropertyType.Vector2:
				prop.vector2Value = (Vector2)value;
				break;
			case SerializedPropertyType.Vector3:
				prop.vector3Value = (Vector3)value;
				break;
			case SerializedPropertyType.Vector4:
				prop.vector4Value = (Vector4)value;
				break;
			case SerializedPropertyType.Vector2Int:
				prop.vector2IntValue = (Vector2Int)value;
				break;
			case SerializedPropertyType.Vector3Int:
				prop.vector3IntValue = (Vector3Int)value;
				break;
			case SerializedPropertyType.Rect:
				prop.rectValue = (Rect)value;
				break;
			case SerializedPropertyType.RectInt:
				prop.rectIntValue = (RectInt)value;
				break;
			case SerializedPropertyType.Bounds:
				prop.boundsValue = (Bounds)value;
				break;
			case SerializedPropertyType.BoundsInt:
				prop.boundsIntValue = (BoundsInt)value;
				break;
			case SerializedPropertyType.Quaternion:
				prop.quaternionValue = (Quaternion)value;
				break;
		}
	}
}

public class AutoSaveSettingsWindow : EditorWindow {
	[MenuItem("Tools/n1lsqn/AutoSave Settings")]
	public static void ShowWindow() {
		GetWindow<AutoSaveSettingsWindow>("AutoSave Settings");
	}

	private void OnGUI() {
		GUILayout.Space(10);
		GUILayout.Label("Play Mode Auto-Save", EditorStyles.boldLabel);

		bool enabled = AutoApplyPlayModeChanges.Enabled;
		bool newEnabled = EditorGUILayout.Toggle("Enabled Auto-Save", enabled);

		if (newEnabled != enabled) {
			AutoApplyPlayModeChanges.Enabled = newEnabled;
		}

		GUILayout.Space(10);
		EditorGUILayout.HelpBox(
			"Playモードで編集したPhysBoneなどの値を自動保存/復元します。\nToggleをOFFにすると自動保存が無効になります。",
			MessageType.Info
		);
	}
}