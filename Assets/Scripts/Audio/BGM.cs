using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGM : MonoBehaviour
{
    [SerializeField] EventReference bgmEvent;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MVP")
        {
            AudioManager.PlayMusic(bgmEvent, "Location", "Pantai");
        }
        else
        {
            AudioManager.PlayMusic(bgmEvent, "Location", "Normal");
        }
    }

    void OnDestroy()
    {
        AudioManager.StopMusic();
    }
}
