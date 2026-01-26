using System;
using System.Collections.Generic;

namespace Noname.GameAbilitySystem
{
    /// <summary>
    /// 어빌리티 카탈로그입니다 (순수 C# 모델).
    /// JSON으로 직렬화/역직렬화 가능합니다.
    /// </summary>
    [Serializable]
    public sealed class AbilityCatalog
    {
        /// <summary>
        /// 카탈로그 버전입니다.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 어빌리티 목록입니다.
        /// </summary>
        public List<GameplayAbilityModel> Abilities { get; set; }

        public AbilityCatalog()
        {
            Version = "1.0";
            Abilities = new List<GameplayAbilityModel>();
        }
    }
}
