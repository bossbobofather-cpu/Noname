using Noname.GameAbilitySystem;
using UnityEngine;

namespace MergeGame.Config
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/AnimationEventData")]
    public class AnimationEventDataConfig : ScriptableObject
    {
        [SerializeField] private GameplayTagContainer _grantTags = new();

        public GameplayTagContainer GrantedTags => _grantTags;
    }
}
