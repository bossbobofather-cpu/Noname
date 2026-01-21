using UnityEngine;
using Noname.GameAbilitySystem;

namespace MyProject.Common.UI
{
    /// <summary>
    /// 대상의 체력바를 관리하는 컴포넌트입니다.
    /// 화면 공간(Screen Space) 체력바를 풀에서 가져와 연결합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HealthBarOwner : MonoBehaviour
    {
        [Header("Target")]
        /// <summary>
        /// 체력 정보를 가져올 AbilitySystem 컴포넌트입니다.
        /// </summary>
        [SerializeField] private AbilitySystemComponent _abilitySystem;

        /// <summary>
        /// 체력바가 따라다닐 대상의 트랜스폼입니다.
        /// </summary>
        [SerializeField] private Transform _followTarget;

        /// <summary>
        /// 대상 위치로부터 체력바가 표시될 월드 공간 오프셋입니다.
        /// </summary>
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);

        /// <summary>
        /// 표시할 체력 속성의 ID입니다.
        /// </summary>
        [SerializeField] private AttributeId _healthAttribute = AttributeId.Health;

        [Header("UI")]
        /// <summary>
        /// 체력바를 가져올 풀입니다.
        /// </summary>
        [SerializeField] private ScreenSpaceHealthBarPool _pool;

        /// <summary>
        /// 월드 좌표를 스크린 좌표로 변환할 때 사용할 카메라입니다.
        /// </summary>
        [SerializeField] private Camera _camera;

        /// <summary>
        /// 활성화 시 자동으로 체력바를 표시할지 여부입니다.
        /// </summary>
        [SerializeField] private bool _autoShow = true;

        private ScreenSpaceHealthBar _activeBar;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            if (_autoShow)
            {
                Show();
            }
        }

        private void OnDisable()
        {
            Hide();
        }

        private void OnDestroy()
        {
            Hide();
        }

        /// <summary>
        /// 체력바를 표시합니다.
        /// </summary>
        public void Show()
        {
            if (_activeBar != null)
            {
                return;
            }

            ResolveReferences();

            var pool = ResolvePool();
            if (pool == null)
            {
                return;
            }

            var bar = pool.Acquire();
            if (bar == null)
            {
                return;
            }

            bar.Bind(_abilitySystem, _followTarget, pool.Canvas, _camera, _worldOffset, _healthAttribute);
            _activeBar = bar;
        }

        /// <summary>
        /// 표시된 체력바를 숨기고 반환합니다.
        /// </summary>
        public void Hide()
        {
            if (_activeBar == null)
            {
                return;
            }

            var pool = ResolvePool();
            if (pool != null)
            {
                pool.Release(_activeBar);
            }
            else
            {
                _activeBar.Unbind();
                _activeBar.gameObject.SetActive(false);
            }

            _activeBar = null;
        }

        private void ResolveReferences()
        {
            if (_abilitySystem == null)
            {
                // 부모에서 AbilitySystemComponent를 찾음
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
                _followTarget = _abilitySystem != null ? _abilitySystem.transform : transform;
            }
        }

        private ScreenSpaceHealthBarPool ResolvePool()
        {
            if (_pool == null)
            {
                _pool = ScreenSpaceHealthBarPool.Instance;
                if (_pool == null)
                {
                    _pool = FindFirstObjectByType<ScreenSpaceHealthBarPool>();
                }
            }

            return _pool;
        }
    }
}
