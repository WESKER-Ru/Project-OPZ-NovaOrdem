// Assets/Scripts/UI/WorldHealthBar.cs
using UnityEngine;
using OPZ.Combat;

namespace OPZ.UI
{
    /// <summary>
    /// Attach to a unit/building. Shows a simple billboard health bar using a quad + material.
    /// Alternative: use UI Canvas in world space. This approach avoids per-unit Canvas overhead.
    /// 
    /// Setup: Create a child Quad named "HealthBar" with an unlit material.
    /// This script scales it based on HP ratio.
    /// </summary>
    public class WorldHealthBar : MonoBehaviour
    {
        [SerializeField] Transform barFill;
        [SerializeField] Transform barBackground;
        [SerializeField] bool hideWhenFull = true;

        HealthComponent _health;
        Camera _cam;

        void Start()
        {
            _health = GetComponentInParent<HealthComponent>();
            _cam = Camera.main;
            if (_health != null) _health.OnHealthChanged += OnHPChanged;
            UpdateBar();
        }

        void OnDestroy()
        {
            if (_health != null) _health.OnHealthChanged -= OnHPChanged;
        }

        void LateUpdate()
        {
            // Billboard: face camera
            if (_cam != null)
                transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
        }

        void OnHPChanged(float current, float max) => UpdateBar();

        void UpdateBar()
        {
            if (_health == null || barFill == null) return;
            float ratio = _health.HealthRatio;

            Vector3 scale = barFill.localScale;
            scale.x = ratio;
            barFill.localScale = scale;

            // Color: green → yellow → red
            var rend = barFill.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = ratio > 0.5f
                    ? Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f)
                    : Color.Lerp(Color.red, Color.yellow, ratio * 2f);
                rend.material.color = c;
            }

            if (hideWhenFull)
            {
                bool show = ratio < 0.99f;
                if (barFill.gameObject.activeSelf != show) barFill.gameObject.SetActive(show);
                if (barBackground != null && barBackground.gameObject.activeSelf != show) barBackground.gameObject.SetActive(show);
            }
        }
    }
}
