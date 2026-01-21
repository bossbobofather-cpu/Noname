namespace MyProject.MergeGame
{
    /// <summary>
    /// 스폰 유닛 선택에 필요한 입력 컨텍스트입니다.
    /// </summary>
    public readonly struct MergeGameSpawnSelectionContext
    {
        /// <summary>
        /// 유닛 카탈로그입니다.
        /// </summary>
        public MergeGameUnitCatalog Catalog { get; }

        /// <summary>
        /// 스폰 런타임 컨텍스트입니다.
        /// </summary>
        public MergeGameSpawnRuntimeContext SpawnContext { get; }

        /// <summary>
        /// 스폰 스펙입니다.
        /// </summary>
        public MergeGameSpawnSpec SpawnSpec { get; }

        /// <summary>
        /// 선택 입력을 구성합니다.
        /// </summary>
        public MergeGameSpawnSelectionContext(
            MergeGameUnitCatalog catalog,
            MergeGameSpawnRuntimeContext spawnContext,
            MergeGameSpawnSpec spawnSpec)
        {
            Catalog = catalog;
            SpawnContext = spawnContext;
            SpawnSpec = spawnSpec;
        }
    }
}
