using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace Noname.GameAbilitySystem.DebugTool
{
    /// <summary>
    /// 태그와 효과를 자동 적용하는 트리거 볼륨입니다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class AbilitySystemTriggerVolume : MonoBehaviour
    {
        [SerializeField] private GameplayTagContainer _grantedTags = new GameplayTagContainer();
        [SerializeField] private List<GameplayEffectConfig> _grantedEffects = new();
        [SerializeField] private LayerMask _targetLayers = ~0;
        [SerializeField] private bool _applyOnEnter = true;
        [SerializeField] private bool _removeOnExit = true;

        [SerializeField] private string _enterMessage;
        [SerializeField] private string _exitMessage;

        private readonly Dictionary<AbilitySystemComponent, int> _overlaps = new();
        private readonly List<AbilitySystemComponent> _cleanupTargets = new();

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                // 트리거로 강제 설정한다.
                col.isTrigger = true;
            }
        }

        private void OnValidate()
        {
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                // 에디터에서 트리거를 유지한다.
                col.isTrigger = true;
            }
        }

        private void OnDisable()
        {
            if (!_removeOnExit || _overlaps.Count == 0)
            {
                _overlaps.Clear();
                return;
            }

            // 비활성화 시점에 남은 대상들을 정리한다.
            _cleanupTargets.Clear();
            foreach (var pair in _overlaps)
            {
                if (pair.Key != null)
                {
                    _cleanupTargets.Add(pair.Key);
                }
            }

            for (var i = 0; i < _cleanupTargets.Count; i++)
            {
                RemoveFrom(_cleanupTargets[i]);
            }

            _overlaps.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_applyOnEnter)
            {
                return;
            }

            var abilitySystem = FindAbilitySystem(other);
            if (abilitySystem == null)
            {
                return;
            }

            if (_overlaps.TryGetValue(abilitySystem, out var count))
            {
                _overlaps[abilitySystem] = count + 1;
                return;
            }

            _overlaps.Add(abilitySystem, 1);
            ApplyTo(abilitySystem);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_removeOnExit || _overlaps.Count == 0)
            {
                return;
            }

            var abilitySystem = FindAbilitySystem(other);
            if (abilitySystem == null)
            {
                return;
            }

            if (!_overlaps.TryGetValue(abilitySystem, out var count))
            {
                return;
            }

            count--;
            if (count <= 0)
            {
                _overlaps.Remove(abilitySystem);
                RemoveFrom(abilitySystem);
            }
            else
            {
                _overlaps[abilitySystem] = count;
            }
        }

        private AbilitySystemComponent FindAbilitySystem(Collider other)
        {
            if (((1 << other.gameObject.layer) & _targetLayers.value) == 0)
            {
                return null;
            }

            var provider = other.GetComponentInParent<IAbilitySystemProvider>();
            if (provider == null)
            {
                return null;
            }

            return provider.GetAbilitySystemComponent();
        }

        private void ApplyTo(AbilitySystemComponent abilitySystem)
        {
            var tags = _grantedTags.Tags;
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                if (!tag.IsValid)
                {
                    continue;
                }

                abilitySystem.AddLooseTag(tag);
            }

            for (var i = 0; i < _grantedEffects.Count; i++)
            {
                var effect = _grantedEffects[i];
                if (effect == null)
                {
                    continue;
                }

                abilitySystem.ApplyGameplayEffect(effect);
            }
        }

        private void RemoveFrom(AbilitySystemComponent abilitySystem)
        {
            var tags = _grantedTags.Tags;
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                if (!tag.IsValid)
                {
                    continue;
                }

                abilitySystem.RemoveLooseTag(tag);
            }

            for (var i = 0; i < _grantedEffects.Count; i++)
            {
                var effect = _grantedEffects[i];
                if (effect == null)
                {
                    continue;
                }

                abilitySystem.RemoveGameplayEffect(effect);
            }
        }
    }
}
