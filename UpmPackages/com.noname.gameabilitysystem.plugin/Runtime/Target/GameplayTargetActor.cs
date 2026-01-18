using System;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 타겟 데이터를 수집하는 액터의 기본 클래스입니다.
    /// </summary>
    public abstract class GameplayTargetActor : MonoBehaviour
    {
        private GameplayTargetActorConfig _config;

        /// <summary>
        /// 연결된 설정입니다.
        /// </summary>
        public GameplayTargetActorConfig Config => _config;

        /// <summary>
        /// 타겟팅을 수행하는 소유자입니다.
        /// </summary>
        public AbilitySystemComponent Owner { get; private set; }

        /// <summary>
        /// 타겟 데이터가 준비되면 호출됩니다.
        /// </summary>
        public event Action<AbilityTargetData> TargetDataReady;

        /// <summary>
        /// 타겟팅이 취소되면 호출됩니다.
        /// </summary>
        public event Action TargetingCancelled;

        /// <summary>
        /// 설정을 초기화합니다.
        /// </summary>
        /// <param name="config">타겟 액터 설정</param>
        public void Initialize(GameplayTargetActorConfig config)
        {
            // 설정을 보관하고 초기화 훅을 호출한다.
            _config = config;
            OnInitialized();
        }

        /// <summary>
        /// 초기화 이후에 호출되는 훅입니다.
        /// </summary>
        protected virtual void OnInitialized()
        {
            // 필요 시 파생 클래스에서 구현한다.
        }

        /// <summary>
        /// 타겟팅을 시작합니다.
        /// </summary>
        /// <param name="owner">소유자</param>
        public void StartTargeting(AbilitySystemComponent owner)
        {
            // 소유자를 저장하고 시작 훅을 호출한다.
            Owner = owner;
            OnStartTargeting();
        }

        /// <summary>
        /// 타겟팅 시작 시점 훅입니다.
        /// </summary>
        protected virtual void OnStartTargeting()
        {
            // 필요 시 파생 클래스에서 구현한다.
        }

        /// <summary>
        /// 타겟 데이터를 확정합니다.
        /// </summary>
        /// <param name="data">완성된 타겟 데이터</param>
        public void ConfirmTargetData(AbilityTargetData data)
        {
            // 준비된 데이터를 외부로 전달한다.
            TargetDataReady?.Invoke(data);
        }

        /// <summary>
        /// 타겟팅을 취소합니다.
        /// </summary>
        public void CancelTargeting()
        {
            // 취소 이벤트를 전파한다.
            TargetingCancelled?.Invoke();
        }

        /// <summary>
        /// 타겟 데이터를 수집합니다.
        /// </summary>
        /// <param name="owner">소유자</param>
        /// <returns>수집된 타겟 데이터</returns>
        public abstract AbilityTargetData AcquireTargetData(AbilitySystemComponent owner);

        /// <summary>
        /// 기준 위치를 계산합니다.
        /// </summary>
        /// <param name="owner">소유자</param>
        /// <returns>기준 위치</returns>
        protected Vector3 ResolveOrigin(AbilitySystemComponent owner)
        {
            if (owner == null || _config == null)
            {
                return Vector3.zero;
            }

            if (string.IsNullOrEmpty(_config.AnchorKey))
            {
                // 앵커가 없으면 소유자 기준 오프셋을 쓴다.
                return owner.transform.TransformPoint(_config.CenterOffset);
            }

            var anchor = owner.transform.Find(_config.AnchorKey);
            if (anchor == null)
            {
                // 앵커를 못 찾으면 소유자 기준으로 대체한다.
                Debug.LogWarning($"Anchor '{_config.AnchorKey}' not found on owner {owner.name}");
                return owner.transform.TransformPoint(_config.CenterOffset);
            }

            return anchor.TransformPoint(_config.CenterOffset);
        }

        /// <summary>
        /// 최대 타겟 수 제한을 적용합니다.
        /// </summary>
        /// <param name="count">현재 타겟 수</param>
        /// <returns>제한 적용 후 타겟 수</returns>
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
