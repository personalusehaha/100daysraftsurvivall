using UnityEngine;
using RaftSurvival.Core;

namespace RaftSurvival.Audio
{
    /// <summary>
    /// Manages looping ambient audio (ocean waves, wind, birds) and
    /// crossfades between day and night ambience. Attach to a single
    /// persistent GameObject (e.g. alongside GameManager).
    /// </summary>
    public class AmbientAudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource oceanAmbienceSource;
        [SerializeField] private AudioSource windAmbienceSource;
        [SerializeField] private AudioSource birdAmbienceSource;

        [Header("Clips")]
        [SerializeField] private AudioClip oceanLoopClip;
        [SerializeField] private AudioClip windLoopClip;
        [SerializeField] private AudioClip dayBirdsLoopClip;
        [SerializeField] private AudioClip nightAmbienceLoopClip;

        [Header("Volumes")]
        [Range(0f, 1f)][SerializeField] private float oceanVolume = 0.6f;
        [Range(0f, 1f)][SerializeField] private float windVolume = 0.3f;
        [Range(0f, 1f)][SerializeField] private float birdVolume = 0.4f;

        [Header("Day/Night Crossfade")]
        [SerializeField] private DayNightCycle dayNightCycle;
        [SerializeField] private float crossfadeSpeed = 0.5f;

        private float currentBirdVolume;

        private void Start()
        {
            SetupSource(oceanAmbienceSource, oceanLoopClip, oceanVolume);
            SetupSource(windAmbienceSource, windLoopClip, windVolume);
            SetupSource(birdAmbienceSource, dayBirdsLoopClip, 0f); // fades in during day
        }

        private void SetupSource(AudioSource source, AudioClip clip, float targetVolume)
        {
            if (source == null || clip == null) return;

            source.clip = clip;
            source.loop = true;
            source.volume = targetVolume;
            source.playOnAwake = false;
            source.Play();
        }

        private void Update()
        {
            if (dayNightCycle == null || birdAmbienceSource == null) return;

            float t = dayNightCycle.GetNormalizedTimeOfDay();

            // Birds are louder during daytime (roughly t = 0.2 to 0.8), quiet at night.
            bool isDaytime = t > 0.2f && t < 0.8f;
            float targetVolume = isDaytime ? birdVolume : 0f;

            currentBirdVolume = Mathf.MoveTowards(currentBirdVolume, targetVolume, crossfadeSpeed * Time.deltaTime);
            birdAmbienceSource.volume = currentBirdVolume;
        }

        public void SetOceanVolume(float volume)
        {
            if (oceanAmbienceSource != null) oceanAmbienceSource.volume = Mathf.Clamp01(volume);
        }

        public void SetWindVolume(float volume)
        {
            if (windAmbienceSource != null) windAmbienceSource.volume = Mathf.Clamp01(volume);
        }
    }
}
