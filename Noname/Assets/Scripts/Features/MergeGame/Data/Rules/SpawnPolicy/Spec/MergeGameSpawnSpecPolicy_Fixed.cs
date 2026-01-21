using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// 고정 사양으로 스폰하는 정책입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "MergeGame/Rules/SpawnSpec/Fixed")]
    public sealed class MergeGameSpawnSpecPolicy_Fixed : MergeGameSpawnSpecPolicy
    {
        [SerializeField] private int _grade = 1;
        [SerializeField] private int _level = 1;

        /// <summary>
        /// 입력 컨텍스트를 기반으로 스폰 사양을 계산합니다.
        /// </summary>
        public override MergeGameSpawnSpec GetSpec(MergeGameSpawnSpecContext context)
        {
            return new MergeGameSpawnSpec(_grade, _level);
        }
    }
}
