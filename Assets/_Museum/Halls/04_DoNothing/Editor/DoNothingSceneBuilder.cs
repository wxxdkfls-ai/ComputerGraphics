using System.Collections.Generic;
using MuseumOfLies.Common;
using MuseumOfLies.DoNothing;
using MuseumOfLies.EditorTools;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace MuseumOfLies.DoNothing.EditorTools
{
    /// <summary>
    /// Menu: Museum ▸ Build Scene ▸ 04 DoNothing
    /// Rebuilds Assets/_Museum/Halls/04_DoNothing/DoNothing.unity from scratch.
    /// Room: 6 m wide × 5 m deep × 3.2 m high. Player stands at origin facing +Z.
    /// </summary>
    public static class DoNothingSceneBuilder
    {
        const string HallDir = "Assets/_Museum/Halls/04_DoNothing/";
        const string ScenePath = HallDir + "DoNothing.unity";
        const string MatDir = "Assets/_Museum/Common/Materials/";
        const string XROriginPrefab = "Assets/Samples/XR Interaction Toolkit/3.7.0/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

        const float RoomW = 8f, RoomH = 3.8f, FrontZ = 2.6f, BackZ = -3.4f;
        const float Tile = 0.5f;
        static readonly Vector2 SlabMin = new Vector2(-0.5f, -0.4f), SlabMax = new Vector2(0.5f, 1.6f); // x, z — player + button
        static readonly Vector3 DrainPos = new Vector3(0.55f, 0f, 1.35f);

        static Material M(string n) => AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_" + n + ".mat");
        static GameObject UIPrefab(string n) => AssetDatabase.LoadAssetAtPath<GameObject>(MuseumUIBuilder.Dir + n + ".prefab");

        [MenuItem("Museum/Build Scene/04 DoNothing")]
        public static void Build()
        {
            if (UIPrefab("UI_Timer") == null) MuseumUIBuilder.BuildAll();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── lighting: luminous ceiling, cool & even (shared gallery recipe)
            MuseumEnvironmentKit.ApplyGalleryLighting();

            // ── room shell: raw concrete gallery (see DESIGN_GUIDELINES › Space & Environment)
            var room = new GameObject("Room").transform;
            float depth = FrontZ - BackZ, cz = (FrontZ + BackZ) / 2f;
            var concrete = MuseumEnvironmentKit.Mat("Concrete");
            float ct = MuseumEnvironmentKit.ConcreteTile;
            // Floor = a fixed slab the player stands on (with the button) + 0.5 m tiles that can collapse.
            var floorMat = MuseumEnvironmentKit.Mat("FloorPolished");
            MuseumEnvironmentKit.WorldBox("StandSlab", room, new Vector3((SlabMin.x + SlabMax.x) / 2, -0.15f, (SlabMin.y + SlabMax.y) / 2),
                new Vector3(SlabMax.x - SlabMin.x, 0.3f, SlabMax.y - SlabMin.y), floorMat, MuseumEnvironmentKit.FloorTile);
            var tilesRoot = new GameObject("FloorTiles").transform;
            tilesRoot.SetParent(room, false);
            for (float x = -RoomW / 2; x < RoomW / 2 - 0.01f; x += Tile)
            for (float z = BackZ; z < FrontZ - 0.01f; z += Tile)
            {
                float mx = x + Tile / 2, mz = z + Tile / 2;
                if (mx > SlabMin.x && mx < SlabMax.x && mz > SlabMin.y && mz < SlabMax.y) continue;
                var tile = MuseumEnvironmentKit.WorldBox($"Tile_{mx:0.00}_{mz:0.00}", tilesRoot, new Vector3(mx, -0.05f, mz), new Vector3(Tile, 0.1f, Tile), floorMat, MuseumEnvironmentKit.FloorTile, false, false);
                if (Mathf.Abs(mx - DrainPos.x) <= Tile / 2 && Mathf.Abs(mz - DrainPos.z) <= Tile / 2)
                {
                    var grate = Box("Drain", tile.transform, new Vector3(DrainPos.x - mx, 0.052f, DrainPos.z - mz), new Vector3(0.12f, 0.004f, 0.12f), "Charcoal", false);
                    GameObjectUtility.SetStaticEditorFlags(grate, 0);
                }
            }
            // the shaft below (revealed when the floor collapses)
            var pitMat = MuseumEnvironmentKit.Mat("ConcreteDark");
            float pitH = 24f;
            MuseumEnvironmentKit.WorldBox("Pit_Front", room, new Vector3(0, -pitH / 2, FrontZ + 0.1f), new Vector3(RoomW + 0.4f, pitH, 0.2f), pitMat, MuseumEnvironmentKit.ConcreteTile);
            MuseumEnvironmentKit.WorldBox("Pit_Back", room, new Vector3(0, -pitH / 2, BackZ - 0.1f), new Vector3(RoomW + 0.4f, pitH, 0.2f), pitMat, MuseumEnvironmentKit.ConcreteTile);
            MuseumEnvironmentKit.WorldBox("Pit_Left", room, new Vector3(-RoomW / 2 - 0.1f, -pitH / 2, cz), new Vector3(0.2f, pitH, depth), pitMat, MuseumEnvironmentKit.ConcreteTile);
            MuseumEnvironmentKit.WorldBox("Pit_Right", room, new Vector3(RoomW / 2 + 0.1f, -pitH / 2, cz), new Vector3(0.2f, pitH, depth), pitMat, MuseumEnvironmentKit.ConcreteTile);
            MuseumEnvironmentKit.WorldBox("Pit_Bottom", room, new Vector3(0, -pitH - 0.1f, cz), new Vector3(RoomW, 0.2f, depth), pitMat, MuseumEnvironmentKit.ConcreteTile);
            // slab pillar going down so the standing area reads as a column over the void
            MuseumEnvironmentKit.WorldBox("Slab_Pillar", room, new Vector3((SlabMin.x + SlabMax.x) / 2, -pitH / 2 - 0.3f, (SlabMin.y + SlabMax.y) / 2),
                new Vector3(SlabMax.x - SlabMin.x - 0.1f, pitH - 0.6f, SlabMax.y - SlabMin.y - 0.1f), pitMat, MuseumEnvironmentKit.ConcreteTile);
            // depth darkness: only affects things farther than the room (> 8 m), i.e. the shaft
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.07f, 0.075f, 0.08f);
            RenderSettings.fogStartDistance = 8f;
            RenderSettings.fogEndDistance = 24f;
            MuseumEnvironmentKit.WorldBox("Ceiling", room, new Vector3(0, RoomH + 0.05f, cz), new Vector3(RoomW, 0.1f, depth), MuseumEnvironmentKit.Mat("CeilingLight"), MuseumEnvironmentKit.CeilingTile);
            MuseumEnvironmentKit.WorldBox("Wall_Front", room, new Vector3(0, RoomH / 2, FrontZ + 0.1f), new Vector3(RoomW + 0.4f, RoomH, 0.2f), concrete, ct);
            MuseumEnvironmentKit.WorldBox("Wall_Back", room, new Vector3(0, RoomH / 2, BackZ - 0.1f), new Vector3(RoomW + 0.4f, RoomH, 0.2f), concrete, ct);
            MuseumEnvironmentKit.WorldBox("Wall_Left", room, new Vector3(-RoomW / 2 - 0.1f, RoomH / 2, cz), new Vector3(0.2f, RoomH, depth), concrete, ct);
            MuseumEnvironmentKit.WorldBox("Wall_Right", room, new Vector3(RoomW / 2 + 0.1f, RoomH / 2, cz), new Vector3(0.2f, RoomH, depth), concrete, ct);
            // a free-standing concrete column for depth (peripheral, not a temptation)
            MuseumEnvironmentKit.WorldBox("Column", room, new Vector3(RoomW / 2 - 1.1f, (RoomH - 24f) / 2, 1.2f), new Vector3(0.55f, RoomH + 24f, 0.55f), concrete, ct, true); // continues down into the shaft
            // entrance door behind the player — the knock comes from here
            MuseumEnvironmentKit.Door("EntranceDoor", room, new Vector3(0.3f, 0f, BackZ), 0f);
            // stand marker
            Box("StandMarker", room, new Vector3(0, 0.002f, 0), new Vector3(0.7f, 0.004f, 0.7f), "Charcoal", false);
            Box("StandMarker_Inner", room, new Vector3(0, 0.003f, 0), new Vector3(0.64f, 0.004f, 0.64f), "LightGray", false);
            // Ivory exhibition placard behind the timer (UI stays on the base palette)
            Box("TimerPlacard", room, new Vector3(0, 1.95f, FrontZ - 0.012f), new Vector3(1.7f, 0.74f, 0.02f), "Ivory", false);
            Box("TimerPlacard_Accent", room, new Vector3(-0.85f + 0.006f, 1.95f, FrontZ - 0.024f), new Vector3(0.012f, 0.74f, 0.004f), "DoNothing", false);
            // hall label on the concrete (Black for contrast)
            var hallLabel = WorldText("HallLabel", room, new Vector3(-2.6f, 3.05f, FrontZ - 0.01f), 1100, 120,
                MuseumUIBuilder.EnBold, 72, MuseumPalette.Black, TextAlignmentOptions.TopLeft, "04  DO NOTHING");
            hallLabel.characterSpacing = 6;
            var hallSub = WorldText("HallSub", room, new Vector3(-2.6f, 2.88f, FrontZ - 0.01f), 1100, 80,
                MuseumUIBuilder.KrRegular, 40, MuseumPalette.Black, TextAlignmentOptions.TopLeft, "의지 — 인간은 호기심과 행동 충동을 얼마나 억제할 수 있을까?");
            Box("HallLabel_Line", room, new Vector3(-2.85f, 2.78f, FrontZ - 0.01f), new Vector3(0.5f, 0.012f, 0.005f), "DoNothing", false);

            // ── XR rig
            var origin = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(XROriginPrefab));
            origin.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(XRUIInputModule));

            var systems = new GameObject("Systems");
            var rig = systems.AddComponent<XRRig>();
            rig.origin = origin.transform;
            rig.Resolve();
            var uiAudio = systems.AddComponent<AudioSource>();
            uiAudio.playOnAwake = false; uiAudio.spatialBlend = 0f; uiAudio.volume = 0.8f;

            // ── UI
            var ui = new GameObject("UI").transform;
            var timer = Place<TimerDisplay>("UI_Timer", ui, new Vector3(0, 1.95f, FrontZ - 0.03f), Quaternion.identity);
            var instr = Place<InstructionPanel>("UI_InstructionPanel", ui, new Vector3(0, 1.4f, 1.6f), Quaternion.identity);
            var result = Place<ResultPanel>("UI_ResultPanel", ui, new Vector3(0, 1.4f, 1.6f), Quaternion.identity);
            result.gameObject.SetActive(false);
            var exitHold = Place<Transform>("UI_ExitHold", ui, new Vector3(-0.72f, 0.95f, 1.25f), Quaternion.Euler(25f, -30f, 0f));
            var exitBar = exitHold.GetComponentInChildren<MuseumProgressBar>();

            // ── temptations & stimuli
            var props = new GameObject("Temptations").transform;
            var stimuli = new List<DoNothingStimulus>();
            var temptations = new List<Temptation>();

            // 1) Red button rises in front (15 s) — the classic "do not press"
            var button = new GameObject("RedButton").transform;
            button.SetParent(props, false);
            var pedestal = Prim(PrimitiveType.Cylinder, "Pedestal", button, new Vector3(0, 0.5f, 0), new Vector3(0.34f, 0.5f, 0.34f), "Charcoal");
            var cap = Prim(PrimitiveType.Cylinder, "ButtonCap", button, new Vector3(0, 1.025f, 0), new Vector3(0.2f, 0.03f, 0.2f), "DoNothing");
            temptations.Add(MakeTemptation(cap, "red_button", "빨간 버튼"));
            var sign = WorldText("Sign", button, new Vector3(0, 0.72f, -0.18f), 300, 160, MuseumUIBuilder.KrBold, 34,
                MuseumPalette.Ivory, TextAlignmentOptions.Center, "누르지 마시오\n<size=22>DO NOT PRESS</size>");
            var buttonAudio = Audio(button.gameObject, 1f);
            var riseHolder = new GameObject("RedButtonStimulus"); riseHolder.transform.SetParent(props, false);
            var sRise = Stim(riseHolder, "red_button", "빨간 버튼", DoNothingStimulus.Kind.Rise, 15f, true);
            sRise.target = button; sRise.shownLocalPos = new Vector3(0, 0, 0.85f); sRise.hiddenLocalPos = new Vector3(0, -1.1f, 0.85f);
            sRise.riseDuration = 2.5f; sRise.audioSource = buttonAudio; sRise.sfx = DoNothingStimulus.Sfx.Rise;
            button.localPosition = sRise.hiddenLocalPos;
            stimuli.Add(sRise);

            // 2) Coin drops to the front-right floor (40 s)
            var coin = Prim(PrimitiveType.Cylinder, "Coin", props, new Vector3(0.85f, 2.4f, 0.95f), new Vector3(0.045f, 0.0015f, 0.045f), null);
            coin.GetComponent<Renderer>().sharedMaterial = CoinMaterial();
            Object.DestroyImmediate(coin.GetComponent<Collider>());
            var mc = coin.AddComponent<MeshCollider>(); mc.convex = true;
            var crb = coin.AddComponent<Rigidbody>(); crb.mass = 0.01f; crb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; crb.isKinematic = true;
            coin.transform.localRotation = Quaternion.Euler(70f, 0f, 15f);
            temptations.Add(MakeTemptation(coin, "coin", "떨어진 동전"));
            var coinHolder = new GameObject("CoinStimulus"); coinHolder.transform.SetParent(props, false);
            var sCoin = Stim(coinHolder, "coin", "떨어진 동전", DoNothingStimulus.Kind.Drop, 35f, false);
            sCoin.target = coin.transform; sCoin.shownLocalPos = coin.transform.localPosition;
            sCoin.audioSource = Audio(coin, 1f); sCoin.sfx = DoNothingStimulus.Sfx.Clink; sCoin.playOnCollision = true;
            coin.AddComponent<StimulusCollisionRelay>().owner = sCoin;
            stimuli.Add(sCoin);

            // 3) Knock from behind (70 s) — sound only, tempts a head turn
            var knock = new GameObject("KnockBehind"); knock.transform.SetParent(props, false);
            knock.transform.localPosition = new Vector3(0.3f, 1.4f, BackZ + 0.1f); // at the entrance door
            var sKnock = Stim(knock, "knock", "뒤에서 들린 노크", DoNothingStimulus.Kind.SoundOnly, 60f, false);
            sKnock.phase = DoNothingStimulus.Phase.Social;
            sKnock.audioSource = Audio(knock, 1f); sKnock.sfx = DoNothingStimulus.Sfx.Knock;
            stimuli.Add(sKnock);

            // 4) Ball rolls in from the left periphery and stops near the feet (100 s)
            var ball = Prim(PrimitiveType.Sphere, "Ball", props, new Vector3(-RoomW / 2 + 0.2f, 0.11f, 1.0f), Vector3.one * 0.22f, "DoNothing");
            var brb = ball.AddComponent<Rigidbody>(); brb.mass = 0.3f; brb.linearDamping = 0.6f; brb.angularDamping = 0.3f; brb.isKinematic = true;
            temptations.Add(MakeTemptation(ball, "ball", "굴러온 공"));
            var ballHolder = new GameObject("BallStimulus"); ballHolder.transform.SetParent(props, false);
            var sBall = Stim(ballHolder, "ball", "굴러온 공", DoNothingStimulus.Kind.Roll, 48f, false);
            sBall.target = ball.transform; sBall.shownLocalPos = ball.transform.localPosition; sBall.launchVelocity = new Vector3(2.0f, 0f, -0.5f);
            sBall.audioSource = Audio(ball, 0.7f); sBall.sfx = DoNothingStimulus.Sfx.Roll;
            stimuli.Add(sBall);

            // 5) The museum lies: the sign changes (130 s)
            var lieHolder = new GameObject("LieSignStimulus"); lieHolder.transform.SetParent(props, false);
            var sLie = Stim(lieHolder, "lie_sign", "바뀐 안내문", DoNothingStimulus.Kind.ChangeLabel, 162f, false);
            sLie.phase = DoNothingStimulus.Phase.BodyThreat;
            sLie.label = sign;
            sLie.newText = "지금 누르면\n바로 성공입니다";
            sLie.audioSource = Audio(lieHolder, 0.8f); sLie.sfx = DoNothingStimulus.Sfx.Ding;
            stimuli.Add(sLie);

            // 6) [새 제안] Someone stands right behind you (63 s → arrives ~69 s) — social / personal space
            var figure = new GameObject("Figure").transform; figure.SetParent(props, false);
            // faceless mannequin standing on the floor (legs + torso + shoulders + neck + head)
            var fBody = Prim(PrimitiveType.Capsule, "Body", figure, new Vector3(0, 1.05f, 0), new Vector3(0.38f, 0.34f, 0.24f), "Charcoal");
            Prim(PrimitiveType.Capsule, "Leg_L", figure, new Vector3(-0.09f, 0.42f, 0), new Vector3(0.14f, 0.43f, 0.14f), "Charcoal");
            Prim(PrimitiveType.Capsule, "Leg_R", figure, new Vector3(0.09f, 0.42f, 0), new Vector3(0.14f, 0.43f, 0.14f), "Charcoal");
            Prim(PrimitiveType.Sphere, "Shoulders", figure, new Vector3(0, 1.34f, 0), new Vector3(0.46f, 0.16f, 0.24f), "Charcoal");
            Prim(PrimitiveType.Capsule, "Arm_L", figure, new Vector3(-0.25f, 1.02f, 0), new Vector3(0.1f, 0.33f, 0.1f), "Charcoal");
            Prim(PrimitiveType.Capsule, "Arm_R", figure, new Vector3(0.25f, 1.02f, 0), new Vector3(0.1f, 0.33f, 0.1f), "Charcoal");
            Prim(PrimitiveType.Cylinder, "Neck", figure, new Vector3(0, 1.46f, 0), new Vector3(0.1f, 0.06f, 0.1f), "Charcoal");
            Prim(PrimitiveType.Sphere, "Head", figure, new Vector3(0, 1.62f, 0.01f), new Vector3(0.2f, 0.25f, 0.22f), "Charcoal");
            foreach (var c in figure.GetComponentsInChildren<Collider>()) if (c.gameObject != fBody) Object.DestroyImmediate(c);
            temptations.Add(MakeTemptation(fBody, "figure", "등 뒤의 사람"));
            var figHolder = new GameObject("FigureStimulus"); figHolder.transform.SetParent(props, false);
            var figAction = figHolder.AddComponent<FigureBehindAction>();
            figAction.figure = figure;
            figAction.doorPosition = new Vector3(0.3f, 0f, BackZ + 0.3f);
            var steps = new GameObject("Footsteps"); steps.transform.SetParent(figHolder.transform, false);
            figAction.footstepAudio = Audio(steps, 0.9f);
            var breath = new GameObject("Breath"); breath.transform.SetParent(figHolder.transform, false);
            figAction.breathAudio = Audio(breath, 0.9f); figAction.breathAudio.minDistance = 0.05f; figAction.breathAudio.maxDistance = 3f;
            var sFig = Stim(figHolder, "figure", "등 뒤의 사람", DoNothingStimulus.Kind.Custom, 63f, false);
            sFig.action = figAction; sFig.phase = DoNothingStimulus.Phase.Social;
            stimuli.Add(sFig);

            // 7) [새 제안] Cockroach: drain → feet → climbs to the hand, walks on it with haptics (115 s)
            var roach = BuildCockroach(props, out var roachAction);
            temptations.Add(roachAction.temptation);
            var roachHolder = new GameObject("CockroachStimulus"); roachHolder.transform.SetParent(props, false);
            var sRoach = Stim(roachHolder, "cockroach", "바퀴벌레", DoNothingStimulus.Kind.Custom, 115f, false);
            sRoach.action = roachAction; sRoach.phase = DoNothingStimulus.Phase.BodyThreat;
            roachAction.drainPosition = DrainPos;
            stimuli.Add(sRoach);

            // 8) [새 제안] The floor collapses around you (150 s) — height
            var collapseHolder = new GameObject("FloorCollapseStimulus"); collapseHolder.transform.SetParent(props, false);
            var collapse = collapseHolder.AddComponent<FloorCollapseAction>();
            collapse.tilesRoot = tilesRoot;
            collapse.center = new Vector3(0, 0, (SlabMin.y + SlabMax.y) / 2);
            var rumble = new GameObject("Rumble"); rumble.transform.SetParent(collapseHolder.transform, false);
            rumble.transform.position = new Vector3(0, -1.5f, 0.6f);
            collapse.rumbleAudio = Audio(rumble, 1f); collapse.rumbleAudio.minDistance = 3f;
            var sCollapse = Stim(collapseHolder, "floor_collapse", "꺼진 바닥", DoNothingStimulus.Kind.Custom, 150f, false);
            sCollapse.action = collapse; sCollapse.phase = DoNothingStimulus.Phase.BodyThreat;
            stimuli.Add(sCollapse);

            // ── game
            var game = systems.AddComponent<DoNothingGame>();
            game.rig = rig; game.instructionPanel = instr; game.resultPanel = result; game.timer = timer;
            game.exitProgress = exitBar; game.uiAudio = uiAudio; game.stimuli = stimuli; game.temptations = temptations;

            EditorSceneManager.SaveScene(scene, ScenePath);
            MuseumEnvironmentKit.BakeProbe("ReflectionProbe", new Vector3(0, RoomH / 2, cz), new Vector3(RoomW, RoomH, depth), HallDir + "DoNothing_Reflection.exr");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Debug.Log("[Museum] DoNothing scene built: " + ScenePath);
        }

        // ───────────── helpers ─────────────

        static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 scale, string mat, bool castShadow)
            => Prim(PrimitiveType.Cube, name, parent, pos, scale, mat, castShadow);

        static GameObject Prim(PrimitiveType t, string name, Transform parent, Vector3 pos, Vector3 scale, string mat, bool castShadow = true)
        {
            var go = GameObject.CreatePrimitive(t);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            if (mat != null) r.sharedMaterial = M(mat);
            r.shadowCastingMode = castShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;
            if (!castShadow) GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            return go;
        }

        static T Place<T>(string prefab, Transform parent, Vector3 pos, Quaternion rot) where T : Component
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(UIPrefab(prefab), parent);
            go.transform.SetPositionAndRotation(pos, rot);
            return go.GetComponent<T>();
        }

        static TextMeshProUGUI WorldText(string name, Transform parent, Vector3 localPos, float w, float h,
            TMP_FontAsset font, float size, Color color, TextAlignmentOptions align, string text)
        {
            var rt = MuseumUIBuilder.WorldCanvas(name, w, h, false);
            rt.SetParent(parent, false);
            rt.localPosition = localPos;
            rt.localRotation = Quaternion.identity;
            var t = MuseumUIBuilder.Text("Text", rt, font, size, color, align, text);
            MuseumUIBuilder.Stretch(t.rectTransform);
            return t;
        }

        static GameObject BuildCockroach(Transform parent, out CockroachAction action)
        {
            var mat = HallMaterial("M_DN_Roach", new Color(0.23f, 0.13f, 0.07f), 0.78f, 0.1f);
            var root = new GameObject("Cockroach");
            root.transform.SetParent(parent, false);
            var bc = root.AddComponent<BoxCollider>(); bc.size = new Vector3(0.045f, 0.025f, 0.075f); bc.center = new Vector3(0, 0.01f, 0);
            var body = Prim(PrimitiveType.Sphere, "Body", root.transform, new Vector3(0, 0.009f, -0.004f), new Vector3(0.03f, 0.014f, 0.058f), null);
            body.GetComponent<Renderer>().sharedMaterial = mat;
            var head = Prim(PrimitiveType.Sphere, "Head", root.transform, new Vector3(0, 0.007f, 0.028f), new Vector3(0.016f, 0.01f, 0.013f), null);
            head.GetComponent<Renderer>().sharedMaterial = mat;
            foreach (var c in root.GetComponentsInChildren<Collider>()) if (c != bc) Object.DestroyImmediate(c);

            action = root.AddComponent<CockroachAction>();
            action.body = body.transform;
            for (int side = 0; side < 2; side++)
            for (int i = 0; i < 3; i++)
            {
                float sx = side == 0 ? -1f : 1f;
                var pivot = new GameObject($"Leg_{(side == 0 ? "L" : "R")}{i}").transform;
                pivot.SetParent(root.transform, false);
                pivot.localPosition = new Vector3(0.01f * sx, 0.007f, 0.014f - i * 0.014f);
                var leg = Prim(PrimitiveType.Cylinder, "Seg", pivot, new Vector3(0.012f * sx, -0.004f, 0), new Vector3(0.0022f, 0.013f, 0.0022f), null);
                leg.transform.localRotation = Quaternion.Euler(0, 0, sx * 70f);
                leg.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(leg.GetComponent<Collider>());
                if (side == 0) action.legsLeft[i] = pivot; else action.legsRight[i] = pivot;
            }
            for (int side = 0; side < 2; side++)
            {
                float sx = side == 0 ? -1f : 1f;
                var pivot = new GameObject(side == 0 ? "Antenna_L" : "Antenna_R").transform;
                pivot.SetParent(root.transform, false);
                pivot.localPosition = new Vector3(0.004f * sx, 0.009f, 0.034f);
                var ant = Prim(PrimitiveType.Cylinder, "Seg", pivot, new Vector3(0, 0, 0.024f), new Vector3(0.0009f, 0.024f, 0.0009f), null);
                ant.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                ant.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(ant.GetComponent<Collider>());
                if (side == 0) action.antennaL = pivot; else action.antennaR = pivot;
            }
            root.AddComponent<XRSimpleInteractable>();
            var t = root.AddComponent<Temptation>();
            t.id = "cockroach"; t.displayName = "바퀴벌레"; t.touchRadius = 0.04f;
            action.temptation = t;
            action.skitterAudio = Audio(root, 1f);
            action.skitterAudio.minDistance = 0.1f; action.skitterAudio.maxDistance = 4f;
            return root;
        }

        static Material HallMaterial(string name, Color c, float smooth, float metal)
        {
            string path = HallDir + "Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            if (!AssetDatabase.IsValidFolder(HallDir + "Materials")) AssetDatabase.CreateFolder(HallDir.TrimEnd('/'), "Materials");
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", c); m.SetFloat("_Smoothness", smooth); m.SetFloat("_Metallic", metal);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static Temptation MakeTemptation(GameObject go, string id, string display)
        {
            go.AddComponent<XRSimpleInteractable>();
            var t = go.AddComponent<Temptation>();
            t.id = id; t.displayName = display;
            return t;
        }

        static DoNothingStimulus Stim(GameObject host, string id, string display, DoNothingStimulus.Kind kind, float time, bool minimal)
        {
            var s = host.AddComponent<DoNothingStimulus>();
            s.id = id; s.displayName = display; s.kind = kind; s.triggerTime = time; s.inMinimalCondition = minimal;
            return s;
        }

        static AudioSource Audio(GameObject host, float volume)
        {
            var a = host.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.spatialBlend = 1f;               // 3D positional sound
            a.rolloffMode = AudioRolloffMode.Linear;
            a.minDistance = 0.5f; a.maxDistance = 12f;
            a.volume = volume;
            return a;
        }

        static Material CoinMaterial()
        {
            const string path = HallDir + "Materials/M_DN_CoinBrass.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            if (!AssetDatabase.IsValidFolder(HallDir + "Materials")) AssetDatabase.CreateFolder(HallDir.TrimEnd('/'), "Materials");
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", MuseumPalette.Weight);
            m.SetFloat("_Metallic", 0.85f);
            m.SetFloat("_Smoothness", 0.6f);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static void AddToBuildSettings(string path)
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            list.RemoveAll(s => s.path == path);
            list.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
