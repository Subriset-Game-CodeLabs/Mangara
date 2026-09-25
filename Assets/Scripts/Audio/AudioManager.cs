using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager
{

    const string MusicBus = "bus:/Music";
    const string SfxBus = "bus:/SFX";
    const string MusicKey = "MusicVolume";
    const string SfxKey = "SfxVolume";

    static EventInstance music;
    static EventReference currentMusic;
    static bool hasMusic;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void init()
    {
        ApplyVolume(MusicBus, GetMusicVolume());
        ApplyVolume(SfxBus, GetSfxVolume());
    }

    public static float GetMusicVolume() => PlayerPrefs.GetFloat(MusicKey, 1f);
    public static float GetSfxVolume() => PlayerPrefs.GetFloat(SfxKey, 1f);

    public static void SetMusicVolume(float v)
    {
        PlayerPrefs.SetFloat(MusicKey, v);
        ApplyVolume(MusicBus, v);
    }

    public static void SetSfxVolume(float v)
    {
        PlayerPrefs.SetFloat(SfxKey, v);
        ApplyVolume(SfxBus, v);
    }

    static void ApplyVolume(string busPath, float value)
    {
        RuntimeManager.GetBus(busPath).setVolume(value);
    }

    public static void PlayMusic(EventReference ev, string paramName, string label)
    {
        if (hasMusic && currentMusic.Guid.Equals(ev.Guid))
        {
            SetMusicLabel(paramName, label);
            return;
        }

        StopMusic();

        music = RuntimeManager.CreateInstance(ev);
        music.setParameterByNameWithLabel(paramName, label, true);
        music.start();
        currentMusic = ev;
        hasMusic = true;
    }

    public static void SetMusicLabel(string paramName, string label)
    {
        if (!hasMusic) return;
        music.setParameterByNameWithLabel(paramName, label);
    }

    public static void StopMusic()
    {
        if (!hasMusic) return;
        music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        music.release();
        hasMusic = false;
    }

}
