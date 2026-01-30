using System.Text;
using MyProject.Common.GameMode;
using MyProject.Common.UI;
using MyProject.DefenseGame.Application;
using MyProject.DefenseGame.Application.Commands;
using MyProject.DefenseGame.Domain;
using MyProject.DefenseGame.Domain.LevelUp;
using Noname.GameHost;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MyProject.DefenseGame.Presentation
{
    /// <summary>
    /// 디펜스 게임의 프레젠테이션 모드입니다.
    /// 호스트 이벤트를 수신해 시스템 메시지로 출력합니다.
    /// </summary>
    public class DefenseGameMode : GameMode<DefenseCommand, DefenseCommandResult, DefenseHostEvent, DefenseHostSnapshot>
    {
        [SerializeField] private long _localUserId = 1;
        private readonly Color _logColor = new Color(0f, 0f, 0f, 0.6f);
        private readonly Color _errorColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);
        private readonly Color _startColor = new Color(0.2f, 0.6f, 1f, 0.6f);
        private readonly Color _spawnColor = new Color(0.3f, 0.7f, 1f, 0.6f);
        private readonly Color _bossSpawnColor = Color.black;
        private readonly Color _killColor = new Color(0.3f, 0.8f, 0.4f, 0.6f);
        private readonly Color _playerActivateAbilityColor = new Color(0f, 0f, 1f, 0.6f);
        private readonly Color _monsterActivateAbilityColor = new Color(1f, 0f, 0f, 0.6f);
        private readonly Color _levelUpColor = new Color(1f, 0.85f, 0.2f, 0.6f);
        private readonly Color _abilitySelectionColor = new Color(1f, 0f, 1f, 0.6f);
        private readonly Color _abilityColor = new Color(0.4f, 0.8f, 0.8f, 0.6f);
        private readonly Color _deathColor = new Color(0.9f, 0.2f, 0.2f, 0.6f);
        private readonly Color _waveColor = new Color(0.7f, 0.5f, 1f, 0.6f);
        private readonly Color _victoryColor = new Color(0.2f, 0.8f, 0.4f, 0.6f);
        private readonly Color _defeatColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);

        private bool _waitOptionSelect = false;
        private DefenseLevelUpOptionsEvent _pendingLvUpOptionEvent = null;
        private DefenseHostSnapshot _latestSnapshot = null;
        private long _latestSnapshotTick = -1;
        protected override void OnInitialize()
        {
            base.OnInitialize();

            Host?.SendCommand(new StartGameCommand(_localUserId));
        }

        protected override void Update()
        {
            base.Update();

            SyncSnapshot();
            CheckLvUpPhaseInteraction();
        }

        private void SyncSnapshot()
        {
            var snapshot = Host?.GetLatestSnapshot();
            if (snapshot == null)
            {
                return;
            }

            if (snapshot.Tick == _latestSnapshotTick)
            {
                return;
            }

            _latestSnapshot = snapshot;
            _latestSnapshotTick = snapshot.Tick;
        }

        private void SelectOption(int optionIndex)
        {
            if (_pendingLvUpOptionEvent == null) return;
            if (_pendingLvUpOptionEvent.Options == null) return;

            int optionCount = _pendingLvUpOptionEvent.Options.Count;
            if (optionIndex <= 0) return;

            if (optionCount < optionIndex)
            {
                return;
            }

            //옵션 선택을 전달한다.
            Host?.SendCommand(new SelectLevelUpAbilityCommand(_localUserId, optionIndex - 1));

            _waitOptionSelect = false;
            _pendingLvUpOptionEvent = null;
        }

        private void CheckLvUpPhaseInteraction()
        {
            if (!_waitOptionSelect 
            || _pendingLvUpOptionEvent == null 
            || _pendingLvUpOptionEvent.Options == null)
                return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            for (var i = 0; i < 9; i++)
            {
                var key = Key.Digit1 + i;
                if (keyboard[key].wasPressedThisFrame)
                {
                    SelectOption(i + 1);
                    break;
                }
            }
        }

        protected override void OnHostResult(DefenseCommandResult result)
        {
            if (result == null)
            {
                return;
            }

            if (!result.Success)
            {
                PublishMessage($"[커맨드 실패] {result.ErrorMessage}", _errorColor);
            }
        }

        /// <summary>
        /// 호스트 이벤트를 처리합니다.
        /// </summary>
        protected override void OnHostEvent(DefenseHostEvent evt)
        {
            switch (evt)
            {
                case DefenseGameStartedEvent e:
                    PublishMessage($"[게임 시작] Tick: {e.Tick}", _startColor);
                    break;

                case DefenseMonsterSpawnedEvent e:
                    if(e.IsBoss)
                    {
                        PublishMessage($"================================================================================", _bossSpawnColor);
                        PublishMessage($"[보스 생성] {e.MonsterType} (UID: {e.MonsterUid})", _bossSpawnColor);
                        PublishMessage($"================================================================================", _bossSpawnColor);
                    }
                    else
                    {
                        PublishMessage($"[몬스터 생성] {e.MonsterType} (UID: {e.MonsterUid})", _spawnColor);
                    }
                    break;

                case DefenseMonsterKilledEvent e:
                    PublishMessage($"[몬스터 처치] {e.MonsterType} (UID: {e.MonsterUid}, EXP: +{e.ExpGained})", _killColor);
                    break;

                case DefensePlayerActivateAbilityEvent e:
                    PlyerActivateAbilityEvent(e);
                    break;
                case DefenseMonsterActivateAbilityEvent e:
                    MonsterActivateAbilityEvent(e);
                    break;
                case DefenseLevelUpEvent e:
                    PublishMessage($"[레벨업] 레벨: {e.NewLevel}", _levelUpColor);
                    break;

                case DefenseLevelUpOptionsEvent e:
                    LevelUpEventOptions(e);
                    _waitOptionSelect = true;
                    _pendingLvUpOptionEvent = e;
                    break;

                case DefenseAbilitySelectedEvent e:
                    PublishMessage($"[능력 선택] {e.AbilityName} ({e.AbilityTag})", _abilityColor);
                    break;

                case DefensePlayerDeathEvent e:
                    PublishMessage($"[플레이어 사망] 생존시간: {e.SurvivalTime:F1}s, 처치: {e.TotalKills}", _deathColor);
                    break;

                case DefenseWaveChangedEvent e:
                    PublishMessage($"[웨이브 변경] Wave: {e.Wave}", _waveColor);
                    break;

                case DefenseGameOverEvent e:
                    PublishMessage(
                        $"[게임 종료] 승리: {e.IsVictory}, 생존시간: {e.SurvivalTime:F1}s, 처치: {e.TotalKills}",
                        e.IsVictory ? _victoryColor : _defeatColor);
                    break;
            }
        }

        /// <summary>
        /// 레벨업 선택지 로그를 출력합니다.
        /// </summary>
        private void LevelUpEventOptions(DefenseLevelUpOptionsEvent evt)
        {
            PublishMessage("[레벨업 선택지]", _abilitySelectionColor);

            var optionCount = evt.Options.Count;
            for (var i = 0; i < evt.Options.Count; i++)
            {
                var option = evt.Options[i];
                PublishMessage($"[{i + 1}] {option.DisplayName}: {option.Description}", _abilitySelectionColor);
            }

            PublishMessage($"1~{optionCount} 키보드 입력 을 통해 선택하세요.", _abilitySelectionColor);
        }

        private void PlyerActivateAbilityEvent(DefensePlayerActivateAbilityEvent evt)
        {
            var ability = evt.Ability;
            foreach(var uid in evt.TargetUids)
            {
                PublishMessage(
                    $"[플레이어 능력 발동] 몬스터 {uid}에게 {ability.DisplayName} 능력 발동. 효과 : {ability.Description}", _playerActivateAbilityColor);   
            }
        }

        private void MonsterActivateAbilityEvent(DefenseMonsterActivateAbilityEvent evt)
        {
            var ability = evt.Ability;
            PublishMessage(
                $"[몬스터 {evt.MonsterUid} 능력 발동] 플레이어에게 {ability.DisplayName} 능력 발동. 효과 : {ability.Description}", _monsterActivateAbilityColor);
        }

        /// <summary>
        /// 시스템 메시지로 로그를 출력합니다.
        /// </summary>
        private void PublishMessage(string message, Color color)
        {
            SystemMessageBus.Publish(message, color);
            Debug.Log($"[DefenseGame] {message}");
        }

        /// <summary>
        /// 기본 색상으로 시스템 메시지를 출력합니다.
        /// </summary>
        private void PublishMessage(string message)
        {
            PublishMessage(message, _logColor);
        }
    }
}
