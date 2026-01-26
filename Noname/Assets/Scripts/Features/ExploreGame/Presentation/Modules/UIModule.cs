using System;
using UnityEngine;
using MyProject.Common.GameEvent;
using MyProject.Common.GameMode;
using MyProject.Common.UI;
using MyProject.ExploreGame.Application;

namespace MyProject.ExploreGame.Presentation
{
    /// <summary>
    /// 게임 진행 상황을 SystemMessageUI로 출력하는 모듈입니다.
    /// </summary>
    public sealed class UIModule : ModuleBase
    {
        [Header("Message Colors")]
        [SerializeField] private Color _defaultColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color _eventColor = new Color(0.2f, 0.4f, 0.6f, 0.8f); // 파란색
        [SerializeField] private Color _combatColor = new Color(0.6f, 0.4f, 0.2f, 0.8f); // 주황색
        [SerializeField] private Color _abilityColor = new Color(0.6f, 0.2f, 0.6f, 0.8f); // 보라색
        [SerializeField] private Color _rewardColor = new Color(0.2f, 0.6f, 0.2f, 0.8f); // 초록색
        [SerializeField] private Color _dangerColor = new Color(0.6f, 0.2f, 0.2f, 0.8f); // 빨간색

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
        /// Host 이벤트를 받아서 SystemMessageUI로 출력합니다.
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
                var color = GetEventColor(evt.Event);
                SystemMessageBus.Publish(message, color);
            }
        }

        private void OnDetectedChangePlayerStatus(string message, Color color)
        {
            SystemMessageBus.Publish(message, color);
        }

        /// <summary>
        /// 이벤트를 사람이 읽을 수 있는 메시지로 변환합니다.
        /// </summary>
        private string FormatEventMessage(ExploreHostEvent evt)
        {
            return evt switch
            {
                ExplorePlayerJoinedEvent e =>
                    $"'{e.CharacterName}' 탐험 시작!",

                ExploreDungeonStartedEvent e =>
                    $"{e.DungeonId} 던전 입장 ({e.TotalStages}층)",

                ExploreStageChangedEvent e =>
                    $"{e.Stage}층 도착",

                ExploreCombatStartedEvent e =>
                    $"{e.MonsterType} {e.MonsterCount}마리 출현!",

                ExploreCombatActionEvent e =>
                    FormatCombatAction(e),

                ExploreAbilityUsedEvent e =>
                    FormatAbilityUsed(e),

                ExploreCombatEndedEvent e =>
                    e.Victory ? "전투 승리!" : "전투 패배...",

                ExploreRewardGrantedEvent e =>
                    $"골드 +{e.Gold}, 경험치 +{e.Experience}",

                ExploreCharacterLevelUpEvent e =>
                    $"레벨 {e.NewLevel} 달성!",

                ExploreDungeonCompletedEvent e =>
                    $"{e.DungeonId} 던전 완료!",

                ExploreDungeonFailedEvent e =>
                    $"{e.DungeonId} {e.ReachedStage}층 실패",

                _ => null
            };
        }

        /// <summary>
        /// 전투 행동 메시지를 포맷합니다.
        /// </summary>
        private string FormatCombatAction(ExploreCombatActionEvent evt)
        {
            var message = $"{evt.ActorName} → {evt.TargetName} ({evt.Damage})";

            if (evt.IsTargetDead)
            {
                message += " ☠";
            }

            return message;
        }

        /// <summary>
        /// 어빌리티 사용 메시지를 포맷합니다.
        /// </summary>
        private string FormatAbilityUsed(ExploreAbilityUsedEvent evt)
        {
            var message = $"⚡ {evt.AbilityName} → {evt.TargetName} ({evt.Damage})";

            if (evt.IsTargetDead)
            {
                message += " ☠";
            }

            return message;
        }

        /// <summary>
        /// 이벤트 타입에 따른 배경 색상을 반환합니다.
        /// </summary>
        private Color GetEventColor(ExploreHostEvent evt)
        {
            return evt switch
            {
                ExplorePlayerJoinedEvent => _eventColor,
                ExploreDungeonStartedEvent => _eventColor,
                ExploreStageChangedEvent => _eventColor,
                ExploreCombatStartedEvent => _dangerColor,
                ExploreCombatActionEvent => _combatColor,
                ExploreAbilityUsedEvent => _abilityColor,
                ExploreCombatEndedEvent e => e.Victory ? _rewardColor : _dangerColor,
                ExploreRewardGrantedEvent => _rewardColor,
                ExploreCharacterLevelUpEvent => _rewardColor,
                ExploreDungeonCompletedEvent => _rewardColor,
                ExploreDungeonFailedEvent => _dangerColor,
                _ => _defaultColor
            };
        }
    }
}
