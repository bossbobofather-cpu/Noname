using MyProject.DefenseGame.Application;
using MyProject.DefenseGame.Domain;
using MyProject.DefenseGame.Presentation;
using UnityEngine;

namespace MyProject.Common.Bootstrap
{
    /// <summary>
    /// 게임 모드를 생성하고 수명주기를 제어하는 부트스트래퍼입니다.
    /// </summary>
    public class DefenseGameBootstrapper : BootstrapperBase
    {
        [SerializeField] private DefenseHostConfig _config;
        [SerializeField] private DefenseGameMode _gameModePrefab;

        private DefenseGameMode _gameModeInstance;

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
            var instance = Instantiate(_gameModePrefab);
            if (instance == null)
            {
                Debug.LogError("GameMode 인스턴스 생성에 실패했습니다.");
                return;
            }

            _gameModeInstance = instance;            
            _gameModeInstance.Initialize(new DefenseGameHost(_config));
        }
    }
}
