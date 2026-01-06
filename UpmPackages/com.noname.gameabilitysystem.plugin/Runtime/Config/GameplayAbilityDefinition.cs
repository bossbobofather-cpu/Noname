using System;
using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayAbilityDefinition")]
    public sealed class GameplayAbilityDefinition : ScriptableObject
    {
        [SerializeField] private string _abilityTypeName;
        [SerializeField] private List<GameplayConfig> _configs;
    
        private static readonly Dictionary<string, Type> TypeCache =
            new(StringComparer.Ordinal);

        public string AbilityTypeName => _abilityTypeName;
        public IReadOnlyList<GameplayConfig> Configs => _configs;
    
    }
}
