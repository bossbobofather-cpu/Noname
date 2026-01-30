using System;
using System.Collections.Generic;
using Noname.GameHost;
using MyProject.DefenseGame.Application.Commands;
using MyProject.DefenseGame.Domain;
using MyProject.DefenseGame.Domain.AI;
using MyProject.DefenseGame.Domain.LevelUp;
using Noname.GameAbilitySystem.Domain;
using System.Transactions;
using Unity.VisualScripting;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// DefenseGame 전용 호스트입니다.
    /// Command를 처리하고 Result/Event를 발행합니다.
    /// </summary>
    public sealed class DefenseGameHost
        : GameHostBase<DefenseCommand, DefenseCommandResult, DefenseHostEvent, DefenseHostSnapshot>, IAIFlagChecker
    {
        /// <summary>
        /// 게임 상태입니다.
        /// </summary>
        private readonly DefenseHostState _state;

        /// <summary>
        /// 게임 설정입니다.
        /// </summary>
        private readonly DefenseHostConfig _config;

        /// <summary>
        /// 스폰 모듈입니다.
        /// </summary>
        private readonly DefenseSpawnModule _spawnModule;

        /// <summary>
        /// 전투 모듈입니다.
        /// </summary>
        private readonly DefenseCombatModule _combatModule;

        /// <summary>
        /// 엔티티 팩토리입니다.
        /// </summary>
        private readonly DefenseEntityFactory _entityFactory;

        /// <summary>
        /// 타겟 컨텍스트 조회용 임시 리스트입니다.
        /// </summary>
        private readonly List<AbilitySystemComponent> _tempEnemyList = new();

        /// <summary>
        /// 레벨업 능력 레지스트리입니다.
        /// </summary>
        private readonly LevelUpAbilityRegistry _levelUpRegistry;

        /// <summary>
        /// 레벨업 옵션입니다.
        /// </summary>
        private readonly List<LevelUpAbilityDefinition> _tempLevelUpOptions = new();

        /// <summary>
        /// 타겟팅 컨텍스트입니다.
        /// </summary>
        private TargetContext _targetContext;

        /// <summary>
        /// AI에서 체크하는 플래그. /AI에서 능력을 활성화 할 수 있는 상태인지 확인용으로 쓰입니다.
        /// </summary>
        private bool _needPlayerAIRecheck = false;

        public DefenseGameHost(DefenseHostConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _state = new DefenseHostState();

            _entityFactory = new DefenseEntityFactory(_config);
            _spawnModule = new DefenseSpawnModule(_state, _config, _entityFactory, PublishEvent, this);
            _combatModule = new DefenseCombatModule(_state, _config, PublishEvent);
            _levelUpRegistry = new LevelUpAbilityRegistry();
        }

        #region GameHostBase Overrides

        protected override GameCommandOutcome<DefenseCommandResult, DefenseHostEvent> HandleCommand(DefenseCommand command)
        {
            switch (command)
            {
                case StartGameCommand startCommand:
                    return HandleStartGame(startCommand);

                case SelectLevelUpAbilityCommand selectCommand:
                    return HandleSelectLevelUpAbility(selectCommand);

                case EndGameCommand endCommand:
                    return HandleEndGame(endCommand);

                default:
                    return default;
            }
        }

        protected override void OnTick(float deltaTime)
        {
            if (_state.SessionPhase == DefenseSessionPhase.None ||
                _state.SessionPhase == DefenseSessionPhase.GameOver)
            {
                return;
            }

            //레벨업 페이즈일때 멈춘다. 다음 진행 커맨드를 전달 받고 진행 되도록
            if (_state.SessionPhase == DefenseSessionPhase.LevelUpSelection)
            {
                return;
            }

            _state.Player?.ASC?.Tick(deltaTime);
            foreach (var monster in _state.Combat?.GetAliveMonsters())
            {
                monster?.ASC?.Tick(deltaTime);
            }

            _spawnModule.Tick(deltaTime, Tick);
            _combatModule.Tick(deltaTime, Tick);
        }

        protected override void HandleInternalEvent(DefenseHostEvent eventData)
        {
            //이벤트 중 내부에서 처리하고 싶은 경우도 있을 수 있어서
            switch (eventData)
            {
                case DefenseMonsterSpawnedEvent spawnedEvent:
                    _needPlayerAIRecheck = true;

                    break;
            }
            ;
        }

        protected override DefenseHostSnapshot BuildSnapshotInternal()
        {
            var player = _state.Player;
            var combat = _state.Combat;

            return new DefenseHostSnapshot(
                tick: Tick,
                sessionPhase: _state.SessionPhase,
                playerUid: player?.Uid ?? 0,
                playerLevel: player != null ? player.GetLevel() : 0,
                playerHp: player != null ? (int)player.GetHp() : 0,
                playerMaxHp: player != null ? (int)player.GetMaxHp() : 0,
                killCount: combat?.KillCount ?? 0,
                bossKillCount: combat?.BossKillCount ?? 0,
                aliveMonsterCount: combat?.AliveMonsterCount ?? 0,
                elapsedTime: combat?.ElapsedTime ?? 0f,
                isGameOver: combat?.IsGameOver ?? false,
                isDefeat: combat?.IsDefeat ?? false
            );
        }

        #endregion

        #region Command Handlers

        private GameCommandOutcome<DefenseCommandResult, DefenseHostEvent> HandleStartGame(StartGameCommand command)
        {
            if (_state.SessionPhase != DefenseSessionPhase.None)
            {
                return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                    StartGameResult.Fail(Tick, command.SenderUid, "이미 게임이 시작되어 있습니다."));
            }

            _targetContext = CreateTargetContext();

            var playerUid = command.SenderUid > 0 ? command.SenderUid : 1;
            var player = _entityFactory.CreatePlayer(playerUid);

            player.FindTargets = pos => _state.Combat?.GetAliveMonsters();

            var playerAI = new PlayerAutoBattleAI
            {
                TargetContext = _targetContext,
                Checker = this
            };

            player.AI = playerAI;
            player.OnLevelUp += HandlePlayerLevelUp;
            player.OnActivateAbility += HandleActivateAbility;

            _state.SetPlayer(player);

            var combat = new DefenseCombatState();
            _state.SetCombat(combat);

            _spawnModule.Initialize(_targetContext);
            _state.SetSessionPhase(DefenseSessionPhase.Playing);

            var events = new List<DefenseHostEvent>
            {
                new DefenseGameStartedEvent(Tick)
            };

            return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                StartGameResult.Ok(Tick, command.SenderUid),
                events);
        }

        private GameCommandOutcome<DefenseCommandResult, DefenseHostEvent> HandleSelectLevelUpAbility(
            SelectLevelUpAbilityCommand command)
        {
            //유효성 검증
            if (_state.SessionPhase != DefenseSessionPhase.LevelUpSelection)
            {
                return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                    SelectLevelUpAbilityResult.Fail(Tick, command.SenderUid, "레벨업 선택 상태가 아닙니다."));
            }

            var options = _state.CurrentLevelUpOptions;
            if (command.AbilityIndex < 0 || command.AbilityIndex >= options.Count)
            {
                return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                    SelectLevelUpAbilityResult.Fail(Tick, command.SenderUid, "잘못된 선택 인덱스입니다."));
            }

            //상태 변경
            var selected = options[command.AbilityIndex];

            selected.ApplyAction?.Invoke(_state.Player);
            _state.AddGrantedAbility(selected.AbilityTag);
            _state.SetSessionPhase(DefenseSessionPhase.Playing);

            //이벤트 발행
            var events = new List<DefenseHostEvent>
            {
                new DefenseAbilitySelectedEvent(Tick, selected.AbilityTag, selected.DisplayName)
            };

            return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                SelectLevelUpAbilityResult.Ok(
                    Tick,
                    command.SenderUid,
                    selected.AbilityTag.ToString(),
                    selected.DisplayName),
                events);
        }

        private GameCommandOutcome<DefenseCommandResult, DefenseHostEvent> HandleEndGame(EndGameCommand command)
        {
            if (_state.SessionPhase == DefenseSessionPhase.None ||
                _state.SessionPhase == DefenseSessionPhase.GameOver)
            {
                return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                    EndGameResult.Fail(Tick, command.SenderUid, "게임이 시작되지 않았습니다."));
            }

            if (_state.Combat != null && !_state.Combat.IsGameOver)
            {
                _state.Combat.SetGameOver(isDefeat: false);
            }

            _state.SetSessionPhase(DefenseSessionPhase.GameOver);

            return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                EndGameResult.Ok(Tick, command.SenderUid));
        }

        #endregion

        #region Internal Event Handlers

        private void HandlePlayerLevelUp(DefensePlayer player, int newLevel)
        {
            _state.SetSessionPhase(DefenseSessionPhase.LevelUpSelection);
            PublishEvent(new DefenseLevelUpEvent(Tick, newLevel));

            _levelUpRegistry.GetRandomAvailableAbilities(
                player,
                _state.GrantedAbilities,
                3,
                _tempLevelUpOptions
            );
            _state.SetLevelUpOptions(_tempLevelUpOptions);

            PublishEvent(new DefenseLevelUpOptionsEvent(Tick, _state.CurrentLevelUpOptions));
        }

        private void HandleActivateAbility(GameplayAbility ability, TargetData targetData)
        {
            if (targetData == null) return;
            if (targetData.Targets.Count == 0) return;

            List<long> targetUids = null;

            foreach (var target in targetData.Targets)
            {
                var targetOwner = target.Owner as DefenseEntity;
                if (targetOwner == null)
                {
                    continue;
                }

                targetUids ??= new List<long>();
                targetUids.Add(targetOwner.Uid);
            }

            if (targetUids == null || targetUids.Count == 0)
            {
                return;
            }

            PublishEvent(new DefensePlayerActivateAbilityEvent(Tick, targetUids, ability));
        }

        #endregion

        #region TargetContext Helpers

        private TargetContext CreateTargetContext()
        {
            return new TargetContext(
                getEnemies: GetEnemiesForASC,
                getAllies: GetAlliesForASC,
                getPosition: GetPositionForASC
            );
        }

        private IReadOnlyList<AbilitySystemComponent> GetEnemiesForASC(AbilitySystemComponent ownerASC)
        {
            _tempEnemyList.Clear();

            if (_state.Player != null && _state.Player.ASC == ownerASC)
            {
                var monsters = _state.Combat?.GetAliveMonsters();
                if (monsters != null)
                {
                    for (var i = 0; i < monsters.Count; i++)
                    {
                        _tempEnemyList.Add(monsters[i].ASC);
                    }
                }
            }
            else if (_state.Player != null && _state.Player.IsAlive)
            {
                _tempEnemyList.Add(_state.Player.ASC);
            }

            return _tempEnemyList;
        }

        private IReadOnlyList<AbilitySystemComponent> GetAlliesForASC(AbilitySystemComponent ownerASC)
        {
            return Array.Empty<AbilitySystemComponent>();
        }

        private Point2D GetPositionForASC(AbilitySystemComponent ownerASC)
        {
            if (_state.Player != null && _state.Player.ASC == ownerASC)
            {
                return _state.Player.Position;
            }

            var monsters = _state.Combat?.GetAliveMonsters();
            if (monsters != null)
            {
                for (var i = 0; i < monsters.Count; i++)
                {
                    if (monsters[i].ASC == ownerASC)
                    {
                        return monsters[i].Position;
                    }
                }
            }

            return default;
        }

        #endregion

        public bool ConsumePlayerAIRecheck()
        {
            var changed = _needPlayerAIRecheck;
            _needPlayerAIRecheck = false;
            return changed;
        }

        public override void Dispose()
        {
            base.Dispose();

            _state?.Dispose();
        }
    }
}
