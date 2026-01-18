using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem.DebugTool
{
    [CreateAssetMenu(menuName = "Debug/Ability Debug Catalog")]
    /// <summary>
    /// 디버그 패널에서 사용할 카탈로그 에셋입니다.
    /// </summary>
    public sealed class AbilityDebugCatalog : ScriptableObject
    {
        [SerializeField] private List<GameplayAbilityDefinition> _abilities = new();
        [SerializeField] private List<GameplayEffectConfig> _effects = new();

        /// <summary>
        /// 표시할 능력 목록입니다.
        /// </summary>
        public IReadOnlyList<GameplayAbilityDefinition> Abilities => _abilities;

        /// <summary>
        /// 표시할 효과 목록입니다.
        /// </summary>
        public IReadOnlyList<GameplayEffectConfig> Effects => _effects;
    }
}
