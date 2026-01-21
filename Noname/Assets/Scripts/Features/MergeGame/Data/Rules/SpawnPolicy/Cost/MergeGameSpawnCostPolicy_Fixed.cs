using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 기본 비용만 사용하는 스폰 비용 정책입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "MergeGame/Rules/SpawnCost/Fixed")]
    public sealed class MergeGameSpawnCostPolicy_Fixed : MergeGameSpawnCostPolicy
    {
        [SerializeField] private int _extraCost;

        /// <summary>
        /// 입력 컨텍스트를 기반으로 비용을 계산합니다.
        /// </summary>
        public override int GetCost(MergeGameSpawnCostContext context)
        {
            return context.BaseCost + _extraCost;
        }
    }
}
