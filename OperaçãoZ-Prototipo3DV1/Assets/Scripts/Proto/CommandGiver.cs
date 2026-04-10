// Assets/Scripts/Proto/CommandGiver.cs
// Processa clique direito: manda unidades selecionadas se moverem.
// Coloque no mesmo GameObject do SelectionManager.
using UnityEngine;
using UnityEngine.InputSystem;

namespace OPZ.Proto
{
    public class CommandGiver : MonoBehaviour
    {
        [Header("Config")]
        public LayerMask groundLayer; // layer do chão (para raycast)

        [Header("Feedback Visual")]
        public GameObject moveMarkerPrefab; // opcional: prefab que aparece no destino

        SelectionManager _selection;
        Camera _cam;

        void Start()
        {
            _selection = GetComponent<SelectionManager>();
            _cam = Camera.main;

            if (_selection == null)
                Debug.LogError("[CommandGiver] Precisa estar no mesmo GameObject que SelectionManager!");
        }

        void Update()
        {
            var mouse = Mouse.current;
            var kb = Keyboard.current;
            if (mouse == null || _cam == null || _selection == null) return;

            // ─── CLIQUE DIREITO: mover ───
            if (mouse.rightButton.wasPressedThisFrame)
            {
                if (_selection.Selected.Count == 0) return;

                Ray ray = _cam.ScreenPointToRay(mouse.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundLayer))
                {
                    // Manda cada unidade selecionada se mover
                    foreach (var sel in _selection.Selected)
                    {
                        var mover = sel.GetComponent<Mover>();
                        if (mover != null)
                            mover.MoveTo(hit.point);
                    }

                    // Feedback visual
                    SpawnMarker(hit.point);
                }
            }

            // ─── TECLA S: parar ───
            if (kb != null && kb.sKey.wasPressedThisFrame)
            {
                foreach (var sel in _selection.Selected)
                {
                    var mover = sel.GetComponent<Mover>();
                    if (mover != null) mover.Stop();
                }
            }
        }

        void SpawnMarker(Vector3 pos)
        {
            if (moveMarkerPrefab == null) return;
            var go = Instantiate(moveMarkerPrefab, pos + Vector3.up * 0.15f, Quaternion.identity);
            Destroy(go, 0.8f);
        }
    }
}
