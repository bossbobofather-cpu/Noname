namespace MyProject.MergeGame
{
    /// <summary>
    /// 스폰 스펙 계산에 필요한 입력 컨텍스트입니다.
    /// </summary>
    public readonly struct MergeGameSpawnSpecContext
    {
        /// <summary>
        /// 스폰 런타임 컨텍스트입니다.
        /// </summary>
        public MergeGameSpawnRuntimeContext SpawnContext { get; }

        /// <summary>
        /// 스펙 계산 입력을 구성합니다.
        /// </summary>
        public MergeGameSpawnSpecContext(MergeGameSpawnRuntimeContext spawnContext)
        {
            SpawnContext = spawnContext;
        }
    }
}
