using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Noname.GameAbilitySystem;

namespace MyProject.Common.UI
{
    /// <summary>
    /// 월드 공간(World Space)에서 대상을 따라다니는 체력바 UI입니다.
    /// 대상의 바로 위에 떠 있는 체력바 등을 구현할 때 사용됩니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldHealthBar : MonoBehaviour
    {
        [Header("Target")]
        /// <summary>
        /// 체력바가 따라다닐 대상입니다.
        /// </summary>
        [SerializeField] private Transform _followTarget;
        
        /// <summary>
        /// 대상 위치로부터의 오프셋입니다.
        /// </summary>
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);

        [Header("Health")]
        /// <summary>
        /// 체력 정보를 가져올 AbilitySystem입니다.
        /// </summary>
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        
        /// <summary>
        /// 표시할 체력 속성의 ID입니다.
        /// </summary>
        [SerializeField] private AttributeId _healthAttribute = AttributeId.Health;
        
        /// <summary>
        /// 체력 상태를 표시할 이미지입니다.
        /// </summary>
        [SerializeField] private Image _fillImage;
        
        /// <summary>
        /// 체력 수치를 표시할 텍스트입니다.
        /// </summary>
        [SerializeField] private TMP_Text _valueLabel;
        
        /// <summary>
        /// 체력 수치 텍스트 표시 여부입니다.
        /// </summary>
        [SerializeField] private bool _showValue = true;
        
        /// <summary>
        /// 체력 정보 갱신 간격(초)입니다.
        /// </summary>
        [SerializeField] private float _refreshInterval = 0.1f;
        
        /// <summary>
        /// 게이지 변화 시 부드러운 이동 속도입니다.
        /// </summary>
        [SerializeField] private float _smoothSpeed = 0f;

        [Header("Facing")]
        /// <summary>
        /// 항상 카메라를 바라볼지 여부입니다.
        /// </summary>
        [SerializeField] private bool _faceCamera = true;
        
        /// <summary>
        /// Y축 회전만 고정하여 빌보드 효과를 줄지 여부입니다.
        /// </summary>
        [SerializeField] private bool _lockYRotation = true;
        
        /// <summary>
        /// 바라볼 카메라입니다. 없으면 MainCamera를 사용합니다.
        /// </summary>
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

            // 최소/최대값 보정
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

            // 방향 벡터가 너무 작으면 회전 계산 생략
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}
