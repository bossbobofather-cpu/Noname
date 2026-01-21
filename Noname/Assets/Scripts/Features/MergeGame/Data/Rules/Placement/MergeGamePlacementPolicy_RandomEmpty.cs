using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 비어있는 슬롯 중 랜덤으로 선택하는 배치 정책입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "MergeGame/Rules/Placement/RandomEmpty")]
    public sealed class MergeGamePlacementPolicy_RandomEmpty : MergeGamePlacementPolicy
    {
        /// <summary>
        /// 입력 컨텍스트를 기반으로 배치할 슬롯을 선택합니다.
        /// </summary>
        public override MergeGameSlot SelectSlot(MergeGamePlacementContext context)
        {
            if (context.Board == null)
            {
                return null;
            }

            return context.Board.TryGetRandomEmptySlot(out var slot) ? slot : null;
        }
    }
}
