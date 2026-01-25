namespace MyProject.ExploreGame.Data
{
    /// <summary>
    /// 탐험 게임 Host 설정입니다.
    /// </summary>
    public sealed class ExploreHostConfig
    {
        /// <summary>
        /// 로컬 플레이어 ID입니다.
        /// </summary>
        public long LocalPlayerId { get; set; } = 1;

        /// <summary>
        /// 전투 턴 간격(초)입니다.
        /// </summary>
        public float CombatTurnInterval { get; set; } = 1f;

        /// <summary>
        /// 던전 자동 진행 간격(초)입니다.
        /// </summary>
        public float AutoProgressInterval { get; set; } = 2f;

        /// <summary>
        /// 초기 캐릭터 레벨입니다.
        /// </summary>
        public int InitialLevel { get; set; } = 1;

        /// <summary>
        /// 초기 최대 HP입니다.
        /// </summary>
        public int InitialMaxHp { get; set; } = 100;

        /// <summary>
        /// 초기 공격력입니다.
        /// </summary>
        public int InitialAttack { get; set; } = 10;

        /// <summary>
        /// 초기 방어력입니다.
        /// </summary>
        public int InitialDefense { get; set; } = 5;
    }
}
