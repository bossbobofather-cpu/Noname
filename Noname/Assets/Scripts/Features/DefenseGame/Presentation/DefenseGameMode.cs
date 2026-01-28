using System.Text;
using MyProject.Common.GameMode;
using MyProject.DefenseGame.Application;
using MyProject.DefenseGame.Application.Commands;
using MyProject.DefenseGame.Domain.LevelUp;
using Noname.GameHost;
using UnityEngine;

namespace MyProject.DefenseGame.Presentation
{
    /// <summary>
    /// 디펜스 게임 뷰입니다.
    /// 호스트 이벤트를 수신하여 로그 출력 및 UI 업데이트를 수행합니다.
    /// </summary>
    public class DefenseGameMode : GameMode<DefenseCommand, DefenseCommandResult, DefenseHostEvent, DefenseHostSnapshot>
    {
        [SerializeField] private long _localUserId = 1;
        private readonly StringBuilder _logBuilder = new();

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void OnHostResult(DefenseCommandResult result)
        {
            if (result == null)
            {
                return;
            }

            if (!result.Success)
            {
                Log($"[커맨드 실패] {result.ErrorMessage}");
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
                    Log($"[게임 시작] Tick: {e.Tick}");
                    break;

                case DefenseMonsterSpawnedEvent e:
                    Log($"[몬스터 생성] {e.MonsterType} (UID: {e.MonsterUid}, Boss: {e.IsBoss})");
                    break;

                case DefenseMonsterKilledEvent e:
                    Log($"[몬스터 처치] {e.MonsterType} (UID: {e.MonsterUid}, EXP: +{e.ExpGained})");
                    break;

                case DefensePlayerAttackEvent e:
                    Log($"[플레이어 공격] 타겟 {e.TargetUid}, 데미지: {e.Damage}, 처치: {e.TargetKilled}");
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
        /// 레벨업 선택지 로그를 출력합니다.
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