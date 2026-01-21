using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 배치 규칙 로직의 베이스입니다.
    /// </summary>
    public abstract class MergeGamePlacementPolicy : ScriptableObject
    {
        /// <summary>
        /// 입력 컨텍스트를 기반으로 배치할 슬롯을 선택합니다.
        /// </summary>
        public abstract MergeGameSlot SelectSlot(MergeGamePlacementContext context);
    }
}
