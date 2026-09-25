using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSFX : MonoBehaviour
{
    [SerializeField]
    private EventReference _sfxEvent;

    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            RuntimeManager.PlayOneShot(_sfxEvent);            
        });
    }
}
