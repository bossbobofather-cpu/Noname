using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 특정 슬롯 인덱스를 고정으로 사용하는 배치 정책입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "MergeGame/Rules/Placement/FixedIndex")]
    public sealed class MergeGamePlacementPolicy_FixedIndex : MergeGamePlacementPolicy
    {
        [SerializeField] private int _slotIndex;

        /// <summary>
        /// 입력 컨텍스트를 기반으로 배치할 슬롯을 선택합니다.
        /// </summary>
        public override MergeGameSlot SelectSlot(MergeGamePlacementContext context)
        {
            if (context.Board == null)
            {
                return null;
            }

            var slots = context.Board.Slots;
            if (_slotIndex < 0 || _slotIndex >= slots.Count)
            {
                return null;
            }

            var slot = slots[_slotIndex];
            return slot != null && slot.IsEmpty ? slot : null;
        }
    }
}
