namespace MyProject.MergeGame
{
    /// <summary>
    /// 승패 판정 결과입니다.
    /// </summary>
    public enum MergeGameRuleResult
    {
        /// <summary>
        /// 아직 승패가 결정되지 않았습니다.
        /// </summary>
        None,

        /// <summary>
        /// 승리입니다.
        /// </summary>
        Win,

        /// <summary>
        /// 패배입니다.
        /// </summary>
        Lose
    }
}
