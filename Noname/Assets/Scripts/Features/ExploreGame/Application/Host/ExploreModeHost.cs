using System;
using MyProject.Common.Host;
using MyProject.ExploreGame.Data;
using MyProject.ExploreGame.Domain;

namespace MyProject.ExploreGame.Application
{
    /// <summary>
    /// 탐험 게임의 게임 모드 로직을 담당하는 Host입니다.
    /// </summary>
    public sealed class ExploreModeHost
    {
        private readonly ExploreHostState _state;
        private readonly ExploreHostConfig _config;
        private readonly ExploreDungeonModuleHost _dungeonModule;
        private readonly ExploreCombatModuleHost _combatModule;
        private readonly Action<ExploreCommandResult> _publishResult;
        private readonly Action<ExploreHostEvent> _publishEvent;

        public ExploreModeHost(
            ExploreHostConfig config,
            Action<ExploreCommandResult> publishResult,
            Action<ExploreHostEvent> publishEvent)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _publishResult = publishResult ?? throw new ArgumentNullException(nameof(publishResult));
            _publishEvent = publishEvent ?? throw new ArgumentNullException(nameof(publishEvent));

            _state = new ExploreHostState();
            _dungeonModule = new ExploreDungeonModuleHost(_state, _config, _publishEvent);
            _combatModule = new ExploreCombatModuleHost(_state, _config, _publishEvent);
        }

        /// <summary>
        /// 커맨드를 처리하고 결과를 반환합니다.
        /// </summary>
        public GameCommandOutcome<ExploreCommandResult, ExploreHostEvent> HandleCommand(
            ExploreCommand command,
            long tick)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            return command switch
            {
                ExploreJoinCommand joinCmd => HandleJoinCommand(joinCmd, tick),
                ExploreStartDungeonCommand startCmd => HandleStartDungeonCommand(startCmd, tick),
                _ => default
            };
        }

        /// <summary>
        /// 매 틱마다 호출되어 게임 상태를 업데이트합니다.
        /// </summary>
        public void OnTick(float deltaSeconds, long tick)
        {
            // 던전 자동 진행
            _dungeonModule.Tick(deltaSeconds, tick, TriggerCombat);

            // 자동 전투 진행
            _combatModule.Tick(deltaSeconds, tick);
        }

        /// <summary>
        /// 현재 상태의 스냅샷을 생성합니다.
        /// </summary>
        public ExploreStateSnapshot BuildSnapshot(long tick)
        {
            return _state.BuildSnapshot(tick);
        }

        /// <summary>
        /// 게임 참가 커맨드를 처리합니다.
        /// </summary>
        private GameCommandOutcome<ExploreCommandResult, ExploreHostEvent> HandleJoinCommand(
            ExploreJoinCommand command,
            long tick)
        {
            var character = new ExploreCharacterState(
                command.SenderUid,
                command.CharacterName,
                _config.InitialLevel,
                _config.InitialMaxHp,
                _config.InitialAttack,
                _config.InitialDefense
            );

            var success = _state.TryAddCharacter(character);

            var result = new ExploreJoinResult(
                tick,
                command.SenderUid,
                success,
                character.Uid,
                success ? null : "이미 참가한 플레이어입니다."
            );

            if (success)
            {
                var joinEvent = new ExplorePlayerJoinedEvent(
                    tick,
                    character.Uid,
                    character.Name
                );

                return new GameCommandOutcome<ExploreCommandResult, ExploreHostEvent>(
                    result,
                    new[] { joinEvent }
                );
            }

            return new GameCommandOutcome<ExploreCommandResult, ExploreHostEvent>(result);
        }

        /// <summary>
        /// 던전 시작 커맨드를 처리합니다.
        /// </summary>
        private GameCommandOutcome<ExploreCommandResult, ExploreHostEvent> HandleStartDungeonCommand(
            ExploreStartDungeonCommand command,
            long tick)
        {
            var character = _state.GetCharacter(command.SenderUid);
            if (character == null)
            {
                var errorResult = new ExploreStartDungeonResult(
                    tick,
                    command.SenderUid,
                    false,
                    command.DungeonId,
                    "게임에 참가하지 않은 플레이어입니다."
                );

                return new GameCommandOutcome<ExploreCommandResult, ExploreHostEvent>(errorResult);
            }

            _dungeonModule.StartDungeon(command.DungeonId, tick);

            var result = new ExploreStartDungeonResult(
                tick,
                command.SenderUid,
                true,
                command.DungeonId,
                null
            );

            return new GameCommandOutcome<ExploreCommandResult, ExploreHostEvent>(result);
        }

        /// <summary>
        /// 전투를 시작합니다.
        /// </summary>
        private void TriggerCombat(long tick)
        {
            _combatModule.StartCombat(tick);
        }
    }
}
