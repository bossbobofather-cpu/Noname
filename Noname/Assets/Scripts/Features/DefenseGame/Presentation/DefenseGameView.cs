using System.Text;
using MyProject.DefenseGame.Application;
using MyProject.DefenseGame.Application.Commands;
using MyProject.DefenseGame.Data;
using MyProject.DefenseGame.Domain.LevelUp;
using UnityEngine;

namespace MyProject.DefenseGame.Presentation
{
    /// <summary>
    /// 디펜스 게임 View입니다.
    /// 호스트 이벤트를 수신하여 로그 출력 및 UI 갱신을 담당합니다.
    /// </summary>
    public class DefenseGameView : MonoBehaviour
    {
        [SerializeField] private DefenseHostConfig _config;
        [SerializeField] private long _localUserId = 1;

        private DefenseGameHost _host;
        private readonly StringBuilder _logBuilder = new();

        /// <summary>
        /// 게임 호스트 인스턴스입니다.
        /// </summary>
        public DefenseGameHost Host => _host;

        private void Awake()
        {
            if (_config == null)
            {
                _config = new DefenseHostConfig();
            }

            _host = new DefenseGameHost(_config);
            _host.ResultProduced += OnHostResult;
            _host.EventRaised += OnHostEvent;
            _host.StartSimulation();
        }

        private void OnDestroy()
        {
            if (_host == null)
            {
                return;
            }

            _host.ResultProduced -= OnHostResult;
            _host.EventRaised -= OnHostEvent;
            _host.Dispose();
            _host = null;
        }

        private void Update()
        {
            _host?.FlushEvents();
        }

        /// <summary>
        /// 게임을 시작합니다.
        /// </summary>
        [ContextMenu("Start Game")]
        public void StartGame()
        {
            _host?.Submit(new StartGameCommand(_localUserId));
        }

        /// <summary>
        /// 레벨업 능력을 선택합니다.
        /// </summary>
        public void SelectLevelUpAbility(int index)
        {
            _host?.Submit(new SelectLevelUpAbilityCommand(index, _localUserId));
        }

        /// <summary>
        /// 호스트 결과를 처리합니다.
        /// </summary>
        private void OnHostResult(DefenseCommandResult result)
        {
            if (result == null)
            {
                return;
            }

            if (!result.Success)
            {
                Log($"[명령 실패] {result.ErrorMessage}");
            }
        }

        /// <summary>
        /// 호스트 이벤트를 처리합니다.
        /// </summary>
        private void OnHostEvent(DefenseHostEvent evt)
        {
            switch (evt)
            {
                case DefenseGameStartedEvent e:
                    Log($"[게임 시작] Tick: {e.Tick}");
                    break;

                case DefenseMonsterSpawnedEvent e:
                    Log($"[몬스터 스폰] {e.MonsterType} (UID: {e.MonsterUid}, Boss: {e.IsBoss})");
                    break;

                case DefenseMonsterKilledEvent e:
                    Log($"[몬스터 처치] {e.MonsterType} (UID: {e.MonsterUid}, EXP: +{e.ExpGained})");
                    break;

                case DefensePlayerAttackEvent e:
                    Log($"[플레이어 공격] 대상 {e.TargetUid}, 데미지: {e.Damage}, 처치: {e.TargetKilled}");
                    break;

                case DefenseLevelUpEvent e:
                    Log($"[레벨업] 레벨: {e.NewLevel}");
                    break;

                case DefenseLevelUpOptionsEvent e:
                    LogLevelUpOptions(e);
                    break;

                case DefenseAbilitySelectedEvent e:
                    Log($"[능력 선택] {e.AbilityName} ({e.AbilityId})");
                    break;

                case DefensePlayerDeathEvent e:
                    Log($"[플레이어 사망] 생존시간: {e.SurvivalTime:F1}s, 처치: {e.TotalKills}");
                    break;

                case DefenseWaveChangedEvent e:
                    Log($"[웨이브 변경] Wave: {e.Wave}");
                    break;

                case DefenseGameOverEvent e:
                    Log($"[게임 종료] 승리: {e.IsVictory}, 생존시간: {e.SurvivalTime:F1}s, 처치: {e.TotalKills}");
                    break;
            }
        }

        /// <summary>
        /// 레벨업 선택지를 로그로 출력합니다.
        /// </summary>
        private void LogLevelUpOptions(DefenseLevelUpOptionsEvent evt)
        {
            _logBuilder.Clear();
            _logBuilder.AppendLine("[레벨업 선택지]");

            for (var i = 0; i < evt.Options.Count; i++)
            {
                var option = evt.Options[i];
                _logBuilder.AppendLine($"  [{i + 1}] {option.DisplayName}: {option.Description}");
            }

            Log(_logBuilder.ToString());
        }

        /// <summary>
        /// 로그를 출력합니다.
        /// </summary>
        private void Log(string message)
        {
            Debug.Log($"[DefenseGame] {message}");
        }
    }
}
