using System.Collections.Generic;
using MuseumOfLies.Common;
using MuseumOfLies.EditorTools;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace MuseumOfLies.CentralHall.EditorTools
{
    /// <summary>
    /// Menu: Museum ▸ Build Scene ▸ 00 Central Hall
    /// Monumental concrete atrium (18 × 24 m, 10 m ceiling): colonnade, grand staircase to the (locked) Truth Room,
    /// museum title wall, empty plinths, bench, and FOUR IDENTICAL automatic doors (2 per side wall).
    /// Which exhibition is behind which door is decided at runtime and never shown before entering.
    /// Movement: left stick walk + right stick snap turn (no teleport).
    /// </summary>
    public static class CentralHallSceneBuilder
    {
        const string HallDir = "Assets/_Museum/Halls/00_CentralHall/";
        const string ScenePath = HallDir + "CentralHall.unity";
        const string XROriginPrefab = "Assets/Samples/XR Interaction Toolkit/3.7.0/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

        const float HalfW = 9f, HalfD = 12f, H = 10f, WallT = 0.4f;
        const float DoorW = 2.2f, DoorH = 3.2f, VestDepth = 4f;
        static readonly float[] GateZ = { -5f, 1f };          // near, far (from the entrance)
        const float StairW = 7f, StepH = 0.2f, StepRun = 0.32f, StairZ0 = 3.4f; const int Steps = 20;

        static Material M(string token) => AssetDatabase.LoadAssetAtPath<Material>("Assets/_Museum/Common/Materials/M_" + token + ".mat");
        static Material Env(string n) => MuseumEnvironmentKit.Mat(n);
        static float CT => MuseumEnvironmentKit.ConcreteTile;

        [MenuItem("Museum/Build Scene/00 Central Hall")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(MuseumUIBuilder.Dir + "UI_Timer.prefab") == null) MuseumUIBuilder.BuildAll();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            MuseumEnvironmentKit.ApplyGalleryLighting();

            var room = new GameObject("Atrium").transform;
            var concrete = Env("Concrete");
            var dark = Env("ConcreteDark");

            // ── floor / ceiling
            var floor = MuseumEnvironmentKit.WorldBox("Floor", room, new Vector3(0, -0.05f, 0), new Vector3(HalfW * 2, 0.1f, HalfD * 2), Env("FloorPolished"), MuseumEnvironmentKit.FloorTile);
            MuseumEnvironmentKit.WorldBox("Ceiling", room, new Vector3(0, H + 0.05f, 0), new Vector3(HalfW * 2, 0.1f, HalfD * 2), Env("CeilingLight"), 1.8f);

            // ── north / south walls
            MuseumEnvironmentKit.WorldBox("Wall_North", room, new Vector3(0, H / 2, HalfD + WallT / 2), new Vector3(HalfW * 2 + 0.8f, H, WallT), concrete, CT);
            MuseumEnvironmentKit.WorldBox("Wall_South", room, new Vector3(0, H / 2, -HalfD - WallT / 2), new Vector3(HalfW * 2 + 0.8f, H, WallT), concrete, CT);

            // ── side walls with two door openings each
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * (HalfW + WallT / 2);
                var cuts = new List<(float a, float b)>();
                foreach (var gz in GateZ) cuts.Add((gz - DoorW / 2, gz + DoorW / 2));
                float z = -HalfD;
                int seg = 0;
                foreach (var c in cuts)
                {
                    MuseumEnvironmentKit.WorldBox($"Wall_{(side < 0 ? "W" : "E")}_{seg++}", room, new Vector3(x, H / 2, (z + c.a) / 2), new Vector3(WallT, H, c.a - z), concrete, CT);
                    MuseumEnvironmentKit.WorldBox($"Wall_{(side < 0 ? "W" : "E")}_Lintel{seg}", room, new Vector3(x, (DoorH + H) / 2, (c.a + c.b) / 2), new Vector3(WallT, H - DoorH, c.b - c.a), concrete, CT);
                    z = c.b;
                }
                MuseumEnvironmentKit.WorldBox($"Wall_{(side < 0 ? "W" : "E")}_{seg}", room, new Vector3(x, H / 2, (z + HalfD) / 2), new Vector3(WallT, H, HalfD - z), concrete, CT);
            }

            // ── colonnade: 2 rows × 5 monumental columns
            var cols = new GameObject("Colonnade").transform; cols.SetParent(room, false);
            foreach (float side in new[] { -1f, 1f })
                for (int i = 0; i < 5; i++)
                {
                    float cz = -8f + i * 3.6f;
                    var p = new Vector3(side * 5.6f, 0, cz);
                    MuseumEnvironmentKit.WorldBox($"Column_{(side < 0 ? "W" : "E")}{i}", cols, p + Vector3.up * H / 2, new Vector3(1.1f, H, 1.1f), concrete, CT, true);
                    MuseumEnvironmentKit.WorldBox($"Column_{(side < 0 ? "W" : "E")}{i}_Base", cols, p + Vector3.up * 0.12f, new Vector3(1.4f, 0.24f, 1.4f), dark, CT);
                }

            // ── grand staircase → landing → Truth Room door (locked)
            BuildStairs(room, concrete, dark);

            // ── title wall
            var title = WorldText("Title", room, new Vector3(0, 9.0f, HalfD - 0.02f), 12000, 1000, MuseumUIBuilder.EnBold, 760, MuseumPalette.Black, TextAlignmentOptions.Center, "THE MUSEUM OF LIES");
            title.characterSpacing = 18;
            WorldText("TitleKR", room, new Vector3(0, 8.15f, HalfD - 0.02f), 8000, 500, MuseumUIBuilder.KrBold, 340, MuseumPalette.Black, TextAlignmentOptions.Center, "거짓말하는 박물관");
            WorldText("Question", room, new Vector3(0, 7.62f, HalfD - 0.02f), 8000, 300, MuseumUIBuilder.KrRegular, 170, MuseumPalette.Charcoal, TextAlignmentOptions.Center, "당신은 당신을 얼마나 믿습니까?");

            // ── museum furniture: bench + two empty plinths (the exhibits "haven't arrived" — foreshadowing)
            var furn = new GameObject("Furniture").transform; furn.SetParent(room, false);
            MuseumEnvironmentKit.WorldBox("Bench", furn, new Vector3(0, 0.22f, -3.2f), new Vector3(2.2f, 0.44f, 0.55f), M("Charcoal"), 1f, true);
            foreach (float side in new[] { -1f, 1f })
            {
                var pp = new Vector3(side * 2.9f, 0, -0.6f);
                MuseumEnvironmentKit.WorldBox($"Plinth_{(side < 0 ? "W" : "E")}", furn, pp + Vector3.up * 0.5f, new Vector3(0.6f, 1f, 0.6f), M("Ivory"), 1f, true);
                var lab = WorldText($"PlinthLabel_{(side < 0 ? "W" : "E")}", furn, pp + new Vector3(0, 0.62f, -0.302f), 520, 300, MuseumUIBuilder.KrRegular, 30, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft,
                    "<size=24>UNTITLED</size>\n작품명 미상\n<size=22>이 자리의 전시물은 아직 도착하지 않았습니다.</size>");
                lab.margin = new Vector4(24, 20, 24, 0);
            }

            // ── main entrance (behind spawn, decorative, closed)
            var ent = new GameObject("MainEntrance").transform; ent.SetParent(room, false);
            ent.localPosition = new Vector3(0, 0, -HalfD + 0.01f);
            MuseumEnvironmentKit.WorldBox("Leaf_L", ent, new Vector3(-0.8f, 1.8f, 0.03f), new Vector3(1.58f, 3.6f, 0.06f), Env("Door"), 1f);
            MuseumEnvironmentKit.WorldBox("Leaf_R", ent, new Vector3(0.8f, 1.8f, 0.03f), new Vector3(1.58f, 3.6f, 0.06f), Env("Door"), 1f);
            MuseumEnvironmentKit.WorldBox("Frame", ent, new Vector3(0, 3.66f, 0.03f), new Vector3(3.4f, 0.12f, 0.08f), dark, CT);

            // ── four identical gates
            var gatesRoot = new GameObject("Gates").transform;
            var gates = new List<Gate>();
            int idx = 0;
            foreach (float side in new[] { -1f, 1f })
                for (int i = 0; i < GateZ.Length; i++)
                {
                    string label = (side < 0 ? "L" : "R") + (i + 1);      // L1 = left-near … R2 = right-far (seen from the entrance)
                    gates.Add(BuildGate(gatesRoot, idx++, label, side, GateZ[i]));
                }

            // ── persistent how-to placard near the entrance (stays after the survey closes)
            var guide = WorldText("HowToPlacard", room, new Vector3(-2.3f, 1.35f, -8.6f), 640, 300, MuseumUIBuilder.KrRegular, 30, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft,
                "<size=24>HOW TO VISIT</size>\n<b>이동</b>  왼쪽 스틱\n<b>회전</b>  오른쪽 스틱\n<b>입장</b>  문 앞에 서면 열립니다 → 걸어 들어가기");
            guide.lineSpacing = 10;
            guide.transform.parent.localRotation = Quaternion.Euler(0, -25f, 0);
            Placard(guide.transform.parent, 0.66f, 0.32f);
            MuseumEnvironmentKit.WorldBox("HowToStand", room, new Vector3(-2.3f, 0.55f, -8.55f), new Vector3(0.06f, 1.1f, 0.06f), M("Charcoal"), 1f, true);

            // ── XR rig & systems
            var origin = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(XROriginPrefab));
            var spawn = new GameObject("Spawn_Entrance").transform;
            spawn.SetPositionAndRotation(new Vector3(0, 0, -9.6f), Quaternion.identity);
            origin.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(XRUIInputModule));

            var systems = new GameObject("Systems");
            var rig = systems.AddComponent<XRRig>();
            rig.origin = origin.transform;
            rig.locomotion = XRRig.LocomotionMode.LeftStickWalk;
            rig.Resolve();
            var tone = systems.AddComponent<AudioSource>();
            tone.spatialBlend = 0f; tone.volume = 0.22f; tone.playOnAwake = false;

            var ctrl = systems.AddComponent<CentralHallController>();
            ctrl.rig = rig; ctrl.gates = gates; ctrl.defaultSpawn = spawn; ctrl.roomTone = tone;
            ctrl.entrySurvey = BuildEntrySurvey(new Vector3(0, 1.45f, -7.9f));

            EditorSceneManager.SaveScene(scene, ScenePath);
            MuseumEnvironmentKit.BakeProbe("ReflectionProbe", new Vector3(0, H / 2, 0), new Vector3(HalfW * 2, H, HalfD * 2), HallDir + "CentralHall_Reflection.exr");
            EditorSceneManager.SaveScene(scene, ScenePath);
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            list.RemoveAll(s => s.path == ScenePath);
            list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
            Debug.Log("[Museum] Central Hall built: " + ScenePath);
        }

        // ───────────────────────── parts ─────────────────────────

        static void BuildStairs(Transform room, Material concrete, Material dark)
        {
            var st = new GameObject("GrandStaircase").transform; st.SetParent(room, false);
            for (int i = 0; i < Steps; i++)
            {
                float top = (i + 1) * StepH;
                float z = StairZ0 + i * StepRun + StepRun / 2;
                MuseumEnvironmentKit.WorldBox($"Step_{i:00}", st, new Vector3(0, top / 2, z), new Vector3(StairW, top, StepRun), concrete, CT);
                foreach (float s in new[] { -1f, 1f }) // stepped solid parapets
                    MuseumEnvironmentKit.WorldBox($"Parapet_{(s < 0 ? "W" : "E")}{i:00}", st, new Vector3(s * (StairW / 2 + 0.15f), (top + 1.0f) / 2, z), new Vector3(0.3f, top + 1.0f, StepRun), dark, CT);
            }
            float landY = Steps * StepH;
            float landZ0 = StairZ0 + Steps * StepRun;
            MuseumEnvironmentKit.WorldBox("Landing", st, new Vector3(0, landY / 2, (landZ0 + HalfD) / 2), new Vector3(StairW + 0.6f, landY, HalfD - landZ0), concrete, CT);

            // Truth Room door on the landing (locked until all four halls are visited)
            var door = new GameObject("TruthRoomDoor").transform; door.SetParent(st, false);
            door.localPosition = new Vector3(0, landY, HalfD - 0.01f);
            door.localRotation = Quaternion.Euler(0, 180, 0);
            MuseumEnvironmentKit.WorldBox("Leaf_L", door, new Vector3(-0.55f, 1.6f, 0.03f), new Vector3(1.08f, 3.2f, 0.06f), Env("Door"), 1f);
            MuseumEnvironmentKit.WorldBox("Leaf_R", door, new Vector3(0.55f, 1.6f, 0.03f), new Vector3(1.08f, 3.2f, 0.06f), Env("Door"), 1f);
            MuseumEnvironmentKit.WorldBox("LightLine", door, new Vector3(0, 3.32f, 0.03f), new Vector3(2.2f, 0.03f, 0.03f), Env("LightStrip"), 1f);
            var tr = WorldText("TruthPlacard", st, new Vector3(1.55f, landY + 1.5f, HalfD - 0.02f), 500, 260, MuseumUIBuilder.KrRegular, 30, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft,
                "<size=24>05 — THE TRUTH ROOM</size>\n<b>진실의 방</b>\n<size=24>네 개의 전시를 모두 관람한 뒤 열립니다.</size>");
            Placard(tr.transform.parent, 0.52f, 0.28f);

            // invisible blocker across the whole stair foot (walking is on; the Truth Room stays closed)
            var blocker = new GameObject("StairBlocker"); blocker.transform.SetParent(st, false);
            blocker.transform.localPosition = new Vector3(0, 1.5f, StairZ0 - 0.4f);
            blocker.AddComponent<BoxCollider>().size = new Vector3(StairW + 0.8f, 3f, 0.3f);

            // velvet rope at the foot of the stairs (classic museum barrier)
            var ropeMat = HallMaterial("M_CH_Velvet", new Color(0.43f, 0.12f, 0.14f), 0.35f, 0f);
            var rope = new GameObject("Stanchions").transform; rope.SetParent(st, false);
            float rz = StairZ0 - 0.6f;
            float[] xs = { -2.6f, -0.9f, 0.9f, 2.6f };
            foreach (var x in xs)
            {
                Prim(PrimitiveType.Cylinder, "Post", rope, new Vector3(x, 0.47f, rz), new Vector3(0.06f, 0.47f, 0.06f), M("Charcoal"));
                Prim(PrimitiveType.Cylinder, "Base", rope, new Vector3(x, 0.015f, rz), new Vector3(0.3f, 0.015f, 0.3f), M("Charcoal"));
                Prim(PrimitiveType.Sphere, "Cap", rope, new Vector3(x, 0.96f, rz), Vector3.one * 0.08f, M("Charcoal"));
            }
            for (int i = 0; i < xs.Length - 1; i++)
            {
                float mid = (xs[i] + xs[i + 1]) / 2, len = xs[i + 1] - xs[i];
                var r = Prim(PrimitiveType.Cylinder, "Rope", rope, new Vector3(mid, 0.82f, rz), new Vector3(0.035f, len / 2, 0.035f), null);
                r.GetComponent<Renderer>().sharedMaterial = ropeMat;
                r.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
        }

        static Gate BuildGate(Transform parent, int index, string label, float side, float z)
        {
            // Gate root sits in the wall opening; its +Z points INTO the vestibule (away from the atrium).
            var root = new GameObject($"Gate_{label}").transform;
            root.SetParent(parent, false);
            root.position = new Vector3(side * HalfW, 0, z);
            root.rotation = Quaternion.Euler(0, side < 0 ? -90f : 90f, 0);
            var gate = root.gameObject.AddComponent<Gate>();
            gate.index = index; gate.positionLabel = label;

            // frame (dark concrete jambs + head) and the light line every gate shares
            MuseumEnvironmentKit.WorldBox("Jamb_L", root, new Vector3(-DoorW / 2 - 0.05f, DoorH / 2, 0.2f), new Vector3(0.1f, DoorH, 0.42f), Env("ConcreteDark"), CT);
            MuseumEnvironmentKit.WorldBox("Jamb_R", root, new Vector3(DoorW / 2 + 0.05f, DoorH / 2, 0.2f), new Vector3(0.1f, DoorH, 0.42f), Env("ConcreteDark"), CT);
            MuseumEnvironmentKit.WorldBox("Head", root, new Vector3(0, DoorH + 0.05f, 0.2f), new Vector3(DoorW + 0.2f, 0.1f, 0.42f), Env("ConcreteDark"), CT);

            // gate sign (design: colour band "GATE 0N" → white line → thin colour strip → light gray double door)
            float sw = DoorW + 0.2f, fz = -0.215f;
            var strip = MuseumEnvironmentKit.WorldBox("Sign_AccentStrip", root, new Vector3(0, DoorH + 0.05f, fz), new Vector3(sw, 0.1f, 0.03f), M("Charcoal"), 1f);
            MuseumEnvironmentKit.WorldBox("Sign_WhiteLine", root, new Vector3(0, DoorH + 0.125f, fz), new Vector3(sw, 0.05f, 0.03f), M("Ivory"), 1f);
            var band = MuseumEnvironmentKit.WorldBox("Sign_Band", root, new Vector3(0, DoorH + 0.4f, fz), new Vector3(sw, 0.5f, 0.03f), M("Charcoal"), 1f);
            var lab = WorldText("Sign_Label", root, new Vector3(0, DoorH + 0.4f, fz - 0.02f), 2400, 500, MuseumUIBuilder.EnBold, 300, MuseumPalette.Black, TextAlignmentOptions.Center, "GATE 00");
            lab.textWrappingMode = TextWrappingModes.NoWrap;
            gate.band = band.GetComponent<Renderer>();
            gate.accentStrip = strip.GetComponent<Renderer>();
            gate.gateLabel = lab;
            gate.hallMaterials = new[] { M("Weight"), M("Body"), M("Ability"), M("DoNothing") };

            // sliding leaves
            var door = root.gameObject.AddComponent<AutoDoor>();
            // light gray leaves that close fully (no light leak); the centre seam is a thin charcoal line riding on the left leaf
            var l = MuseumEnvironmentKit.WorldBox("Leaf_L", root, new Vector3(-DoorW / 4, DoorH / 2, 0.2f), new Vector3(DoorW / 2, DoorH, 0.06f), M("LightGray"), 1f, true, false);
            var r = MuseumEnvironmentKit.WorldBox("Leaf_R", root, new Vector3(DoorW / 4, DoorH / 2, 0.2f), new Vector3(DoorW / 2, DoorH, 0.06f), M("LightGray"), 1f, true, false);
            var seam = MuseumEnvironmentKit.WorldBox("Seam", l.transform, new Vector3(DoorW / 4 - 0.004f, 0, -0.032f), new Vector3(0.006f, DoorH, 0.004f), M("Charcoal"), 1f, false, false);
            Object.DestroyImmediate(seam.GetComponent<BoxCollider>());
            door.leafLeft = l.transform; door.leafRight = r.transform; door.slide = DoorW / 2 - 0.04f;
            var a = root.gameObject.AddComponent<AudioSource>();
            a.playOnAwake = false; a.spatialBlend = 1f; a.minDistance = 1f; a.maxDistance = 15f; a.volume = 0.8f;
            door.audioSource = a;
            gate.door = door;

            // vestibule behind the door — dark, short, reveals nothing
            var v = new GameObject("Vestibule").transform; v.SetParent(root, false);
            float d0 = WallT / 2, d1 = d0 + VestDepth, vw = DoorW + 0.4f, vh = DoorH + 0.2f;
            var vf = MuseumEnvironmentKit.WorldBox("Floor", v, new Vector3(0, -0.05f, (d0 + d1) / 2), new Vector3(vw, 0.1f, d1 - d0), Env("CorridorFloor"), MuseumEnvironmentKit.FloorTile);
            MuseumEnvironmentKit.WorldBox("Ceiling", v, new Vector3(0, vh + 0.05f, (d0 + d1) / 2), new Vector3(vw, 0.1f, d1 - d0), Env("CorridorCeiling"), CT);
            MuseumEnvironmentKit.WorldBox("Wall_L", v, new Vector3(-vw / 2 - 0.1f, vh / 2, (d0 + d1) / 2), new Vector3(0.2f, vh, d1 - d0), Env("CorridorWall"), CT);
            MuseumEnvironmentKit.WorldBox("Wall_R", v, new Vector3(vw / 2 + 0.1f, vh / 2, (d0 + d1) / 2), new Vector3(0.2f, vh, d1 - d0), Env("CorridorWall"), CT);
            MuseumEnvironmentKit.WorldBox("Wall_End", v, new Vector3(0, vh / 2, d1 + 0.1f), new Vector3(vw + 0.4f, vh, 0.2f), Env("CorridorWall"), CT);
            MuseumEnvironmentKit.WorldBox("Strip", v, new Vector3(0, vh - 0.02f, (d0 + d1) / 2), new Vector3(0.06f, 0.02f, d1 - d0 - 0.4f), Env("LightStrip"), 1f);
            var soon = WorldText("ComingSoon", v, new Vector3(0, 1.55f, d1 - 0.01f), 900, 300, MuseumUIBuilder.KrBold, 54, MuseumPalette.Charcoal, TextAlignmentOptions.Center,
                "이 전시는 아직 준비 중입니다\n<size=32>다른 문을 선택해 주세요</size>");
            var soonRoot = new GameObject("ComingSoon").transform; soonRoot.SetParent(v, false);
            soon.transform.parent.SetParent(soonRoot, true);
            Placard(soon.transform.parent, 0.95f, 0.36f);
            gate.comingSoon = soon;
            gate.comingSoonRoot = soonRoot.gameObject;
            soonRoot.gameObject.SetActive(false);
            gate.enterDepth = 1.0f; // one step past the door frame = enter

            // identical floor mark in front of every door: "stand here and it opens" (no hint about the content)
            MuseumEnvironmentKit.WorldBox("Mat_Outer", root, new Vector3(0, 0.003f, -1.1f), new Vector3(1.8f, 0.006f, 1.2f), M("Charcoal"), 1f);
            MuseumEnvironmentKit.WorldBox("Mat_Inner", root, new Vector3(0, 0.004f, -1.1f), new Vector3(1.72f, 0.006f, 1.12f), Env("FloorPolished"), MuseumEnvironmentKit.FloorTile);

            // identical placard beside the door on the atrium side (no number, no colour, no name)
            var plc = WorldText("Placard", root, new Vector3(DoorW / 2 + 0.75f, 1.45f, -0.215f), 460, 220, MuseumUIBuilder.KrRegular, 30, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft,
                "<size=24>EXHIBITION</size>\n<b>전시실</b>\n<size=26>전시 관람 가능</size>");
            Placard(plc.transform.parent, 0.48f, 0.24f);
            plc.text = "<size=24>EXHIBITION</size>\n<b>전시실</b>";
            var status = MuseumUIBuilder.Text("Status", plc.transform.parent, MuseumUIBuilder.KrBold, 26, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft, "전시 관람 가능");
            MuseumUIBuilder.TopLeft(status.rectTransform, 22, 150, 416, 44);
            gate.placardStatus = status;

            // where you stand when you come back out of this gate (2 m into the atrium, facing it)
            var ret = new GameObject("ReturnSpawn").transform; ret.SetParent(root, false);
            ret.localPosition = new Vector3(0, 0, -2.2f);
            ret.localRotation = Quaternion.Euler(0, 180, 0);
            gate.returnSpawn = ret;
            return gate;
        }

        static EntrySurvey BuildEntrySurvey(Vector3 pos)
        {
            const float W = 1200, Hh = 720, P = 64;
            var root = MuseumUIBuilder.WorldCanvas("EntrySurvey", W, Hh, true);
            root.position = pos;
            MuseumUIBuilder.Stretch(MuseumUIBuilder.Img("Background", root, MuseumPalette.Ivory).rectTransform);
            MuseumUIBuilder.TopLeft(MuseumUIBuilder.Img("AccentBar", root, MuseumPalette.Charcoal).rectTransform, 0, 0, 12, Hh);
            var eb = MuseumUIBuilder.Text("Eyebrow", root, MuseumUIBuilder.EnBold, 28, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft, "ENTRANCE — 입장 질문");
            eb.characterSpacing = 4;
            MuseumUIBuilder.TopLeft(eb.rectTransform, P, 56, W - 2 * P, 40);
            var q = MuseumUIBuilder.Text("Question", root, MuseumUIBuilder.KrBold, 50, MuseumPalette.Black, TextAlignmentOptions.TopLeft,
                "당신은 자신의 감각과 행동을 얼마나 정확하게 알고 있다고 생각합니까?");
            q.lineSpacing = 8;
            MuseumUIBuilder.TopLeft(q.rectTransform, P, 104, W - 2 * P, 140);
            MuseumUIBuilder.TopLeft(MuseumUIBuilder.Img("Divider", root, MuseumPalette.LightGray).rectTransform, P, 262, W - 2 * P, 2);

            var survey = root.gameObject.AddComponent<EntrySurvey>();
            float bw = 120, gap = 16, total = 7 * bw + 6 * gap, x0 = (W - total) / 2;
            for (int i = 0; i < 7; i++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(MuseumUIBuilder.Dir + "UI_Button_Secondary.prefab"), root);
                go.name = $"Scale_{i + 1}";
                var mb = go.GetComponent<MuseumButton>();
                mb.icon = ""; mb.SetText((i + 1).ToString());
                mb.label.font = MuseumUIBuilder.Numeric;
                MuseumUIBuilder.TopLeft((RectTransform)go.transform, x0 + i * (bw + gap), 300, bw, 96);
                survey.scaleButtons.Add(mb);
            }
            var lo = MuseumUIBuilder.Text("LabelLow", root, MuseumUIBuilder.KrRegular, MuseumTypeScale.Caption, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft, "전혀 모른다");
            MuseumUIBuilder.TopLeft(lo.rectTransform, x0, 408, 300, 40);
            var hi = MuseumUIBuilder.Text("LabelHigh", root, MuseumUIBuilder.KrRegular, MuseumTypeScale.Caption, MuseumPalette.Charcoal, TextAlignmentOptions.TopRight, "매우 정확히 안다");
            MuseumUIBuilder.TopLeft(hi.rectTransform, x0 + total - 300, 408, 300, 40);

            var ok = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(MuseumUIBuilder.Dir + "UI_Button_Primary.prefab"), root);
            ok.name = "ConfirmButton";
            var okb = ok.GetComponent<MuseumButton>();
            okb.icon = "→"; okb.SetText("입장하기");
            var ort = (RectTransform)ok.transform;
            ort.anchorMin = ort.anchorMax = Vector2.zero; ort.pivot = Vector2.zero; ort.anchoredPosition = new Vector2(P, 56);
            survey.confirmButton = okb;

            var how = MuseumUIBuilder.Text("HowTo", root, MuseumUIBuilder.KrRegular, MuseumTypeScale.Caption, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft,
                "<b>이동</b> 왼쪽 스틱   ·   <b>회전</b> 오른쪽 스틱\n<b>입장</b> 문 앞 표시에 서면 문이 열리고, 안으로 걸어 들어가면 전시가 시작됩니다.");
            how.lineSpacing = 12;
            MuseumUIBuilder.TopLeft(how.rectTransform, P, 470, W - 2 * P, 90);
            return survey;
        }

        // ───────────────────────── helpers ─────────────────────────

        static TextMeshProUGUI WorldText(string name, Transform parent, Vector3 localPos, float w, float h, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align, string text)
        {
            var rt = MuseumUIBuilder.WorldCanvas(name, w, h, false);
            rt.SetParent(parent, false);
            rt.localPosition = localPos;
            rt.localRotation = Quaternion.identity;
            var t = MuseumUIBuilder.Text("Text", rt, font, size, color, align, text);
            MuseumUIBuilder.Stretch(t.rectTransform);
            t.textWrappingMode = TextWrappingModes.Normal;
            return t;
        }

        /// <summary>Ivory placard board behind a world text canvas (UI stays on the base palette).</summary>
        static void Placard(Transform canvas, float w, float h)
        {
            var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.name = "Board";
            Object.DestroyImmediate(b.GetComponent<Collider>());
            b.transform.SetParent(canvas.parent, false);
            b.transform.localPosition = canvas.localPosition + canvas.localRotation * new Vector3(0, 0, 0.012f);
            b.transform.localRotation = canvas.localRotation;
            b.transform.localScale = new Vector3(w, h, 0.02f);
            b.GetComponent<Renderer>().sharedMaterial = M("Ivory");
            b.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            var t = canvas.GetComponentInChildren<TextMeshProUGUI>();
            if (t && t.margin == Vector4.zero) t.margin = new Vector4(22, 18, 22, 0);
        }

        static GameObject Prim(PrimitiveType t, string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(t);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (mat) go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
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
    }
}
