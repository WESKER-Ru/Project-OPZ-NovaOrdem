using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using OPZ.Data;
using OPZ.Economy;

namespace OPZ.EditorTools
{
    /// <summary>
    /// Creates simple economy test objects in the currently opened scene.
    /// Safe to run only outside Play Mode.
    /// </summary>
    public static class OPZ_SetupEconomyTest
    {
        [MenuItem("OPZ/Prototype/Setup Economy Nodes In Current Scene")]
        public static void Setup()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[OPZ] Setup cancelado: este utilitário não pode modificar a cena durante Play Mode.");
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("[OPZ] Nenhuma cena válida está aberta.");
                return;
            }

            var root = GameObject.Find("_EconomyTest");
            if (root != null)
                Object.DestroyImmediate(root);

            root = new GameObject("_EconomyTest");
            SceneManager.MoveGameObjectToScene(root, scene);

            var economy = Object.FindFirstObjectByType<EconomyManager>();
            if (economy == null)
            {
                var economyGo = new GameObject("_EconomyManager");
                SceneManager.MoveGameObjectToScene(economyGo, scene);
                economy = economyGo.AddComponent<EconomyManager>();
            }

            CreateSide(root.transform, Faction.AR, new Vector3(-112f, 0.5f, 112f));
            CreateSide(root.transform, Faction.EG, new Vector3(112f, 0.5f, -112f));

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[OPZ] Economy test nodes criados para AR e EG.");
        }

        private static void CreateSide(Transform parent, Faction faction, Vector3 center)
        {
            var sideRoot = new GameObject(faction.ToString());
            sideRoot.transform.SetParent(parent);
            sideRoot.transform.position = center;

            var depositGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            depositGo.name = "DepositPoint";
            depositGo.transform.SetParent(sideRoot.transform);
            depositGo.transform.position = center + new Vector3(0f, 0.5f, -6f);
            depositGo.transform.localScale = new Vector3(4f, 1f, 4f);

            var deposit = depositGo.AddComponent<DepositPoint>();
            deposit.SetFaction(faction);

            CreateNode(sideRoot.transform, ResourceType.Supplies, center + new Vector3(-8f, 0.5f, 6f), new Color(0.76f, 0.64f, 0.20f), 600);
            CreateNode(sideRoot.transform, ResourceType.Supplies, center + new Vector3(8f, 0.5f, 6f), new Color(0.76f, 0.64f, 0.20f), 600);
            CreateNode(sideRoot.transform, ResourceType.Metal, center + new Vector3(0f, 0.5f, 10f), new Color(0.60f, 0.62f, 0.70f), 400);
        }

        private static void CreateNode(Transform parent, ResourceType type, Vector3 pos, Color color, int amount)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = $"{type}_Node";
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(2f, 1f, 2f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                var mat = new Material(shader);
                mat.color = color;
                renderer.sharedMaterial = mat;
            }

            var node = go.AddComponent<ResourceNode>();

            var so = new SerializedObject(node);
            so.FindProperty("type").enumValueIndex = (int)type;
            so.FindProperty("totalAmount").intValue = amount;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
