using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

namespace OPZ.EditorTools
{
    public static class OPZ_SetupPrototypeScene
    {
        private const string MenuRoot = "OPZ/Prototype";
        private const string RuntimeRootName = "_TestUnits";
        private const string SelectionRootName = "_SelectionManager";
        private const string SpawnARName = "Spawn_AR";
        private const string MapRootName = "MAP_01_CinturaoDeRuina";
        private const string NavMeshRootName = "_PrototypeNavMesh";

        [MenuItem(MenuRoot + "/Setup Test Units In Current Scene", priority = 20)]
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

            SetLayerName(6, "Units");
            SetLayerName(7, "Ground");

            int groundCount = MarkGroundObjects();
            Debug.Log($"[OPZ] {groundCount} objetos marcados como Ground.");

            SetupSelectionSystem();
            EnsurePlayableCamera();
            BakePrototypeNavMesh();
            PositionCameraToSpawnAR();

            var oldUnits = GameObject.Find(RuntimeRootName);
            if (oldUnits != null)
                Object.DestroyImmediate(oldUnits);

            var unitsParent = new GameObject(RuntimeRootName);
            SceneManager.MoveGameObjectToScene(unitsParent, scene);

            int count = CreateTestUnits(unitsParent.transform);
            Debug.Log($"[OPZ] {count} unidades criadas no Spawn AR.");

            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("════════════════════════════════════════════");
            Debug.Log("[OPZ] SETUP COMPLETO!");
            Debug.Log("[OPZ] Agora aperte Play e teste:");
            Debug.Log("[OPZ]   - Clique esquerdo = selecionar");
            Debug.Log("[OPZ]   - Arrastar = box select");
            Debug.Log("[OPZ]   - Clique direito no chão = mover");
            Debug.Log("════════════════════════════════════════════");
        }

        private static int MarkGroundObjects()
        {
            int count = 0;
            foreach (var col in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if (col == null) continue;
                if (col.GetComponent<OPZ.Proto.Selectable>() != null) continue;
                if (col.GetComponent<NavMeshAgent>() != null) continue;
                if (col.gameObject.name == "SelectionCircle") continue;

                col.gameObject.layer = 7;
                count++;
            }
            return count;
        }

        private static void SetupSelectionSystem()
        {
            var managerGO = GameObject.Find(SelectionRootName);
            if (managerGO == null)
                managerGO = new GameObject(SelectionRootName);

            var selMgr = managerGO.GetComponent<OPZ.Proto.SelectionManager>();
            if (selMgr == null)
                selMgr = managerGO.AddComponent<OPZ.Proto.SelectionManager>();
            selMgr.selectableLayer = 1 << 6;

            var cmdGiver = managerGO.GetComponent<OPZ.Proto.CommandGiver>();
            if (cmdGiver == null)
                cmdGiver = managerGO.AddComponent<OPZ.Proto.CommandGiver>();
            cmdGiver.groundLayer = 1 << 7;
        }

        private static void EnsurePlayableCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            var simple = cam.GetComponent<OPZ.Core.SimpleRTSCamera>();
            if (simple == null)
                simple = cam.gameObject.AddComponent<OPZ.Core.SimpleRTSCamera>();

            var refined = cam.GetComponent<OPZ.Core.RTSCameraController>();
            if (refined != null)
                refined.enabled = false;
        }

        private static void PositionCameraToSpawnAR()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[OPZ] Main Camera não encontrada. Pulando posicionamento da câmera.");
                return;
            }

            var spawn = GameObject.Find(SpawnARName);
            if (spawn == null)
            {
                Debug.LogWarning("[OPZ] Spawn_AR não encontrado. Pulando posicionamento da câmera.");
                return;
            }

            Vector3 target = spawn.transform.position;
            cam.transform.position = target + new Vector3(0f, 50f, -28f);
            cam.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            Debug.Log("[OPZ] Camera posicionada no Spawn AR.");
        }

        private static void BakePrototypeNavMesh()
        {
            GameObject mapRoot = GameObject.Find(MapRootName);
            GameObject host = mapRoot != null ? mapRoot : GameObject.Find(NavMeshRootName);
            if (host == null)
                host = new GameObject(NavMeshRootName);

            var surface = host.GetComponent<NavMeshSurface>();
            if (surface == null)
                surface = host.AddComponent<NavMeshSurface>();

            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~0;
            surface.defaultArea = 0;
            surface.ignoreNavMeshAgent = true;
            surface.ignoreNavMeshObstacle = true;

            surface.BuildNavMesh();
            Debug.Log("[OPZ] NavMesh bake concluído para o setup de protótipo.");
        }

        private static int CreateTestUnits(Transform parent)
        {
            var spawn = GameObject.Find(SpawnARName);
            if (spawn == null)
            {
                Debug.LogWarning("[OPZ] Spawn_AR não encontrado. Nenhuma unidade criada.");
                return 0;
            }

            int created = 0;
            Vector3 spawnCenter = spawn.transform.position + new Vector3(0f, 0.5f, 0f);
            Vector3[] positions =
            {
                spawnCenter + new Vector3(0, 0, 0),
                spawnCenter + new Vector3(4, 0, 0),
                spawnCenter + new Vector3(-4, 0, 0),
                spawnCenter + new Vector3(0, 0, 4),
                spawnCenter + new Vector3(4, 0, 4),
                spawnCenter + new Vector3(-4, 0, 4),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                CreateTestUnit(parent, $"Unit_{i}", positions[i]);
                created++;
            }

            return created;
        }

        private static void CreateTestUnit(Transform parent, string name, Vector3 desiredPosition)
        {
            Vector3 finalPos = desiredPosition;
            bool hasNavMesh = NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 8f, NavMesh.AllAreas);
            if (hasNavMesh)
                finalPos = hit.position + Vector3.up * 0.05f;

            var unit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unit.name = name;
            unit.transform.SetParent(parent);
            unit.transform.position = finalPos;
            unit.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);
            unit.layer = 6;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { color = new Color(0.15f, 0.4f, 0.95f) };
            unit.GetComponent<Renderer>().sharedMaterial = mat;

            var circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            circle.name = "SelectionCircle";
            circle.transform.SetParent(unit.transform);
            circle.transform.localPosition = new Vector3(0f, -0.65f, 0f);
            circle.transform.localScale = new Vector3(2f, 0.015f, 2f);
            circle.layer = 6;
            Object.DestroyImmediate(circle.GetComponent<Collider>());
            var cMat = new Material(shader) { color = new Color(0f, 1f, 0f) };
            circle.GetComponent<Renderer>().sharedMaterial = cMat;
            circle.SetActive(false);

            var sel = unit.AddComponent<OPZ.Proto.Selectable>();
            sel.selectionCircle = circle;

            if (hasNavMesh)
            {
                var agent = unit.AddComponent<NavMeshAgent>();
                agent.speed = 8f;
                agent.angularSpeed = 360f;
                agent.acceleration = 15f;
                agent.stoppingDistance = 0.5f;
                agent.radius = 0.6f;
                agent.height = 3f;

                if (unit.GetComponent<OPZ.Proto.Mover>() == null)
                    unit.AddComponent<OPZ.Proto.Mover>();
            }
            else
            {
                Debug.LogWarning($"[OPZ] Unidade criada sem NavMeshAgent porque não existe NavMesh válida perto de {desiredPosition}.");
            }
        }

        private static void SetLayerName(int layer, string name)
        {
            var tagMgr = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var prop = tagMgr.FindProperty("layers").GetArrayElementAtIndex(layer);
            if (string.IsNullOrEmpty(prop.stringValue))
            {
                prop.stringValue = name;
                tagMgr.ApplyModifiedProperties();
            }
        }
    }
}
