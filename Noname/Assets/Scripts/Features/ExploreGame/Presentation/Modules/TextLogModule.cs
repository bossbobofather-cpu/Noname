using System;
using UnityEngine;
using MyProject.Common.GameEvent;
using MyProject.Common.GameMode;
using MyProject.ExploreGame.Application;

namespace MyProject.ExploreGame.Presentation
{
    /// <summary>
    /// 게임 진행 상황을 텍스트 로그로 출력하는 모듈입니다.
    /// </summary>
    public sealed class TextLogModule : ModuleBase
    {
        [Header("Log Settings")]
        [SerializeField] private bool _enableDetailedLogs = true;
        [SerializeField] private Color _eventColor = Color.cyan;
        [SerializeField] private Color _combatColor = Color.yellow;
        [SerializeField] private Color _rewardColor = Color.green;

        private Action<ExploreHostEventRaisedEvent> _eventHandler;

        protected override void OnInit()
        {
            base.OnInit();

            // Host 이벤트 구독
            _eventHandler = OnHostEvent;
            Mode.Subscribe(_eventHandler);
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();

            if (_eventHandler != null)
            {
                Mode.Unsubscribe(_eventHandler);
                _eventHandler = null;
            }
        }

        /// <summary>
        /// Host 이벤트를 받아서 로그로 출력합니다.
        /// </summary>
        private void OnHostEvent(ExploreHostEventRaisedEvent evt)
        {
            if (evt?.Event == null)
            {
                return;
            }

            var message = FormatEventMessage(evt.Event);
            if (!string.IsNullOrEmpty(message))
            {
                LogMessage(message, GetEventColor(evt.Event));
            }
        }

        /// <summary>
        /// 이벤트를 사람이 읽을 수 있는 메시지로 변환합니다.
        /// </summary>
        private string FormatEventMessage(ExploreHostEvent evt)
        {
            return evt switch
            {
                ExplorePlayerJoinedEvent e =>
                    $"[게임 시작] '{e.CharacterName}'이(가) 탐험을 시작했습니다!",

                ExploreDungeonStartedEvent e =>
                    $"[던전 시작] {e.DungeonId} 던전 입장! (총 {e.TotalStages}층)",

                ExploreStageChangedEvent e =>
                    $"[층 이동] {e.Stage}층에 도착했습니다.",

                ExploreCombatStartedEvent e =>
                    $"[전투 시작] {e.MonsterType} {e.MonsterCount}마리가 나타났습니다!",

                ExploreCombatActionEvent e =>
                    FormatCombatAction(e),

                ExploreCombatEndedEvent e =>
                    e.Victory
                        ? "[승리] 전투에서 승리했습니다!"
                        : "[패배] 전투에서 패배했습니다...",

                ExploreRewardGrantedEvent e =>
                    $"[보상] 골드 +{e.Gold}, 경험치 +{e.Experience}",

                ExploreCharacterLevelUpEvent e =>
                    $"[레벨업] 레벨 {e.NewLevel}로 레벨업했습니다!",

                ExploreDungeonCompletedEvent e =>
                    $"[던전 완료] {e.DungeonId} 던전을 완료했습니다!",

                ExploreDungeonFailedEvent e =>
                    $"[던전 실패] {e.DungeonId} 던전 {e.ReachedStage}층에서 실패했습니다.",

                _ => _enableDetailedLogs ? $"[이벤트] {evt.GetType().Name}" : null
            };
        }

        /// <summary>
        /// 전투 행동 메시지를 포맷합니다.
        /// </summary>
        private string FormatCombatAction(ExploreCombatActionEvent evt)
        {
            var message = $"{evt.ActorName}이(가) {evt.TargetName}에게 {evt.Damage} 데미지!";

            if (evt.IsTargetDead)
            {
                message += " [사망]";
            }

            return message;
        }

        /// <summary>
        /// 이벤트 타입에 따른 색상을 반환합니다.
        /// </summary>
        private Color GetEventColor(ExploreHostEvent evt)
        {
            return evt switch
            {
                ExploreCombatActionEvent => _combatColor,
                ExploreRewardGrantedEvent => _rewardColor,
                ExploreCharacterLevelUpEvent => _rewardColor,
                _ => _eventColor
            };
        }

        /// <summary>
        /// 색상이 적용된 로그를 출력합니다.
        /// </summary>
        private void LogMessage(string message, Color color)
        {
            var hexColor = ColorUtility.ToHtmlStringRGB(color);
            Debug.Log($"<color=#{hexColor}>{message}</color>");
        }
    }
}
