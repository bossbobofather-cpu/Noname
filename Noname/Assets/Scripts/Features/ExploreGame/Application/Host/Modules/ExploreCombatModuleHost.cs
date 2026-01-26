using System;
using MyProject.ExploreGame.Data;
using MyProject.ExploreGame.Domain;
using Noname.GameAbilitySystem;
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
            var combat = new ExploreCombatState(_config.MaxCombatTurns);

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
        /// ATB 기반 자동 전투 턴을 실행합니다.
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

            // 전투 종료 조건 확인
            if (combat.AreAllMonstersDead())
            {
                EndCombat(tick, victory: true);
                return;
            }

            if (combat.IsTurnLimitExceeded())
            {
                EndCombat(tick, victory: false);
                return;
            }

            // 턴 게이지 증가 (deltaSeconds는 CombatTurnInterval 사용)
            var deltaSeconds = _config.CombatTurnInterval;
            UpdateAllTurnGauges(character, combat, deltaSeconds);

            // 어빌리티 충전량 증가
            UpdateAbilityCharges(character, deltaSeconds);

            // 행동 가능한 유닛 처리
            ProcessReadyActors(character, combat, tick);
        }

        /// <summary>
        /// 모든 유닛의 턴 게이지를 업데이트합니다.
        /// </summary>
        private void UpdateAllTurnGauges(ExploreCharacterState character, ExploreCombatState combat, float deltaSeconds)
        {
            // 플레이어 게이지 증가
            if (character.IsAlive)
            {
                character.AddTurnGauge(character.Speed * deltaSeconds);
            }

            // 몬스터 게이지 증가
            combat.UpdateTurnGauges(deltaSeconds);
        }

        /// <summary>
        /// 플레이어 어빌리티의 충전량을 업데이트합니다.
        /// </summary>
        private void UpdateAbilityCharges(ExploreCharacterState character, float deltaSeconds)
        {
            for (var i = 0; i < character.Abilities.Count; i++)
            {
                var ability = character.Abilities[i];
                if (!ability.IsReady)
                {
                    ability.AddCharge(ability.ChargeRate * deltaSeconds);
                }
            }
        }

        /// <summary>
        /// 행동 가능한 모든 유닛의 행동을 처리합니다.
        /// </summary>
        private void ProcessReadyActors(ExploreCharacterState character, ExploreCombatState combat, long tick)
        {
            var actionsProcessed = 0;
            var maxActionsPerTick = 10; // 무한 루프 방지

            while (actionsProcessed < maxActionsPerTick)
            {
                var actedThisCycle = false;

                // 플레이어 행동
                if (character.CanAct())
                {
                    ExecutePlayerAction(character, combat, tick);
                    actedThisCycle = true;
                    actionsProcessed++;
                }

                // 몬스터 행동
                var monster = combat.GetReadyMonster();
                if (monster != null)
                {
                    ExecuteMonsterAction(monster, character, combat, tick);
                    actedThisCycle = true;
                    actionsProcessed++;
                }

                // 더 이상 행동 가능한 유닛이 없으면 종료
                if (!actedThisCycle)
                {
                    break;
                }

                // 전투 종료 조건 재확인
                if (!character.IsAlive || combat.AreAllMonstersDead())
                {
                    break;
                }
            }

            // 턴 카운트 증가 (한 사이클 종료)
            if (actionsProcessed > 0)
            {
                combat.AdvanceTurn();
            }
        }

        /// <summary>
        /// 플레이어의 행동을 실행합니다.
        /// </summary>
        private void ExecutePlayerAction(ExploreCharacterState character, ExploreCombatState combat, long tick)
        {
            var monsters = combat.GetAliveMonsters();
            if (monsters.Count == 0)
            {
                return;
            }

            // 사용 가능한 어빌리티 확인
            var ability = character.GetReadyAbility();

            if (ability != null)
            {
                // 어빌리티를 사용한 공격
                var target = monsters[0];
                var baseDamage = character.AttackPower;

                // 어빌리티 효과 적용 (데미지 증폭)
                var finalDamage = ApplyAbilityEffects(baseDamage, ability, target.Defense);
                target.TakeDamage(finalDamage);

                _publishEvent(new ExploreAbilityUsedEvent(
                    tick,
                    character.Name,
                    ability.Config.DisplayName,
                    target.MonsterType,
                    finalDamage,
                    target.IsDead
                ));

                ability.Consume();
            }
            else
            {
                // 기본 공격
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
            }

            // 턴 게이지 소비
            character.ConsumeTurnGauge(100f);
        }

        /// <summary>
        /// 몬스터의 행동을 실행합니다.
        /// </summary>
        private void ExecuteMonsterAction(
            ExploreMonsterState monster,
            ExploreCharacterState character,
            ExploreCombatState combat,
            long tick)
        {
            // 몬스터는 기본 공격만 수행
            var damage = CalculateDamage(monster.AttackPower, character.Defense);
            character.TakeDamage(damage);

            _publishEvent(new ExploreCombatActionEvent(
                tick,
                monster.MonsterType,
                character.Name,
                damage,
                !character.IsAlive
            ));

            // 턴 게이지 소비
            monster.ConsumeTurnGauge(100f);
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
        /// 어빌리티 효과를 적용한 데미지를 계산합니다.
        /// </summary>
        private int ApplyAbilityEffects(int baseDamage, ExploreAbilityState ability, int targetDefense)
        {
            var modifiedDamage = (float)baseDamage;

            // 어빌리티의 모든 효과 적용
            for (var i = 0; i < ability.Config.AppliedEffects.Count; i++)
            {
                var effect = ability.Config.AppliedEffects[i];

                // 각 효과의 수정자 그룹 적용
                for (var j = 0; j < effect.ModifierGroups.Count; j++)
                {
                    var group = effect.ModifierGroups[j];

                    // 각 수정자 적용
                    for (var k = 0; k < group.Modifiers.Count; k++)
                    {
                        var modifier = group.Modifiers[k];

                        // "Damage" 속성만 적용
                        if (modifier.AttributeName == "Damage")
                        {
                            modifiedDamage = ApplyModifier(modifiedDamage, modifier);
                        }
                    }
                }
            }

            // 방어력 적용
            var finalDamage = Mathf.Max(1, (int)modifiedDamage - targetDefense / 2);
            return finalDamage;
        }

        /// <summary>
        /// 단일 수정자를 적용합니다.
        /// </summary>
        private float ApplyModifier(float currentValue, GameplayModifier modifier)
        {
            switch (modifier.ModifierType)
            {
                case ModifierOperationType.Add:
                    return currentValue + modifier.Value;

                case ModifierOperationType.AddPercent:
                    // 100 = +100% (2배)
                    return currentValue * (1f + modifier.Value / 100f);

                case ModifierOperationType.Multiply:
                    return currentValue * modifier.Value;

                case ModifierOperationType.Override:
                    return modifier.Value;

                default:
                    return currentValue;
            }
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
            var speed = 8 + level; // 플레이어보다 약간 느림
            var goldReward = 10 * level;
            var expReward = 20 * level;

            return new ExploreMonsterState(
                uid,
                monsterType,
                level,
                maxHp,
                attackPower,
                defense,
                speed,
                goldReward,
                expReward
            );
        }
    }
}
