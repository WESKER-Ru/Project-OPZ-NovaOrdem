using System.Collections.Generic;
using OPZ.Core;
using OPZ.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OPZ.EditorTools
{
    /// <summary>
    /// Generates a playable graybox/blockout scene for the MAP_01 document.
    /// It does not try to ship final art; it creates spatial logic, spawn pads,
    /// biomes, river crossings, the central city, basic roads and tactical markers.
    /// </summary>
    public static class CinturaoDeRuinaBlockoutGenerator
    {
        private const float MapSize = 300f;
        private const float GroundY = 0f;
        private const string SceneFolder = "Assets/Scenes";
        private const string ScenePath = "Assets/Scenes/MAP_01_CinturaoDeRuina_Blockout.unity";

        [MenuItem("OPZ/Maps/Create Cinturao de Ruina Blockout Scene")]
        public static void CreateNewSceneAndGenerate()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[OPZ] Não é possível criar a scene do blockout durante o Play Mode. Saia do Play e tente novamente.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            GenerateIntoScene(scene);
            EnsureFolder(SceneFolder);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"OPZ blockout scene created at: {ScenePath}");
        }

        [MenuItem("OPZ/Maps/Generate Cinturao de Ruina Into Current Scene")]
        public static void GenerateIntoCurrentScene()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[OPZ] Não é possível gerar o mapa durante o Play Mode. Saia do Play e tente novamente.");
                return;
            }

            GenerateIntoScene(SceneManager.GetActiveScene());
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void GenerateIntoScene(Scene scene)
        {
            var oldRoot = GameObject.Find("MAP_01_CinturaoDeRuina");
            if (oldRoot != null)
            {
                Object.DestroyImmediate(oldRoot);
            }

            var root = new GameObject("MAP_01_CinturaoDeRuina");
            MoveToScene(root, scene);

            SetupLighting(scene);
            SetupPlayableRTSCamera(scene);
            SetupGround(root.transform);
            SetupBiomePlates(root.transform);
            SetupBorderBlockers(root.transform);
            SetupRoadNetwork(root.transform);
            SetupRiverAndBridges(root.transform);
            SetupCity(root.transform);
            SetupSpawns(root.transform);
            SetupRaidIsland(root.transform);
            SetupResources(root.transform);
            SetupPvEMarkers(root.transform);
            SetupHotspots(root.transform);
        }

        private static void SetupLighting(Scene scene)
        {
            var light = RenderSettings.sun;
            if (light == null)
            {
                var go = new GameObject("Directional Light");
                MoveToScene(go, scene);
                var dir = go.AddComponent<Light>();
                dir.type = LightType.Directional;
                dir.intensity = 1.1f;
                go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                RenderSettings.sun = dir;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.58f);
        }

        private static void SetupPlayableRTSCamera(Scene scene)
        {
            Camera cam = Camera.main;

            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                MoveToScene(go, scene);
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }

            cam.transform.position = new Vector3(0f, 88f, -68f);
            cam.transform.rotation = Quaternion.Euler(54f, 0f, 0f);
            cam.orthographic = false;
            cam.fieldOfView = 46f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 600f;

            if (cam.GetComponent<AudioListener>() == null)
                cam.gameObject.AddComponent<AudioListener>();

            var controller = cam.GetComponent<RTSCameraController>();
            if (controller == null)
                controller = cam.gameObject.AddComponent<RTSCameraController>();

            SetPrivateField(controller, "mapMin", new Vector2(-145f, -145f));
            SetPrivateField(controller, "mapMax", new Vector2(145f, 145f));
            SetPrivateField(controller, "minHeight", 30f);
            SetPrivateField(controller, "maxHeight", 105f);
            SetPrivateField(controller, "minPitch", 48f);
            SetPrivateField(controller, "maxPitch", 62f);
        }

        private static void SetupGround(Transform parent)
        {
            CreateBox(
                "Ground_Base",
                parent,
                new Vector3(0f, GroundY - 0.5f, 0f),
                new Vector3(MapSize, 1f, MapSize),
                new Color(0.22f, 0.22f, 0.22f));
        }

        private static void SetupBiomePlates(Transform parent)
        {
            var biomes = new GameObject("Biomes");
            biomes.transform.SetParent(parent);
            biomes.transform.localPosition = Vector3.zero;

            CreateBiomePlate(biomes.transform, "Biome_Deserto_NW", new Vector3(-78f, 0.05f, 78f), new Vector3(130f, 0.2f, 130f), new Color(0.61f, 0.53f, 0.35f));
            CreateBiomePlate(biomes.transform, "Biome_Planicie_NE", new Vector3(78f, 0.05f, 78f), new Vector3(130f, 0.2f, 130f), new Color(0.41f, 0.50f, 0.31f));
            CreateBiomePlate(biomes.transform, "Biome_Floresta_SW", new Vector3(-78f, 0.05f, -78f), new Vector3(130f, 0.2f, 130f), new Color(0.18f, 0.29f, 0.18f));
            CreateBiomePlate(biomes.transform, "Biome_Pantano_SE", new Vector3(78f, 0.05f, -78f), new Vector3(130f, 0.2f, 130f), new Color(0.20f, 0.31f, 0.27f));

            CreateBox(
                "Transition_Belt",
                biomes.transform,
                new Vector3(0f, 0.09f, 0f),
                new Vector3(128f, 0.1f, 128f),
                new Color(0.32f, 0.30f, 0.28f));
        }

        private static void CreateBiomePlate(Transform parent, string name, Vector3 pos, Vector3 size, Color color)
        {
            var go = CreateBox(name, parent, pos, size, color);
            var marker = go.AddComponent<OPZMapMarker>();
            marker.displayName = name;
            marker.markerType = OPZMarkerType.Biome;
            marker.gizmoColor = color;
            marker.gizmoSize = new Vector3(size.x, 2f, size.z);
        }

        private static void SetupBorderBlockers(Transform parent)
        {
            var borders = new GameObject("Border_Blockers");
            borders.transform.SetParent(parent);

            var edgeColor = new Color(0.11f, 0.11f, 0.11f);
            CreateBox("North_Edge", borders.transform, new Vector3(0f, 4f, 152f), new Vector3(270f, 8f, 6f), edgeColor);
            CreateBox("South_Edge", borders.transform, new Vector3(0f, 4f, -152f), new Vector3(270f, 8f, 6f), edgeColor);
            CreateBox("East_Edge", borders.transform, new Vector3(152f, 4f, 0f), new Vector3(6f, 8f, 270f), edgeColor);
            CreateBox("West_Edge", borders.transform, new Vector3(-152f, 4f, 0f), new Vector3(6f, 8f, 270f), edgeColor);
        }

        private static void SetupRoadNetwork(Transform parent)
        {
            var roads = new GameObject("Roads");
            roads.transform.SetParent(parent);

            var roadColor = new Color(0.12f, 0.12f, 0.12f);
            CreateBox("Road_NorthToCity", roads.transform, new Vector3(-35f, 0.08f, 78f), new Vector3(18f, 0.08f, 104f), roadColor);
            CreateBox("Road_SouthToCity", roads.transform, new Vector3(35f, 0.08f, -78f), new Vector3(18f, 0.08f, 104f), roadColor);
            CreateBox("Road_OuterRing_North", roads.transform, new Vector3(0f, 0.08f, 118f), new Vector3(180f, 0.08f, 12f), roadColor);
            CreateBox("Road_OuterRing_South", roads.transform, new Vector3(0f, 0.08f, -118f), new Vector3(180f, 0.08f, 12f), roadColor);
            CreateBox("Road_OuterRing_West", roads.transform, new Vector3(-118f, 0.08f, 0f), new Vector3(12f, 0.08f, 180f), roadColor);
            CreateBox("Road_OuterRing_East", roads.transform, new Vector3(118f, 0.08f, 0f), new Vector3(12f, 0.08f, 180f), roadColor);
            CreateBox("Road_City_NS", roads.transform, new Vector3(0f, 0.09f, 0f), new Vector3(12f, 0.09f, 92f), roadColor);
            CreateBox("Road_City_EW", roads.transform, new Vector3(0f, 0.09f, 0f), new Vector3(92f, 0.09f, 12f), roadColor);
        }

        private static void SetupRiverAndBridges(Transform parent)
        {
            var river = new GameObject("River_And_Bridges");
            river.transform.SetParent(parent);

            var waterColor = new Color(0.17f, 0.33f, 0.52f);
            var bridgeColor = new Color(0.38f, 0.28f, 0.18f);

            CreateBox("River_Segment_01", river.transform, new Vector3(78f, 0.02f, 96f), new Vector3(54f, 0.06f, 16f), waterColor, 0.86f, 22f);
            CreateBox("River_Segment_02", river.transform, new Vector3(36f, 0.02f, 52f), new Vector3(66f, 0.06f, 16f), waterColor, 0.86f, 36f);
            CreateBox("River_Segment_03", river.transform, new Vector3(0f, 0.02f, -6f), new Vector3(84f, 0.06f, 20f), waterColor, 0.86f, 8f);
            CreateBox("River_Segment_04", river.transform, new Vector3(-48f, 0.02f, -56f), new Vector3(72f, 0.06f, 16f), waterColor, 0.86f, 28f);
            CreateBox("River_Segment_05", river.transform, new Vector3(-92f, 0.02f, -100f), new Vector3(54f, 0.06f, 14f), waterColor, 0.86f, 8f);

            var southBridge = CreateBox("Bridge_South", river.transform, new Vector3(16f, 0.22f, -18f), new Vector3(14f, 0.35f, 8f), bridgeColor);
            AddMarker(southBridge, "Ponte Sul", OPZMarkerType.Bridge, bridgeColor, new Vector3(16f, 3f, 10f));

            var eastBridge = CreateBox("Bridge_East", river.transform, new Vector3(38f, 0.22f, 8f), new Vector3(8f, 0.35f, 14f), bridgeColor);
            AddMarker(eastBridge, "Ponte Leste", OPZMarkerType.Bridge, bridgeColor, new Vector3(10f, 3f, 16f));

            var forestBridge = CreateBox("Bridge_Forest", river.transform, new Vector3(-56f, 0.22f, -44f), new Vector3(8f, 0.35f, 12f), bridgeColor);
            AddMarker(forestBridge, "Ponte Florestal", OPZMarkerType.Bridge, bridgeColor, new Vector3(10f, 3f, 14f));
        }

        private static void SetupCity(Transform parent)
        {
            var city = new GameObject("City_NovaCinza");
            city.transform.SetParent(parent);

            var roadlessColor = new Color(0.30f, 0.30f, 0.31f);
            var blockPositions = new List<Vector3>
            {
                new Vector3(-24f, 2f, 24f), new Vector3(-8f, 2.4f, 24f), new Vector3(8f, 2.2f, 24f), new Vector3(24f, 2f, 24f),
                new Vector3(-24f, 2.2f, 8f), new Vector3(24f, 2.6f, 8f),
                new Vector3(-24f, 2.1f, -8f), new Vector3(24f, 2.1f, -8f),
                new Vector3(-24f, 2.4f, -24f), new Vector3(-8f, 2.1f, -24f), new Vector3(8f, 2.7f, -24f), new Vector3(24f, 2.2f, -24f),
                new Vector3(-8f, 2.1f, 8f), new Vector3(8f, 2.4f, -8f)
            };

            int idx = 0;
            foreach (var pos in blockPositions)
            {
                var size = new Vector3(RandomRange(8f, 12f, idx), RandomRange(4f, 10f, idx + 10), RandomRange(8f, 12f, idx + 20));
                var block = CreateBox($"CityBlock_{idx:00}", city.transform, pos, size, roadlessColor);
                AddMarker(block, $"Bloco Urbano {idx:00}", OPZMarkerType.CityBlock, roadlessColor, size + new Vector3(1f, 0.5f, 1f));
                idx++;
            }

            var plaza = CreateBox("City_Plaza", city.transform, new Vector3(0f, 0.12f, 0f), new Vector3(20f, 0.18f, 20f), new Color(0.45f, 0.42f, 0.38f));
            AddMarker(plaza, "Praça Central", OPZMarkerType.Hotspot, new Color(0.90f, 0.78f, 0.21f), new Vector3(22f, 2f, 22f));
        }

        private static void SetupSpawns(Transform parent)
        {
            var spawns = new GameObject("Spawns");
            spawns.transform.SetParent(parent);

            CreateSpawnArea(spawns.transform, "Spawn_AR", new Vector3(-112f, 0.15f, 112f), new Color(0.18f, 0.42f, 0.86f));
            CreateSpawnArea(spawns.transform, "Spawn_EG", new Vector3(112f, 0.15f, -112f), new Color(0.85f, 0.20f, 0.20f));
        }

        private static void CreateSpawnArea(Transform parent, string name, Vector3 center, Color color)
        {
            var spawnRoot = new GameObject(name);
            spawnRoot.transform.SetParent(parent);
            spawnRoot.transform.localPosition = center;

            var pad = CreateBox("BuildPad", spawnRoot.transform, Vector3.zero, new Vector3(30f, 0.2f, 30f), Color.Lerp(color, Color.gray, 0.65f));
            AddMarker(pad, name, OPZMarkerType.Spawn, color, new Vector3(32f, 2f, 32f));

            CreateTentPlaceholder(spawnRoot.transform, Vector3.zero, color);
            CreateBox("Exit_Primary", spawnRoot.transform, new Vector3(name.Contains("AR") ? 14f : -14f, 0.05f, name.Contains("AR") ? -14f : 14f), new Vector3(8f, 0.1f, 18f), new Color(0.16f, 0.16f, 0.16f));
            CreateBox("Exit_Secondary", spawnRoot.transform, new Vector3(name.Contains("AR") ? 18f : -18f, 0.05f, 2f), new Vector3(8f, 0.1f, 14f), new Color(0.18f, 0.18f, 0.18f));
        }

        private static void CreateTentPlaceholder(Transform parent, Vector3 localPos, Color accent)
        {
            var tentRoot = new GameObject("InitialTent");
            tentRoot.transform.SetParent(parent);
            tentRoot.transform.localPosition = localPos + new Vector3(0f, 0.75f, 0f);

            CreateBox("Tent_Base", tentRoot.transform, Vector3.zero, new Vector3(8f, 1.5f, 6f), new Color(0.28f, 0.28f, 0.28f));
            CreateBox("Tent_Roof_A", tentRoot.transform, new Vector3(0f, 1.05f, -0.95f), new Vector3(8f, 0.25f, 3f), accent, 1f, 22f);
            CreateBox("Tent_Roof_B", tentRoot.transform, new Vector3(0f, 1.05f, 0.95f), new Vector3(8f, 0.25f, 3f), accent, 1f, -22f);
        }

        private static void SetupRaidIsland(Transform parent)
        {
            var raid = new GameObject("Raid_Island_Reservatorio");
            raid.transform.SetParent(parent);

            CreateBox("Reservatorio_Water", raid.transform, new Vector3(116f, 0.01f, 36f), new Vector3(58f, 0.05f, 74f), new Color(0.12f, 0.29f, 0.47f), 0.82f);
            CreateBox("Raid_Island", raid.transform, new Vector3(118f, 0.2f, 38f), new Vector3(25f, 0.4f, 20f), new Color(0.30f, 0.27f, 0.22f));
            var causeway = CreateBox("Raid_Causeway", raid.transform, new Vector3(102f, 0.22f, 34f), new Vector3(14f, 0.25f, 6f), new Color(0.37f, 0.30f, 0.22f));
            AddMarker(causeway, "Causeway Raid", OPZMarkerType.Choke, new Color(0.83f, 0.59f, 0.15f), new Vector3(15f, 3f, 8f));

            var island = GameObject.Find("Raid_Island");
            AddMarker(island, "Raid Island", OPZMarkerType.Objective, new Color(0.94f, 0.64f, 0.17f), new Vector3(26f, 2f, 21f));
        }

        private static void SetupResources(Transform parent)
        {
            var resources = new GameObject("Resources");
            resources.transform.SetParent(parent);

            CreateResourceCluster(resources.transform, new Vector3(-96f, 0.3f, 98f), "AR_Safe_Supplies_A", new Color(0.74f, 0.61f, 0.19f));
            CreateResourceCluster(resources.transform, new Vector3(-124f, 0.3f, 96f), "AR_Safe_Supplies_B", new Color(0.74f, 0.61f, 0.19f));
            CreateResourceCluster(resources.transform, new Vector3(-112f, 0.3f, 84f), "AR_Safe_Metal", new Color(0.56f, 0.56f, 0.62f));

            CreateResourceCluster(resources.transform, new Vector3(96f, 0.3f, -98f), "EG_Safe_Supplies_A", new Color(0.74f, 0.61f, 0.19f));
            CreateResourceCluster(resources.transform, new Vector3(124f, 0.3f, -96f), "EG_Safe_Supplies_B", new Color(0.74f, 0.61f, 0.19f));
            CreateResourceCluster(resources.transform, new Vector3(112f, 0.3f, -84f), "EG_Safe_Metal", new Color(0.56f, 0.56f, 0.62f));

            CreateResourceCluster(resources.transform, new Vector3(-58f, 0.3f, 42f), "Suburb_Metal_NW", new Color(0.65f, 0.65f, 0.72f));
            CreateResourceCluster(resources.transform, new Vector3(58f, 0.3f, -42f), "Suburb_Fuel_SE", new Color(0.23f, 0.66f, 0.74f));
            CreateResourceCluster(resources.transform, new Vector3(68f, 0.3f, 88f), "Plain_Fuel_NE", new Color(0.23f, 0.66f, 0.74f));
            CreateResourceCluster(resources.transform, new Vector3(-76f, 0.3f, -76f), "Forest_Metal_SW", new Color(0.56f, 0.56f, 0.62f));
            CreateResourceCluster(resources.transform, new Vector3(0f, 0.3f, 0f), "Plaza_Premium_Supplies", new Color(0.94f, 0.78f, 0.21f));
            CreateResourceCluster(resources.transform, new Vector3(-16f, 0.3f, 14f), "City_Depot_Metal", new Color(0.70f, 0.70f, 0.80f));
            CreateResourceCluster(resources.transform, new Vector3(16f, 0.3f, -14f), "City_Treatment_Fuel", new Color(0.25f, 0.71f, 0.80f));
            CreateResourceCluster(resources.transform, new Vector3(118f, 0.3f, 38f), "RaidIsland_HighValue", new Color(0.97f, 0.54f, 0.18f));
        }

        private static void CreateResourceCluster(Transform parent, Vector3 position, string label, Color color)
        {
            var root = new GameObject(label);
            root.transform.SetParent(parent);
            root.transform.localPosition = position;

            for (int i = 0; i < 3; i++)
            {
                var piece = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                piece.name = $"Piece_{i}";
                piece.transform.SetParent(root.transform);
                piece.transform.localPosition = new Vector3((i - 1) * 1.5f, 0f, (i % 2 == 0 ? 1f : -1f));
                piece.transform.localScale = new Vector3(0.85f, 0.6f + i * 0.15f, 0.85f);
                ApplyColor(piece, color);
            }

            AddMarker(root, label, OPZMarkerType.Resource, color, new Vector3(6f, 3f, 6f));
        }

        private static void SetupPvEMarkers(Transform parent)
        {
            var pve = new GameObject("PvE_Pressure_Zones");
            pve.transform.SetParent(parent);

            CreatePvEZone(pve.transform, "PvE_Floresta", new Vector3(-84f, 0.2f, -58f), new Vector3(34f, 0.2f, 28f), new Color(0.72f, 0.31f, 0.19f));
            CreatePvEZone(pve.transform, "PvE_Pantano", new Vector3(92f, 0.2f, -56f), new Vector3(34f, 0.2f, 28f), new Color(0.72f, 0.31f, 0.19f));
            CreatePvEZone(pve.transform, "PvE_Plaza", new Vector3(0f, 0.2f, 0f), new Vector3(26f, 0.2f, 26f), new Color(0.89f, 0.19f, 0.19f));
            CreatePvEZone(pve.transform, "PvE_RaidIsland", new Vector3(118f, 0.2f, 38f), new Vector3(21f, 0.2f, 16f), new Color(0.82f, 0.21f, 0.21f));
        }

        private static void CreatePvEZone(Transform parent, string name, Vector3 pos, Vector3 size, Color color)
        {
            var zone = CreateBox(name, parent, pos, size, color, 0.22f);
            var marker = zone.AddComponent<OPZMapMarker>();
            marker.displayName = name;
            marker.markerType = OPZMarkerType.PvE;
            marker.gizmoColor = color;
            marker.gizmoSize = new Vector3(size.x, 3f, size.z);
            marker.drawWireCube = true;
            marker.drawSolidSphere = false;
        }

        private static void SetupHotspots(Transform parent)
        {
            var hotspots = new GameObject("Hotspots_And_Chokes");
            hotspots.transform.SetParent(parent);

            CreateMarkerOnly(hotspots.transform, "Hotspot_PonteSul", new Vector3(16f, 0.2f, -18f), OPZMarkerType.Choke, new Color(0.90f, 0.64f, 0.16f), new Vector3(14f, 3f, 8f));
            CreateMarkerOnly(hotspots.transform, "Hotspot_PonteLeste", new Vector3(38f, 0.2f, 8f), OPZMarkerType.Choke, new Color(0.90f, 0.64f, 0.16f), new Vector3(10f, 3f, 14f));
            CreateMarkerOnly(hotspots.transform, "Hotspot_CrossFlorestal", new Vector3(-56f, 0.2f, -44f), OPZMarkerType.Hotspot, new Color(0.84f, 0.74f, 0.20f), new Vector3(16f, 3f, 16f));
            CreateMarkerOnly(hotspots.transform, "Hotspot_FazendaNE", new Vector3(62f, 0.2f, 72f), OPZMarkerType.Hotspot, new Color(0.84f, 0.74f, 0.20f), new Vector3(18f, 3f, 18f));
            CreateMarkerOnly(hotspots.transform, "Choke_CausewayRaid", new Vector3(102f, 0.2f, 34f), OPZMarkerType.Choke, new Color(0.97f, 0.65f, 0.18f), new Vector3(14f, 3f, 8f));
        }

        private static void CreateMarkerOnly(Transform parent, string name, Vector3 position, OPZMarkerType type, Color color, Vector3 gizmo)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = position;

            var marker = go.AddComponent<OPZMapMarker>();
            marker.displayName = name;
            marker.markerType = type;
            marker.gizmoColor = color;
            marker.gizmoSize = gizmo;
            marker.drawSolidSphere = true;
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Color color, float alpha = 1f, float yRotation = 0f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
            go.transform.localScale = localScale;
            ApplyColor(go, color, alpha);
            return go;
        }

        private static void ApplyColor(GameObject go, Color color, float alpha = 1f)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader);
            var c = new Color(color.r, color.g, color.b, alpha);
            material.color = c;
            renderer.sharedMaterial = material;
        }

        private static void AddMarker(GameObject go, string displayName, OPZMarkerType type, Color color, Vector3 gizmoSize)
        {
            if (go == null) return;

            var marker = go.GetComponent<OPZMapMarker>();
            if (marker == null)
                marker = go.AddComponent<OPZMapMarker>();

            marker.displayName = displayName;
            marker.markerType = type;
            marker.gizmoColor = color;
            marker.gizmoSize = gizmoSize;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            var split = folderPath.Split('/');
            var current = split[0];

            for (int i = 1; i < split.Length; i++)
            {
                var next = current + "/" + split[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, split[i]);
                }
                current = next;
            }
        }

        private static void MoveToScene(GameObject go, Scene scene)
        {
            if (scene.IsValid())
                SceneManager.MoveGameObjectToScene(go, scene);
        }

        private static float RandomRange(float min, float max, int seed)
        {
            var state = Random.state;
            Random.InitState(15437 + seed * 977);
            float value = Random.Range(min, max);
            Random.state = state;
            return value;
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            if (target == null) return;

            var type = target.GetType();
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
                field.SetValue(target, value);
        }
    }
}
