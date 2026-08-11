using System;
using Input;
using UnityEngine;

namespace Player
{
    public class PlayerInteractor: MonoBehaviour
    {
        private IInteractable _currentInteractable;

        public event Action<IInteractable> OnInteractionFound,  OnInteractionLost;
        
        private void OnEnable()
        {
            InputManager.Instance.PlayerInput.Interact.OnDown += OnInteract;
            InputManager.Instance.UIInput.Submit.OnDown += OnInteract;
        }

        private void OnDisable()
        {
            InputManager.Instance.PlayerInput.Interact.OnDown -= OnInteract;
            InputManager.Instance.UIInput.Submit.OnDown -= OnInteract;
        }

        private void Update()
        {
            if (_currentInteractable != null && _currentInteractable is UnityEngine.Object obj && obj == null)
            {
                IInteractable lost = _currentInteractable;
                _currentInteractable = null;
                OnInteractionLost?.Invoke(lost);
            }
        }

        private void OnInteract()
        {
            if (_currentInteractable != null)
            {
                IInteractable interactable = _currentInteractable;
                interactable.Interact();

                if (_currentInteractable is UnityEngine.Object obj && obj == null)
                {
                    _currentInteractable = null;
                    OnInteractionLost?.Invoke(interactable);
                }
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IInteractable interactable))
            {
                _currentInteractable = interactable;
                OnInteractionFound?.Invoke(interactable);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IInteractable interactable))
            {
                if (_currentInteractable == interactable)
                {
                    _currentInteractable = null;
                    OnInteractionLost?.Invoke(interactable);
                }
            }
        }
    }
}