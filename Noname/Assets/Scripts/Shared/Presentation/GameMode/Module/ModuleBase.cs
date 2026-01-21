using UnityEngine;

namespace MyProject.Common.GameMode
{
    /// <summary>
    /// 공통 모듈 베이스 클래스입니다.
    /// </summary>
    public abstract class ModuleBase : MonoBehaviour, IModule
    {
        private GameMode _mode;

        /// <summary>
        /// 연결된 게임 모드입니다.
        /// </summary>
        public GameMode Mode => _mode;

        /// <summary>
        /// 모드와 연결되는 초기화 단계입니다.
        /// </summary>
        public void Initialize(GameMode mode)
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
        /// 구독 등록이 필요한 경우 이 단계에서 처리합니다.
        /// </summary>
        protected virtual void OnInit()
        {
        }

        protected virtual void OnStartup()
        {
        }

        protected virtual void OnShutdown()
        {
        }
    }
}
