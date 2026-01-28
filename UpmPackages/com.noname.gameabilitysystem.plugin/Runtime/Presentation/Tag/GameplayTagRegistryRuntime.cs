using UnityEngine;

namespace Noname.GameAbilitySystem.Presentation
{
    /// <summary>
    /// 런타임 레지스트리를 초기화하는 컴포넌트입니다.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class GameplayTagRegistryRuntime : MonoBehaviour
    {
        [SerializeField] private GameplayTagRegistry _registry;

        private void Awake()
        {
            if (_registry == null)
            {
                // 레지스트리가 없으면 리소스에서 찾는다.
                _registry = FindRegistry();
            }

            GameplayTagRegistry.SetRuntimeRegistry(_registry);
        }

        private static GameplayTagRegistry FindRegistry()
        {
            var found = Resources.FindObjectsOfTypeAll<GameplayTagRegistry>();
            if (found == null || found.Length == 0)
            {
                return null;
            }

            // 첫 번째로 찾은 레지스트리를 사용한다.
            return found[0];
        }
    }
}
