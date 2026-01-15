using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayTargetConfig")]
    public sealed class GameplayTargetConfig : GameplayConfig
    {
        [SerializeField] private GameplayTargetActorConfig _targetActorConfig;
        [SerializeField] private TargetConfirmationMode _confirmationMode = TargetConfirmationMode.Instant;
        [SerializeField] private GameObject _reticlePrefab;
        [SerializeField] private bool _attachActorToOwner = true;
        [SerializeField] private bool _destroyActorOnAcquire = true;
        [SerializeField] private List<GameplayEffectConfig> _effects = new();

        public GameplayTargetActorConfig TargetActorConfig => _targetActorConfig;
        public TargetConfirmationMode ConfirmationMode => _confirmationMode;
        public GameObject ReticlePrefab => _reticlePrefab;
        public IReadOnlyList<GameplayEffectConfig> Effects => _effects;

        public GameplayTargetActor SpawnTargetActor(AbilitySystemComponent owner)
        {
            if (_targetActorConfig == null)
            {
                return null;
            }

            var parent = _attachActorToOwner && owner != null ? owner.transform : null;
            return _targetActorConfig.Spawn(parent);
        }

        public void ReleaseTargetActor(GameplayTargetActor actor)
        {
            if (actor == null || !_destroyActorOnAcquire)
            {
                return;
            }

            Object.Destroy(actor.gameObject);
        }
    }
}
