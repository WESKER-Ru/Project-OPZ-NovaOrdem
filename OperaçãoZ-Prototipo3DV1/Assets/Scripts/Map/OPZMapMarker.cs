using UnityEngine;

namespace OPZ.Map
{
    public enum OPZMarkerType
    {
        Spawn,
        Resource,
        Choke,
        Hotspot,
        Route,
        PvE,
        Bridge,
        Objective,
        CityBlock,
        Biome,
        Misc
    }

    /// <summary>
    /// Lightweight editor/runtime marker for blockout objects.
    /// Useful for reading the map in Scene view before the final art pass exists.
    /// </summary>
    public class OPZMapMarker : MonoBehaviour
    {
        public string displayName = "Marker";
        public OPZMarkerType markerType = OPZMarkerType.Misc;
        public Color gizmoColor = Color.white;
        public Vector3 gizmoSize = Vector3.one;
        public bool drawWireCube = true;
        public bool drawSolidSphere = false;
        public bool drawLabel = true;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;

            if (drawWireCube)
            {
                Gizmos.DrawWireCube(transform.position, gizmoSize);
            }

            if (drawSolidSphere)
            {
                Gizmos.DrawSphere(transform.position, Mathf.Max(0.35f, gizmoSize.x * 0.15f));
            }

#if UNITY_EDITOR
            if (drawLabel)
            {
                UnityEditor.Handles.color = gizmoColor;
                UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"{displayName} [{markerType}]");
            }
#endif
        }
    }
}
