using DG.Tweening;
using UnityEngine;

namespace Mangrove
{
    public class PlantWaterIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MangroveController _mangroveController;

        [Header("Bobbing Animation")]
        [SerializeField] private float _bobSpeed = 3f;
        [SerializeField] private float _bobHeight = 0.15f;

        [Header("Juice & Transitions")]
        [SerializeField] private float _popDuration = 0.35f;
        [SerializeField] private bool _billboardToCamera = true;

        private Vector3 _initialLocalPosition;
        private Vector3 _targetScale;
        private Tween _scaleTween;
        private bool _isVisible;

        private void Awake()
        {
            if (_mangroveController == null)
            {
                _mangroveController = GetComponentInParent<MangroveController>();
                if (_mangroveController == null)
                {
                    _mangroveController = GetComponent<MangroveController>();
                }
            }

            _initialLocalPosition = transform.localPosition;
            _targetScale = transform.localScale;
            if (_targetScale == Vector3.zero)
            {
                _targetScale = Vector3.one;
            }

            transform.localScale = Vector3.zero;
        }

        private void OnEnable()
        {
            if (_mangroveController != null)
            {
                _mangroveController.OnStatusChanged += UpdateStatus;
            }
            UpdateStatus();
        }

        private void OnDisable()
        {
            if (_mangroveController != null)
            {
                _mangroveController.OnStatusChanged -= UpdateStatus;
            }
            _scaleTween?.Kill();
        }

        private void LateUpdate()
        {
            if (!_isVisible) return;

            // Smooth floating bobbing animation up and down
            float newY = _initialLocalPosition.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            transform.localPosition = new Vector3(_initialLocalPosition.x, newY, _initialLocalPosition.z);

            // Always face camera (Billboard)
            if (_billboardToCamera && Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        public void UpdateStatus()
        {
            if (_mangroveController == null) return;

            bool needsWater = _mangroveController.NeedsWater;

            if (needsWater && !_isVisible)
            {
                Show();
            }
            else if (!needsWater && _isVisible)
            {
                Hide();
            }
        }

        private void Show()
        {
            _isVisible = true;
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_targetScale, _popDuration).SetEase(Ease.OutBack);
        }

        private void Hide()
        {
            _isVisible = false;
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(Vector3.zero, _popDuration * 0.8f)
                .SetEase(Ease.InBack);
        }
    }
}
