using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 스폰 비용 계산 로직의 베이스입니다.
    /// </summary>
    public abstract class MergeGameSpawnCostPolicy : ScriptableObject
    {
        /// <summary>
        /// 입력 컨텍스트를 기반으로 비용을 계산합니다.
        /// </summary>
        public abstract int GetCost(MergeGameSpawnCostContext context);
    }
}
