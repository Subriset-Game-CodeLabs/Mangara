using Player;
using TMPro;
using UnityEngine;

public class UIInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInteractor _playerInteractor;
    [SerializeField] private TMP_Text _interactText;
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 0);

    private IInteractable _currentInteractable;
    private Transform _targetTransform;

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

            if (_targetTransform != null && Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(_targetTransform.position + _offset);
                if (screenPos.z > 0)
                {
                    _interactText.transform.position = screenPos;
                }
            }
        }
    }

    private void Show(IInteractable interactable)
    {
        _currentInteractable = interactable;
        _targetTransform = (interactable as MonoBehaviour)?.transform;
        _interactText.text = interactable.GetInteractText();
        _interactText.gameObject.SetActive(true);
    }

    private void Hide(IInteractable interactable)
    {
        if (_currentInteractable == interactable)
        {
            _currentInteractable = null;
            _targetTransform = null;
            _interactText.gameObject.SetActive(false);
        }
    }
}
