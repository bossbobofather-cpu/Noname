using System.Collections.Generic;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// AbilitySystem의 불변 스냅샷입니다 (순수 C# 모델).
    /// 스레드 간 안전한 데이터 전송을 위해 사용됩니다.
    /// Unity에 의존하지 않으며 Host 환경에서 사용 가능합니다.
    /// </summary>
    public sealed class AbilitySystemSnapshot
    {
        /// <summary>
        /// 속성 값 스냅샷입니다.
        /// </summary>
        public IReadOnlyDictionary<AttributeId, float> Attributes { get; }

        /// <summary>
        /// 소유한 태그 스냅샷입니다.
        /// </summary>
        public IReadOnlyList<FGameplayTag> OwnedTags { get; }

        /// <summary>
        /// 스킬 목록 스냅샷입니다.
        /// </summary>
        public IReadOnlyList<GameplayAbility> Abilities { get; }

        /// <summary>
        /// 활성 효과 스냅샷입니다.
        /// </summary>
        public IReadOnlyList<ActiveGameplayEffectSnapshot> ActiveEffects { get; }

        public AbilitySystemSnapshot(
            Dictionary<AttributeId, float> attributes,
            List<FGameplayTag> ownedTags,
            List<GameplayAbility> abilites,
            List<ActiveGameplayEffectSnapshot> activeEffects)
        {
            Attributes = new Dictionary<AttributeId, float>(attributes);
            OwnedTags = new List<FGameplayTag>(ownedTags);
            Abilities = new List<GameplayAbility>(abilites);
            ActiveEffects = new List<ActiveGameplayEffectSnapshot>(activeEffects);
        }
    }

    /// <summary>
    /// 활성 효과의 불변 스냅샷입니다 (순수 C# 모델).
    /// </summary>
    public struct ActiveGameplayEffectSnapshot
    {
        public long EffectUid { get; }
        public GameplayEffect Effect { get; }
        public float EndTime { get; }

        public ActiveGameplayEffectSnapshot(long effectUid, GameplayEffect effect, float endTime)
        {
            EffectUid = effectUid;
            Effect = effect;
            EndTime = endTime;
        }
    }
}
