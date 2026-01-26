using System;
using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayAbilityDefinition")]
    /// <summary>
    /// 어빌리티 정의 데이터를 담는 에셋입니다.
    /// </summary>
    public sealed class GameplayAbilityDefinition : ScriptableObject
    {
        [SerializeField] private string _abilityTypeName;
        [SerializeField] private List<GameplayConfig> _configs;
    
        private static readonly Dictionary<string, Type> TypeCache =
            new(StringComparer.Ordinal);

        /// <summary>
        /// 어빌리티 타입 이름입니다.
        /// </summary>
        public string AbilityTypeName => _abilityTypeName;

        /// <summary>
        /// 연결된 구성 목록입니다.
        /// </summary>
        public IReadOnlyList<GameplayConfig> Configs => _configs;
    
    }
}
