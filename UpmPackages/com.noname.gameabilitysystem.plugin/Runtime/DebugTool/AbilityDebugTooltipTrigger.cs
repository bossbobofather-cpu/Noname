using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Noname.GameAbilitySystem.DebugTool
{
    public sealed class AbilityDebugTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        private AbilityDebugTooltip _tooltip;
        private string _title;
        private string _description;
        private float _delay = 1f;
        private Coroutine _routine;
        private Vector2 _lastPosition;

        public void Setup(AbilityDebugTooltip tooltip, string title, string description, float delay)
        {
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

            _lastPosition = eventData.position;
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(ShowAfterDelay());
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            _lastPosition = eventData.position;
            if (_tooltip != null && _tooltip.IsVisible)
            {
                _tooltip.SetPosition(_lastPosition);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
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
