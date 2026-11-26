using System.IO;
using MuseumOfLies.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MuseumOfLies.EditorTools
{
    /// <summary>
    /// Shared environment kit for every hall (see Docs/DESIGN_GUIDELINES.md › Space & Environment).
    /// - Gallery room: raw concrete walls, luminous ceiling panel grid, polished floor (exhibit halls)
    /// - Corridor: cool gray walls, flush doors, linear ceiling lights (central hall)
    /// Textures are generated procedurally (tileable) — no downloaded assets.
    /// Menu: Museum ▸ Build Environment Materials
    /// </summary>
    public static class MuseumEnvironmentKit
    {
        const string TexDir = "Assets/_Museum/Common/Textures/Env/";
        const string MatDir = "Assets/_Museum/Common/Materials/";

        public const float ConcreteTile = 4f;     // meters covered by one concrete texture repeat (Poly Haven "concrete" = 4 m)
        public const float CeilingTile = 1.2f;    // one ceiling light panel
        public const float FloorTile = 3f;        // Poly Haven "concrete_floor_worn_001" = 3 m
        const string PhDir = "Assets/_Museum/Common/Textures/Env/PolyHaven/";

        public static Material Mat(string envName)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Env_" + envName + ".mat");
            if (m == null) { BuildMaterials(); m = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Env_" + envName + ".mat"); }
            return m;
        }

        // ───────────────────────── materials ─────────────────────────

        [MenuItem("Museum/Build Environment Materials")]
        public static void BuildMaterials()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Museum/Common/Textures")) AssetDatabase.CreateFolder("Assets/_Museum/Common", "Textures");
            if (!AssetDatabase.IsValidFolder("Assets/_Museum/Common/Textures/Env")) AssetDatabase.CreateFolder("Assets/_Museum/Common/Textures", "Env");

            var ceiling = SaveTex("T_CeilingPanel", CeilingTexture(512), false);

            if (AssetDatabase.LoadAssetAtPath<Texture2D>(PhDir + "concrete_diff_2k.jpg") != null)
            {
                // Photographic CC0 PBR (Poly Haven): albedo matched to the palette token, roughness → smoothness (alpha)
                var wallAS = AlbedoSmooth("T_Concrete_AlbedoSmooth", PhDir + "concrete_diff_2k.jpg", PhDir + "concrete_rough_2k.jpg", MuseumPalette.ConcreteWall, 0.05f, 0.45f);
                var floorAS = AlbedoSmooth("T_Floor_AlbedoSmooth", PhDir + "concrete_floor_worn_001_diff_2k.jpg", PhDir + "concrete_floor_worn_001_rough_2k.jpg", MuseumPalette.FloorPolished, 0.55f, 0.95f);
                var wallN = ImportLinear(PhDir + "concrete_nor_gl_2k.jpg", true);
                var floorN = ImportLinear(PhDir + "concrete_floor_worn_001_nor_gl_2k.jpg", true);
                var wallAO = ImportLinear(PhDir + "concrete_ao_2k.jpg", false);
                var floorAO = ImportLinear(PhDir + "concrete_floor_worn_001_ao_2k.jpg", false);
                Pbr("Concrete", Color.white, wallAS, wallN, wallAO, 1f, 0f, 1f);
                Pbr("ConcreteDark", MuseumPalette.ConcreteDark / Mathf.Max(0.01f, MuseumPalette.ConcreteWall.grayscale) * 0.95f, wallAS, wallN, wallAO, 1f, 0f, 1f);
                Pbr("FloorPolished", Color.white, floorAS, floorN, floorAO, 1f, 0.35f, 0.5f); // smoothness from map; light metallic = readable probe reflections
            }
            else
            {
                // Fallback: procedural textures (no external files)
                var concrete = SaveTex("T_Concrete", ConcreteTexture(1024, 11), false);
                var concreteN = SaveTex("T_Concrete_N", ConcreteTexture(1024, 11), true);
                var floor = SaveTex("T_FloorPolished", FloorTexture(1024, 23), false);
                Lit("Concrete", MuseumPalette.ConcreteWall, concrete, concreteN, 0.12f, 0f, 0.6f);
                Lit("ConcreteDark", MuseumPalette.ConcreteDark, concrete, concreteN, 0.12f, 0f, 0.6f);
                Lit("FloorPolished", Color.white, floor, null, 0.92f, 0.55f, 0f);
            }
            // Light sources use Unlit: always read as luminous, no emission-keyword fragility, cheaper on Quest.
            Unlit("CeilingLight", MuseumPalette.LightCoolWhite, ceiling);
            Lit("Door", MuseumPalette.DoorSteel, null, null, 0.45f, 0.35f, 0f);
            Lit("CorridorWall", MuseumPalette.CorridorWall, null, null, 0.2f, 0f, 0f);
            Lit("CorridorFloor", MuseumPalette.CorridorFloor, null, null, 0.88f, 0.4f, 0f);
            Lit("CorridorCeiling", MuseumPalette.CorridorCeiling, null, null, 0.1f, 0f, 0f);
            Unlit("LightStrip", MuseumPalette.LightCoolWhite, null);
            AssetDatabase.SaveAssets();
            Debug.Log("[Museum] Environment materials built.");
        }

        static Material Lit(string name, Color tint, Texture2D albedo, Texture2D normal, float smooth, float metal, float bump)
        {
            string path = MatDir + "M_Env_" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool isNew = m == null;
            if (isNew) m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", tint);
            m.SetTexture("_BaseMap", albedo);
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Metallic", metal);
            if (normal != null)
            {
                m.SetTexture("_BumpMap", normal);
                m.SetFloat("_BumpScale", bump);
                m.EnableKeyword("_NORMALMAP");
            }
            else { m.SetTexture("_BumpMap", null); m.DisableKeyword("_NORMALMAP"); }
            m.enableInstancing = true;
            if (isNew) AssetDatabase.CreateAsset(m, path); else EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>
        /// Builds RGB = albedo scaled so its mean matches `target` (palette token), A = smoothness remapped from (1 - roughness).
        /// Keeps the photo's real variation while the overall tone follows the design system.
        /// </summary>
        static Texture2D AlbedoSmooth(string outName, string diffPath, string roughPath, Color target, float smoothMin, float smoothMax)
        {
            var diff = ReadPixels(diffPath, out int w, out int h);
            var rough = ReadPixels(roughPath, out int rw, out int rh);
            double mr = 0, mg = 0, mb = 0;
            foreach (var c in diff) { mr += c.r; mg += c.g; mb += c.b; }
            int n = diff.Length;
            float sr = target.r / (float)(mr / n), sg = target.g / (float)(mg / n), sb = target.b / (float)(mb / n);
            var px = new Color[n];
            for (int i = 0; i < n; i++)
            {
                var c = diff[i];
                float r = rough[(i % w) * rw / w + (i / w) * rh / h * rw].r;
                px[i] = new Color(Mathf.Clamp01(c.r * sr), Mathf.Clamp01(c.g * sg), Mathf.Clamp01(c.b * sb), Mathf.Lerp(smoothMin, smoothMax, 1f - r));
            }
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels(px); tex.Apply();
            string path = TexDir + outName + ".png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.wrapMode = TextureWrapMode.Repeat; imp.mipmapEnabled = true; imp.anisoLevel = 4; imp.maxTextureSize = 2048;
            imp.alphaSource = TextureImporterAlphaSource.FromInput; imp.alphaIsTransparency = false;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Color[] ReadPixels(string assetPath, out int w, out int h)
        {
            var t = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            t.LoadImage(File.ReadAllBytes(assetPath));
            w = t.width; h = t.height;
            var px = t.GetPixels();
            Object.DestroyImmediate(t);
            return px;
        }

        static Texture2D ImportLinear(string path, bool normal)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.wrapMode = TextureWrapMode.Repeat; imp.mipmapEnabled = true; imp.maxTextureSize = 2048;
            if (normal) imp.textureType = TextureImporterType.NormalMap;
            else imp.sRGBTexture = false;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Material Pbr(string name, Color tint, Texture2D albedoSmooth, Texture2D normal, Texture2D ao, float smoothScale, float metal, float aoStrength)
        {
            var m = Lit(name, tint, albedoSmooth, normal, smoothScale, metal, 1f);
            m.SetFloat("_SmoothnessTextureChannel", 1f);               // smoothness from albedo alpha
            m.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            m.SetTexture("_OcclusionMap", ao);
            m.SetFloat("_OcclusionStrength", aoStrength);
            if (ao) m.EnableKeyword("_OCCLUSIONMAP");
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material Unlit(string name, Color c, Texture2D tex)
        {
            string path = MatDir + "M_Env_" + name + ".mat";
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool isNew = m == null;
            if (isNew) m = new Material(shader); else m.shader = shader;
            m.SetColor("_BaseColor", c);
            m.SetTexture("_BaseMap", tex);
            m.enableInstancing = true;
            if (isNew) AssetDatabase.CreateAsset(m, path); else EditorUtility.SetDirty(m);
            return m;
        }

        static void Emissive(Material m, Texture2D map, Color c)
        {
            m.EnableKeyword("_EMISSION");
            m.SetTexture("_EmissionMap", map);
            m.SetColor("_EmissionColor", c);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(m);
        }

        static Texture2D SaveTex(string name, Color[] px, bool normal)
        {
            int size = Mathf.RoundToInt(Mathf.Sqrt(px.Length));
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.SetPixels(px);
            tex.Apply();
            string path = TexDir + name + ".png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.mipmapEnabled = true;
            imp.anisoLevel = 4;
            imp.maxTextureSize = size;
            if (normal)
            {
                imp.textureType = TextureImporterType.NormalMap;
                imp.convertToNormalmap = true;
                imp.heightmapScale = 0.04f;
            }
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // ───────────────────────── procedural textures ─────────────────────────

        static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 982451653;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0xffff) / 65535f;
            }
        }

        /// <summary>Tileable value noise: lattice wraps every `period` cells.</summary>
        static float VNoise(float x, float y, int period, int seed)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
            int Wrap(int v) => ((v % period) + period) % period;
            float a = Hash(Wrap(x0), Wrap(y0), seed), b = Hash(Wrap(x0 + 1), Wrap(y0), seed);
            float c = Hash(Wrap(x0), Wrap(y0 + 1), seed), d = Hash(Wrap(x0 + 1), Wrap(y0 + 1), seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        static float Fbm(float u, float v, int basePeriod, int octaves, int seed)
        {
            float sum = 0, amp = 0.5f, norm = 0; int p = basePeriod;
            for (int i = 0; i < octaves; i++)
            {
                sum += amp * VNoise(u * p, v * p, p, seed + i * 17);
                norm += amp; amp *= 0.5f; p *= 2;
            }
            return sum / norm;
        }

        /// <summary>Raw cast concrete: cloudy tone, fine grain, small pores, faint formwork seams.</summary>
        static Color[] ConcreteTexture(int n, int seed)
        {
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = x / (float)n, v = y / (float)n;
                float cloud = Fbm(u, v, 3, 4, seed);              // large blotches
                float grain = Fbm(u, v, 64, 3, seed + 101);       // fine grain
                float streak = Fbm(u * 0.5f, v, 8, 3, seed + 55); // vertical-ish streaks
                float g = 0.95f + (cloud - 0.5f) * 0.22f + (grain - 0.5f) * 0.10f + (streak - 0.5f) * 0.06f;
                // pores
                if (Hash(x, y, seed + 7) > 0.9975f) g -= 0.25f;
                // faint formwork seams at the tile border (half height, full width)
                int sx = Mathf.Min(x, n - 1 - x), sy = Mathf.Min(y, n - 1 - y);
                if (sx < 2 || sy < 2) g -= 0.03f;
                g = Mathf.Clamp01(g);
                px[y * n + x] = new Color(g, g, g * 0.99f, 1f);
            }
            return px;
        }

        /// <summary>Polished concrete floor: low-contrast mottling + sparse aggregate specks.</summary>
        static Color[] FloorTexture(int n, int seed)
        {
            var px = new Color[n * n];
            Color baseC = MuseumPalette.FloorPolished;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = x / (float)n, v = y / (float)n;
                float k = 1f + (Fbm(u, v, 4, 4, seed) - 0.5f) * 0.12f + (Fbm(u, v, 96, 2, seed + 9) - 0.5f) * 0.05f;
                if (Hash(x, y, seed + 3) > 0.998f) k -= 0.12f;
                px[y * n + x] = new Color(baseC.r * k, baseC.g * k, baseC.b * k * 1.01f, 1f);
            }
            return px;
        }

        /// <summary>One luminous ceiling panel: white field, thin seams, fixture dots at the corners.</summary>
        static Color[] CeilingTexture(int n)
        {
            var px = new Color[n * n];
            Color panel = MuseumPalette.CeilingPanel, seam = new Color(0.72f, 0.74f, 0.77f), dot = new Color(0.18f, 0.19f, 0.21f);
            int seamW = 3, dotR = 9;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                int dx = Mathf.Min(x, n - 1 - x), dy = Mathf.Min(y, n - 1 - y);
                Color c = panel;
                // soft falloff toward the seams so panels read as individual light boxes
                float edge = Mathf.Clamp01(Mathf.Min(dx, dy) / (n * 0.12f));
                c = Color.Lerp(panel * 0.9f, panel, edge);
                if (dx < seamW || dy < seamW) c = seam;
                if (dx < dotR && dy < dotR) c = dot;
                c.a = 1f;
                px[y * n + x] = c;
            }
            return px;
        }

        // ───────────────────────── geometry ─────────────────────────

        /// <summary>Box with world-scale UVs (texture density stays constant regardless of size).</summary>
        public static GameObject WorldBox(string name, Transform parent, Vector3 center, Vector3 size, Material mat, float tile, bool castShadow = false, bool isStatic = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = BoxMesh(size, tile, parent != null ? parent.TransformPoint(center) : center);
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = castShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;
            var bc = go.AddComponent<BoxCollider>();
            bc.size = size;
            if (isStatic)
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
            else
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ReflectionProbeStatic); // movable, but still seen by the baked probe
            return go;
        }

        /// <summary>UVs are projected from world position, so neighbouring boxes (e.g. floor tiles) continue the texture seamlessly.</summary>
        static Mesh BoxMesh(Vector3 s, float tile, Vector3 worldCenter)
        {
            Vector3 h = s * 0.5f;
            var verts = new System.Collections.Generic.List<Vector3>();
            var norms = new System.Collections.Generic.List<Vector3>();
            var uvs = new System.Collections.Generic.List<Vector2>();
            var tris = new System.Collections.Generic.List<int>();
            void Face(Vector3 n, Vector3 u, Vector3 v, float w, float hgt)
            {
                int i = verts.Count;
                Vector3 c = Vector3.Scale(n, h);
                Vector3 hu = u * (w * 0.5f), hv = v * (hgt * 0.5f);
                verts.Add(c - hu - hv); verts.Add(c + hu - hv); verts.Add(c + hu + hv); verts.Add(c - hu + hv);
                for (int k = 0; k < 4; k++) norms.Add(n);
                for (int k = verts.Count - 4; k < verts.Count; k++)
                {
                    Vector3 wp = verts[k] + worldCenter;
                    uvs.Add(new Vector2(Vector3.Dot(wp, u), Vector3.Dot(wp, v)) / tile);
                }
                tris.AddRange(new[] { i, i + 2, i + 1, i, i + 3, i + 2 });
            }
            Face(Vector3.forward, Vector3.left, Vector3.up, s.x, s.y);
            Face(Vector3.back, Vector3.right, Vector3.up, s.x, s.y);
            Face(Vector3.right, Vector3.forward, Vector3.up, s.z, s.y);
            Face(Vector3.left, Vector3.back, Vector3.up, s.z, s.y);
            Face(Vector3.up, Vector3.right, Vector3.forward, s.x, s.z);
            Face(Vector3.down, Vector3.right, Vector3.back, s.x, s.z);
            var m = new Mesh { name = $"Box_{s.x:0.##}x{s.y:0.##}x{s.z:0.##}" };
            m.SetVertices(verts); m.SetNormals(norms); m.SetUVs(0, uvs); m.SetTriangles(tris, 0);
            m.RecalculateBounds(); m.RecalculateTangents();
            return m;
        }

        /// <summary>Flush steel door set into a wall. `facing` = direction the door faces (into the room).</summary>
        public static GameObject Door(string name, Transform parent, Vector3 floorCenter, float yaw, float width = 1.0f, float height = 2.15f)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = floorCenter;
            root.localRotation = Quaternion.Euler(0, yaw, 0);
            var frame = Mat("ConcreteDark");
            WorldBox("Frame_L", root, new Vector3(-width / 2 - 0.02f, height / 2, 0.01f), new Vector3(0.04f, height, 0.04f), frame, ConcreteTile);
            WorldBox("Frame_R", root, new Vector3(width / 2 + 0.02f, height / 2, 0.01f), new Vector3(0.04f, height, 0.04f), frame, ConcreteTile);
            WorldBox("Frame_T", root, new Vector3(0, height + 0.02f, 0.01f), new Vector3(width + 0.08f, 0.04f, 0.04f), frame, ConcreteTile);
            WorldBox("Leaf", root, new Vector3(0, height / 2, 0.015f), new Vector3(width, height, 0.03f), Mat("Door"), 1f);
            var handle = WorldBox("Handle", root, new Vector3(width / 2 - 0.1f, 1.02f, 0.05f), new Vector3(0.14f, 0.022f, 0.022f), AssetDatabase.LoadAssetAtPath<Material>(MatDir + "M_Charcoal.mat"), 1f, true);
            Object.DestroyImmediate(handle.GetComponent<BoxCollider>());
            return root.gameObject;
        }

        /// <summary>Gallery lighting recipe: light comes from the luminous ceiling (cool, soft, top-down).</summary>
        public static Light ApplyGalleryLighting()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.95f, 0.96f, 0.99f);
            RenderSettings.ambientEquatorColor = new Color(0.82f, 0.83f, 0.85f);
            RenderSettings.ambientGroundColor = new Color(0.55f, 0.55f, 0.56f);
            RenderSettings.fog = false;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            var sun = new GameObject("Ceiling Light (Directional)").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = MuseumPalette.LightCoolWhite;
            sun.intensity = 0.5f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.55f;
            sun.transform.rotation = Quaternion.Euler(78f, 25f, 0f);
            DynamicGI.UpdateEnvironment(); // recompute the ambient probe so Trilight colors actually apply (no lightmap bake needed)
            return sun;
        }

        /// <summary>Box-projected reflection probe baked once (cheap on Quest; makes the polished floor read).</summary>
        public static ReflectionProbe BakeProbe(string name, Vector3 center, Vector3 size, string cubemapPath)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            var p = go.AddComponent<ReflectionProbe>();
            p.mode = ReflectionProbeMode.Custom;
            p.boxProjection = true;
            p.size = size + new Vector3(0.4f, 0.6f, 0.4f); // include floor/ceiling slabs so their bounds fall inside
            p.resolution = 256;
            p.hdr = true;
            p.clearFlags = ReflectionProbeClearFlags.SolidColor;
            p.backgroundColor = MuseumPalette.CeilingPanel;
            if (Lightmapping.BakeReflectionProbe(p, cubemapPath))
            {
                AssetDatabase.ImportAsset(cubemapPath);
                p.customBakedTexture = AssetDatabase.LoadAssetAtPath<Texture>(cubemapPath);
                // fallback for anything outside the box
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                RenderSettings.customReflectionTexture = p.customBakedTexture;
            }
            return p;
        }
    }
}
