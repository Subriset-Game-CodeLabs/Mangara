using Player;
using TMPro;
using UnityEngine;

public class UIInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInteractor _playerInteractor;
    [SerializeField] private TMP_Text _interactText;

    private IInteractable _currentInteractable;

    private void Start()
    {
        _interactText.text = "";
        _interactText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _playerInteractor.OnInteractionFound += Show;
        _playerInteractor.OnInteractionLost += Hide;
    }

    private void OnDisable()
    {
        _playerInteractor.OnInteractionFound -= Show;
        _playerInteractor.OnInteractionLost -= Hide;
    }

    private void Update()
    {
        if (_currentInteractable != null && _interactText.gameObject.activeSelf)
        {
            _interactText.text = _currentInteractable.GetInteractText();
        }
    }

    private void Show(IInteractable interactable)
    {
        _currentInteractable = interactable;
        _interactText.text = interactable.GetInteractText();
        _interactText.gameObject.SetActive(true);
    }

    private void Hide(IInteractable interactable)
    {
        if (_currentInteractable == interactable)
        {
            _currentInteractable = null;
            _interactText.gameObject.SetActive(false);
        }
    }
}
