using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// 게임 플레이 태그 유틸리티 클래스
/// </summary>
public static class GameplayTagUtility
{
    /// <summary>
    /// 태그 문자열이 유효한지 확인합니다.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsValidTagString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // 시작 또는 끝이 점(.)인 경우 잘못된 태그
        if (value[0] == '.' || value[value.Length - 1] == '.')
        {
            return false;
        }

        var previousDot = false;
        for (var i = 0; i < value.Length; i++)
        {
            // 연속된 점(.)이 있는 경우 잘못된 태그
            var c = value[i];
            if (c == '.')
            {
                if (previousDot)
                {
                    return false;
                }

                previousDot = true;
                continue;
            }

            // 허용되지 않는 문자가 있는 경우 잘못된 태그
            if (!IsAllowedTagChar(c))
            {
                return false;
            }

            previousDot = false;
        }

        return !previousDot;
    }

    /// <summary>
    /// 부모 태그들을 열거합니다.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IEnumerable<string> EnumerateParents(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield break;
        }

        var index = value.Length;
        while ((index = value.LastIndexOf('.', index - 1)) >= 0)
        {
            yield return value.Substring(0, index);
        }
    }

    /// <summary>
    /// 태그와 부모 태그들을 열거합니다.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IEnumerable<string> EnumerateTagAndParents(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield break;
        }

        yield return value;
        foreach (var parent in EnumerateParents(value))
        {
            yield return parent;
        }
    }

    /// <summary>
    /// child 태그가 parent 태그의 자손인지 확인합니다. (메모리 할당 없음)
    /// 예: "Status.Debuff"는 "Status"의 자손입니다.
    /// </summary>
    /// <param name="child">자식 태그 문자열</param>
    /// <param name="parent">부모 태그 문자열</param>
    /// <returns></returns>
    public static bool IsDescendant(string child, string parent)
    {
        if (string.IsNullOrEmpty(child) || string.IsNullOrEmpty(parent))
        {
            return false;
        }

        // 부모 길이가 자식보다 길거나 같으면 자손이 될 수 없음 (같으면 exact match)
        if (parent.Length >= child.Length)
        {
            return false;
        }

        // parent로 시작하고, 그 바로 뒤가 점(.)이어야 함
        return child.StartsWith(parent, StringComparison.Ordinal) && child[parent.Length] == '.';
    }

    /// 태그에 사용 할 수 있는 단어 체크
    /// 영문자(a-z, A-Z), 숫자(0-9), 언더스코어(_)만 허용
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAllowedTagChar(char c)
    {
        return (c >= 'a' && c <= 'z')
            || (c >= 'A' && c <= 'Z')
            || (c >= '0' && c <= '9')
            || c == '_';
    }
}