using UnityEngine;

namespace MyProject.Common.GameMode
{
    /// <summary>
    /// 공통 모듈 베이스 클래스입니다.
    /// </summary>
    public abstract class ModuleBase : MonoBehaviour, IModule
    {
        private IGameMode _mode;

        /// <summary>
        /// 연결된 게임 모드입니다.
        /// </summary>
        public IGameMode Mode => _mode;

        /// <summary>
        /// 모듈 식별 키입니다.
        /// </summary>
        public virtual string ModuleKey => GetType().Name;

        /// <summary>
        /// 모드와 연결되는 초기화 단계입니다.
        /// </summary>
        public void Initialize(IGameMode mode)
        {
            _mode = mode;
            OnInit();
        }

        /// <summary>
        /// 모드가 활성화될 때 호출됩니다.
        /// </summary>
        public void Startup()
        {
            OnStartup();
        }

        /// <summary>
        /// 모드가 비활성화될 때 호출됩니다.
        /// </summary>
        public void Shutdown()
        {
            OnShutdown();
        }

        /// <summary>
        /// 초기 구독 등록 등 준비 처리를 수행합니다.
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        /// 모듈 시작 시점 로직을 수행합니다.
        /// </summary>
        protected virtual void OnStartup()
        {
        }

        /// <summary>
        /// 모듈 종료 시점 정리 로직을 수행합니다.
        /// </summary>
        protected virtual void OnShutdown()
        {
        }
    }
}
