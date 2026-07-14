using UnityEngine;
using RaftSurvival.Core;

namespace RaftSurvival.Core
{
    /// <summary>
    /// Rotates a directional light (the Sun) to simulate a full day/night
    /// cycle. Default cycle length is 5 minutes (300 seconds) per full day,
    /// as requested for Phase 1.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The directional light acting as the sun.")]
        [SerializeField] private Light sunLight;

        [Header("Cycle Settings")]
        [Tooltip("Total seconds for one full 24-hour in-game day.")]
        [SerializeField] private float dayLengthSeconds = 300f;

        [Tooltip("0 = midnight, 0.5 = noon, 1 = next midnight.")]
        [Range(0f, 1f)]
        [SerializeField] private float startTimeOfDay = 0.25f; // start at sunrise

        [Header("Lighting Curves")]
        [SerializeField] private Gradient sunColorOverDay;
        [SerializeField] private AnimationCurve sunIntensityOverDay = AnimationCurve.EaseInOut(0, 0.05f, 1, 1.2f);
        [SerializeField] private float maxSunIntensity = 1.4f;

        private float timeOfDay; // 0..1
        private int lastDayIndex = 0;

        private void Awake()
        {
            timeOfDay = startTimeOfDay;

            if (sunLight == null)
            {
                Debug.LogWarning("[DayNightCycle] No sunLight assigned. Please assign a Directional Light.");
            }
        }

        private void Update()
        {
            if (dayLengthSeconds <= 0f) return;

            timeOfDay += Time.deltaTime / dayLengthSeconds;

            if (timeOfDay >= 1f)
            {
                timeOfDay -= 1f;
                GameManager.Instance?.AdvanceDay();
            }

            UpdateSun();
        }

        private void UpdateSun()
        {
            if (sunLight == null) return;

            // Rotate sun: 0 = sunrise (east), 0.5 = sunset direction, full 360 over the day.
            float sunAngle = timeOfDay * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

            // Adjust intensity and color based on curve/gradient.
            sunLight.intensity = sunIntensityOverDay.Evaluate(timeOfDay) * maxSunIntensity;

            if (sunColorOverDay != null)
            {
                sunLight.color = sunColorOverDay.Evaluate(timeOfDay);
            }

            // Fade ambient light with sun height for a believable night.
            float sunHeight = Mathf.Sin(sunAngle * Mathf.Deg2Rad);
            RenderSettings.ambientIntensity = Mathf.Clamp01(0.15f + sunHeight * 0.5f);
        }

        public float GetNormalizedTimeOfDay() => timeOfDay;
    }
}
