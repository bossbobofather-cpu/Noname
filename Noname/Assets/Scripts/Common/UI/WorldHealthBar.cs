using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Noname.GameAbilitySystem;

namespace MyProject.Common.UI
{
    [DisallowMultipleComponent]
    public sealed class WorldHealthBar : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _followTarget;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);

        [Header("Health")]
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        [SerializeField] private AttributeId _healthAttribute = AttributeId.Health;
        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _valueLabel;
        [SerializeField] private bool _showValue = true;
        [SerializeField] private float _refreshInterval = 0.1f;
        [SerializeField] private float _smoothSpeed = 0f;

        [Header("Facing")]
        [SerializeField] private bool _faceCamera = true;
        [SerializeField] private bool _lockYRotation = true;
        [SerializeField] private Camera _camera;

        private float _nextRefreshTime;
        private float _targetFill = 1f;

        private void Awake()
        {
            ResolveTargets();
            UpdateHealth(true);
        }

        private void OnEnable()
        {
            ResolveTargets();
            _nextRefreshTime = 0f;
        }

        private void LateUpdate()
        {
            UpdatePosition();

            if (_faceCamera)
            {
                FaceCamera();
            }

            if (Time.unscaledTime >= _nextRefreshTime)
            {
                _nextRefreshTime = Time.unscaledTime + Mathf.Max(0.02f, _refreshInterval);
                UpdateHealth(false);
            }

            UpdateFill();
        }

        private void ResolveTargets()
        {
            if (_abilitySystem == null)
            {
                var provider = GetComponentInParent<IAbilitySystemProvider>();
                if (provider != null)
                {
                    _abilitySystem = provider.GetAbilitySystemComponent();
                }
                else
                {
                    _abilitySystem = GetComponentInParent<AbilitySystemComponent>();
                }
            }

            if (_followTarget == null)
            {
                _followTarget = _abilitySystem != null ? _abilitySystem.transform : transform.parent;
            }
        }

        private void UpdatePosition()
        {
            if (_followTarget == null)
            {
                return;
            }

            transform.position = _followTarget.position + _worldOffset;
        }

        private void UpdateHealth(bool force)
        {
            if (_abilitySystem == null || _fillImage == null)
            {
                return;
            }

            if (!_abilitySystem.Attributes.TryGet(_healthAttribute, out var health) || health == null)
            {
                if (force)
                {
                    _fillImage.fillAmount = 0f;
                }
                return;
            }

            var min = health.MinValue;
            var max = health.MaxValue;
            if (max <= min)
            {
                max = Mathf.Max(health.BaseValue, health.CurrentValue, 1f);
                min = 0f;
            }

            var current = Mathf.Clamp(health.CurrentValue, min, max);
            _targetFill = Mathf.InverseLerp(min, max, current);

            if (_valueLabel != null)
            {
                _valueLabel.gameObject.SetActive(_showValue);
                if (_showValue)
                {
                    _valueLabel.text = $"{current:0}/{max:0}";
                }
            }
        }

        private void UpdateFill()
        {
            if (_fillImage == null)
            {
                return;
            }

            if (_smoothSpeed > 0f)
            {
                _fillImage.fillAmount = Mathf.MoveTowards(
                    _fillImage.fillAmount,
                    _targetFill,
                    _smoothSpeed * Time.unscaledDeltaTime);
            }
            else
            {
                _fillImage.fillAmount = _targetFill;
            }
        }

        private void FaceCamera()
        {
            var camera = _camera != null ? _camera : Camera.main;
            if (camera == null)
            {
                return;
            }

            var direction = camera.transform.position - transform.position;
            if (_lockYRotation)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}
