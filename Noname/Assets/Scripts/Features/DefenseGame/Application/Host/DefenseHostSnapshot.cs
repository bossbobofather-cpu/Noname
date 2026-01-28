using System;
using Noname.GameHost;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// 디펜스 게임 호스트 상태를 저장하는 스냅샷입니다.
    /// View/클라이언트에서 이 데이터를 읽어 화면을 갱신합니다.
    /// </summary>
    public sealed class DefenseHostSnapshot : GameSnapshotBase
    {
        public DefenseSessionPhase SessionPhase { get; }
        public long PlayerUid { get; }
        public int PlayerLevel { get; }
        public int PlayerHp { get; }
        public int PlayerMaxHp { get; }
        public int KillCount { get; }
        public int BossKillCount { get; }
        public int AliveMonsterCount { get; }
        public float ElapsedTime { get; }
        public bool IsGameOver { get; }
        public bool IsDefeat { get; }

        public DefenseHostSnapshot(
            long tick,
            DefenseSessionPhase sessionPhase,
            long playerUid,
            int playerLevel,
            int playerHp,
            int playerMaxHp,
            int killCount,
            int bossKillCount,
            int aliveMonsterCount,
            float elapsedTime,
            bool isGameOver,
            bool isDefeat) : base(tick)
        {
            SessionPhase = sessionPhase;
            PlayerUid = playerUid;
            PlayerLevel = playerLevel;
            PlayerHp = playerHp;
            PlayerMaxHp = playerMaxHp;
            KillCount = killCount;
            BossKillCount = bossKillCount;
            AliveMonsterCount = aliveMonsterCount;
            ElapsedTime = elapsedTime;
            IsGameOver = isGameOver;
            IsDefeat = isDefeat;
        }
    }
}