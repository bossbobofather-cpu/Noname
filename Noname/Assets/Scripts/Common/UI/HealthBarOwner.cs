using UnityEngine;
using Noname.GameAbilitySystem;

namespace MyProject.Common.UI
{
    [DisallowMultipleComponent]
    public sealed class HealthBarOwner : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private AbilitySystemComponent _abilitySystem;
        [SerializeField] private Transform _followTarget;
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private AttributeId _healthAttribute = AttributeId.Health;

        [Header("UI")]
        [SerializeField] private ScreenSpaceHealthBarPool _pool;
        [SerializeField] private Camera _camera;
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
