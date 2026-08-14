using UnityEngine;

namespace Manager
{
    /// <summary>
    /// Component to easily trigger primary and optional secondary BGM when a scene or panel starts.
    /// Attach this to a GameObject in a scene to play BGM automatically on Start.
    /// </summary>
    public class PlayBGMOnStart : MonoBehaviour
    {
        [Header("Primary BGM")]
        [Tooltip("The ID of the primary BGM registered in AudioManager to play.")]
        [SerializeField] private string _bgmId;

        [Header("Secondary BGM (Simultaneous Layer/Ambient)")]
        [Tooltip("Optional ID of a secondary BGM to play simultaneously with primary BGM.")]
        [SerializeField] private string _secondaryBgmId;

        [Tooltip("Sync playback timestamp with primary BGM track.")]
        [SerializeField] private bool _syncTimeWithPrimary = true;

        [Header("Settings")]
        [Tooltip("Fade duration for transitioning into these BGMs.")]
        [SerializeField] private float _fadeDuration = 0.5f;

        [Tooltip("If true, primary BGM will only play if no primary BGM is currently playing.")]
        [SerializeField] private bool _onlyIfNotPlaying = false;

        private void Start()
        {
            if (AudioManager.Instance == null) return;

            if (!string.IsNullOrEmpty(_bgmId))
            {
                if (!_onlyIfNotPlaying || !AudioManager.Instance.IsBGMPlaying)
                {
                    AudioManager.Instance.PlayBGM(_bgmId, _fadeDuration);
                }
            }

            if (!string.IsNullOrEmpty(_secondaryBgmId))
            {
                AudioManager.Instance.PlaySecondaryBGM(_secondaryBgmId, _syncTimeWithPrimary, _fadeDuration);
            }
        }
    }
}
