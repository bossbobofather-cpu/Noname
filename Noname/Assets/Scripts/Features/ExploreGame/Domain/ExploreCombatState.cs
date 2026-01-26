using System;
using System.Collections.Generic;

namespace MyProject.ExploreGame.Domain
{
    /// <summary>
    /// 전투 진행 단계입니다.
    /// </summary>
    public enum CombatPhase
    {
        /// <summary>전투 준비 중</summary>
        Preparing,
        /// <summary>전투 진행 중</summary>
        InProgress,
        /// <summary>전투 종료</summary>
        Ended
    }

    /// <summary>
    /// 전투 상태를 나타냅니다.
    /// </summary>
    public sealed class ExploreCombatState
    {
        private readonly object _combatLock = new();
        private readonly List<ExploreMonsterState> _monsters = new();

        public CombatPhase Phase { get; private set; }
        public int TurnCount { get; private set; }
        public int MaxTurns { get; }
        public bool IsVictory { get; private set; }

        public ExploreCombatState(int maxTurns = 10)
        {
            if (maxTurns < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxTurns), "최대 턴 수는 1 이상이어야 합니다.");
            }

            Phase = CombatPhase.Preparing;
            TurnCount = 0;
            MaxTurns = maxTurns;
            IsVictory = false;
        }

        /// <summary>
        /// 몬스터를 추가합니다.
        /// </summary>
        public void AddMonster(ExploreMonsterState monster)
        {
            if (monster == null)
            {
                throw new ArgumentNullException(nameof(monster));
            }

            lock (_combatLock)
            {
                if (Phase != CombatPhase.Preparing)
                {
                    throw new InvalidOperationException("전투 준비 중에만 몬스터를 추가할 수 있습니다.");
                }

                _monsters.Add(monster);
            }
        }

        /// <summary>
        /// 전투를 시작합니다.
        /// </summary>
        public void StartCombat()
        {
            lock (_combatLock)
            {
                if (Phase != CombatPhase.Preparing)
                {
                    throw new InvalidOperationException("이미 전투가 시작되었습니다.");
                }

                if (_monsters.Count == 0)
                {
                    throw new InvalidOperationException("전투할 몬스터가 없습니다.");
                }

                Phase = CombatPhase.InProgress;
                TurnCount = 0;
            }
        }

        /// <summary>
        /// 턴을 진행합니다.
        /// </summary>
        public void AdvanceTurn()
        {
            lock (_combatLock)
            {
                if (Phase != CombatPhase.InProgress)
                {
                    throw new InvalidOperationException("전투가 진행 중이 아닙니다.");
                }

                TurnCount++;
            }
        }

        /// <summary>
        /// 살아있는 몬스터 목록을 반환합니다 (스레드 안전).
        /// </summary>
        public List<ExploreMonsterState> GetAliveMonsters()
        {
            lock (_combatLock)
            {
                var alive = new List<ExploreMonsterState>();
                for (var i = 0; i < _monsters.Count; i++)
                {
                    if (!_monsters[i].IsDead)
                    {
                        alive.Add(_monsters[i]);
                    }
                }
                return alive;
            }
        }

        /// <summary>
        /// 모든 몬스터 목록을 반환합니다 (스레드 안전).
        /// </summary>
        public List<ExploreMonsterState> GetAllMonsters()
        {
            lock (_combatLock)
            {
                return new List<ExploreMonsterState>(_monsters);
            }
        }

        /// <summary>
        /// 전투를 종료합니다.
        /// </summary>
        public void EndCombat(bool victory)
        {
            lock (_combatLock)
            {
                if (Phase != CombatPhase.InProgress)
                {
                    throw new InvalidOperationException("전투가 진행 중이 아닙니다.");
                }

                Phase = CombatPhase.Ended;
                IsVictory = victory;
            }
        }

        /// <summary>
        /// 전투가 종료되었는지 확인합니다.
        /// </summary>
        public bool IsEnded()
        {
            lock (_combatLock)
            {
                return Phase == CombatPhase.Ended;
            }
        }

        /// <summary>
        /// 최대 턴 수를 초과했는지 확인합니다.
        /// </summary>
        public bool IsTurnLimitExceeded()
        {
            lock (_combatLock)
            {
                return TurnCount >= MaxTurns;
            }
        }

        /// <summary>
        /// 모든 몬스터가 사망했는지 확인합니다.
        /// </summary>
        public bool AreAllMonstersDead()
        {
            lock (_combatLock)
            {
                for (var i = 0; i < _monsters.Count; i++)
                {
                    if (!_monsters[i].IsDead)
                    {
                        return false;
                    }
                }
                return _monsters.Count > 0;
            }
        }

        /// <summary>
        /// 행동 가능한 몬스터를 반환합니다 (TurnGauge >= 100).
        /// </summary>
        public ExploreMonsterState GetReadyMonster()
        {
            lock (_combatLock)
            {
                for (var i = 0; i < _monsters.Count; i++)
                {
                    if (_monsters[i].CanAct())
                    {
                        return _monsters[i];
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 모든 유닛(몬스터)의 턴 게이지를 증가시킵니다.
        /// </summary>
        public void UpdateTurnGauges(float deltaSeconds)
        {
            lock (_combatLock)
            {
                if (Phase != CombatPhase.InProgress)
                {
                    return;
                }

                for (var i = 0; i < _monsters.Count; i++)
                {
                    if (!_monsters[i].IsDead)
                    {
                        // Speed에 비례해서 게이지 증가
                        var gaugeIncrease = _monsters[i].Speed * deltaSeconds;
                        _monsters[i].AddTurnGauge(gaugeIncrease);
                    }
                }
            }
        }
    }
}
