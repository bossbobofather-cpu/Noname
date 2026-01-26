using MyProject.Common.Host;
using MyProject.ExploreGame.Data;
using MyProject.ExploreGame.Domain;

namespace MyProject.ExploreGame.Application
{
    /// <summary>
    /// 탐험 게임의 Host입니다.
    /// 별도 스레드에서 시뮬레이션을 실행합니다.
    /// </summary>
    public sealed class ExploreHost
        : GameHostBase<ExploreCommand, ExploreCommandResult, ExploreHostEvent, ExploreStateSnapshot>
    {
        private readonly ExploreModeHost _modeHost;

        public ExploreHost(ExploreHostConfig config)
        {
            if (config == null)
            {
                config = new ExploreHostConfig();
            }

            _modeHost = new ExploreModeHost(config, PublishResult, PublishEvent);

            // 시뮬레이션 설정
            FixedStep = 1f / 30f; // 30 FPS
            SnapshotInterval = 0.1f; // 0.1초마다 스냅샷 생성
        }

        /// <summary>
        /// 커맨드를 처리하고 결과를 반환합니다.
        /// </summary>
        protected override GameCommandOutcome<ExploreCommandResult, ExploreHostEvent> HandleCommand(
            ExploreCommand command)
        {
            return _modeHost.HandleCommand(command, Tick);
        }

        /// <summary>
        /// 매 틱마다 호출되어 게임 상태를 업데이트합니다.
        /// </summary>
        protected override void OnTick(float deltaSeconds)
        {
            _modeHost.OnTick(deltaSeconds, Tick);
        }

        /// <summary>
        /// 현재 게임 상태의 스냅샷을 생성합니다.
        /// </summary>
        protected override ExploreStateSnapshot BuildSnapshotInternal()
        {
            return _modeHost.BuildSnapshot(Tick);
        }
    }
}
