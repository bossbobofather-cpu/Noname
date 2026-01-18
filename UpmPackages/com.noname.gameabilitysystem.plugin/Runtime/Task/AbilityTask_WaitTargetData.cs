using System;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 타겟 데이터를 기다리는 태스크입니다.
    /// </summary>
    public sealed class AbilityTask_WaitTargetData : AbilityTask
    {
        private GameplayTargetConfig _config;
        private GameplayTargetActor _targetActor;
        private GameObject _reticleInstance;

        /// <summary>
        /// 타겟 데이터가 준비되면 호출됩니다.
        /// </summary>
        public event Action<AbilityTargetData> TargetDataReady;

        /// <summary>
        /// 타겟팅이 취소되면 호출됩니다.
        /// </summary>
        public event Action TargetDataCancelled;

        /// <summary>
        /// 타겟 데이터 대기 태스크를 생성합니다.
        /// </summary>
        /// <param name="owner">태스크 소유자</param>
        /// <param name="config">타겟 설정</param>
        /// <returns>생성된 태스크</returns>
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
                // 필수 설정이 없으면 종료한다.
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
                // 조준 표시 프리팹을 생성한다.
                _reticleInstance = UnityEngine.Object.Instantiate(_config.ReticlePrefab, _targetActor.transform);
            }

            _targetActor.TargetDataReady += HandleTargetDataReady;
            _targetActor.TargetingCancelled += HandleTargetDataCancelled;

            if (_config.ConfirmationMode == TargetConfirmationMode.Instant)
            {
                // 즉시 확정이면 바로 데이터 수집한다.
                var data = _targetActor.AcquireTargetData(ASC);
                HandleTargetDataReady(data);
                return;
            }

            _targetActor.StartTargeting(ASC);
        }

        /// <summary>
        /// 사용자가 타겟팅을 확정합니다.
        /// </summary>
        public void ConfirmTargeting()
        {
            if (!IsActive || _targetActor == null)
            {
                return;
            }

            var data = _targetActor.AcquireTargetData(ASC);
            HandleTargetDataReady(data);
        }

        /// <summary>
        /// 타겟팅을 취소합니다.
        /// </summary>
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
