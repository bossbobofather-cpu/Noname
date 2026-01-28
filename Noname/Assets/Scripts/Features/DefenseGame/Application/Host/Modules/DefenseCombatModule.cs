using System;
using System.Collections.Generic;
using MyProject.DefenseGame.Data;
using MyProject.DefenseGame.Domain;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// 전투를 처리하는 모듈입니다.
    /// </summary>
    public sealed class DefenseCombatModule
    {
        private readonly DefenseHostState _state;
        private readonly DefenseHostConfig _config;
        private readonly Action<DefenseHostEvent> _publishEvent;

        private readonly List<DefenseMonster> _deadMonsters = new();

        public DefenseCombatModule(
            DefenseHostState state,
            DefenseHostConfig config,
            Action<DefenseHostEvent> publishEvent)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _publishEvent = publishEvent ?? throw new ArgumentNullException(nameof(publishEvent));
        }

        /// <summary>
        /// 매 틱마다 호출되어 전투를 처리합니다.
        /// </summary>
        public void Tick(float deltaTime, long tick)
        {
            if (_state.SessionPhase != DefenseSessionPhase.Playing)
            {
                return;
            }

            var combat = _state.Combat;
            var player = _state.Player;

            if (combat == null || combat.IsGameOver || player == null)
            {
                return;
            }

            // 시간 경과
            combat.AddElapsedTime(deltaTime);

            // 플레이어 Tick (자동 공격 포함)
            player.Tick(deltaTime);

            // 모든 몬스터 Tick
            var monsters = combat.GetAliveMonsters();
            for (var i = 0; i < monsters.Count; i++)
            {
                monsters[i].Tick(deltaTime);
            }

            // 죽은 몬스터 처리
            ProcessDeadMonsters(tick);

            // 플레이어 사망 확인
            if (player.IsDead)
            {
                combat.SetGameOver(isDefeat: true);
                _state.SetSessionPhase(DefenseSessionPhase.GameOver);
                _publishEvent(new DefensePlayerDeathEvent(tick, combat.ElapsedTime, combat.KillCount));
            }
        }

        /// <summary>
        /// 죽은 몬스터를 처리합니다.
        /// </summary>
        private void ProcessDeadMonsters(long tick)
        {
            var combat = _state.Combat;
            var player = _state.Player;

            combat.CollectDeadMonsters(_deadMonsters);

            for (var i = 0; i < _deadMonsters.Count; i++)
            {
                var monster = _deadMonsters[i];

                // 경험치 지급
                var leveledUp = player.AddExperience(monster.ExpReward);

                // 몬스터 제거
                combat.RemoveMonster(monster);

                _publishEvent(new DefenseMonsterKilledEvent(
                    tick,
                    monster.Uid,
                    monster.MonsterTypeName,
                    monster.IsBoss,
                    monster.ExpReward
                ));

                // 레벨업 처리
                if (leveledUp)
                {
                    _state.SetSessionPhase(DefenseSessionPhase.LevelUpSelection);
                    _publishEvent(new DefenseLevelUpEvent(tick, player.GetLevel()));
                }
            }
        }
    }
}
