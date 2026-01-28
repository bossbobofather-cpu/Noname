using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Noname.GameAbilitySystem.Domain
{
    /// <summary>
    /// 게임플레이 태그 문자열 유틸리티입니다 (순수 C#).
    /// Unity에 의존하지 않으며 Host 환경에서 사용 가능합니다.
    /// </summary>
    public static class GameplayTagUtility
    {
        public static bool IsValidTagString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            if (value[0] == '.' || value[value.Length - 1] == '.')
                return false;

            var previousDot = false;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == '.')
                {
                    if (previousDot) return false;
                    previousDot = true;
                    continue;
                }
                if (!IsAllowedTagChar(c)) return false;
                previousDot = false;
            }
            return !previousDot;
        }

        public static IEnumerable<string> EnumerateParents(string value)
        {
            if (string.IsNullOrEmpty(value)) yield break;
            var index = value.Length;
            while ((index = value.LastIndexOf('.', index - 1)) >= 0)
                yield return value.Substring(0, index);
        }

        public static IEnumerable<string> EnumerateTagAndParents(string value)
        {
            if (string.IsNullOrEmpty(value)) yield break;
            yield return value;
            foreach (var parent in EnumerateParents(value))
                yield return parent;
        }

        public static bool IsDescendant(string child, string parent)
        {
            if (string.IsNullOrEmpty(child) || string.IsNullOrEmpty(parent))
                return false;
            if (parent.Length >= child.Length)
                return false;
            return child.StartsWith(parent, StringComparison.Ordinal) && child[parent.Length] == '.';
        }

        //TagUtility가 아니어도 될듯. 딴데서도 쓸일 생기면 옮기자.
        public static int Fnv1a32(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
            
            const uint offset = 2166136261;
            const uint prime = 16777619;
            uint hash = offset;

            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= prime;
            }

            return unchecked((int)hash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAllowedTagChar(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
        }

    }
}
