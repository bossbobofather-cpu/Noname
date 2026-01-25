using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// AbilitySystem의 불변 스냅샷입니다.
    /// 스레드 간 안전한 데이터 전송을 위해 사용됩니다.
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
        public IReadOnlyList<string> Skills { get; }

        /// <summary>
        /// 활성 효과 스냅샷입니다.
        /// </summary>
        public IReadOnlyList<ActiveGameplayEffectSnapshot> ActiveEffects { get; }

        public AbilitySystemSnapshot(
            Dictionary<AttributeId, float> attributes,
            List<FGameplayTag> ownedTags,
            List<string> skills,
            List<ActiveGameplayEffectSnapshot> activeEffects)
        {
            Attributes = new Dictionary<AttributeId, float>(attributes);
            OwnedTags = new List<FGameplayTag>(ownedTags);
            Skills = new List<string>(skills);
            ActiveEffects = new List<ActiveGameplayEffectSnapshot>(activeEffects);
        }
    }

    /// <summary>
    /// 활성 효과의 불변 스냅샷입니다.
    /// </summary>
    public struct ActiveGameplayEffectSnapshot
    {
        public long EffectUid { get; }
        public GameplayEffectConfig Config { get; }
        public float EndTime { get; }

        public ActiveGameplayEffectSnapshot(long effectUid, GameplayEffectConfig config, float endTime)
        {
            EffectUid = effectUid;
            Config = config;
            EndTime = endTime;
        }
    }
}
