using System;
using System.Collections;
using UnityEngine;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame의 전체 진행을 제어하는 컨트롤러입니다.
    /// </summary>
    public sealed class MergeGameController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MergeGameBoard _board;
        [SerializeField] private MergeGameUnitCatalog _unitCatalog;
        [SerializeField] private MergeGameWaveSpawner _waveSpawner;

        [Header("Game Rules")]
        [SerializeField] private int _startingGold = 10;
        [SerializeField] private int _summonCost = 2;
        [SerializeField] private int _goalLimit = 3;
        [SerializeField] private float _gameDuration = 60f;
        [SerializeField] private bool _autoStart = true;

        private int _gold;
        private int _goalReached;
        private float _timeRemaining;
        private bool _running;

        /// <summary>
        /// 현재 보유 골드입니다.
        /// </summary>
        public int Gold => _gold;

        /// <summary>
        /// 남은 게임 시간입니다.
        /// </summary>
        public float TimeRemaining => _timeRemaining;

        /// <summary>
        /// 도착한 몬스터 수입니다.
        /// </summary>
        public int GoalReached => _goalReached;

        /// <summary>
        /// 골드 변경 이벤트입니다.
        /// </summary>
        public event Action<int> GoldChanged;

        /// <summary>
        /// 도착 카운트 변경 이벤트입니다.
        /// </summary>
        public event Action<int, int> GoalCountChanged;

        /// <summary>
        /// 게임 종료 이벤트입니다. true면 승리, false면 패배입니다.
        /// </summary>
        public event Action<bool> GameEnded;

        private void Awake()
        {
            if (_board == null)
            {
                _board = FindFirstObjectByType<MergeGameBoard>();
            }

            if (_unitCatalog == null)
            {
                _unitCatalog = FindFirstObjectByType<MergeGameUnitCatalog>();
            }

            if (_waveSpawner == null)
            {
                _waveSpawner = FindFirstObjectByType<MergeGameWaveSpawner>();
            }
        }

        private void Start()
        {
            if (_autoStart)
            {
                StartGame();
            }
        }

        /// <summary>
        /// 게임을 시작합니다.
        /// </summary>
        public void StartGame()
        {
            _gold = _startingGold;
            _goalReached = 0;
            _timeRemaining = _gameDuration;
            _running = true;

            GoldChanged?.Invoke(_gold);
            GoalCountChanged?.Invoke(_goalReached, _goalLimit);

            if (_waveSpawner != null)
            {
                _waveSpawner.MonsterSpawned += HandleMonsterSpawned;
                _waveSpawner.StartSpawning();
            }

            StartCoroutine(GameTimer());
        }

        /// <summary>
        /// 골드를 사용해 랜덤 유닛을 소환합니다.
        /// </summary>
        public bool TrySummonRandomUnit()
        {
            if (!_running || _board == null || _unitCatalog == null)
            {
                return false;
            }

            if (!_board.TryGetRandomEmptySlot(out var slot))
            {
                return false;
            }

            var definition = _unitCatalog.GetRandomDefinitionByGrade(1);
            if (definition == null || definition.Prefab == null)
            {
                return false;
            }

            var cost = definition.Cost > 0 ? definition.Cost : _summonCost;
            if (_gold < cost)
            {
                return false;
            }

            _board.SpawnUnit(definition, slot);
            _gold -= cost;
            GoldChanged?.Invoke(_gold);
            return true;
        }

        private IEnumerator GameTimer()
        {
            while (_running && _timeRemaining > 0f)
            {
                // 타이머를 갱신한다.
                _timeRemaining -= Time.deltaTime;
                yield return null;
            }

            if (_running)
            {
                EndGame(true);
            }
        }

        private void HandleMonsterSpawned(MergeGameMonster monster)
        {
            if (monster == null)
            {
                return;
            }

            // 목표 도착 이벤트를 구독한다.
            monster.GoalReached += HandleMonsterReachedGoal;
        }

        private void HandleMonsterReachedGoal(MergeGameMonster monster)
        {
            if (!_running)
            {
                return;
            }

            _goalReached++;
            GoalCountChanged?.Invoke(_goalReached, _goalLimit);

            if (monster != null)
            {
                Destroy(monster.gameObject);
            }

            if (_goalReached >= _goalLimit)
            {
                EndGame(false);
            }
        }

        private void EndGame(bool win)
        {
            _running = false;

            if (_waveSpawner != null)
            {
                // 스포너를 정지한다.
                _waveSpawner.StopSpawning();
            }

            GameEnded?.Invoke(win);
        }

        private void OnDisable()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.MonsterSpawned -= HandleMonsterSpawned;
            }
        }
    }
}
