using System;
using System.Collections.Generic;
using MyProject.ExploreGame.Data;
using MyProject.ExploreGame.Domain;

namespace MyProject.ExploreGame.Application
{
    /// <summary>
    /// 던전 탐험 로직을 관리하는 Module Host입니다.
    /// </summary>
    public sealed class ExploreDungeonModuleHost
    {
        private readonly ExploreHostState _state;
        private readonly ExploreHostConfig _config;
        private readonly Action<ExploreHostEvent> _publishEvent;

        private float _autoProgressTimer;

        public ExploreDungeonModuleHost(
            ExploreHostState state,
            ExploreHostConfig config,
            Action<ExploreHostEvent> publishEvent)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _publishEvent = publishEvent ?? throw new ArgumentNullException(nameof(publishEvent));
        }

        /// <summary>
        /// 던전 탐험을 시작합니다.
        /// </summary>
        public void StartDungeon(string dungeonId, long tick)
        {
            var dungeon = CreateDungeon(dungeonId);
            _state.SetCurrentDungeon(dungeon);
            dungeon.StartExploration();

            _autoProgressTimer = 0f;

            _publishEvent(new ExploreDungeonStartedEvent(tick, dungeonId, dungeon.TotalStages));
            _publishEvent(new ExploreStageChangedEvent(tick, dungeon.CurrentStage));
        }

        /// <summary>
        /// 매 틱마다 호출되어 자동 진행을 처리합니다.
        /// </summary>
        public void Tick(float deltaSeconds, long tick, Action<long> triggerCombat)
        {
            var dungeon = _state.GetCurrentDungeon();
            if (dungeon == null || dungeon.Phase != DungeonPhase.Exploring)
            {
                return;
            }

            _autoProgressTimer += deltaSeconds;
            if (_autoProgressTimer < _config.AutoProgressInterval)
            {
                return;
            }

            _autoProgressTimer -= _config.AutoProgressInterval;

            // 다음 스테이지로 이동
            var hasNext = dungeon.AdvanceStage();
            if (hasNext)
            {
                _publishEvent(new ExploreStageChangedEvent(tick, dungeon.CurrentStage));

                // 몬스터 조우
                triggerCombat(tick);
            }
            else
            {
                // 던전 완료
                _state.SetSessionPhase(ExploreSessionPhase.Ended);
                _publishEvent(new ExploreDungeonCompletedEvent(tick, dungeon.DungeonId));
            }
        }

        /// <summary>
        /// 하드코딩된 던전을 생성합니다.
        /// </summary>
        private ExploreDungeonState CreateDungeon(string dungeonId)
        {
            var stages = new DungeonStageData[]
            {
                new DungeonStageData(1, "슬라임", 1, 1),
                new DungeonStageData(2, "고블린", 2, 2),
                new DungeonStageData(3, "오크", 3, 1),
                new DungeonStageData(4, "트롤", 4, 2),
                new DungeonStageData(5, "드래곤", 5, 1)
            };

            return new ExploreDungeonState(dungeonId, 5, stages);
        }
    }
}
