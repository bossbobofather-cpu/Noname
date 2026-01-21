#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

using MyProject.Common.Bootstrap;
using MyProject.Common.GameMode;

namespace MyProject.Common.Editor
{
    /// <summary>
    /// 매니저/모듈에서 Unity 생명주기 메서드 사용을 검사합니다.
    /// </summary>
    [InitializeOnLoad]
    internal static class ManagerModuleLifecycleValidator
    {
        static ManagerModuleLifecycleValidator()
        {
            // 에디터 로드 이후 한 번 검사합니다.
            EditorApplication.delayCall += Validate;
        }

        private static void Validate()
        {
            var managerType = typeof(IManager);
            var moduleType = typeof(IModule);
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            foreach (var type in TypeCache.GetTypesDerivedFrom<MonoBehaviour>())
            {
                if (type == null || type.IsAbstract || type.IsGenericType)
                {
                    continue;
                }

                if (!managerType.IsAssignableFrom(type) && !moduleType.IsAssignableFrom(type))
                {
                    continue;
                }

                var hasAwake = type.GetMethod("Awake", flags) != null;
                var hasStart = type.GetMethod("Start", flags) != null;
                var hasEnable = type.GetMethod("OnEnable", flags) != null;
                if (!hasAwake && !hasStart && !hasEnable)
                {
                    continue;
                }

                var kind = managerType.IsAssignableFrom(type) ? "Manager" : "Module";
                Debug.LogError(
                    $"[{kind}] {type.FullName} declares Awake/Start/OnEnable. Use Initialize/Startup instead.");
            }
        }
    }
}
#endif
