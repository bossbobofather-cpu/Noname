using System;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    public abstract class GameplayTargetActor : MonoBehaviour
    {
        private GameplayTargetActorConfig _config;

        public GameplayTargetActorConfig Config => _config;
        public AbilitySystemComponent Owner { get; private set; }

        public event Action<AbilityTargetData> TargetDataReady;
        public event Action TargetingCancelled;

        public void Initialize(GameplayTargetActorConfig config)
        {
            _config = config;
            OnInitialized();
        }

        protected virtual void OnInitialized()
        {
        }

        public void StartTargeting(AbilitySystemComponent owner)
        {
            Owner = owner;
            OnStartTargeting();
        }

        protected virtual void OnStartTargeting()
        {
        }

        public void ConfirmTargetData(AbilityTargetData data)
        {
            TargetDataReady?.Invoke(data);
        }

        public void CancelTargeting()
        {
            TargetingCancelled?.Invoke();
        }

        public abstract AbilityTargetData AcquireTargetData(AbilitySystemComponent owner);

        protected Vector3 ResolveOrigin(AbilitySystemComponent owner)
        {
            if (owner == null || _config == null)
            {
                return Vector3.zero;
            }

            if (string.IsNullOrEmpty(_config.AnchorKey))
            {
                return owner.transform.TransformPoint(_config.CenterOffset);
            }

            var anchor = owner.transform.Find(_config.AnchorKey);
            if (anchor == null)
            {
                Debug.LogWarning($"Anchor '{_config.AnchorKey}' not found on owner {owner.name}");
                return owner.transform.TransformPoint(_config.CenterOffset);
            }

            return anchor.TransformPoint(_config.CenterOffset);
        }

        protected int ClampMaxTargets(int count)
        {
            if (_config == null || _config.MaxTargets <= 0)
            {
                return count;
            }

            return Mathf.Min(_config.MaxTargets, count);
        }
    }
}
