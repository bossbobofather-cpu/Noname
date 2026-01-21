using UnityEngine;
using MyProject.GameplayAbilitySystem.Define;
using Noname.GameAbilitySystem;

namespace MyProject.GameplayAbilitySystem.Config
{
    /// <summary>
    /// 타겟 레지스트리를 통해 대상을 탐색하는 방식에 대한 설정입니다.
    /// 대상 그룹, 선택 모드, 진영 모드, 탐색 각도 등을 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "GameAbilitySystem/Config/TargetActor/Registry")]
    public sealed class GameplayTargetActorRegistryConfig : GameplayTargetActorConfig
    {
        [SerializeField] private TargetGroup _targetGroup = TargetGroup.Opponent;
        [SerializeField] private TargetSelectionMode _selectionMode = TargetSelectionMode.All;
        [SerializeField] private TargetFactionMode _factionMode = TargetFactionMode.Any;
        [SerializeField] private float _leftAngle = 30f;
        [SerializeField] private float _rightAngle = 30f;

        /// <summary>
        /// 탐색할 대상 그룹입니다 (예: 플레이어, 적).
        /// </summary>
        public TargetGroup TargetGroup => _targetGroup;
        
        /// <summary>
        /// 탐색된 대상 중 어떤 대상을 선택할지 결정하는 모드입니다 (예: 전체, 가장 가까운 대상).
        /// </summary>
        public TargetSelectionMode SelectionMode => _selectionMode;
        
        /// <summary>
        /// 시전자와의 진영 관계에 따른 필터링 모드입니다 (예: 아군, 적군).
        /// </summary>
        public TargetFactionMode FactionMode => _factionMode;
        
        /// <summary>
        /// 탐색 시야의 왼쪽 각도입니다.
        /// </summary>
        public float LeftAngle => Mathf.Max(0f, _leftAngle);
        
        /// <summary>
        /// 탐색 시야의 오른쪽 각도입니다.
        /// </summary>
        public float RightAngle => Mathf.Max(0f, _rightAngle);
    }
}
