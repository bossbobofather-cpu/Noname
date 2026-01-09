using UnityEngine;

namespace Noname.GameAbilitySystem
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/GameplayTagConfig")]
    public class GameplayTagConfig : GameplayConfig
    {
        [SerializeField] private GameplayTagContainer _abilityTags = new();
        [SerializeField] private GameplayTagContainer _activationRequiredTags = new();
        [SerializeField] private GameplayTagContainer _activationBlockedTags = new();

        public GameplayTagContainer AbilityTags => _abilityTags;
        public GameplayTagContainer ActivationRequiredTags => _activationRequiredTags;
        public GameplayTagContainer ActivationBlockedTags => _activationBlockedTags;


        private void OnValidate()
        {

        }
    }
}
