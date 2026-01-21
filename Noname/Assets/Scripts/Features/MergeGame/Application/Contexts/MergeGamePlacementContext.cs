namespace MyProject.MergeGame
{
    /// <summary>
    /// 배치 슬롯 선택에 필요한 입력 컨텍스트입니다.
    /// </summary>
    public readonly struct MergeGamePlacementContext
    {
        /// <summary>
        /// 보드 참조입니다.
        /// </summary>
        public MergeGameBoard Board { get; }

        /// <summary>
        /// 스폰 대상 유닛 정의입니다.
        /// </summary>
        public MergeGameUnitDefinition Definition { get; }

        /// <summary>
        /// 스폰 런타임 컨텍스트입니다.
        /// </summary>
        public MergeGameSpawnRuntimeContext SpawnContext { get; }

        /// <summary>
        /// 배치 입력을 구성합니다.
        /// </summary>
        public MergeGamePlacementContext(
            MergeGameBoard board,
            MergeGameUnitDefinition definition,
            MergeGameSpawnRuntimeContext spawnContext)
        {
            Board = board;
            Definition = definition;
            SpawnContext = spawnContext;
        }
    }
}
