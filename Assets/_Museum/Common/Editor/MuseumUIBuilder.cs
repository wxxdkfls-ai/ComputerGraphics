using MuseumOfLies.Common;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace MuseumOfLies.EditorTools
{
    /// <summary>
    /// Builds the shared design-system UI prefabs into Assets/_Museum/Common/Prefabs/UI.
    /// Menu: Museum ▸ Build Common UI Prefabs. Safe to re-run (overwrites prefabs in place, GUIDs kept).
    /// </summary>
    public static class MuseumUIBuilder
    {
        public const string Dir = "Assets/_Museum/Common/Prefabs/UI/";
        const string FontDir = "Assets/_Museum/Common/Fonts/";

        public static TMP_FontAsset KrBold => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontDir + "Pretendard-Bold SDF.asset");
        public static TMP_FontAsset KrRegular => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontDir + "Pretendard-Regular SDF.asset");
        public static TMP_FontAsset EnBold => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontDir + "SpaceGrotesk-Bold SDF.asset");
        public static TMP_FontAsset EnRegular => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontDir + "SpaceGrotesk-Regular SDF.asset");
        public static TMP_FontAsset Numeric => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontDir + "SpaceMono-Bold SDF.asset");

        [MenuItem("Museum/Build Common UI Prefabs")]
        public static void BuildAll()
        {
            BuildButton("UI_Button_Primary", MuseumButton.Style.Primary, "확인", "");
            BuildButton("UI_Button_Secondary", MuseumButton.Style.Secondary, "다시 도전", "↺");
            BuildButton("UI_Button_Exit", MuseumButton.Style.Exit, "나가기", "←");
            BuildButton("UI_Button_Disabled", MuseumButton.Style.Disabled, "입장", "");
            BuildProgressBar();
            BuildInstructionPanel();
            BuildTimer();
            BuildResultPanel();
            BuildExitHold();
            AssetDatabase.SaveAssets();
            Debug.Log("[Museum] Common UI prefabs built in " + Dir);
        }

        // ───────────── primitives ─────────────

        public static RectTransform UI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)go.transform;
            if (parent) rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>Top-left anchored rect at (x, -y) with size (w, h).</summary>
        public static RectTransform TopLeft(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Img(string name, Transform parent, Color c, bool raycast = false)
        {
            var rt = UI(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = c;
            img.raycastTarget = raycast;
            return img;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, TMP_FontAsset font, float size, Color c,
            TextAlignmentOptions align, string text)
        {
            var rt = UI(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.fontSize = size;
            t.color = c;
            t.alignment = align;
            t.text = text;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            return t;
        }

        public static RectTransform WorldCanvas(string name, float w, float h, bool interactive)
        {
            var rt = UI(name, null);
            var canvas = rt.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            rt.sizeDelta = new Vector2(w, h);
            rt.localScale = Vector3.one * MuseumTypeScale.WorldCanvasScale;
            if (interactive) rt.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            return rt;
        }

        static GameObject Save(GameObject root, string name)
        {
            var path = Dir + name + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static GameObject Load(string name) => AssetDatabase.LoadAssetAtPath<GameObject>(Dir + name + ".prefab");

        static T Nest<T>(string prefabName, Transform parent) where T : Component
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(Load(prefabName), parent);
            return go.GetComponent<T>();
        }

        // ───────────── components ─────────────

        static void BuildButton(string name, MuseumButton.Style style, string text, string icon)
        {
            var rt = UI(name, null);
            rt.sizeDelta = new Vector2(360, 96);
            var img = rt.gameObject.AddComponent<Image>();
            img.raycastTarget = true;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var label = Text("Label", rt, KrBold, MuseumTypeScale.Body, MuseumPalette.Ivory, TextAlignmentOptions.Center, text);
            Stretch(label.rectTransform);
            var mb = rt.gameObject.AddComponent<MuseumButton>();
            mb.style = style;
            mb.label = label;
            mb.icon = icon;
            mb.Apply();
            Save(rt.gameObject, name);
        }

        static void BuildProgressBar()
        {
            var rt = UI("UI_ProgressBar", null);
            rt.sizeDelta = new Vector2(400, 12);
            var track = rt.gameObject.AddComponent<Image>();
            track.color = MuseumPalette.LightGray;
            track.raycastTarget = false;
            var fill = Img("Fill", rt, MuseumPalette.Charcoal);
            var frt = fill.rectTransform;
            frt.anchorMin = Vector2.zero; frt.anchorMax = new Vector2(0f, 1f);
            frt.pivot = new Vector2(0, 0.5f);
            frt.offsetMin = frt.offsetMax = Vector2.zero;
            var pb = rt.gameObject.AddComponent<MuseumProgressBar>();
            pb.fill = frt; pb.fillImage = fill;
            pb.SetValue(0f);
            Save(rt.gameObject, "UI_ProgressBar");
        }

        static void BuildInstructionPanel()
        {
            const float W = 1200, H = 820, P = 64;
            var root = WorldCanvas("UI_InstructionPanel", W, H, true);
            Stretch(Img("Background", root, MuseumPalette.Ivory).rectTransform);
            var bar = Img("AccentBar", root, MuseumPalette.Charcoal);
            TopLeft(bar.rectTransform, 0, 0, 12, H);
            var eyebrow = Text("Eyebrow", root, EnBold, 28, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft, "00 — EXHIBITION");
            TopLeft(eyebrow.rectTransform, P, 56, W - 2 * P, 40);
            eyebrow.characterSpacing = 4;
            var title = Text("Title", root, KrBold, MuseumTypeScale.Heading, MuseumPalette.Black, TextAlignmentOptions.TopLeft, "전시 제목");
            TopLeft(title.rectTransform, P, 104, W - 2 * P, 88);
            TopLeft(Img("Divider", root, MuseumPalette.LightGray).rectTransform, P, 216, W - 2 * P, 2);
            var body = Text("Body", root, KrRegular, MuseumTypeScale.Body, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft, "설명");
            body.lineSpacing = 18;
            TopLeft(body.rectTransform, P, 248, W - 2 * P, 400);
            var btn = Nest<MuseumButton>("UI_Button_Primary", root);
            var brt = (RectTransform)btn.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(0, 0); brt.pivot = new Vector2(0, 0);
            brt.anchoredPosition = new Vector2(P, 56);
            btn.name = "PrimaryButton";
            var ip = root.gameObject.AddComponent<InstructionPanel>();
            ip.accentBar = bar; ip.eyebrow = eyebrow; ip.title = title; ip.body = body; ip.primaryButton = btn;
            Save(root.gameObject, "UI_InstructionPanel");
        }

        static void BuildTimer()
        {
            const float W = 1400, H = 520;
            var root = WorldCanvas("UI_Timer", W, H, false);
            var time = Text("Time", root, Numeric, MuseumTypeScale.Numeric, MuseumPalette.Charcoal, TextAlignmentOptions.Center, "03:00");
            time.textWrappingMode = TextWrappingModes.NoWrap;
            TopLeft(time.rectTransform, 0, 24, W, 280);
            var line = Img("AccentLine", root, MuseumPalette.Charcoal);
            TopLeft(line.rectTransform, (W - 320) / 2, 320, 320, 6);
            var status = Text("Status", root, KrRegular, MuseumTypeScale.Subheading, MuseumPalette.Charcoal, TextAlignmentOptions.Top, "상태");
            status.lineSpacing = 10;
            TopLeft(status.rectTransform, 0, 352, W, 150);
            var td = root.gameObject.AddComponent<TimerDisplay>();
            td.timeText = time; td.statusText = status; td.accentLine = line;
            Save(root.gameObject, "UI_Timer");
        }

        static void BuildResultPanel()
        {
            const float W = 1200, H = 960, P = 64;
            var root = WorldCanvas("UI_ResultPanel", W, H, true);
            Stretch(Img("Background", root, MuseumPalette.Ivory).rectTransform);
            var bar = Img("AccentBar", root, MuseumPalette.Charcoal);
            TopLeft(bar.rectTransform, 0, 0, 12, H);
            var eyebrow = Text("Eyebrow", root, EnBold, 28, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft, "00 — RESULT");
            eyebrow.characterSpacing = 4;
            TopLeft(eyebrow.rectTransform, P, 56, W - 2 * P - 300, 40);
            var badge = Img("Badge", root, MuseumPalette.Success);
            TopLeft(badge.rectTransform, W - P - 260, 44, 260, 56);
            var title = Text("Title", root, KrBold, 56, MuseumPalette.Black, TextAlignmentOptions.TopLeft, "결과");
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.enableAutoSizing = true; title.fontSizeMin = 40; title.fontSizeMax = 56;
            TopLeft(title.rectTransform, P, 108, W - 2 * P, 80);
            var badgeText = Text("BadgeText", badge.transform, KrBold, 28, MuseumPalette.Ivory, TextAlignmentOptions.Center, "✓  성공");
            Stretch(badgeText.rectTransform);
            TopLeft(Img("Divider", root, MuseumPalette.LightGray).rectTransform, P, 208, W - 2 * P, 2);

            var rows = UI("Rows", root);
            TopLeft(rows, P, 232, W - 2 * P, 420);
            var vlg = rows.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8; vlg.childControlHeight = false; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;

            var row = UI("RowTemplate", rows);
            row.sizeDelta = new Vector2(W - 2 * P, 64);
            var rowLine = Img("Line", row, MuseumPalette.LightGray);
            rowLine.rectTransform.anchorMin = new Vector2(0, 0); rowLine.rectTransform.anchorMax = new Vector2(1, 0);
            rowLine.rectTransform.pivot = new Vector2(0.5f, 0); rowLine.rectTransform.sizeDelta = new Vector2(0, 1);
            var l = Text("Label", row, KrRegular, MuseumTypeScale.Body, MuseumPalette.Charcoal, TextAlignmentOptions.MidlineLeft, "항목");
            l.rectTransform.anchorMin = new Vector2(0, 0); l.rectTransform.anchorMax = new Vector2(0.6f, 1);
            l.rectTransform.offsetMin = l.rectTransform.offsetMax = Vector2.zero;
            var v = Text("Value", row, Numeric, 34, MuseumPalette.Charcoal, TextAlignmentOptions.MidlineRight, "00:00");
            v.rectTransform.anchorMin = new Vector2(0.6f, 0); v.rectTransform.anchorMax = new Vector2(1, 1);
            v.rectTransform.offsetMin = v.rectTransform.offsetMax = Vector2.zero;
            row.gameObject.SetActive(false);

            var foot = Text("Footnote", root, KrRegular, MuseumTypeScale.Caption, new Color(0.15f, 0.15f, 0.15f, 0.7f), TextAlignmentOptions.TopLeft, "");
            TopLeft(foot.rectTransform, P, 680, W - 2 * P, 60);

            var sec = Nest<MuseumButton>("UI_Button_Secondary", root);
            sec.name = "SecondaryButton";
            var srt = (RectTransform)sec.transform;
            srt.anchorMin = srt.anchorMax = Vector2.zero; srt.pivot = Vector2.zero; srt.anchoredPosition = new Vector2(P, 56);
            var pri = Nest<MuseumButton>("UI_Button_Primary", root);
            pri.name = "PrimaryButton";
            pri.icon = "→";
            pri.SetText("박물관으로");
            var prt = (RectTransform)pri.transform;
            prt.anchorMin = prt.anchorMax = Vector2.zero; prt.pivot = Vector2.zero; prt.anchoredPosition = new Vector2(P + 360 + 24, 56);

            var rp = root.gameObject.AddComponent<ResultPanel>();
            rp.accentBar = bar; rp.eyebrow = eyebrow; rp.title = title; rp.badge = badge; rp.badgeText = badgeText;
            rp.rowContainer = rows; rp.rowTemplate = row.gameObject; rp.footnote = foot;
            rp.primaryButton = pri; rp.secondaryButton = sec;
            Save(root.gameObject, "UI_ResultPanel");
        }

        static void BuildExitHold()
        {
            const float W = 560, H = 150;
            var root = WorldCanvas("UI_ExitHold", W, H, false);
            var bg = Img("Background", root, MuseumPalette.Ivory);
            Stretch(bg.rectTransform);
            var ol = bg.gameObject.AddComponent<Outline>();
            ol.effectColor = MuseumPalette.Charcoal; ol.effectDistance = new Vector2(3, -3);
            var label = Text("Label", root, KrBold, 30, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft, "←  나가기");
            TopLeft(label.rectTransform, 32, 24, W - 64, 44);
            var hint = Text("Hint", root, KrRegular, MuseumTypeScale.Caption, MuseumPalette.Charcoal, TextAlignmentOptions.TopLeft, "B 또는 Y 버튼을 2초간 누르세요");
            TopLeft(hint.rectTransform, 32, 68, W - 64, 36);
            var pb = Nest<MuseumProgressBar>("UI_ProgressBar", root);
            pb.name = "HoldProgress";
            TopLeft((RectTransform)pb.transform, 32, 118, W - 64, 10);
            Save(root.gameObject, "UI_ExitHold");
        }
    }
}
