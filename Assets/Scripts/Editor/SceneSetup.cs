using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CGJ2026
{
    public static class SceneSetup
    {
        const string ConfigAssetPath = "Assets/Resources/PlayerConfig.asset";
        const string PhysicsMaterialPath = "Assets/Resources/PlayerPhysics.asset";
        const string ScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("CGJ2026/Setup Game Scene")]
        public static void SetupGameScene()
        {
            EnsureLayer("Ground");
            int groundLayer = LayerMask.NameToLayer("Ground");

            var config = CreatePlayerConfig();
            var playerPhysics = CreateZeroFrictionMaterial();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Game";

            // ── Camera (2D orthographic) ────────────────────
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.1f, 0.12f, 0.18f);
            cam.transform.position = new Vector3(0, 1, -10);

            // ── Ground ───────────────────────────────────────
            var ground = CreateGround("Ground", new Vector3(0, -5f, 0), new Vector2(24, 1.5f), groundLayer);

            // ── Platforms (ascending tower) ──────────────────
            CreatePlatform("Plat_L1", new Vector3(-2.5f, -2.5f, 0), new Vector2(5, 0.4f), groundLayer);
            CreatePlatform("Plat_R1", new Vector3(2.5f, -2.5f, 0), new Vector2(5, 0.4f), groundLayer);
            CreatePlatform("Plat_C2", new Vector3(0, 0.5f, 0), new Vector2(5, 0.4f), groundLayer);
            CreatePlatform("Plat_L3", new Vector3(-3.5f, 3f, 0), new Vector2(4, 0.4f), groundLayer);
            CreatePlatform("Plat_R3", new Vector3(3.5f, 3f, 0), new Vector2(4, 0.4f), groundLayer);
            CreatePlatform("Plat_Top", new Vector3(0, 6f, 0), new Vector2(6, 0.4f), groundLayer);

            // ── Players ──────────────────────────────────────
            var p1 = CreatePlayer("Player1", new Vector3(-2, -3.5f, 0), new Color(0.2f, 0.7f, 1f), 0, groundLayer, config, playerPhysics);
            var p2 = CreatePlayer("Player2", new Vector3(2, -3.5f, 0), new Color(1f, 0.3f, 0.7f), 1, groundLayer, config, playerPhysics);

            // ── Rope System ──────────────────────────────────
            var ropeGo = new GameObject("RopeSystem");
            var lr = ropeGo.AddComponent<LineRenderer>();
            lr.startWidth = 0.06f;
            lr.endWidth = 0.06f;
            lr.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
            lr.startColor = new Color(0.8f, 0.65f, 0.2f);
            lr.endColor = new Color(0.8f, 0.65f, 0.2f);
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.sortingOrder = -1;
            var rs = ropeGo.AddComponent<RopeSystem>();
            var rsSo = new SerializedObject(rs);
            rsSo.FindProperty("playerA").objectReferenceValue = p1.GetComponent<PlayerController>();
            rsSo.FindProperty("playerB").objectReferenceValue = p2.GetComponent<PlayerController>();
            rsSo.FindProperty("maxLength").floatValue = config.MaxRopeLength;
            rsSo.ApplyModifiedProperties();

            // ── Persistent Managers ──────────────────────────
            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            var lmGo = new GameObject("LocalizationManager");
            lmGo.AddComponent<LocalizationManager>();

            // ── Save ─────────────────────────────────────────
            EditorSceneManager.SaveScene(scene, ScenePath);

            // Register scene in build settings
            var buildScenes = EditorBuildSettings.scenes;
            bool found = false;
            foreach (var s in buildScenes)
                if (s.path == ScenePath) { found = true; break; }
            if (!found)
            {
                var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
                buildScenes.CopyTo(newScenes, 0);
                newScenes[^1] = new EditorBuildSettingsScene(ScenePath, true);
                EditorBuildSettings.scenes = newScenes;
            }

            EditorSceneManager.OpenScene(ScenePath);
            Selection.activeObject = config;
            Debug.Log("Game scene setup complete. PlayerConfig selected in Inspector.");
        }

        static PlayerConfig CreatePlayerConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<PlayerConfig>(ConfigAssetPath);
            if (config != null) return config;

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            config = ScriptableObject.CreateInstance<PlayerConfig>();
            AssetDatabase.CreateAsset(config, ConfigAssetPath);
            AssetDatabase.SaveAssets();
            return config;
        }

        static PhysicsMaterial2D CreateZeroFrictionMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PhysicsMaterialPath);
            if (mat != null) return mat;

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            mat = new PhysicsMaterial2D("PlayerZeroFriction")
            {
                friction = 0f,
                bounciness = 0f
            };
            AssetDatabase.CreateAsset(mat, PhysicsMaterialPath);
            AssetDatabase.SaveAssets();
            return mat;
        }

        static GameObject CreateGround(string name, Vector3 pos, Vector2 size, int layer)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.layer = layer;
            go.isStatic = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(0.25f, 0.3f, 0.35f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            return go;
        }

        static void CreatePlatform(string name, Vector3 pos, Vector2 size, int layer)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.layer = layer;
            go.isStatic = true;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(0.3f, 0.35f, 0.4f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
        }

        static GameObject CreatePlayer(string name, Vector3 pos, Color color, int index, int groundLayer, PlayerConfig config, PhysicsMaterial2D physicsMaterial)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.layer = 0; // Default layer for players

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = color;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(0.75f, 1f);
            sr.sortingOrder = 1;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.mass = 1f;
            rb.drag = 0.5f;
            rb.sharedMaterial = null;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.75f, 1f);
            col.sharedMaterial = null;

            var pc = go.AddComponent<PlayerController>();
            var pcSo = new SerializedObject(pc);
            pcSo.FindProperty("playerIndex").intValue = index;
            pcSo.FindProperty("config").objectReferenceValue = config;
            pcSo.FindProperty("groundLayer").intValue = 1 << groundLayer;
            pcSo.ApplyModifiedProperties();

            // GroundCheck child
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(go.transform);
            gc.transform.localPosition = new Vector3(0, -0.55f, 0);

            pcSo.FindProperty("groundCheck").objectReferenceValue = gc.transform;
            pcSo.ApplyModifiedProperties();

            return go;
        }

        static void EnsureLayer(string name)
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (layer.stringValue == name) return;
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = name;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }
            Debug.LogWarning($"Could not create layer '{name}' — all slots full.");
        }
    }
}
