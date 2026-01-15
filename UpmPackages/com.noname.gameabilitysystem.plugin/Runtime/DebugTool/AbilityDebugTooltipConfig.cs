using System;
using System.Collections.Generic;
using UnityEngine;

namespace Noname.GameAbilitySystem.DebugTool
{
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

        public bool TryGetAbilityTooltip(GameplayAbilityDefinition ability, out string title, out string description)
        {
            title = string.Empty;
            description = string.Empty;

            if (ability == null)
            {
                return false;
            }

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

        public bool TryGetEffectTooltip(GameplayEffectConfig effect, out string title, out string description)
        {
            title = string.Empty;
            description = string.Empty;

            if (effect == null)
            {
                return false;
            }

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
