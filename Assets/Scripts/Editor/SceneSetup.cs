using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CGJ2026
{
    public static class SceneSetup
    {
        const string ConfigAssetPath = "Assets/Resources/PlayerConfig.asset";
        const string PhysicsMaterialPath = "Assets/Resources/PlayerPhysics.asset";
        const string GameScenePath = "Assets/Scenes/Game.unity";
        const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        const string GameOverScenePath = "Assets/Scenes/GameOver.unity";

        // ═══════════════════════════════════════════════════════════
        //  Game Scene (players, rope, terrain)
        // ═══════════════════════════════════════════════════════════

        [MenuItem("CGJ2026/Setup Game Scene")]
        public static void SetupGameScene()
        {
            EnsureLayer("Ground");
            int groundLayer = LayerMask.NameToLayer("Ground");

            var config = CreatePlayerConfig();
            var playerPhysics = CreateZeroFrictionMaterial();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Game";

            // ── Camera ────────────────────────────────────────
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.1f, 0.12f, 0.18f);
            cam.transform.position = new Vector3(0, 1, -10);

            // ── Ground ────────────────────────────────────────
            CreateGround("Ground", new Vector3(0, -5f, 0), new Vector2(24, 1.5f), groundLayer);

            // ── Platforms ─────────────────────────────────────
            CreatePlatform("Plat_L1", new Vector3(-2.5f, -2.5f, 0), new Vector2(5, 0.4f), groundLayer);
            CreatePlatform("Plat_R1", new Vector3(2.5f, -2.5f, 0), new Vector2(5, 0.4f), groundLayer);
            CreatePlatform("Plat_C2", new Vector3(0, 0.5f, 0), new Vector2(5, 0.4f), groundLayer);
            CreatePlatform("Plat_L3", new Vector3(-3.5f, 3f, 0), new Vector2(4, 0.4f), groundLayer);
            CreatePlatform("Plat_R3", new Vector3(3.5f, 3f, 0), new Vector2(4, 0.4f), groundLayer);
            CreatePlatform("Plat_Top", new Vector3(0, 6f, 0), new Vector2(6, 0.4f), groundLayer);

            // ── Players ───────────────────────────────────────
            var p1 = CreatePlayer("Player1", new Vector3(-2, -3.5f, 0), new Color(0.2f, 0.7f, 1f), 0, groundLayer, config, playerPhysics);
            var p2 = CreatePlayer("Player2", new Vector3(2, -3.5f, 0), new Color(1f, 0.3f, 0.7f), 1, groundLayer, config, playerPhysics);

            // ── Rope System ───────────────────────────────────
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

            // ── Level Manager ─────────────────────────────────
            var lmGo = new GameObject("LevelManager");
            var lm = lmGo.AddComponent<LevelManager>();
            var lmSo = new SerializedObject(lm);
            lmSo.FindProperty("playerA").objectReferenceValue = p1.transform;
            lmSo.FindProperty("playerB").objectReferenceValue = p2.transform;
            lmSo.FindProperty("config").objectReferenceValue = config;
            lmSo.ApplyModifiedProperties();

            // ── Checkpoint (spawn area) ───────────────────────
            CreateCheckpoint("Checkpoint_Start", new Vector3(0, -3.5f, 0), new Vector2(4, 2f));

            // ── Win Zone ──────────────────────────────────────
            var winZone = CreateTriggerZone("WinZone",
                new Vector3(0, 7.5f, 0), new Vector2(8, 0.5f));
            winZone.AddComponent<WinZone>();

            // ── Death Zone ────────────────────────────────────
            var deathZone = CreateTriggerZone("DeathZone",
                new Vector3(0, -8f, 0), new Vector2(30, 1f));
            deathZone.AddComponent<DeathZone>();

            // ── Test Enemy ────────────────────────────────────
            CreateEnemy("Enemy_Test", new Vector3(0, -1.5f, 0), groundLayer);

            // ── Persistent Managers ───────────────────────────
            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            var locGo = new GameObject("LocalizationManager");
            locGo.AddComponent<LocalizationManager>();

            // ── Save ──────────────────────────────────────────
            EditorSceneManager.SaveScene(scene, GameScenePath);
            RegisterScene(GameScenePath);
            EditorSceneManager.OpenScene(GameScenePath);
            Selection.activeObject = config;
            Debug.Log("Game scene setup complete.");
        }

        // ═══════════════════════════════════════════════════════════
        //  Level Flow (WinZone, DeathZone, LevelManager)
        //  Run this AFTER Setup Game Scene, on the open Game scene.
        // ═══════════════════════════════════════════════════════════

        [MenuItem("CGJ2026/Add Level Flow To Scene")]
        public static void AddLevelFlowToScene()
        {
            var p1 = GameObject.Find("Player1");
            var p2 = GameObject.Find("Player2");
            var topPlat = GameObject.Find("Plat_Top");

            if (p1 == null || p2 == null)
            {
                Debug.LogError("Players not found. Open the Game scene first.");
                return;
            }

            // ── Win Zone ──────────────────────────────────────
            float winY = topPlat != null ? topPlat.transform.position.y + 1.5f : 7.5f;
            var winZone = CreateTriggerZone("WinZone",
                new Vector3(0, winY, 0), new Vector2(8, 0.5f));
            winZone.AddComponent<WinZone>();

            // ── Death Zone ────────────────────────────────────
            var deathZone = CreateTriggerZone("DeathZone",
                new Vector3(0, -8f, 0), new Vector2(30, 1f));
            deathZone.AddComponent<DeathZone>();

            // ── Level Manager ─────────────────────────────────
            var lmGo = new GameObject("LevelManager");
            var lm = lmGo.AddComponent<LevelManager>();
            var config = AssetDatabase.LoadAssetAtPath<PlayerConfig>(ConfigAssetPath);
            var lmSo = new SerializedObject(lm);
            lmSo.FindProperty("playerA").objectReferenceValue = p1.transform;
            lmSo.FindProperty("playerB").objectReferenceValue = p2.transform;
            lmSo.FindProperty("winZone").objectReferenceValue = winZone.transform;
            lmSo.FindProperty("config").objectReferenceValue = config;
            lmSo.ApplyModifiedProperties();

            // ── Checkpoint ────────────────────────────────────
            CreateCheckpoint("Checkpoint_Start", new Vector3(0, -3.5f, 0), new Vector2(4, 2f));

            // ── Test Enemy ────────────────────────────────────
            int groundLayer = LayerMask.NameToLayer("Ground");
            CreateEnemy("Enemy_Test", new Vector3(0, -1.5f, 0), groundLayer);

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("Level flow added: LevelManager, WinZone, DeathZone, Checkpoint, Enemy.");
        }

        // ═══════════════════════════════════════════════════════════
        //  MainMenu Scene
        // ═══════════════════════════════════════════════════════════

        [MenuItem("CGJ2026/Setup MainMenu Scene")]
        public static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.1f, 0.12f, 0.18f);
            cam.transform.position = new Vector3(0, 0, -10);

            var uiGo = new GameObject("MainMenuUI");
            var ui = uiGo.AddComponent<MainMenuUI>();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            CreateUIText("Title", canvasGo.transform, "CGJ2026", 48, new Vector2(0, 120));
            CreateUIButton("PlayButton", canvasGo.transform, "PLAY", new Vector2(0, 20), ui, "OnPlayClicked");
            CreateUIButton("QuitButton", canvasGo.transform, "QUIT", new Vector2(0, -60), ui, "OnQuitClicked");

            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            RegisterScene(MainMenuScenePath);
            Debug.Log("MainMenu scene setup complete.");
        }

        // ═══════════════════════════════════════════════════════════
        //  GameOver Scene
        // ═══════════════════════════════════════════════════════════

        [MenuItem("CGJ2026/Setup GameOver Scene")]
        public static void SetupGameOverScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "GameOver";

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.1f, 0.12f, 0.18f);
            cam.transform.position = new Vector3(0, 0, -10);

            var uiGo = new GameObject("GameOverUI");
            var ui = uiGo.AddComponent<GameOverUI>();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            CreateUIText("Title", canvasGo.transform, "LEVEL COMPLETE", 42, new Vector2(0, 100));
            CreateUIButton("RetryButton", canvasGo.transform, "RETRY", new Vector2(0, 0), ui, "OnRetryClicked");
            CreateUIButton("MenuButton", canvasGo.transform, "MAIN MENU", new Vector2(0, -80), ui, "OnMainMenuClicked");

            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            EditorSceneManager.SaveScene(scene, GameOverScenePath);
            RegisterScene(GameOverScenePath);
            Debug.Log("GameOver scene setup complete.");
        }

        // ═══════════════════════════════════════════════════════════
        //  Helpers
        // ═══════════════════════════════════════════════════════════

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

            mat = new PhysicsMaterial2D("PlayerZeroFriction") { friction = 0f, bounciness = 0f };
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

        static GameObject CreatePlatform(string name, Vector3 pos, Vector2 size, int layer)
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
            return go;
        }

        static GameObject CreatePlayer(string name, Vector3 pos, Color color, int index, int groundLayer, PlayerConfig config, PhysicsMaterial2D physicsMaterial)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.layer = 0;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = color;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(0.75f, 1f);
            sr.sortingOrder = 1;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = config.GravityScale;
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

            if (index == 0)
            {
                pcSo.FindProperty("moveLeftKey").intValue = (int)KeyCode.A;
                pcSo.FindProperty("moveRightKey").intValue = (int)KeyCode.D;
                pcSo.FindProperty("jumpKey").intValue = (int)KeyCode.W;
                pcSo.FindProperty("anchorKey").intValue = (int)KeyCode.E;
            }
            else
            {
                pcSo.FindProperty("moveLeftKey").intValue = (int)KeyCode.LeftArrow;
                pcSo.FindProperty("moveRightKey").intValue = (int)KeyCode.RightArrow;
                pcSo.FindProperty("jumpKey").intValue = (int)KeyCode.UpArrow;
                pcSo.FindProperty("anchorKey").intValue = (int)KeyCode.RightShift;
            }
            pcSo.ApplyModifiedProperties();

            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(go.transform);
            gc.transform.localPosition = new Vector3(0, -0.55f, 0);

            pcSo.FindProperty("groundCheck").objectReferenceValue = gc.transform;
            pcSo.ApplyModifiedProperties();

            return go;
        }

        static GameObject CreateTriggerZone(string name, Vector3 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.position = pos;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(0f, 1f, 0f, 0.3f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = size;
            return go;
        }

        static GameObject CreateUIText(string name, Transform parent, string text, int fontSize, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 80);
            rt.anchoredPosition = anchoredPos;

            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            return go;
        }

        static GameObject CreateUIButton(string name, Transform parent, string label, Vector2 anchoredPos, MonoBehaviour target, string methodName)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(200, 50);
            rt.anchoredPosition = anchoredPos;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.25f, 0.3f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => target.SendMessage(methodName));

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;
            var t = textGo.AddComponent<Text>();
            t.text = label;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 24;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;

            return go;
        }

        static void RegisterScene(string path)
        {
            var buildScenes = EditorBuildSettings.scenes;
            foreach (var s in buildScenes)
                if (s.path == path) return;

            var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
            buildScenes.CopyTo(newScenes, 0);
            newScenes[^1] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = newScenes;
        }

        static GameObject CreateCheckpoint(string name, Vector3 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.position = pos;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;

            go.AddComponent<Checkpoint>();
            return go;
        }

        static GameObject CreateEnemy(string name, Vector3 pos, int groundLayer)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.layer = 0;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            sr.color = new Color(0.9f, 0.2f, 0.2f);
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(0.8f, 0.8f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.drag = 2f;
            rb.mass = 2f;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.8f);

            go.AddComponent<Enemy>();
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
