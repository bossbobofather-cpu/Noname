using UnityEngine;

namespace MyProject.Common.Bootstrap
{
    /// <summary>
    /// 씬 전용 게임 모드를 생성하고 초기화하는 부트스트래퍼입니다.
    /// </summary>
    public sealed class GameBootstrapper : BootstrapperBase
    {
        [SerializeField] private GameMode.GameMode _gameMode;

        private GameMode.GameMode _gameModeInstance;

        protected override void OnInit()
        {
            base.OnInit();

            CreateGameMode();
        }

        private void CreateGameMode()
        {
            if (_gameMode == null)
            {
                Debug.LogError("GameMode Prefab Is Null.");
                return;
            }

            // 게임 모드 프리팹을 생성합니다.
            _gameModeInstance = Instantiate(_gameMode);
            if (_gameModeInstance == null)
            {
                Debug.LogError("Failed to instantiate game mode.");
                return;
            }

            // 모듈 초기화와 시작을 순서대로 수행합니다.
            _gameModeInstance.Initialize();
            _gameModeInstance.StartupModule();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 게임 모드를 종료합니다.
            _gameModeInstance?.ShutdownModule();
        }
    }
}
