using UnityEngine;
using MyProject.Common.GameEvent;
using MyProject.Common.GameMode;
using MyProject.ExploreGame.Application;
using MyProject.ExploreGame.Data;
using MyProject.ExploreGame.Domain;
using MyProject.Common.Host;

namespace MyProject.ExploreGame.Presentation
{
    /// <summary>
    /// 탐험 게임 모드입니다.
    /// Host를 관리하고 Module들과 통신합니다.
    /// </summary>
    public sealed class ExploreMode : GameMode
    {
        [Header("Host Settings")]
        [SerializeField] private long _localUserId = 1;
        [SerializeField] private string _characterName = "용사";
        [SerializeField] private float _snapshotInterval = 0.1f;

        [Header("Game Settings")]
        [SerializeField] private bool _autoStartDungeon = true;
        [SerializeField] private string _startDungeonId = "dungeon_1";

        private ExploreHost _host;
        private readonly ExploreViewModel _viewModel = new();
        private float _snapshotTimer;
        private bool _joined;

        /// <summary>
        /// 현재 View Model입니다.
        /// </summary>
        public ExploreViewModel ViewModel => _viewModel;

        /// <summary>
        /// GameMode의 추상 메서드 구현: Host에 커맨드를 제출합니다.
        /// </summary>
        public override void RequestCommand(GameCommandBase command)
        {
            if (_host == null)
            {
                Debug.LogWarning("[ExploreMode] Host가 초기화되지 않았습니다.");
                return;
            }

            if (command is ExploreCommand exploreCommand)
            {
                _host.Submit(exploreCommand);
            }
            else
            {
                Debug.LogWarning($"[ExploreMode] 지원하지 않는 커맨드 타입: {command?.GetType().Name}");
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            // Host 생성
            var config = new ExploreHostConfig
            {
                LocalPlayerId = _localUserId,
                CombatTurnInterval = 1f,
                AutoProgressInterval = 2f,
                InitialLevel = 1,
                InitialMaxHp = 100,
                InitialAttack = 10,
                InitialDefense = 5
            };

            _host = new ExploreHost(config);
            _host.ResultProduced += OnHostResult;
            _host.EventRaised += OnHostEvent;
            _host.StartSimulation();

            Debug.Log($"[ExploreMode] Host 시뮬레이션 시작");
        }

        protected override void OnStartup()
        {
            base.OnStartup();

            // 자동 참가
            if (!_joined)
            {
                var joinCmd = new ExploreJoinCommand(_localUserId, _characterName);
                _host.Submit(joinCmd);
                _joined = true;

                Debug.Log($"[ExploreMode] '{_characterName}' 캐릭터로 게임 참가");
            }
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            if (_host != null)
            {
                _host.StopSimulation();
                _host.Dispose();
                _host = null;
            }

            Debug.Log("[ExploreMode] Host 정리 완료");
        }

        private void Update()
        {
            if (_host == null)
            {
                return;
            }

            // Host 이벤트 플러시
            _host.FlushEvents();

            // ViewModel 업데이트
            UpdateViewModel(Time.deltaTime);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_host != null)
            {
                _host.StopSimulation();
                _host.Dispose();
            }
        }

        /// <summary>
        /// ViewModel을 업데이트합니다.
        /// </summary>
        private void UpdateViewModel(float deltaTime)
        {
            _snapshotTimer += deltaTime;
            if (_snapshotTimer < _snapshotInterval)
            {
                return;
            }

            _snapshotTimer -= _snapshotInterval;

            var snapshot = _host.BuildSnapshot();
            _viewModel.ApplySnapshot(snapshot, _localUserId);
        }

        /// <summary>
        /// Host에서 커맨드 결과를 받았을 때 호출됩니다.
        /// </summary>
        private void OnHostResult(ExploreCommandResult result)
        {
            if (result == null)
            {
                return;
            }

            // Join 성공 시 던전 시작
            if (result is ExploreJoinResult joinResult && joinResult.Success && _autoStartDungeon)
            {
                var startCmd = new ExploreStartDungeonCommand(_localUserId, _startDungeonId);
                _host.Submit(startCmd);
                Debug.Log($"[ExploreMode] 던전 '{_startDungeonId}' 시작 요청");
            }
        }

        /// <summary>
        /// Host에서 이벤트를 받았을 때 호출됩니다.
        /// </summary>
        private void OnHostEvent(ExploreHostEvent evt)
        {
            if (evt == null)
            {
                return;
            }

            // GameEventBus로 이벤트 발행
            GameEventBus.Scene.Publish(new ExploreHostEventRaisedEvent(this, evt));
        }
    }
}
