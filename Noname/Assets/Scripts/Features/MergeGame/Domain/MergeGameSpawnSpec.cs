namespace MyProject.MergeGame
{
    /// <summary>
    /// 스폰 사양 정보입니다.
    /// </summary>
    public readonly struct MergeGameSpawnSpec
    {
        /// <summary>
        /// 기본 스폰 사양입니다.
        /// </summary>
        public static MergeGameSpawnSpec Default => new MergeGameSpawnSpec(1, 1);

        /// <summary>
        /// 스폰 등급입니다.
        /// </summary>
        public int Grade { get; }

        /// <summary>
        /// 스폰 레벨입니다.
        /// </summary>
        public int Level { get; }

        public MergeGameSpawnSpec(int grade, int level)
        {
            Grade = grade < 1 ? 1 : grade;
            Level = level < 1 ? 1 : level;
        }
    }
}
