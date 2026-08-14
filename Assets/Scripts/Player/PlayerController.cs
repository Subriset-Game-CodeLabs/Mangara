using Input;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject bestiaryUI;

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PlayerInput.Bestiary.OnDown += ToggleBestiary;
            InputManager.Instance.UIInput.Bestiary.OnDown += ToggleBestiary;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PlayerInput.Bestiary.OnDown -= ToggleBestiary;
            InputManager.Instance.UIInput.Bestiary.OnDown -= ToggleBestiary;
        }
    }

    public void ToggleBestiary()
    {
        if (bestiaryUI == null) return;

        bool newState = !bestiaryUI.activeSelf;
        bestiaryUI.SetActive(newState);

        if (newState)
        {
            InputManager.Instance.UIMode();
            Time.timeScale = 0f;
        }
        else
        {
            InputManager.Instance.PlayerMode();
            Time.timeScale = 1f;
            }
    }
}