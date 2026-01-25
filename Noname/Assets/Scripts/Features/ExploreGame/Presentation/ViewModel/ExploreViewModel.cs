using MyProject.ExploreGame.Domain;

namespace MyProject.ExploreGame.Presentation
{
    /// <summary>
    /// 탐험 게임의 View Model입니다.
    /// Snapshot 데이터를 UI에 표시 가능한 형태로 제공합니다.
    /// </summary>
    public sealed class ExploreViewModel
    {
        private ExploreStateSnapshot _snapshot;

        /// <summary>
        /// 현재 세션 단계입니다.
        /// </summary>
        public ExploreSessionPhase SessionPhase { get; private set; }

        /// <summary>
        /// 현재 던전 정보입니다.
        /// </summary>
        public ExploreDungeonSnapshot CurrentDungeon { get; private set; }

        /// <summary>
        /// 현재 전투 정보입니다.
        /// </summary>
        public ExploreCombatSnapshot CurrentCombat { get; private set; }

        /// <summary>
        /// 로컬 플레이어 캐릭터 정보입니다.
        /// </summary>
        public ExploreCharacterSnapshot LocalCharacter { get; private set; }

        /// <summary>
        /// ViewModel을 생성합니다.
        /// </summary>
        public ExploreViewModel()
        {
            SessionPhase = ExploreSessionPhase.Idle;
        }

        /// <summary>
        /// Snapshot을 적용하여 ViewModel을 업데이트합니다.
        /// </summary>
        public void ApplySnapshot(ExploreStateSnapshot snapshot, long localPlayerId)
        {
            if (snapshot == null)
            {
                return;
            }

            _snapshot = snapshot;
            SessionPhase = snapshot.SessionPhase;
            CurrentDungeon = snapshot.CurrentDungeon;
            CurrentCombat = snapshot.CurrentCombat;

            // 로컬 플레이어 찾기
            LocalCharacter = null;
            if (snapshot.Characters != null)
            {
                for (var i = 0; i < snapshot.Characters.Count; i++)
                {
                    if (snapshot.Characters[i].Uid == localPlayerId)
                    {
                        LocalCharacter = snapshot.Characters[i];
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 현재 상태의 요약 문자열을 반환합니다.
        /// </summary>
        public string GetStatusSummary()
        {
            if (LocalCharacter == null)
            {
                return "[대기 중] 게임 참가 대기";
            }

            var summary = $"[{LocalCharacter.Name}] Lv.{LocalCharacter.Level} " +
                         $"HP:{LocalCharacter.CurrentHp}/{LocalCharacter.MaxHp} " +
                         $"ATK:{LocalCharacter.AttackPower} DEF:{LocalCharacter.Defense} " +
                         $"Gold:{LocalCharacter.Gold}";

            if (CurrentDungeon != null)
            {
                summary += $" | 던전:{CurrentDungeon.DungeonId} ({CurrentDungeon.CurrentStage}/{CurrentDungeon.TotalStages}층)";
            }

            if (CurrentCombat != null && !CurrentCombat.Phase.Equals(CombatPhase.Ended))
            {
                summary += $" | 전투중 (턴:{CurrentCombat.TurnCount})";
            }

            return summary;
        }
    }
}
