using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayTagConfig")]
    /// <summary>
    /// 능력 태그 관련 설정을 담는 에셋입니다.
    /// </summary>
    public class GameplayTagConfig : GameplayConfig
    {
        [SerializeField] private GameplayTagContainer _abilityTags = new();
        [SerializeField] private GameplayTagContainer _activationRequiredTags = new();
        [SerializeField] private GameplayTagContainer _activationBlockedTags = new();

        /// <summary>
        /// 능력 자체에 부여되는 태그입니다.
        /// </summary>
        public GameplayTagContainer AbilityTags => _abilityTags;

        /// <summary>
        /// 활성화에 필요한 태그입니다.
        /// </summary>
        public GameplayTagContainer ActivationRequiredTags => _activationRequiredTags;

        /// <summary>
        /// 활성화를 막는 태그입니다.
        /// </summary>
        public GameplayTagContainer ActivationBlockedTags => _activationBlockedTags;

        private void OnValidate()
        {
            // 현재는 검증 로직을 두지 않는다.
        }
    }
}
