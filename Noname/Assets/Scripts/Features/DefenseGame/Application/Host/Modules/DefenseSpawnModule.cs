using System;
using System.Collections.Generic;
using MyProject.DefenseGame.Data;
using MyProject.DefenseGame.Domain;
using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// 몬스???�폰??처리?�는 모듈?�니??
    /// </summary>
    public sealed class DefenseSpawnModule
    {
        private readonly DefenseHostState _state;
        private readonly DefenseHostConfig _config;
        private readonly DefenseEntityFactory _factory;
        private readonly Action<DefenseHostEvent> _publishEvent;

        private float _spawnTimer;
        private float _bossSpawnTimer;
        private int _currentWave;

        public DefenseSpawnModule(
            DefenseHostState state,
            DefenseHostConfig config,
            DefenseEntityFactory factory,
            Action<DefenseHostEvent> publishEvent)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _publishEvent = publishEvent ?? throw new ArgumentNullException(nameof(publishEvent));
        }

        /// <summary>
        /// ?�폰 ?�스?�을 초기?�합?�다.
        /// </summary>
        public void Initialize()
        {
            _spawnTimer = 0f;
            _bossSpawnTimer = 0f;
            _currentWave = 1;
        }

        /// <summary>
        /// �??�마???�출?�어 ?�폰??처리?�니??
        /// </summary>
        public void Tick(float deltaTime, long tick)
        {
            if (_state.SessionPhase != DefenseSessionPhase.Playing)
            {
                return;
            }

            var combat = _state.Combat;
            if (combat == null || combat.IsGameOver)
            {
                return;
            }

            // ?�이�??�데?�트 (10초마??
            var newWave = 1 + (int)(combat.ElapsedTime / _config.SpawnRateIncreaseInterval);
            if (newWave != _currentWave)
            {
                _currentWave = newWave;
                _publishEvent(new DefenseWaveChangedEvent(tick, _currentWave));
            }

            // ?�반 몬스???�폰
            _spawnTimer += deltaTime;
            if (_spawnTimer >= _config.BaseSpawnInterval)
            {
                _spawnTimer -= _config.BaseSpawnInterval;
                SpawnNormalMonsters(tick, _currentWave);
            }

            // 보스 몬스???�폰
            _bossSpawnTimer += deltaTime;
            if (_bossSpawnTimer >= _config.BossSpawnInterval)
            {
                _bossSpawnTimer -= _config.BossSpawnInterval;
                SpawnBossMonster(tick);
            }

            // ?�배 조건 ?�인
            if (combat.AliveMonsterCount >= _config.MaxMonsterCount)
            {
                combat.SetGameOver(isDefeat: true);
                _state.SetSessionPhase(DefenseSessionPhase.GameOver);
                _publishEvent(new DefenseGameOverEvent(tick, isVictory: false, combat.ElapsedTime, combat.KillCount));
            }
        }

        /// <summary>
        /// ?�반 몬스?��? ?�폰?�니??
        /// </summary>
        private void SpawnNormalMonsters(long tick, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var monster = CreateNormalMonster();
                _state.Combat.AddMonster(monster);

                _publishEvent(new DefenseMonsterSpawnedEvent(
                    tick,
                    monster.Uid,
                    monster.MonsterTypeName,
                    monster.IsBoss,
                    monster.Position
                ));
            }
        }

        /// <summary>
        /// 보스 몬스?��? ?�폰?�니??
        /// </summary>
        private void SpawnBossMonster(long tick)
        {
            var boss = CreateBossMonster();
            _state.Combat.AddMonster(boss);

            _publishEvent(new DefenseMonsterSpawnedEvent(
                tick,
                boss.Uid,
                boss.MonsterTypeName,
                boss.IsBoss,
                boss.Position
            ));
        }

        /// <summary>
        /// ?�반 몬스?��? ?�성?�니??
        /// </summary>
        private DefenseMonster CreateNormalMonster()
        {
            var uid = _state.GenerateMonsterUid();
            // 팩토리에 위임
            return _factory.CreateMonster(uid, _currentWave, isBoss: false);
        }

        /// <summary>
        /// 보스 몬스?��? ?�성?�니??
        /// </summary>
        private DefenseMonster CreateBossMonster()
        {
            var uid = _state.GenerateMonsterUid();
            // 팩토리에 위임
            return _factory.CreateMonster(uid, _currentWave, isBoss: true);
        }
    }
}
