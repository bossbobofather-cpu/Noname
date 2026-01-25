using UnityEngine;
using MyProject.Common.GameMode;

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
            Debug.Log($"<color=#00FFFF>[상태] {summary}</color>");
        }
    }
}
