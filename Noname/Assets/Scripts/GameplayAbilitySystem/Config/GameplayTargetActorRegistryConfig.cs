using UnityEngine;
using MyProject.GameplayAbilitySystem.Define;
using Noname.GameAbilitySystem;

namespace MyProject.GameplayAbilitySystem.Config
{
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/TargetActor/Registry")]
    public sealed class GameplayTargetActorRegistryConfig : GameplayTargetActorConfig
    {
        [SerializeField] private TargetGroup _targetGroup = TargetGroup.Opponent;
        [SerializeField] private TargetSelectionMode _selectionMode = TargetSelectionMode.All;
        [SerializeField] private TargetFactionMode _factionMode = TargetFactionMode.Any;
        [SerializeField] private float _leftAngle = 30f;
        [SerializeField] private float _rightAngle = 30f;

        public TargetGroup TargetGroup => _targetGroup;
        public TargetSelectionMode SelectionMode => _selectionMode;
        public TargetFactionMode FactionMode => _factionMode;
        public float LeftAngle => Mathf.Max(0f, _leftAngle);
        public float RightAngle => Mathf.Max(0f, _rightAngle);
    }
}
