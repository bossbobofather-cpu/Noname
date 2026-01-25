using MyProject.Common.Host;

namespace MyProject.Common.GameMode
{
    /// <summary>
    /// 세션 설명서를 받아 모듈 설정을 적용하는 인터페이스입니다.
    /// </summary>
    public interface IModuleDescriptorReceiver
    {
        /// <summary>
        /// 모듈을 식별하기 위한 키입니다.
        /// </summary>
        string ModuleKey { get; }

        /// <summary>
        /// 모듈 설정 데이터를 적용합니다.
        /// </summary>
        void ApplyDescriptor(GameModuleDescriptor descriptor);
    }
}
