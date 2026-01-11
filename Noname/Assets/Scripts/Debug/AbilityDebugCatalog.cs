using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MergeGame.Debug
{
    [CreateAssetMenu(menuName = "Debug/Ability Debug Catalog")]
    public sealed class AbilityDebugCatalog : ScriptableObject
    {
        [SerializeField] private List<GameplayAbilityDefinition> _abilities = new();
        [SerializeField] private List<GameplayEffectConfig> _effects = new();

        public IReadOnlyList<GameplayAbilityDefinition> Abilities => _abilities;
        public IReadOnlyList<GameplayEffectConfig> Effects => _effects;
    }
}
