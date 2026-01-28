using System;
using System.Collections.Generic;
using Noname.GameHost;
using MyProject.DefenseGame.Application.Commands;
using MyProject.DefenseGame.Domain;
using MyProject.DefenseGame.Domain.AI;
using MyProject.DefenseGame.Domain.LevelUp;
using Noname.GameAbilitySystem.Domain;

namespace MyProject.DefenseGame.Application
{
    /// <summary>
    /// ���潺 ���� ���� ������ ���� ȣ��Ʈ�Դϴ�.
    /// CQRS �������� Command�� ó���ϰ� Result/Event�� �����մϴ�.
    /// </summary>
    public sealed class DefenseGameHost
        : GameHostBase<DefenseCommand, DefenseCommandResult, DefenseHostEvent, DefenseHostSnapshot>
    {
        private readonly DefenseHostState _state;
        private readonly DefenseHostConfig _config;

        private readonly DefenseSpawnModule _spawnModule;
        private readonly DefenseCombatModule _combatModule;
        private readonly DefenseEntityFactory _entityFactory;

        private readonly List<AbilitySystemComponent> _tempEnemyList = new();
        private readonly LevelUpAbilityRegistry _levelUpRegistry;
        private readonly List<LevelUpAbilityDefinition> _tempLevelUpOptions = new();
        private TargetContext _targetContext;

        public DefenseGameHost(DefenseHostConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _state = new DefenseHostState();

            _entityFactory = new DefenseEntityFactory(_config);
            _spawnModule = new DefenseSpawnModule(_state, _config, _entityFactory, PublishEvent);
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

            if (_state.SessionPhase == DefenseSessionPhase.LevelUpSelection)
            {
                return;
            }

            _spawnModule.Tick(deltaTime, Tick);
            _combatModule.Tick(deltaTime, Tick);
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
                    StartGameResult.Fail(Tick, command.SenderUid, "������ �̹� ���� ���Դϴ�."));
            }

            _targetContext = CreateTargetContext();

            var playerUid = command.SenderUid > 0 ? command.SenderUid : 1;
            var player = _entityFactory.CreatePlayer(playerUid);

            player.FindTargets = pos => _state.Combat?.GetAliveMonsters();

            var playerAI = new PlayerAutoBattleAI
            {
                TargetContext = _targetContext
            };
            player.AI = playerAI;

            player.OnAttack += HandlePlayerAttack;
            player.OnLevelUp += HandlePlayerLevelUp;

            _state.SetPlayer(player);

            var combat = new DefenseCombatState();
            _state.SetCombat(combat);

            _spawnModule.Initialize();
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
            if (_state.SessionPhase != DefenseSessionPhase.LevelUpSelection)
            {
                return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                    SelectLevelUpAbilityResult.Fail(Tick, command.SenderUid, "������ ���� ���°� �ƴմϴ�."));
            }

            var options = _state.CurrentLevelUpOptions;
            if (command.AbilityIndex < 0 || command.AbilityIndex >= options.Count)
            {
                return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                    SelectLevelUpAbilityResult.Fail(Tick, command.SenderUid, "��ȿ���� ���� �ɷ� �ε����Դϴ�."));
            }

            var selected = options[command.AbilityIndex];

            selected.ApplyAction?.Invoke(_state.Player);
            _state.AddGrantedAbility(selected.Id);
            _state.SetSessionPhase(DefenseSessionPhase.Playing);

            var events = new List<DefenseHostEvent>
            {
                new DefenseAbilitySelectedEvent(Tick, selected.Id, selected.DisplayName)
            };

            return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                SelectLevelUpAbilityResult.Ok(
                    Tick,
                    command.SenderUid,
                    selected.Id.ToString(),
                    selected.DisplayName),
                events);
        }

        private GameCommandOutcome<DefenseCommandResult, DefenseHostEvent> HandleEndGame(EndGameCommand command)
        {
            if (_state.SessionPhase == DefenseSessionPhase.None ||
                _state.SessionPhase == DefenseSessionPhase.GameOver)
            {
                return new GameCommandOutcome<DefenseCommandResult, DefenseHostEvent>(
                    EndGameResult.Fail(Tick, command.SenderUid, "������ ���� ���� �ƴմϴ�."));
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

        private void HandlePlayerAttack(DefensePlayer player, DefenseMonster target, int damage)
        {
            PublishEvent(new DefensePlayerAttackEvent(
                Tick,
                target.Uid,
                damage,
                target.IsDead
            ));
        }

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
    }
}
