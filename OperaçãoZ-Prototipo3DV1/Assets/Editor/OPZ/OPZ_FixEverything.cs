// Assets/Editor/OPZ/OPZ_FixEverything.cs
// ONE CLICK: fixes URP pipeline, cleans missing scripts, generates blockout map.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace OPZ.EditorTools
{
    public static class OPZ_FixEverything
    {
        [MenuItem("OPZ/FIX EVERYTHING (pipeline + map)", priority = 0)]
        public static void FixAll()
        {
            Debug.Log("[OPZ] === STARTING FULL FIX ===");

            // STEP 1: Create URP assets if missing
            FixURPPipeline();

            // STEP 2: Clean missing scripts from current scene
            CleanMissingScripts();

            // STEP 3: Generate blockout map into a fresh scene
            GenerateBlockoutScene();

            Debug.Log("[OPZ] === ALL DONE. Check the Scene view. ===");
        }

        [MenuItem("OPZ/Tools/Fix URP Pipeline Only")]
        static void FixURPPipeline()
        {
            // Ensure folders
            EnsureFolder("Assets/Settings");
            EnsureFolder("Assets/Settings/URP");

            const string rendererPath = "Assets/Settings/URP/OPZ_URP_Renderer.asset";
            const string pipelinePath = "Assets/Settings/URP/OPZ_URP_PipelineAsset.asset";

            // Create renderer if missing
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
                Debug.Log("[OPZ] Created URP Renderer at " + rendererPath);
            }

            // Create pipeline asset if missing
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
                Debug.Log("[OPZ] Created URP Pipeline Asset at " + pipelinePath);
            }

            // Assign to graphics settings
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[OPZ] URP Pipeline assigned to Graphics + Quality settings.");
        }

        [MenuItem("OPZ/Tools/Remove All Missing Scripts")]
        static void CleanMissingScripts()
        {
            int removed = 0;
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (count > 0)
                {
                    removed += count;
                    Debug.Log("[OPZ] Cleaned " + count + " missing script(s) from: " + go.name);
                }
            }
            Debug.Log("[OPZ] Total missing scripts removed: " + removed);
        }

        // ===================== BLOCKOUT GENERATOR =====================
        static void GenerateBlockoutScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("MAP_01_CinturaoDeRuina");
            SceneManager.MoveGameObjectToScene(root, scene);

            SetupLighting(scene);
            SetupCamera(scene);
            SetupGround(root.transform);
            SetupBiomes(root.transform);
            SetupBorders(root.transform);
            SetupRoads(root.transform);
            SetupRiver(root.transform);
            SetupCity(root.transform);
            SetupSpawns(root.transform);
            SetupRaidIsland(root.transform);
            SetupResources(root.transform);
            SetupPvEZones(root.transform);
            SetupHotspots(root.transform);

            EnsureFolder("Assets/Scenes");
            string path = "Assets/Scenes/MAP_01_CinturaoDeRuina_Blockout.unity";
            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.SaveAssets();
            Debug.Log("[OPZ] Blockout scene saved at: " + path);
        }

        // ---------- LIGHTING ----------
        static void SetupLighting(Scene scene)
        {
            var go = new GameObject("Directional Light");
            SceneManager.MoveGameObjectToScene(go, scene);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.96f, 0.88f);
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.sun = light;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.58f);
        }

        // ---------- CAMERA ----------
        static void SetupCamera(Scene scene)
        {
            var go = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            // Add URP camera data
            go.AddComponent<UniversalAdditionalCameraData>();
            cam.transform.position = new Vector3(0f, 165f, -78f);
            cam.transform.rotation = Quaternion.Euler(62f, 0f, 0f);
            cam.fieldOfView = 42f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 600f;
        }

        // ---------- GROUND ----------
        static void SetupGround(Transform p)
        {
            Box("Ground_Base", p, new Vector3(0, -0.5f, 0), new Vector3(300, 1, 300), C(0.22f, 0.22f, 0.22f));
        }

        // ---------- BIOMES ----------
        static void SetupBiomes(Transform p)
        {
            var g = Group("Biomes", p);
            Box("Biome_Deserto_NW",  g, new Vector3(-78, 0.05f, 78),  new Vector3(130, 0.2f, 130), C(0.61f, 0.53f, 0.35f));
            Box("Biome_Planicie_NE", g, new Vector3(78, 0.05f, 78),   new Vector3(130, 0.2f, 130), C(0.41f, 0.50f, 0.31f));
            Box("Biome_Floresta_SW", g, new Vector3(-78, 0.05f, -78), new Vector3(130, 0.2f, 130), C(0.18f, 0.29f, 0.18f));
            Box("Biome_Pantano_SE",  g, new Vector3(78, 0.05f, -78),  new Vector3(130, 0.2f, 130), C(0.20f, 0.31f, 0.27f));
            Box("Transition_Belt",   g, new Vector3(0, 0.09f, 0),     new Vector3(128, 0.1f, 128), C(0.32f, 0.30f, 0.28f));
        }

        // ---------- BORDERS ----------
        static void SetupBorders(Transform p)
        {
            var g = Group("Borders", p);
            var c = C(0.11f, 0.11f, 0.11f);
            Box("N", g, new Vector3(0, 4, 152),    new Vector3(270, 8, 6), c);
            Box("S", g, new Vector3(0, 4, -152),   new Vector3(270, 8, 6), c);
            Box("E", g, new Vector3(152, 4, 0),    new Vector3(6, 8, 270), c);
            Box("W", g, new Vector3(-152, 4, 0),   new Vector3(6, 8, 270), c);
        }

        // ---------- ROADS ----------
        static void SetupRoads(Transform p)
        {
            var g = Group("Roads", p);
            var c = C(0.12f, 0.12f, 0.12f);
            Box("Road_N_ToCity",    g, new Vector3(-35, 0.08f, 78),  new Vector3(18, 0.08f, 104), c);
            Box("Road_S_ToCity",    g, new Vector3(35, 0.08f, -78),  new Vector3(18, 0.08f, 104), c);
            Box("Ring_N",           g, new Vector3(0, 0.08f, 118),   new Vector3(180, 0.08f, 12), c);
            Box("Ring_S",           g, new Vector3(0, 0.08f, -118),  new Vector3(180, 0.08f, 12), c);
            Box("Ring_W",           g, new Vector3(-118, 0.08f, 0),  new Vector3(12, 0.08f, 180), c);
            Box("Ring_E",           g, new Vector3(118, 0.08f, 0),   new Vector3(12, 0.08f, 180), c);
            Box("City_NS",          g, new Vector3(0, 0.09f, 0),     new Vector3(12, 0.09f, 92),  c);
            Box("City_EW",          g, new Vector3(0, 0.09f, 0),     new Vector3(92, 0.09f, 12),  c);
        }

        // ---------- RIVER ----------
        static void SetupRiver(Transform p)
        {
            var g = Group("River_Bridges", p);
            var w = C(0.17f, 0.33f, 0.52f);
            var b = C(0.38f, 0.28f, 0.18f);
            BoxR("River_01", g, new Vector3(78, 0.02f, 96),    new Vector3(54, 0.06f, 16),  w, 0.86f, 22);
            BoxR("River_02", g, new Vector3(36, 0.02f, 52),    new Vector3(66, 0.06f, 16),  w, 0.86f, 36);
            BoxR("River_03", g, new Vector3(0, 0.02f, -6),     new Vector3(84, 0.06f, 20),  w, 0.86f, 8);
            BoxR("River_04", g, new Vector3(-48, 0.02f, -56),  new Vector3(72, 0.06f, 16),  w, 0.86f, 28);
            BoxR("River_05", g, new Vector3(-92, 0.02f, -100), new Vector3(54, 0.06f, 14),  w, 0.86f, 8);
            Box("Bridge_South",  g, new Vector3(16, 0.22f, -18),  new Vector3(14, 0.35f, 8),  b);
            Box("Bridge_East",   g, new Vector3(38, 0.22f, 8),    new Vector3(8, 0.35f, 14),  b);
            Box("Bridge_Forest", g, new Vector3(-56, 0.22f, -44), new Vector3(8, 0.35f, 12),  b);
        }

        // ---------- CITY ----------
        static void SetupCity(Transform p)
        {
            var g = Group("City_NovaCinza", p);
            var bc = C(0.30f, 0.30f, 0.31f);
            Vector3[] pos = {
                new(-24,2,24), new(-8,2.4f,24), new(8,2.2f,24), new(24,2,24),
                new(-24,2.2f,8), new(24,2.6f,8),
                new(-24,2.1f,-8), new(24,2.1f,-8),
                new(-24,2.4f,-24), new(-8,2.1f,-24), new(8,2.7f,-24), new(24,2.2f,-24),
                new(-8,2.1f,8), new(8,2.4f,-8)
            };
            for (int i = 0; i < pos.Length; i++)
            {
                var sz = new Vector3(Seed(8,12,i), Seed(4,10,i+10), Seed(8,12,i+20));
                Box("Block_"+i.ToString("00"), g, pos[i], sz, bc);
            }
            Box("Plaza", g, new Vector3(0, 0.12f, 0), new Vector3(20, 0.18f, 20), C(0.45f, 0.42f, 0.38f));
        }

        // ---------- SPAWNS ----------
        static void SetupSpawns(Transform p)
        {
            var g = Group("Spawns", p);
            Spawn(g, "Spawn_AR", new Vector3(-112, 0.15f, 112), C(0.18f, 0.42f, 0.86f), true);
            Spawn(g, "Spawn_EG", new Vector3(112, 0.15f, -112), C(0.85f, 0.20f, 0.20f), false);
        }

        static void Spawn(Transform p, string name, Vector3 pos, Color col, bool isAR)
        {
            var r = Group(name, p); r.localPosition = pos;
            Box("Pad", r, Vector3.zero, new Vector3(30, 0.2f, 30), Color.Lerp(col, Color.gray, 0.65f));
            var t = Group("Tent", r); t.localPosition = new Vector3(0, 0.75f, 0);
            Box("Base",  t, Vector3.zero, new Vector3(8, 1.5f, 6), C(0.28f, 0.28f, 0.28f));
            BoxR("RoofA", t, new Vector3(0, 1.05f, -0.95f), new Vector3(8, 0.25f, 3), col, 1, 22);
            BoxR("RoofB", t, new Vector3(0, 1.05f, 0.95f),  new Vector3(8, 0.25f, 3), col, 1, -22);
            float d = isAR ? 1 : -1;
            Box("Exit1", r, new Vector3(d*14, 0.05f, -d*14), new Vector3(8, 0.1f, 18), C(0.16f, 0.16f, 0.16f));
            Box("Exit2", r, new Vector3(d*18, 0.05f, 2), new Vector3(8, 0.1f, 14), C(0.18f, 0.18f, 0.18f));
        }

        // ---------- RAID ISLAND ----------
        static void SetupRaidIsland(Transform p)
        {
            var g = Group("Raid_Island", p);
            Box("Water",    g, new Vector3(116, 0.01f, 36), new Vector3(58, 0.05f, 74), C(0.12f, 0.29f, 0.47f), 0.82f);
            Box("Island",   g, new Vector3(118, 0.2f, 38),  new Vector3(25, 0.4f, 20),  C(0.30f, 0.27f, 0.22f));
            Box("Causeway", g, new Vector3(102, 0.22f, 34), new Vector3(14, 0.25f, 6),  C(0.37f, 0.30f, 0.22f));
        }

        // ---------- RESOURCES ----------
        static void SetupResources(Transform p)
        {
            var g = Group("Resources", p);
            Res(g, new Vector3(-96,0.3f,98),   "AR_Supplies_A",     C(0.74f,0.61f,0.19f));
            Res(g, new Vector3(-124,0.3f,96),  "AR_Supplies_B",     C(0.74f,0.61f,0.19f));
            Res(g, new Vector3(-112,0.3f,84),  "AR_Metal",          C(0.56f,0.56f,0.62f));
            Res(g, new Vector3(96,0.3f,-98),   "EG_Supplies_A",     C(0.74f,0.61f,0.19f));
            Res(g, new Vector3(124,0.3f,-96),  "EG_Supplies_B",     C(0.74f,0.61f,0.19f));
            Res(g, new Vector3(112,0.3f,-84),  "EG_Metal",          C(0.56f,0.56f,0.62f));
            Res(g, new Vector3(-58,0.3f,42),   "Mid_Metal_NW",      C(0.65f,0.65f,0.72f));
            Res(g, new Vector3(58,0.3f,-42),   "Mid_Fuel_SE",       C(0.23f,0.66f,0.74f));
            Res(g, new Vector3(68,0.3f,88),    "Mid_Fuel_NE",       C(0.23f,0.66f,0.74f));
            Res(g, new Vector3(-76,0.3f,-76),  "Mid_Metal_SW",      C(0.56f,0.56f,0.62f));
            Res(g, new Vector3(0,0.3f,0),      "Plaza_Supplies",    C(0.94f,0.78f,0.21f));
            Res(g, new Vector3(-16,0.3f,14),   "City_Metal",        C(0.70f,0.70f,0.80f));
            Res(g, new Vector3(16,0.3f,-14),   "City_Fuel",         C(0.25f,0.71f,0.80f));
            Res(g, new Vector3(118,0.3f,38),   "Raid_HighValue",    C(0.97f,0.54f,0.18f));
        }

        static void Res(Transform p, Vector3 pos, string name, Color col)
        {
            var r = Group(name, p); r.localPosition = pos;
            for (int i = 0; i < 3; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                c.name = "Piece_" + i;
                c.transform.SetParent(r);
                c.transform.localPosition = new Vector3((i-1)*1.5f, 0, i%2==0?1:-1);
                c.transform.localScale = new Vector3(0.85f, 0.6f+i*0.15f, 0.85f);
                SetColor(c, col);
            }
        }

        // ---------- PVE ----------
        static void SetupPvEZones(Transform p)
        {
            var g = Group("PvE_Zones", p);
            Box("PvE_Forest", g, new Vector3(-84,0.2f,-58), new Vector3(34,0.2f,28), C(0.72f,0.31f,0.19f), 0.22f);
            Box("PvE_Swamp",  g, new Vector3(92,0.2f,-56),  new Vector3(34,0.2f,28), C(0.72f,0.31f,0.19f), 0.22f);
            Box("PvE_Plaza",  g, new Vector3(0,0.2f,0),     new Vector3(26,0.2f,26), C(0.89f,0.19f,0.19f), 0.22f);
            Box("PvE_Raid",   g, new Vector3(118,0.2f,38),  new Vector3(21,0.2f,16), C(0.82f,0.21f,0.21f), 0.22f);
        }

        // ---------- HOTSPOTS ----------
        static void SetupHotspots(Transform p)
        {
            var g = Group("Hotspots_Chokes", p);
            Empty("Choke_PonteSul",       g, new Vector3(16, 0.2f, -18));
            Empty("Choke_PonteLeste",     g, new Vector3(38, 0.2f, 8));
            Empty("Hotspot_Florestal",    g, new Vector3(-56, 0.2f, -44));
            Empty("Hotspot_FazendaNE",    g, new Vector3(62, 0.2f, 72));
            Empty("Choke_CausewayRaid",   g, new Vector3(102, 0.2f, 34));
        }

        // ===================== HELPERS =====================
        static Transform Group(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            return go.transform;
        }

        static void Empty(string name, Transform parent, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = pos;
        }

        static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 scale, Color col, float alpha = 1f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            SetColor(go, col, alpha);
            return go;
        }

        static GameObject BoxR(string name, Transform parent, Vector3 pos, Vector3 scale, Color col, float alpha, float yRot)
        {
            var go = Box(name, parent, pos, scale, col, alpha);
            go.transform.localRotation = Quaternion.Euler(0, yRot, 0);
            return go;
        }

        static void SetColor(GameObject go, Color col, float alpha = 1f)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            var m = new Material(s);
            m.color = new Color(col.r, col.g, col.b, alpha);
            r.sharedMaterial = m;
        }

        static Color C(float r, float g, float b) => new Color(r, g, b);

        static float Seed(float min, float max, int seed)
        {
            var st = Random.state;
            Random.InitState(15437 + seed * 977);
            float v = Random.Range(min, max);
            Random.state = st;
            return v;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
