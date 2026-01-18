using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Noname.GameAbilitySystem.DebugTool
{
    /// <summary>
    /// 툴팁 표시를 담당하는 트리거 컴포넌트입니다.
    /// </summary>
    public sealed class AbilityDebugTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        private AbilityDebugTooltip _tooltip;
        private string _title;
        private string _description;
        private float _delay = 1f;
        private Coroutine _routine;
        private Vector2 _lastPosition;

        /// <summary>
        /// 툴팁 데이터를 설정합니다.
        /// </summary>
        public void Setup(AbilityDebugTooltip tooltip, string title, string description, float delay)
        {
            // 표시할 데이터를 저장한다.
            _tooltip = tooltip;
            _title = title;
            _description = description;
            _delay = Mathf.Max(0f, delay);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltip == null)
            {
                return;
            }

            // 포인터 진입 시 표시 예약을 건다.
            _lastPosition = eventData.position;
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(ShowAfterDelay());
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            // 위치를 갱신하고 표시 중이면 따라가게 한다.
            _lastPosition = eventData.position;
            if (_tooltip != null && _tooltip.IsVisible)
            {
                _tooltip.SetPosition(_lastPosition);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // 포인터 이탈 시 예약을 취소한다.
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            if (_tooltip != null)
            {
                _tooltip.Hide();
            }
        }

        private IEnumerator ShowAfterDelay()
        {
            if (_delay > 0f)
            {
                // 지정된 시간만큼 대기한다.
                yield return new WaitForSecondsRealtime(_delay);
            }

            if (_tooltip != null)
            {
                _tooltip.Show(_title, _description, _lastPosition);
            }

            _routine = null;
        }

        private void OnDisable()
        {
            // 비활성화 시 예약과 표시를 정리한다.
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            if (_tooltip != null)
            {
                _tooltip.Hide();
            }
        }
    }
}
