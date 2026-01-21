using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 스폰 대상 선택 로직의 베이스입니다.
    /// </summary>
    public abstract class MergeGameSpawnSelectionPolicy : ScriptableObject
    {
        /// <summary>
        /// 입력 컨텍스트를 기반으로 유닛 타입을 선택합니다.
        /// </summary>
        public abstract MergeGameUnitType SelectUnitType(MergeGameSpawnSelectionContext context);
    }
}
