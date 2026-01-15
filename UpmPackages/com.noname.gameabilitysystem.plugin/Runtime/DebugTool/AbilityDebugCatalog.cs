using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem.DebugTool
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
