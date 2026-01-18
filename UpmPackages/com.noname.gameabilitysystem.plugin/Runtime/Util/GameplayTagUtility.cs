using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// 게임플레이 태그 문자열 유틸리티입니다.
/// </summary>
public static class GameplayTagUtility
{
    /// <summary>
    /// 태그 문자열이 유효한지 확인합니다.
    /// </summary>
    /// <param name="value">태그 문자열</param>
    /// <returns>유효 여부</returns>
    public static bool IsValidTagString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            // 비어 있거나 공백이면 유효하지 않다.
            return false;
        }

        // 시작이나 끝이 점이면 잘못된 태그다.
        if (value[0] == '.' || value[value.Length - 1] == '.')
        {
            return false;
        }

        var previousDot = false;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '.')
            {
                // 점이 연속되면 잘못된 태그다.
                if (previousDot)
                {
                    return false;
                }

                previousDot = true;
                continue;
            }

            // 허용되지 않은 문자가 있으면 실패한다.
            if (!IsAllowedTagChar(c))
            {
                return false;
            }

            previousDot = false;
        }

        return !previousDot;
    }

    /// <summary>
    /// 부모 태그를 순서대로 열거합니다.
    /// </summary>
    /// <param name="value">태그 문자열</param>
    /// <returns>부모 태그 열거자</returns>
    public static IEnumerable<string> EnumerateParents(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield break;
        }

        // 뒤에서부터 점을 찾아 부모를 만든다.
        var index = value.Length;
        while ((index = value.LastIndexOf('.', index - 1)) >= 0)
        {
            yield return value.Substring(0, index);
        }
    }

    /// <summary>
    /// 태그와 부모 태그를 함께 열거합니다.
    /// </summary>
    /// <param name="value">태그 문자열</param>
    /// <returns>태그와 부모 태그 열거자</returns>
    public static IEnumerable<string> EnumerateTagAndParents(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield break;
        }

        // 자기 자신을 먼저 내보낸다.
        yield return value;
        foreach (var parent in EnumerateParents(value))
        {
            yield return parent;
        }
    }

    /// <summary>
    /// 자식 태그가 부모 태그의 하위인지 확인합니다.
    /// </summary>
    /// <param name="child">자식 태그</param>
    /// <param name="parent">부모 태그</param>
    /// <returns>하위 여부</returns>
    public static bool IsDescendant(string child, string parent)
    {
        if (string.IsNullOrEmpty(child) || string.IsNullOrEmpty(parent))
        {
            return false;
        }

        // 부모 길이가 더 길면 하위가 될 수 없다.
        if (parent.Length >= child.Length)
        {
            return false;
        }

        // 부모로 시작하고 바로 다음이 점인지 확인한다.
        return child.StartsWith(parent, StringComparison.Ordinal) && child[parent.Length] == '.';
    }

    // 태그 문자열에 허용되는 문자만 통과시킨다.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAllowedTagChar(char c)
    {
        // 영문, 숫자, 밑줄만 허용한다.
        return (c >= 'a' && c <= 'z')
            || (c >= 'A' && c <= 'Z')
            || (c >= '0' && c <= '9')
            || c == '_';
    }
}
