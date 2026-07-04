#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CGJ2026.Editor
{
    public static class RopeLengthGuideMenu
    {
        [MenuItem("CGJ2026/Add Rope Length Guide")]
        static void AddGuide()
        {
            var config = Resources.Load<PlayerConfig>("PlayerConfig");
            float length = config != null ? config.MaxRopeLength : 5f;

            var go = new GameObject("RopeLengthGuide");
            var guide = go.AddComponent<RopeLengthGuide>();

            if (Selection.activeTransform != null)
            {
                go.transform.SetParent(Selection.activeTransform, false);
                go.transform.localPosition = Vector3.zero;
            }

            if (config != null)
            {
                var so = new SerializedObject(guide);
                so.FindProperty("config").objectReferenceValue = config;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            Undo.RegisterCreatedObjectUndo(go, "Add Rope Length Guide");
            Selection.activeGameObject = go;

            Debug.Log($"RopeLengthGuide added. Max rope length = {length} world units.");
        }
    }
}
#endif
