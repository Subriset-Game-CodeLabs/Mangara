using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    [System.Serializable]
    public class Sound
    {
        [Tooltip("Unique ID to play this sound by name (e.g. 'main_menu', 'game_bgm', 'click', 'footstep')")]
        public string id;

        [Tooltip("The audio clip asset")]
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1.0f;

        [Range(0.1f, 3f)]
        public float pitch = 1.0f;

        [Header("Pitch Randomization (SFX)")]
        public bool randomizePitch = false;

        [Range(0f, 0.5f)]
        public float pitchVariance = 0.1f;
    }

    [System.Serializable]
    public class SceneBgmMapping
    {
        [Tooltip("Exact name of the scene (e.g., 'Start', 'MVP', 'BeachScene')")]
        public string sceneName;

        [Tooltip("Primary BGM ID registered in BGM List to play when this scene loads")]
        public string bgmId;

        [Tooltip("Secondary BGM ID registered in BGM List (optional, played simultaneously)")]
        public string secondaryBgmId;

        [Tooltip("Fade duration in seconds when transitioning into this scene's BGM")]
        public float fadeDuration = 0.5f;
    }

    public class AudioManager : PersistentSingleton<AudioManager>
    {
        [Header("Audio Lists")]
        [SerializeField] private List<Sound> _bgmList = new List<Sound>();
        [SerializeField] private List<Sound> _sfxList = new List<Sound>();

        [Header("Scene BGM Configuration")]
        [Tooltip("Enable auto-playing BGM when scenes load based on the mappings below.")]
        [SerializeField] private bool _autoPlaySceneBGM = true;

        [Tooltip("Default Menu BGM ID (fallback for menu scenes like 'Start')")]
        [SerializeField] private string _defaultMenuBgmId = "main_menu";

        [Tooltip("Default Game BGM ID (fallback for gameplay scenes like 'MVP')")]
        [SerializeField] private string _defaultGameBgmId = "game";

        [Tooltip("Specific scene-to-BGM mappings")]
        [SerializeField] private List<SceneBgmMapping> _sceneBgmMappings = new List<SceneBgmMapping>();

        [Header("Audio Sources (Dual BGM Support)")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _bgmSource2;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Volume Settings")]
        [Range(0f, 1f)] [SerializeField] private float _masterVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float _bgmVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float _secondaryBgmVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 1.0f;

        private readonly Dictionary<string, Sound> _bgmDict = new Dictionary<string, Sound>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Sound> _sfxDict = new Dictionary<string, Sound>(StringComparer.OrdinalIgnoreCase);

        private Coroutine _bgmFadeCoroutine;
        private Coroutine _bgmFadeCoroutine2;

        private Sound _currentBGM;
        private Sound _currentSecondaryBGM;

        private const string PREF_MASTER_VOL = "AudioManager_MasterVolume";
        private const string PREF_BGM_VOL = "AudioManager_BGMVolume";
        private const string PREF_SECONDARY_BGM_VOL = "AudioManager_SecondaryBGMVolume";
        private const string PREF_SFX_VOL = "AudioManager_SFXVolume";

        public bool IsBGMPlaying => _bgmSource != null && _bgmSource.isPlaying;
        public bool IsSecondaryBGMPlaying => _bgmSource2 != null && _bgmSource2.isPlaying;

        public string CurrentBGMId => _currentBGM != null ? _currentBGM.id : null;
        public string CurrentSecondaryBGMId => _currentSecondaryBGM != null ? _currentSecondaryBGM.id : null;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_MASTER_VOL, _masterVolume);
                UpdateVolumes();
            }
        }

        public float BGMVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_BGM_VOL, _bgmVolume);
                UpdateVolumes();
            }
        }

        public float SecondaryBGMVolume
        {
            get => _secondaryBgmVolume;
            set
            {
                _secondaryBgmVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_SECONDARY_BGM_VOL, _secondaryBgmVolume);
                UpdateVolumes();
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_SFX_VOL, _sfxVolume);
                UpdateVolumes();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            SetupAudioSources();
            LoadVolumeSettings();
            InitializeDictionaries();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_autoPlaySceneBGM) return;

            // Check explicit scene mappings first
            foreach (var mapping in _sceneBgmMappings)
            {
                if (string.Equals(mapping.sceneName, scene.name, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(mapping.bgmId))
                    {
                        PlayBGM(mapping.bgmId, mapping.fadeDuration);
                    }
                    if (!string.IsNullOrEmpty(mapping.secondaryBgmId))
                    {
                        PlaySecondaryBGM(mapping.secondaryBgmId, syncTime: true, mapping.fadeDuration);
                    }
                    else
                    {
                        StopSecondaryBGM(mapping.fadeDuration);
                    }
                    return;
                }
            }

            // Fallback for menu vs game scenes
            if (scene.name.Equals("Start", StringComparison.OrdinalIgnoreCase) || scene.name.Contains("Menu", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(_defaultMenuBgmId) && _bgmDict.ContainsKey(_defaultMenuBgmId))
                {
                    PlayBGM(_defaultMenuBgmId, 0.5f);
                    StopSecondaryBGM(0.5f);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_defaultGameBgmId) && _bgmDict.ContainsKey(_defaultGameBgmId))
                {
                    PlayBGM(_defaultGameBgmId, 0.5f);
                }
            }
        }

        private void SetupAudioSources()
        {
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
            }
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;

            if (_bgmSource2 == null)
            {
                _bgmSource2 = gameObject.AddComponent<AudioSource>();
            }
            _bgmSource2.loop = true;
            _bgmSource2.playOnAwake = false;

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
            }
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
        }

        private void LoadVolumeSettings()
        {
            _masterVolume = PlayerPrefs.GetFloat(PREF_MASTER_VOL, 1.0f);
            _bgmVolume = PlayerPrefs.GetFloat(PREF_BGM_VOL, 1.0f);
            _secondaryBgmVolume = PlayerPrefs.GetFloat(PREF_SECONDARY_BGM_VOL, 1.0f);
            _sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOL, 1.0f);
            UpdateVolumes();
        }

        private void InitializeDictionaries()
        {
            _bgmDict.Clear();
            foreach (var sound in _bgmList)
            {
                if (sound != null && !string.IsNullOrEmpty(sound.id))
                {
                    _bgmDict[sound.id] = sound;
                }
            }

            _sfxDict.Clear();
            foreach (var sound in _sfxList)
            {
                if (sound != null && !string.IsNullOrEmpty(sound.id))
                {
                    _sfxDict[sound.id] = sound;
                }
            }
        }

        private void UpdateVolumes()
        {
            if (_bgmSource != null && _currentBGM != null)
            {
                _bgmSource.volume = _currentBGM.volume * _bgmVolume * _masterVolume;
            }
            else if (_bgmSource != null)
            {
                _bgmSource.volume = _bgmVolume * _masterVolume;
            }

            if (_bgmSource2 != null && _currentSecondaryBGM != null)
            {
                _bgmSource2.volume = _currentSecondaryBGM.volume * _secondaryBgmVolume * _bgmVolume * _masterVolume;
            }
            else if (_bgmSource2 != null)
            {
                _bgmSource2.volume = _secondaryBgmVolume * _bgmVolume * _masterVolume;
            }
        }

        #region BGM Management (Dual / Simultaneous BGM Channels)

        public void PlayMenuBGM(float fadeDuration = 0.5f)
        {
            if (!string.IsNullOrEmpty(_defaultMenuBgmId))
            {
                PlayBGM(_defaultMenuBgmId, fadeDuration);
                StopSecondaryBGM(fadeDuration);
            }
        }

        public void PlayGameBGM(float fadeDuration = 0.5f)
        {
            if (!string.IsNullOrEmpty(_defaultGameBgmId))
            {
                PlayBGM(_defaultGameBgmId, fadeDuration);
            }
        }

        /// <summary>
        /// Play BGM on channel 0 (Primary) or channel 1 (Secondary).
        /// </summary>
        public void PlayBGM(string id, int channel, float fadeDuration = 0.5f)
        {
            if (channel == 1)
            {
                PlaySecondaryBGM(id, syncTime: true, fadeDuration);
            }
            else
            {
                PlayBGM(id, fadeDuration);
            }
        }

        /// <summary>
        /// Plays Primary BGM (Channel 0).
        /// </summary>
        public void PlayBGM(string id, float fadeDuration = 0.5f)
        {
            if (_bgmDict.TryGetValue(id, out Sound sound))
            {
                PlayBGM(sound, fadeDuration);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] BGM ID '{id}' not found in configuration.");
            }
        }

        /// <summary>
        /// Plays Secondary BGM (Channel 1) simultaneously with Primary BGM.
        /// </summary>
        public void PlaySecondaryBGM(string id, bool syncTime = true, float fadeDuration = 0.5f)
        {
            if (_bgmDict.TryGetValue(id, out Sound sound))
            {
                PlaySecondaryBGM(sound, syncTime, fadeDuration);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Secondary BGM ID '{id}' not found in configuration.");
            }
        }

        public void PlayBGM(AudioClip clip, float volume = 1.0f, float fadeDuration = 0.5f)
        {
            if (clip == null) return;
            Sound customSound = new Sound
            {
                id = clip.name,
                clip = clip,
                volume = volume,
                pitch = 1.0f
            };
            PlayBGM(customSound, fadeDuration);
        }

        public void PlaySecondaryBGM(AudioClip clip, bool syncTime = true, float volume = 1.0f, float fadeDuration = 0.5f)
        {
            if (clip == null) return;
            Sound customSound = new Sound
            {
                id = clip.name,
                clip = clip,
                volume = volume,
                pitch = 1.0f
            };
            PlaySecondaryBGM(customSound, syncTime, fadeDuration);
        }

        public void PlayBGM(Sound sound, float fadeDuration = 0.5f)
        {
            if (sound == null || sound.clip == null) return;

            if (_currentBGM != null && _currentBGM.clip == sound.clip && _bgmSource.isPlaying)
            {
                return;
            }

            _currentBGM = sound;

            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
            }

            if (fadeDuration > 0f && _bgmSource.isPlaying)
            {
                _bgmFadeCoroutine = StartCoroutine(FadeToNewBGM(_bgmSource, sound, _bgmVolume, fadeDuration, () => _currentBGM = sound));
            }
            else
            {
                _bgmSource.clip = sound.clip;
                _bgmSource.pitch = sound.pitch;
                _bgmSource.volume = sound.volume * _bgmVolume * _masterVolume;
                _bgmSource.Play();
            }
        }

        public void PlaySecondaryBGM(Sound sound, bool syncTime = true, float fadeDuration = 0.5f)
        {
            if (sound == null || sound.clip == null) return;

            if (_currentSecondaryBGM != null && _currentSecondaryBGM.clip == sound.clip && _bgmSource2.isPlaying)
            {
                return;
            }

            _currentSecondaryBGM = sound;

            if (_bgmFadeCoroutine2 != null)
            {
                StopCoroutine(_bgmFadeCoroutine2);
            }

            if (fadeDuration > 0f && _bgmSource2.isPlaying)
            {
                _bgmFadeCoroutine2 = StartCoroutine(FadeToNewBGM(_bgmSource2, sound, _secondaryBgmVolume * _bgmVolume, fadeDuration, () => _currentSecondaryBGM = sound, syncTime ? _bgmSource : null));
            }
            else
            {
                _bgmSource2.clip = sound.clip;
                _bgmSource2.pitch = sound.pitch;
                _bgmSource2.volume = sound.volume * _secondaryBgmVolume * _bgmVolume * _masterVolume;

                if (syncTime && _bgmSource != null && _bgmSource.isPlaying && sound.clip.length > 0)
                {
                    _bgmSource2.time = _bgmSource.time % sound.clip.length;
                }

                _bgmSource2.Play();
            }
        }

        public void StopBGM(float fadeDuration = 0.5f)
        {
            if (!_bgmSource.isPlaying) return;

            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
            }

            if (fadeDuration > 0f)
            {
                _bgmFadeCoroutine = StartCoroutine(FadeOutBGM(_bgmSource, fadeDuration, () => _currentBGM = null));
            }
            else
            {
                _bgmSource.Stop();
                _bgmSource.clip = null;
                _currentBGM = null;
            }
        }

        public void StopSecondaryBGM(float fadeDuration = 0.5f)
        {
            if (!_bgmSource2.isPlaying) return;

            if (_bgmFadeCoroutine2 != null)
            {
                StopCoroutine(_bgmFadeCoroutine2);
            }

            if (fadeDuration > 0f)
            {
                _bgmFadeCoroutine2 = StartCoroutine(FadeOutBGM(_bgmSource2, fadeDuration, () => _currentSecondaryBGM = null));
            }
            else
            {
                _bgmSource2.Stop();
                _bgmSource2.clip = null;
                _currentSecondaryBGM = null;
            }
        }

        public void StopAllBGM(float fadeDuration = 0.5f)
        {
            StopBGM(fadeDuration);
            StopSecondaryBGM(fadeDuration);
        }

        public void PauseAllBGM()
        {
            if (_bgmSource.isPlaying) _bgmSource.Pause();
            if (_bgmSource2.isPlaying) _bgmSource2.Pause();
        }

        public void ResumeAllBGM()
        {
            if (_currentBGM != null && !_bgmSource.isPlaying) _bgmSource.UnPause();
            if (_currentSecondaryBGM != null && !_bgmSource2.isPlaying) _bgmSource2.UnPause();
        }

        private IEnumerator FadeToNewBGM(AudioSource source, Sound newSound, float channelVolumeMultiplier, float duration, Action onComplete, AudioSource syncSource = null)
        {
            float startVolume = source.volume;
            float timer = 0f;

            // Fade out current track
            while (timer < duration / 2f)
            {
                timer += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, timer / (duration / 2f));
                yield return null;
            }

            // Switch clip
            source.clip = newSound.clip;
            source.pitch = newSound.pitch;

            if (syncSource != null && syncSource.isPlaying && newSound.clip.length > 0)
            {
                source.time = syncSource.time % newSound.clip.length;
            }

            source.Play();

            // Fade in new track
            timer = 0f;
            float targetVolume = newSound.volume * channelVolumeMultiplier * _masterVolume;
            while (timer < duration / 2f)
            {
                timer += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, timer / (duration / 2f));
                yield return null;
            }

            source.volume = targetVolume;
            onComplete?.Invoke();
        }

        private IEnumerator FadeOutBGM(AudioSource source, float duration, Action onComplete)
        {
            float startVolume = source.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }

            source.Stop();
            source.clip = null;
            onComplete?.Invoke();
        }

        #endregion

        #region SFX Management

        public void PlaySFX(string id)
        {
            if (_sfxDict.TryGetValue(id, out Sound sound))
            {
                PlaySFX(sound);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX ID '{id}' not found in configuration.");
            }
        }

        public void PlaySFX(string id, Vector3 position)
        {
            if (_sfxDict.TryGetValue(id, out Sound sound))
            {
                PlaySFXAtPosition(sound, position);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX ID '{id}' not found in configuration.");
            }
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1.0f, float pitch = 1.0f)
        {
            if (clip == null) return;
            Sound customSound = new Sound
            {
                id = clip.name,
                clip = clip,
                volume = volumeScale,
                pitch = pitch
            };
            PlaySFX(customSound);
        }

        public void PlaySFX(Sound sound)
        {
            if (sound == null || sound.clip == null) return;

            float calculatedVolume = sound.volume * _sfxVolume * _masterVolume;
            float calculatedPitch = sound.pitch;

            if (sound.randomizePitch)
            {
                calculatedPitch += UnityEngine.Random.Range(-sound.pitchVariance, sound.pitchVariance);
            }

            _sfxSource.pitch = calculatedPitch;
            _sfxSource.PlayOneShot(sound.clip, calculatedVolume);
        }

        public void PlaySFXAtPosition(Sound sound, Vector3 position)
        {
            if (sound == null || sound.clip == null) return;

            float calculatedVolume = sound.volume * _sfxVolume * _masterVolume;
            float calculatedPitch = sound.pitch;

            if (sound.randomizePitch)
            {
                calculatedPitch += UnityEngine.Random.Range(-sound.pitchVariance, sound.pitchVariance);
            }

            GameObject tempGO = new GameObject($"TempAudio_{sound.clip.name}");
            tempGO.transform.position = position;

            AudioSource tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = sound.clip;
            tempSource.volume = calculatedVolume;
            tempSource.pitch = calculatedPitch;
            tempSource.spatialBlend = 1.0f;
            tempSource.rolloffMode = AudioRolloffMode.Logarithmic;
            tempSource.minDistance = 1f;
            tempSource.maxDistance = 50f;
            tempSource.Play();

            Destroy(tempGO, sound.clip.length / Mathf.Max(0.01f, Mathf.Abs(calculatedPitch)));
        }

        #endregion

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                InitializeDictionaries();
                UpdateVolumes();
            }
        }
    }
}
