using System.Collections.Generic;

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

    /// 태그에 사용 할 수 있는 단어 체크
    /// 영문자(a-z, A-Z), 숫자(0-9), 언더스코어(_)만 허용
    private static bool IsAllowedTagChar(char c)
    {
        return (c >= 'a' && c <= 'z')
            || (c >= 'A' && c <= 'Z')
            || (c >= '0' && c <= '9')
            || c == '_';
    }
}