using System;
using MyProject.ExploreGame.Data;
using MyProject.ExploreGame.Domain;
using UnityEngine;

namespace MyProject.ExploreGame.Application
{
    /// <summary>
    /// 자동 전투 시스템을 관리하는 Module Host입니다.
    /// </summary>
    public sealed class ExploreCombatModuleHost
    {
        private readonly ExploreHostState _state;
        private readonly ExploreHostConfig _config;
        private readonly Action<ExploreHostEvent> _publishEvent;

        private float _combatTimer;

        public ExploreCombatModuleHost(
            ExploreHostState state,
            ExploreHostConfig config,
            Action<ExploreHostEvent> publishEvent)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _publishEvent = publishEvent ?? throw new ArgumentNullException(nameof(publishEvent));
        }

        /// <summary>
        /// 전투를 시작합니다.
        /// </summary>
        public void StartCombat(long tick)
        {
            var dungeon = _state.GetCurrentDungeon();
            if (dungeon == null)
            {
                return;
            }

            var stageData = dungeon.GetCurrentStageData();
            if (stageData == null)
            {
                return;
            }

            // 던전을 전투 상태로 전환
            dungeon.EnterCombat();

            // 전투 상태 생성
            var combat = new ExploreCombatState();

            // 몬스터 생성
            for (var i = 0; i < stageData.MonsterCount; i++)
            {
                var monster = CreateMonster(
                    stageData.MonsterType,
                    stageData.MonsterLevel
                );
                combat.AddMonster(monster);
            }

            combat.StartCombat();
            _state.SetCurrentCombat(combat);

            _combatTimer = 0f;

            _publishEvent(new ExploreCombatStartedEvent(
                tick,
                stageData.MonsterType,
                stageData.MonsterCount
            ));
        }

        /// <summary>
        /// 매 틱마다 호출되어 자동 전투를 진행합니다.
        /// </summary>
        public void Tick(float deltaSeconds, long tick)
        {
            if (_state.SessionPhase != ExploreSessionPhase.CombatActive)
            {
                return;
            }

            var combat = _state.GetCurrentCombat();
            if (combat == null || combat.IsEnded())
            {
                return;
            }

            _combatTimer += deltaSeconds;
            if (_combatTimer < _config.CombatTurnInterval)
            {
                return;
            }

            _combatTimer -= _config.CombatTurnInterval;

            ExecuteAutoCombatTurn(tick);
        }

        /// <summary>
        /// 자동 전투 턴을 실행합니다.
        /// </summary>
        private void ExecuteAutoCombatTurn(long tick)
        {
            var character = _state.GetCharacter(_config.LocalPlayerId);
            if (character == null || !character.IsAlive)
            {
                EndCombat(tick, victory: false);
                return;
            }

            var combat = _state.GetCurrentCombat();
            var monsters = combat.GetAliveMonsters();

            if (monsters.Count == 0)
            {
                EndCombat(tick, victory: true);
                return;
            }

            combat.AdvanceTurn();

            // 플레이어 공격
            var target = monsters[0];
            var damage = CalculateDamage(character.AttackPower, target.Defense);
            target.TakeDamage(damage);

            _publishEvent(new ExploreCombatActionEvent(
                tick,
                character.Name,
                target.MonsterType,
                damage,
                target.IsDead
            ));

            // 몬스터 사망 확인
            if (target.IsDead)
            {
                monsters = combat.GetAliveMonsters();
                if (monsters.Count == 0)
                {
                    EndCombat(tick, victory: true);
                    return;
                }
            }

            // 몬스터 공격
            for (var i = 0; i < monsters.Count; i++)
            {
                var monster = monsters[i];
                var dmg = CalculateDamage(monster.AttackPower, character.Defense);
                character.TakeDamage(dmg);

                _publishEvent(new ExploreCombatActionEvent(
                    tick,
                    monster.MonsterType,
                    character.Name,
                    dmg,
                    !character.IsAlive
                ));

                if (!character.IsAlive)
                {
                    EndCombat(tick, victory: false);
                    return;
                }
            }
        }

        /// <summary>
        /// 전투를 종료합니다.
        /// </summary>
        private void EndCombat(long tick, bool victory)
        {
            var combat = _state.GetCurrentCombat();
            if (combat == null)
            {
                return;
            }

            combat.EndCombat(victory);

            _publishEvent(new ExploreCombatEndedEvent(tick, victory));

            if (victory)
            {
                // 보상 지급
                GrantRewards(tick);

                // 던전 상태 복귀
                var dungeon = _state.GetCurrentDungeon();
                if (dungeon != null)
                {
                    dungeon.ExitCombat(victory: true);
                    _state.SetSessionPhase(ExploreSessionPhase.Exploring);
                }
            }
            else
            {
                // 던전 실패
                var dungeon = _state.GetCurrentDungeon();
                if (dungeon != null)
                {
                    dungeon.ExitCombat(victory: false);
                    _publishEvent(new ExploreDungeonFailedEvent(
                        tick,
                        dungeon.DungeonId,
                        dungeon.CurrentStage
                    ));
                }

                _state.SetSessionPhase(ExploreSessionPhase.Ended);
            }

            _state.SetCurrentCombat(null);
        }

        /// <summary>
        /// 보상을 지급합니다.
        /// </summary>
        private void GrantRewards(long tick)
        {
            var character = _state.GetCharacter(_config.LocalPlayerId);
            if (character == null)
            {
                return;
            }

            var combat = _state.GetCurrentCombat();
            var monsters = combat.GetAllMonsters();

            var totalGold = 0;
            var totalExp = 0;

            for (var i = 0; i < monsters.Count; i++)
            {
                totalGold += monsters[i].GoldReward;
                totalExp += monsters[i].ExpReward;
            }

            character.AddGold(totalGold);
            var leveledUp = character.AddExperience(totalExp);

            _publishEvent(new ExploreRewardGrantedEvent(tick, totalGold, totalExp));

            if (leveledUp)
            {
                _publishEvent(new ExploreCharacterLevelUpEvent(tick, character.Uid, character.Level));
            }

            // 전투 후 체력 회복
            character.ResetForBattle();
        }

        /// <summary>
        /// 데미지를 계산합니다.
        /// </summary>
        private int CalculateDamage(int attack, int defense)
        {
            return Mathf.Max(1, attack - defense / 2);
        }

        /// <summary>
        /// 몬스터를 생성합니다.
        /// </summary>
        private ExploreMonsterState CreateMonster(string monsterType, int level)
        {
            var uid = _state.GenerateMonsterUid();

            var maxHp = 50 * level;
            var attackPower = 5 * level;
            var defense = 2 * level;
            var goldReward = 10 * level;
            var expReward = 20 * level;

            return new ExploreMonsterState(
                uid,
                monsterType,
                level,
                maxHp,
                attackPower,
                defense,
                goldReward,
                expReward
            );
        }
    }
}
