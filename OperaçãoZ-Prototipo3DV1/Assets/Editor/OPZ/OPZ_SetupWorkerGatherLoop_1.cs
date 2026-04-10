using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using OPZ.Combat;
using OPZ.Core;
using OPZ.Data;
using OPZ.Economy;
using OPZ.Units;

namespace OPZ.EditorTools
{
    public static class OPZ_SetupWorkerGatherLoop
    {
        private const string MenuRoot = "OPZ/Prototype";
        private const string RuntimeRootName = "_WorkerGatherLoop";
        private const string ManagerRootName = "_CoreRTS_Runtime";
        private const string DefFolder = "Assets/Generated/Defs";
        private const int UnitLayer = 6;
        private const int GroundLayer = 7;
        private const int ResourceLayer = 8;
        private const int BuildingLayer = 9;

        [MenuItem(MenuRoot + "/Setup Worker Gather Loop In Current Scene", priority = 40)]
        public static void Setup()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[OPZ] Setup cancelado: este utilitário não pode modificar a cena durante Play Mode.");
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("[OPZ] Nenhuma cena válida está carregada.");
                return;
            }

            EnsureLayerName(UnitLayer, "Units");
            EnsureLayerName(GroundLayer, "Ground");
            EnsureLayerName(ResourceLayer, "Resources");
            EnsureLayerName(BuildingLayer, "Buildings");

            DisableProtoControllers();
            EnsureCoreManagers(scene);
            MarkSceneLayers();
            EnsureEconomyBasics(scene);
            CreateWorkerDefs();
            CreateWorkers(scene);
            RefreshEconomy();

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[OPZ] Worker gather loop configurado. Teste: selecione o Worker AR e clique com o botão direito em um ResourceNode.");
        }

        private static void DisableProtoControllers()
        {
            var protoRoot = GameObject.Find("_SelectionManager");
            if (protoRoot == null) return;

            foreach (var mb in protoRoot.GetComponents<MonoBehaviour>())
            {
                if (mb == null) continue;
                string fullName = mb.GetType().FullName;
                if (fullName == "OPZ.Proto.SelectionManager" || fullName == "OPZ.Proto.CommandGiver")
                    mb.enabled = false;
            }
        }

        private static void EnsureCoreManagers(Scene scene)
        {
            var root = GameObject.Find(ManagerRootName);
            if (root == null)
            {
                root = new GameObject(ManagerRootName);
                SceneManager.MoveGameObjectToScene(root, scene);
            }

            var gm = Object.FindFirstObjectByType<GameManager>();
            if (gm == null) gm = root.AddComponent<GameManager>();

            var em = Object.FindFirstObjectByType<EconomyManager>();
            if (em == null) em = root.AddComponent<EconomyManager>();

            var sm = Object.FindFirstObjectByType<SelectionManager>();
            if (sm == null) sm = root.AddComponent<SelectionManager>();
            SetMask(sm, "selectableLayer", 1 << UnitLayer);
            SetMask(sm, "groundLayer", 1 << GroundLayer);

            var cs = Object.FindFirstObjectByType<CommandSystem>();
            if (cs == null) cs = root.AddComponent<CommandSystem>();
            SetMask(cs, "groundLayer", 1 << GroundLayer);
            SetMask(cs, "unitLayer", 1 << UnitLayer);
            SetMask(cs, "resourceLayer", 1 << ResourceLayer);
            SetMask(cs, "buildingLayer", 1 << BuildingLayer);

            var cam = Camera.main;
            if (cam != null)
            {
                var simple = cam.GetComponent<SimpleRTSCamera>();
                if (simple == null)
                    cam.gameObject.AddComponent<SimpleRTSCamera>();
            }
        }

        private static void MarkSceneLayers()
        {
            foreach (var col in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if (col == null) continue;
                var go = col.gameObject;
                if (go == null) continue;

                if (go.GetComponentInParent<UnitBase>() != null)
                {
                    go.layer = UnitLayer;
                    continue;
                }

                if (go.GetComponentInParent<ResourceNode>() != null)
                {
                    go.layer = ResourceLayer;
                    continue;
                }

                if (go.GetComponentInParent<DepositPoint>() != null || go.GetComponentInParent<OPZ.Building.BuildingBase>() != null)
                {
                    go.layer = BuildingLayer;
                    continue;
                }

                go.layer = GroundLayer;
            }
        }

        private static void EnsureEconomyBasics(Scene scene)
        {
            if (Object.FindFirstObjectByType<EconomyManager>() == null)
            {
                var go = new GameObject("_EconomyManager");
                SceneManager.MoveGameObjectToScene(go, scene);
                go.AddComponent<EconomyManager>();
            }
        }

        private static void RefreshEconomy()
        {
            var economy = Object.FindFirstObjectByType<EconomyManager>();
            if (economy != null)
                economy.RefreshDepotRegistry();

            foreach (var node in Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))
            {
                if (node.gameObject.layer != ResourceLayer)
                    node.gameObject.layer = ResourceLayer;
            }

            foreach (var depot in Object.FindObjectsByType<DepositPoint>(FindObjectsSortMode.None))
            {
                if (depot.gameObject.layer != BuildingLayer)
                    depot.gameObject.layer = BuildingLayer;
            }
        }

        private static void CreateWorkerDefs()
        {
            EnsureFolder("Assets/Generated");
            EnsureFolder(DefFolder);

            CreateOrUpdateWorkerDef(Path.Combine(DefFolder, "Worker_AR_Test.asset").Replace("\\", "/"), Faction.AR);
            CreateOrUpdateWorkerDef(Path.Combine(DefFolder, "Worker_EG_Test.asset").Replace("\\", "/"), Faction.EG);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateOrUpdateWorkerDef(string assetPath, Faction faction)
        {
            var def = AssetDatabase.LoadAssetAtPath<UnitDef>(assetPath);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<UnitDef>();
                AssetDatabase.CreateAsset(def, assetPath);
            }

            def.unitName = faction + "_Worker_Test";
            def.faction = faction;
            def.role = UnitRole.Worker;
            def.isWorker = true;
            def.maxHealth = 65f;
            def.moveSpeed = 6f;
            def.carryCapacity = 25;
            def.gatherRate = 10f;
            def.attackDamage = 0f;
            def.attackRange = 0f;
            def.attackCooldown = 1f;
            def.lineOfSight = 10f;
            def.suppliesCost = 0;
            def.metalCost = 0;
            def.fuelCost = 0;
            def.trainTime = 1f;
            EditorUtility.SetDirty(def);
        }

        private static void CreateWorkers(Scene scene)
        {
            var oldRoot = GameObject.Find(RuntimeRootName);
            if (oldRoot != null)
                Object.DestroyImmediate(oldRoot);

            var root = new GameObject(RuntimeRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            var spawnAR = GameObject.Find("Spawn_AR");
            var spawnEG = GameObject.Find("Spawn_EG");
            if (spawnAR == null || spawnEG == null)
            {
                Debug.LogError("[OPZ] Spawn_AR ou Spawn_EG não encontrado. Não foi possível criar workers.");
                return;
            }

            var defAR = AssetDatabase.LoadAssetAtPath<UnitDef>("Assets/Generated/Defs/Worker_AR_Test.asset");
            var defEG = AssetDatabase.LoadAssetAtPath<UnitDef>("Assets/Generated/Defs/Worker_EG_Test.asset");

            CreateWorker(root.transform, "AR_Worker_01", spawnAR.transform.position + new Vector3(-2f, 0.2f, -2f), defAR, new Color(0.18f, 0.42f, 0.86f));
            CreateWorker(root.transform, "AR_Worker_02", spawnAR.transform.position + new Vector3(2f, 0.2f, -2f), defAR, new Color(0.18f, 0.42f, 0.86f));
            CreateWorker(root.transform, "EG_Worker_01", spawnEG.transform.position + new Vector3(-2f, 0.2f, 2f), defEG, new Color(0.85f, 0.20f, 0.20f));
        }

        private static void CreateWorker(Transform parent, string name, Vector3 desiredPosition, UnitDef def, Color color)
        {
            Vector3 finalPos = desiredPosition;
            if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 8f, NavMesh.AllAreas))
                finalPos = hit.position + Vector3.up * 0.05f;

            var unit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unit.name = name;
            unit.transform.SetParent(parent);
            unit.transform.position = finalPos;
            unit.transform.localScale = new Vector3(1.1f, 1.35f, 1.1f);
            unit.layer = UnitLayer;

            var rend = unit.GetComponent<Renderer>();
            if (rend != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                var mat = new Material(shader) { color = color };
                rend.sharedMaterial = mat;
            }

            var circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            circle.name = "SelectionCircle";
            circle.transform.SetParent(unit.transform);
            circle.transform.localPosition = new Vector3(0f, -0.70f, 0f);
            circle.transform.localScale = new Vector3(2f, 0.02f, 2f);
            circle.layer = UnitLayer;
            Object.DestroyImmediate(circle.GetComponent<Collider>());
            var circleRend = circle.GetComponent<Renderer>();
            if (circleRend != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                var mat = new Material(shader) { color = new Color(0f, 1f, 0f, 1f) };
                circleRend.sharedMaterial = mat;
            }
            circle.SetActive(false);

            unit.AddComponent<NavMeshAgent>();
            unit.AddComponent<HealthComponent>();
            var worker = unit.AddComponent<WorkerUnit>();

            var so = new SerializedObject(worker);
            so.FindProperty("unitDef").objectReferenceValue = def;
            so.FindProperty("selectionCircle").objectReferenceValue = circle;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureLayerName(int layerIndex, string desiredName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");
            var sp = layersProp.GetArrayElementAtIndex(layerIndex);
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = desiredName;
                tagManager.ApplyModifiedProperties();
            }
        }

        private static void SetMask(Object obj, string fieldName, int maskValue)
        {
            var so = new SerializedObject(obj);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.intValue = maskValue;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var split = path.Split('/');
            var current = split[0];
            for (int i = 1; i < split.Length; i++)
            {
                string next = current + "/" + split[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, split[i]);
                current = next;
            }
        }
    }
}
