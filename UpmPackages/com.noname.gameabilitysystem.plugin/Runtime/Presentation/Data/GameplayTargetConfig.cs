using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayTargetConfig")]
    /// <summary>
    /// 타겟팅 동작을 구성하는 에셋입니다.
    /// </summary>
    public sealed class GameplayTargetConfig : GameplayConfig
    {
        [SerializeField] private GameplayTargetActorConfig _targetActorConfig;
        [SerializeField] private TargetConfirmationMode _confirmationMode = TargetConfirmationMode.Instant;
        [SerializeField] private GameObject _reticlePrefab;
        [SerializeField] private bool _attachActorToOwner = true;
        [SerializeField] private bool _destroyActorOnAcquire = true;
        [SerializeField] private List<GameplayEffectConfig> _effects = new();

        /// <summary>
        /// 타겟 액터 설정입니다.
        /// </summary>
        public GameplayTargetActorConfig TargetActorConfig => _targetActorConfig;

        /// <summary>
        /// 타겟 확정 방식입니다.
        /// </summary>
        public TargetConfirmationMode ConfirmationMode => _confirmationMode;

        /// <summary>
        /// 조준 표시 프리팹입니다.
        /// </summary>
        public GameObject ReticlePrefab => _reticlePrefab;

        /// <summary>
        /// 타겟 확정 후 적용할 효과 목록입니다.
        /// </summary>
        public IReadOnlyList<GameplayEffectConfig> Effects => _effects;

        /// <summary>
        /// 타겟 액터를 생성합니다.
        /// </summary>
        /// <param name="owner">소유자</param>
        /// <returns>생성된 타겟 액터</returns>
        public GameplayTargetActor SpawnTargetActor(AbilitySystemComponent owner)
        {
            if (_targetActorConfig == null)
            {
                // 설정이 없으면 생성하지 않는다.
                return null;
            }

            // 소유자에 붙일지 여부를 반영한다.
            var parent = _attachActorToOwner && owner != null ? owner.transform : null;
            return _targetActorConfig.Spawn(parent);
        }

        /// <summary>
        /// 타겟 액터를 반환합니다.
        /// </summary>
        /// <param name="actor">회수할 타겟 액터</param>
        public void ReleaseTargetActor(GameplayTargetActor actor)
        {
            if (actor == null || !_destroyActorOnAcquire)
            {
                // 회수 조건이 아니면 그대로 둔다.
                return;
            }

            Object.Destroy(actor.gameObject);
        }
    }
}
