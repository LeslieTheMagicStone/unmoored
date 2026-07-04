using UnityEditor;
using UnityEngine;

namespace CGJ2026.Editor
{
	public class ReplaceWithGroundPrefab
	{
		[MenuItem("Tools/Replace Ground & HighPlatform with Prefab")]
		public static void Run()
		{
			string prefabPath = "Assets/Prefabs/Ground.prefab";
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
			if (prefab == null)
			{
				Debug.LogError($"Prefab not found at {prefabPath}");
				return;
			}

			GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
			int replaced = 0;
			int skipped = 0;

			foreach (GameObject go in allObjects)
			{
				string name = go.name;
				if (!name.StartsWith("Ground") && !name.StartsWith("HighPlatform"))
					continue;

				if (PrefabUtility.IsPartOfPrefabInstance(go))
				{
					skipped++;
					Debug.Log($"Skipped (already prefab): {name}", go);
					continue;
				}

				// Capture original transform values
				Transform t = go.transform;
				Vector3 pos = t.localPosition;
				Quaternion rot = t.localRotation;
				Vector3 scale = t.localScale;
				Transform parent = t.parent;
				int siblingIndex = t.GetSiblingIndex();

				// Capture original collider size
				Vector2 colliderSize = Vector2.one;
				BoxCollider2D originalCol = go.GetComponent<BoxCollider2D>();
				if (originalCol != null)
					colliderSize = originalCol.size;

				// Instantiate prefab
				GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
				instance.name = name;

				Transform instT = instance.transform;
				instT.SetParent(parent);
				instT.localPosition = pos;
				instT.localRotation = rot;
				instT.localScale = scale;
				instT.SetSiblingIndex(siblingIndex);

				// Copy collider size
				BoxCollider2D newCol = instance.GetComponent<BoxCollider2D>();
				if (newCol != null)
					newCol.size = colliderSize;

				// Destroy original
				Undo.DestroyObjectImmediate(go);
				Undo.RecordObject(instance, "Replace with Ground Prefab");

				replaced++;
			}

			Debug.Log($"Replace complete: {replaced} replaced, {skipped} skipped (already prefab).");
		}
	}
}
