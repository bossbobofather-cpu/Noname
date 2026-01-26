using UnityEngine;
using MyProject.Common.GameMode;
using MyProject.Common.UI;

namespace MyProject.ExploreGame.Presentation
{
    /// <summary>
    /// 주기적으로 캐릭터 상태를 출력하는 모듈입니다.
    /// </summary>
    public sealed class StatusDisplayModule : ModuleBase
    {
        [Header("Display Settings")]
        [SerializeField] private float _updateInterval = 5f;
        [SerializeField] private bool _enabled = true;
        [SerializeField] private Color _statusColor = new Color(0.3f, 0.3f, 0.5f, 0.8f); // 진한 회색-파랑

        private ExploreMode _exploreMode;
        private float _timer;

        protected override void OnInit()
        {
            base.OnInit();

            _exploreMode = Mode as ExploreMode;
            if (_exploreMode == null)
            {
                Debug.LogError("[StatusDisplayModule] ExploreMode를 찾을 수 없습니다.");
            }
        }

        private void Update()
        {
            if (!_enabled || _exploreMode == null)
            {
                return;
            }

            // 게임이 종료되면 상태 출력 중지
            if (_exploreMode.ViewModel.SessionPhase == Domain.ExploreSessionPhase.Ended)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer < _updateInterval)
            {
                return;
            }

            _timer -= _updateInterval;

            DisplayStatus();
        }

        /// <summary>
        /// 현재 상태를 출력합니다.
        /// </summary>
        private void DisplayStatus()
        {
            var summary = _exploreMode.ViewModel.GetStatusSummary();

            // 콘솔 로그
            Debug.Log($"<color=#00FFFF>[상태] {summary}</color>");

            // SystemMessageUI에도 표시
            SystemMessageBus.Publish(summary, _statusColor);
        }
    }
}
