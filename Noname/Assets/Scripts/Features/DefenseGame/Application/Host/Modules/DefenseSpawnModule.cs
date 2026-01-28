using System;
using MyProject.DefenseGame.Domain;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// 몬스터 스폰을 담당하는 모듈입니다.
    /// </summary>
    public sealed class DefenseSpawnModule
    {
        /// <summary>
        /// 호스트 상태입니다.
        /// </summary>
        private readonly DefenseHostState _state;

        /// <summary>
        /// 스폰 설정입니다.
        /// </summary>
        private readonly DefenseHostConfig _config;

        /// <summary>
        /// 엔티티 생성기입니다.
        /// </summary>
        private readonly DefenseEntityFactory _factory;

        /// <summary>
        /// 이벤트 발행 델리게이트입니다.
        /// </summary>
        private readonly Action<DefenseHostEvent> _publishEvent;

        /// <summary>
        /// 일반 몬스터 스폰 타이머입니다.
        /// </summary>
        private float _spawnTimer;

        /// <summary>
        /// 보스 몬스터 스폰 타이머입니다.
        /// </summary>
        private float _bossSpawnTimer;

        /// <summary>
        /// 현재 웨이브 번호입니다.
        /// </summary>
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
        /// 스폰 시스템을 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            _spawnTimer = 0f;
            _bossSpawnTimer = 0f;
            _currentWave = 1;
        }

        /// <summary>
        /// 매 프레임 호출되어 스폰을 처리합니다.
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

            // 난이도 업데이트 (시간 경과에 따라 웨이브 증가)
            var newWave = 1 + (int)(combat.ElapsedTime / _config.SpawnRateIncreaseInterval);
            if (newWave != _currentWave)
            {
                _currentWave = newWave;
                _publishEvent(new DefenseWaveChangedEvent(tick, _currentWave));
            }

            // 일반 몬스터 스폰
            _spawnTimer += deltaTime;
            if (_spawnTimer >= _config.BaseSpawnInterval)
            {
                _spawnTimer -= _config.BaseSpawnInterval;
                SpawnNormalMonsters(tick, _currentWave);
            }

            // 보스 몬스터 스폰
            _bossSpawnTimer += deltaTime;
            if (_bossSpawnTimer >= _config.BossSpawnInterval)
            {
                _bossSpawnTimer -= _config.BossSpawnInterval;
                SpawnBossMonster(tick);
            }

            // 패배 조건 확인
            if (combat.AliveMonsterCount >= _config.MaxMonsterCount)
            {
                combat.SetGameOver(isDefeat: true);
                _state.SetSessionPhase(DefenseSessionPhase.GameOver);
                _publishEvent(new DefenseGameOverEvent(tick, isVictory: false, combat.ElapsedTime, combat.KillCount));
            }
        }

        /// <summary>
        /// 일반 몬스터를 스폰합니다.
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
        /// 보스 몬스터를 스폰합니다.
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
        /// 일반 몬스터를 생성합니다.
        /// </summary>
        private DefenseMonster CreateNormalMonster()
        {
            var uid = _state.GenerateMonsterUid();
            // 팩토리에서 생성
            return _factory.CreateMonster(uid, _currentWave, isBoss: false);
        }

        /// <summary>
        /// 보스 몬스터를 생성합니다.
        /// </summary>
        private DefenseMonster CreateBossMonster()
        {
            var uid = _state.GenerateMonsterUid();
            // 팩토리에서 생성
            return _factory.CreateMonster(uid, _currentWave, isBoss: true);
        }
    }
}
