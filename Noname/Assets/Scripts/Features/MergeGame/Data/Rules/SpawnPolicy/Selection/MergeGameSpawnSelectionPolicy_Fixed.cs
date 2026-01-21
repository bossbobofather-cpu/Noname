using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 특정 유닛 타입을 고정 선택하는 정책입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "MergeGame/Rules/SpawnSelection/Fixed")]
    public sealed class MergeGameSpawnSelectionPolicy_Fixed : MergeGameSpawnSelectionPolicy
    {
        [SerializeField] private MergeGameUnitType _unitType = MergeGameUnitType.Ranged;

        /// <summary>
        /// 입력 컨텍스트를 기반으로 유닛 타입을 선택합니다.
        /// </summary>
        public override MergeGameUnitType SelectUnitType(MergeGameSpawnSelectionContext context)
        {
            return _unitType;
        }
    }
}
