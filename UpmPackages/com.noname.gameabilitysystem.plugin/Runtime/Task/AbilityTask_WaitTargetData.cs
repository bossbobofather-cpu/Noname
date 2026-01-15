using System;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    public sealed class AbilityTask_WaitTargetData : AbilityTask
    {
        private GameplayTargetConfig _config;
        private GameplayTargetActor _targetActor;
        private GameObject _reticleInstance;

        public event Action<AbilityTargetData> TargetDataReady;
        public event Action TargetDataCancelled;

        public static AbilityTask_WaitTargetData Create(
            IAbilityTaskOwner owner,
            GameplayTargetConfig config)
        {
            var task = new AbilityTask_WaitTargetData
            {
                _config = config
            };

            task.Initialize(owner);
            return task;
        }

        protected override void OnActivate()
        {
            if (ASC == null || _config == null || _config.TargetActorConfig == null)
            {
                EndTask();
                return;
            }

            _targetActor = _config.SpawnTargetActor(ASC);
            if (_targetActor == null)
            {
                EndTask();
                return;
            }

            if (_config.ReticlePrefab != null)
            {
                _reticleInstance = UnityEngine.Object.Instantiate(_config.ReticlePrefab, _targetActor.transform);
            }

            _targetActor.TargetDataReady += HandleTargetDataReady;
            _targetActor.TargetingCancelled += HandleTargetDataCancelled;

            if (_config.ConfirmationMode == TargetConfirmationMode.Instant)
            {
                var data = _targetActor.AcquireTargetData(ASC);
                HandleTargetDataReady(data);
                return;
            }

            _targetActor.StartTargeting(ASC);
        }

        public void ConfirmTargeting()
        {
            if (!IsActive || _targetActor == null)
            {
                return;
            }

            var data = _targetActor.AcquireTargetData(ASC);
            HandleTargetDataReady(data);
        }

        public void CancelTargeting()
        {
            if (!IsActive)
            {
                return;
            }

            HandleTargetDataCancelled();
        }

        private void HandleTargetDataReady(AbilityTargetData data)
        {
            if (!IsActive)
            {
                return;
            }

            if (data != null)
            {
                UpdateContextTargetData(data);
            }

            TargetDataReady?.Invoke(data);
            EndTask();
        }

        private void HandleTargetDataCancelled()
        {
            if (!IsActive)
            {
                return;
            }

            TargetDataCancelled?.Invoke();
            Cancel();
        }

        protected override void OnEnd()
        {
            CleanupTargetActor();
        }

        protected override void OnCancel()
        {
            CleanupTargetActor();
        }

        private void CleanupTargetActor()
        {
            if (_targetActor != null)
            {
                _targetActor.TargetDataReady -= HandleTargetDataReady;
                _targetActor.TargetingCancelled -= HandleTargetDataCancelled;
            }

            if (_reticleInstance != null)
            {
                UnityEngine.Object.Destroy(_reticleInstance);
                _reticleInstance = null;
            }

            if (_targetActor != null && _config != null)
            {
                _config.ReleaseTargetActor(_targetActor);
            }

            _targetActor = null;
        }
    }
}
