using System;
using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem.DebugTool
{
    /// <summary>
    /// 디버그 툴팁 문구를 관리하는 에셋입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Debug/Ability Debug Tooltip Config")]
    public sealed class AbilityDebugTooltipConfig : ScriptableObject
    {
        [Serializable]
        private sealed class AbilityTooltipEntry
        {
            public GameplayAbilityDefinition Ability;
            public string Title;
            [TextArea(2, 4)]
            public string Description;
        }

        [Serializable]
        private sealed class EffectTooltipEntry
        {
            public GameplayEffectConfig Effect;
            public string Title;
            [TextArea(2, 4)]
            public string Description;
        }

        [SerializeField] private List<AbilityTooltipEntry> _abilities = new();
        [SerializeField] private List<EffectTooltipEntry> _effects = new();

        /// <summary>
        /// 능력에 대한 툴팁을 가져옵니다.
        /// </summary>
        public bool TryGetAbilityTooltip(GameplayAbilityDefinition ability, out string title, out string description)
        {
            title = string.Empty;
            description = string.Empty;

            if (ability == null)
            {
                return false;
            }

            // 목록에서 일치 항목을 찾는다.
            for (var i = 0; i < _abilities.Count; i++)
            {
                var entry = _abilities[i];
                if (entry == null || entry.Ability != ability)
                {
                    continue;
                }

                title = entry.Title;
                description = entry.Description;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 효과에 대한 툴팁을 가져옵니다.
        /// </summary>
        public bool TryGetEffectTooltip(GameplayEffectConfig effect, out string title, out string description)
        {
            title = string.Empty;
            description = string.Empty;

            if (effect == null)
            {
                return false;
            }

            // 목록에서 일치 항목을 찾는다.
            for (var i = 0; i < _effects.Count; i++)
            {
                var entry = _effects[i];
                if (entry == null || entry.Effect != effect)
                {
                    continue;
                }

                title = entry.Title;
                description = entry.Description;
                return true;
            }

            return false;
        }
    }
}
