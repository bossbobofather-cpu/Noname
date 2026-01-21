using System;
using UnityEngine;

using MyProject.Common.GameMode;

namespace MyProject.MergeGame
{
    /// <summary>
    /// MergeGame 규칙을 관리하는 모듈입니다.
    /// </summary>
    public sealed class RuleModule : ModuleBase
    {
        [Header("Rule Settings")]
        [SerializeField] private MergeGameWinCondition _winCondition;
        [SerializeField] private float _gameSpeed = 1f;
        [SerializeField] private bool _autoStart = true;

        private readonly MergeGameRuleContext _context = new MergeGameRuleContext();
        private bool _running;
        private float _cachedTimeScale = 1f;

        /// <summary>
        /// 현재 룰 상태를 담는 컨텍스트입니다.
        /// </summary>
        public MergeGameRuleContext Context => _context;

        /// <summary>
        /// 게임 진행 중 여부입니다.
        /// </summary>
        public bool IsRunning => _running;

        /// <summary>
        /// 목표 도달 횟수 변경 이벤트입니다.
        /// </summary>
        public event Action<int> GoalCountChanged;

        /// <summary>
        /// 경과 시간 변경 이벤트입니다.
        /// </summary>
        public event Action<float> TimeChanged;

        /// <summary>
        /// 게임 종료 이벤트입니다.
        /// </summary>
        public event Action<bool> GameEnded;

        /// <summary>
        /// 모듈 초기화 훅입니다.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
        }

        /// <summary>
        /// 모듈 시작 시 동작을 처리합니다.
        /// </summary>
        protected override void OnStartup()
        {
            base.OnStartup();

            _cachedTimeScale = Time.timeScale;
            ApplyGameSpeed();

            if (_autoStart)
            {
                StartGame();
            }
        }

        /// <summary>
        /// 모듈 종료 시 정리 작업을 수행합니다.
        /// </summary>
        protected override void OnShutdown()
        {
            base.OnShutdown();

            StopGame(false, false);
            Time.timeScale = _cachedTimeScale;
        }

        /// <summary>
        /// 게임을 시작합니다.
        /// </summary>
        public void StartGame()
        {
            if (_running)
            {
                return;
            }

            _running = true;
            _context.Reset();
            _context.SetRunning(true);

            if (_winCondition != null)
            {
                // 시작 시점에 승리 조건을 초기화합니다.
                _winCondition.ResetCondition(_context);
            }
        }

        /// <summary>
        /// 게임을 종료합니다.
        /// </summary>
        public void EndGame(bool win)
        {
            StopGame(true, win);
        }

        /// <summary>
        /// 목표에 도달했음을 알립니다.
        /// </summary>
        public void NotifyGoalReached()
        {
            if (!_running)
            {
                return;
            }

            _context.AddGoal();
            GoalCountChanged?.Invoke(_context.GoalReached);
            PublishGoalReached();
            EvaluateRules();
        }

        /// <summary>
        /// 시간 경과를 반영합니다.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!_running)
            {
                return;
            }

            // 경과 시간을 누적합니다.
            _context.AdvanceTime(deltaTime);
            TimeChanged?.Invoke(_context.ElapsedTime);
            EvaluateRules();
        }

        /// <summary>
        /// 게임 속도를 TimeScale에 반영합니다.
        /// </summary>
        private void ApplyGameSpeed()
        {
            var speed = _gameSpeed <= 0f ? 1f : _gameSpeed;
            Time.timeScale = speed;
        }

        /// <summary>
        /// 승패 규칙을 평가합니다.
        /// </summary>
        private void EvaluateRules()
        {
            if (_winCondition == null)
            {
                return;
            }

            var result = _winCondition.Evaluate(_context);
            if (result == MergeGameRuleResult.None)
            {
                return;
            }

            EndGame(result == MergeGameRuleResult.Win);
        }

        /// <summary>
        /// 게임을 정지하고 상태를 정리합니다.
        /// </summary>
        private void StopGame(bool notify, bool win)
        {
            if (!_running && !notify)
            {
                return;
            }

            _running = false;
            _context.SetRunning(false);

            if (notify)
            {
                GameEnded?.Invoke(win);
                PublishGameEnded(win);
            }
        }

        /// <summary>
        /// 목표 도달 이벤트를 발행합니다.
        /// </summary>
        private void PublishGoalReached()
        {
            Mode?.Publish(new MergeGameGoalReachedEvent(this, _context.GoalReached));
        }

        /// <summary>
        /// 게임 종료 이벤트를 발행합니다.
        /// </summary>
        private void PublishGameEnded(bool win)
        {
            Mode?.Publish(new MergeGameGameEndedEvent(this, win));
        }

        /// <summary>
        /// 매 프레임 룰 타이머를 갱신합니다.
        /// </summary>
        private void Update()
        {
            // 프레임마다 룰 타이머를 갱신합니다.
            Tick(Time.deltaTime);
        }
    }
}
