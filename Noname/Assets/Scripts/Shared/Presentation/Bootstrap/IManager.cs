namespace MyProject.Common.Bootstrap
{
    /// <summary>
    /// 씬과 무관하게 유지되는 매니저 인터페이스입니다.
    /// </summary>
    public interface IManager
    {
        /// <summary>
        /// 매니저를 명시적으로 초기화합니다.
        /// </summary>
        void Initialize();
    }
}
