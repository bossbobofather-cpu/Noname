namespace MyProject.GameplayAbilitySystem.Define
{
    /// <summary>
    /// 타겟 그룹 정의입니다.
    /// </summary>
    public enum TargetGroup
    {
        /// <summary>
        /// 플레이어 그룹
        /// </summary>
        Player,
        
        /// <summary>
        /// 적(상대방) 그룹
        /// </summary>
        Opponent
    }
    
    /// <summary>
    /// 타겟 선택 방식 모드입니다.
    /// </summary>
    public enum TargetSelectionMode
    {
        /// <summary>
        /// 조건에 맞는 모든 대상
        /// </summary>
        All,                    // 모든 대상
        
        /// <summary>
        /// 기준점(Origin)에서 가장 가까운 대상
        /// </summary>
        ClosestToOrigin,        // 출발지점으로부터 가장 가까운 대상
        
        /// <summary>
        /// 목표점(Goal)에서 가장 가까운 대상 (미구현 가능성 있음)
        /// </summary>
        ClosestFromGoal,        // 목표지점으로부터 가장 가까운 대상
        
        /// <summary>
        /// 특정 속성값이 가장 높은 대상
        /// </summary>
        HighestAttribute,       // 가장 높은 속성값을 가진 대상
        
        /// <summary>
        /// 특정 속성값이 가장 낮은 대상
        /// </summary>
        LowestAttribute,        // 가장 낮은 속성값을 가진 대상
        
        /// <summary>
        /// 무작위 대상
        /// </summary>
        Random                  // 랜덤 대상
    }

    /// <summary>
    /// 시전자와의 진영 관계(Faction)에 따른 필터 모드입니다.
    /// </summary>
    public enum TargetFactionMode
    {
        /// <summary>
        /// 진영 관계 무관
        /// </summary>
        Any,                        // 아무나
        
        /// <summary>
        /// 시전자와 같은 진영(아군)
        /// </summary>
        SameAsOwner,                // 소유자와 동일한 TargetGroup
        
        /// <summary>
        /// 시전자와 다른 진영(적군)
        /// </summary>
        DifferentFromOwner,         // 소유자와 다른 TargetGroup    
    }
}
