using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameplayTagRegistryRuntime : MonoBehaviour
    {
        [SerializeField] private GameplayTagRegistry _registry;

        private void Awake()
        {
            if (_registry == null)
            {
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

            return found[0];
        }
    }
}
