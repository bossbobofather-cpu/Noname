using System;
using System.Collections.Generic;

namespace MyProject.ExploreGame.Domain
{
    /// <summary>
    /// 탐험 게임 세션 단계입니다.
    /// </summary>
    public enum ExploreSessionPhase
    {
        /// <summary>대기 중</summary>
        Idle,
        /// <summary>던전 탐험 중</summary>
        Exploring,
        /// <summary>전투 진행 중</summary>
        CombatActive,
        /// <summary>세션 종료</summary>
        Ended
    }

    /// <summary>
    /// 탐험 게임의 전체 상태를 관리하는 스레드 안전 컨테이너입니다.
    /// </summary>
    public sealed class ExploreHostState
    {
        private readonly object _stateLock = new();
        private readonly Dictionary<long, ExploreCharacterState> _characters = new();
        private ExploreDungeonState _currentDungeon;
        private ExploreCombatState _currentCombat;
        private ExploreSessionPhase _sessionPhase;
        private long _nextMonsterUid;

        public ExploreHostState()
        {
            _sessionPhase = ExploreSessionPhase.Idle;
            _nextMonsterUid = 10000; // 몬스터 UID는 10000부터 시작
        }

        /// <summary>
        /// 현재 세션 단계를 반환합니다.
        /// </summary>
        public ExploreSessionPhase SessionPhase
        {
            get
            {
                lock (_stateLock)
                {
                    return _sessionPhase;
                }
            }
        }

        /// <summary>
        /// 캐릭터를 추가합니다.
        /// </summary>
        public bool TryAddCharacter(ExploreCharacterState character)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            lock (_stateLock)
            {
                if (_characters.ContainsKey(character.Uid))
                {
                    return false;
                }

                _characters.Add(character.Uid, character);
                return true;
            }
        }

        /// <summary>
        /// 캐릭터를 조회합니다.
        /// </summary>
        public ExploreCharacterState GetCharacter(long uid)
        {
            lock (_stateLock)
            {
                return _characters.TryGetValue(uid, out var character) ? character : null;
            }
        }

        /// <summary>
        /// 현재 던전을 설정합니다.
        /// </summary>
        public void SetCurrentDungeon(ExploreDungeonState dungeon)
        {
            lock (_stateLock)
            {
                _currentDungeon = dungeon;
                if (dungeon != null)
                {
                    _sessionPhase = ExploreSessionPhase.Exploring;
                }
            }
        }

        /// <summary>
        /// 현재 던전을 반환합니다.
        /// </summary>
        public ExploreDungeonState GetCurrentDungeon()
        {
            lock (_stateLock)
            {
                return _currentDungeon;
            }
        }

        /// <summary>
        /// 현재 전투를 설정합니다.
        /// </summary>
        public void SetCurrentCombat(ExploreCombatState combat)
        {
            lock (_stateLock)
            {
                _currentCombat = combat;
                if (combat != null)
                {
                    _sessionPhase = ExploreSessionPhase.CombatActive;
                }
            }
        }

        /// <summary>
        /// 현재 전투를 반환합니다.
        /// </summary>
        public ExploreCombatState GetCurrentCombat()
        {
            lock (_stateLock)
            {
                return _currentCombat;
            }
        }

        /// <summary>
        /// 세션 단계를 변경합니다.
        /// </summary>
        public void SetSessionPhase(ExploreSessionPhase phase)
        {
            lock (_stateLock)
            {
                _sessionPhase = phase;
            }
        }

        /// <summary>
        /// 다음 몬스터 UID를 생성합니다.
        /// </summary>
        public long GenerateMonsterUid()
        {
            lock (_stateLock)
            {
                return _nextMonsterUid++;
            }
        }

        /// <summary>
        /// 스냅샷 생성을 위해 내부 데이터에 안전하지 않은 접근을 제공합니다.
        /// 반드시 lock 내부에서만 호출해야 합니다.
        /// </summary>
        internal IReadOnlyDictionary<long, ExploreCharacterState> GetCharactersUnsafe()
        {
            return _characters;
        }

        /// <summary>
        /// 현재 상태의 스냅샷을 생성합니다.
        /// </summary>
        public ExploreStateSnapshot BuildSnapshot(long tick)
        {
            lock (_stateLock)
            {
                return new ExploreStateSnapshot(
                    tick,
                    _sessionPhase,
                    _characters,
                    _currentDungeon,
                    _currentCombat
                );
            }
        }
    }
}
