using UnityEngine;

namespace MyProject.Common.Bootstrap
{
    /// <summary>
    /// 게임 모드를 생성하고 수명주기를 제어하는 부트스트래퍼입니다.
    /// </summary>
    public sealed class GameBootstrapper : BootstrapperBase
    {
        [SerializeField] private GameMode.GameMode _gameModePrefab;

        private GameMode.GameMode _gameModeInstance;

        protected override void OnInit()
        {
            base.OnInit();

            CreateGameMode();
        }

        private void CreateGameMode()
        {
            if (_gameModePrefab == null)
            {
                Debug.LogError("GameMode Prefab이 설정되지 않았습니다.");
                return;
            }

            // 게임 모드 프리팹을 생성합니다.
            _gameModeInstance = Instantiate(_gameModePrefab);
            if (_gameModeInstance == null)
            {
                Debug.LogError("GameMode 인스턴스 생성에 실패했습니다.");
                return;
            }

            // 모듈 초기화/시작 순서로 진행합니다.
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
