using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 스폰 사양 계산 로직의 베이스입니다.
    /// </summary>
    public abstract class MergeGameSpawnSpecPolicy : ScriptableObject
    {
        /// <summary>
        /// 입력 컨텍스트를 기반으로 스폰 사양을 계산합니다.
        /// </summary>
        public abstract MergeGameSpawnSpec GetSpec(MergeGameSpawnSpecContext context);
    }
}
