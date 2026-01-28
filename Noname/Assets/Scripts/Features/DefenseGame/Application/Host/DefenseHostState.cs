using System.Collections.Generic;
using MyProject.DefenseGame.Domain;
using MyProject.DefenseGame.Domain.LevelUp;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// 디펜스 게임 세션 상태입니다.
    /// </summary>
    public enum DefenseSessionPhase
    {
        None,
        Playing,
        LevelUpSelection,
        GameOver
    }

    /// <summary>
    /// 디펜스 게임 호스트 상태입니다.
    /// </summary>
    public sealed class DefenseHostState
    {
        private long _nextMonsterUid = 1;
        private readonly HashSet<LevelUpAbilityId> _grantedAbilities = new();
        private readonly List<LevelUpAbilityDefinition> _currentLevelUpOptions = new();

        /// <summary>
        /// 획득한 레벨업 어빌리티 ID 목록입니다.
        /// </summary>
        public HashSet<LevelUpAbilityId> GrantedAbilities => _grantedAbilities;

        /// <summary>
        /// 현재 레벨업 선택지 목록입니다.
        /// </summary>
        public IReadOnlyList<LevelUpAbilityDefinition> CurrentLevelUpOptions => _currentLevelUpOptions;

        /// <summary>
        /// 레벨업 선택지를 설정합니다.
        /// </summary>
        public void SetLevelUpOptions(List<LevelUpAbilityDefinition> options)
        {
            _currentLevelUpOptions.Clear();
            if (options != null)
            {
                _currentLevelUpOptions.AddRange(options);
            }
        }

        /// <summary>
        /// 레벨업 어빌리티를 획득 목록에 추가합니다.
        /// </summary>
        public void AddGrantedAbility(LevelUpAbilityId id)
        {
            _grantedAbilities.Add(id);
        }

        /// <summary>
        /// 현재 세션 상태입니다.
        /// </summary>
        public DefenseSessionPhase SessionPhase { get; private set; }

        /// <summary>
        /// 플레이어입니다.
        /// </summary>
        public DefensePlayer Player { get; private set; }

        /// <summary>
        /// 전투 상태입니다.
        /// </summary>
        public DefenseCombatState Combat { get; private set; }

        /// <summary>
        /// 세션 상태를 설정합니다.
        /// </summary>
        public void SetSessionPhase(DefenseSessionPhase phase)
        {
            SessionPhase = phase;
        }

        /// <summary>
        /// 플레이어를 설정합니다.
        /// </summary>
        public void SetPlayer(DefensePlayer player)
        {
            Player = player;
        }

        /// <summary>
        /// 전투 상태를 설정합니다.
        /// </summary>
        public void SetCombat(DefenseCombatState combat)
        {
            Combat = combat;
        }

        /// <summary>
        /// 몬스터 UID를 생성합니다.
        /// </summary>
        public long GenerateMonsterUid()
        {
            return _nextMonsterUid++;
        }
    }
}
