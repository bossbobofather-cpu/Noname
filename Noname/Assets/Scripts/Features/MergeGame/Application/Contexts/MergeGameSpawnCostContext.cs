namespace MyProject.MergeGame
{
    /// <summary>
    /// 스폰 비용 계산에 필요한 입력 컨텍스트입니다.
    /// </summary>
    public readonly struct MergeGameSpawnCostContext
    {
        /// <summary>
        /// 기본 스폰 비용입니다.
        /// </summary>
        public int BaseCost { get; }

        /// <summary>
        /// 누적 스폰 횟수입니다.
        /// </summary>
        public int SpawnCount { get; }

        /// <summary>
        /// 경과 시간입니다.
        /// </summary>
        public float ElapsedTime { get; }

        /// <summary>
        /// 스폰 비용 계산 입력을 구성합니다.
        /// </summary>
        public MergeGameSpawnCostContext(int baseCost, int spawnCount, float elapsedTime)
        {
            BaseCost = baseCost;
            SpawnCount = spawnCount;
            ElapsedTime = elapsedTime;
        }
    }
}
