namespace MyProject.Common.GameMode
{
    /// <summary>
    /// 공통 모듈 인터페이스입니다.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// 모드와 연결되는 초기화 단계입니다.
        /// </summary>
        void Initialize(GameMode mode);

        /// <summary>
        /// 모드가 활성화될 때 호출됩니다.
        /// </summary>
        void Startup();

        /// <summary>
        /// 모드가 비활성화될 때 호출됩니다.
        /// </summary>
        void Shutdown();
    }
}

