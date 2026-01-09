using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;

namespace MergeGame.Define
{
    public enum TargetGroup
    {
        Player,
        Opponent
    }
    public enum TargetSelectionMode
    {
        All,                    // 모든 대상
        ClosestToOrigin,        // 출발지점으로부터 가장 가까운 대상
        ClosestFromGoal,        // 목표지점으로부터 가장 가까운 대상
        HighestAttribute,       // 가장 높은 속성값을 가진 대상
        LowestAttribute,        // 가장 낮은 속성값을 가진 대상
        Random                  // 랜덤 대상
    }

    public enum TargetFactionMode
    {
        Any,                        // 아무나
        SameAsOwner,                // 소유자와 동일한 TargetGroup
        DifferentFromOwner,         // 소유자와 다른 TargetGroup    
    }
}
