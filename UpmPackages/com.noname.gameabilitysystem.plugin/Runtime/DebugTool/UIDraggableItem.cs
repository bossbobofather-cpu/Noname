using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Noname.GameAbilitySystem.DebugTool
{
    /// <summary>
    /// 간단한 드래그 기능을 제공하는 컴포넌트입니다.
    /// </summary>
    public sealed class UIDraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private bool _returnToStart = true;
        [SerializeField] private RectTransform _dragTarget;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private LayoutElement _layoutElement;
        private Vector2 _startAnchoredPosition;

        private void Awake()
        {
            // 참조를 캐싱한다.
            CacheReferences();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_rectTransform == null)
            {
                return;
            }

            // 시작 위치를 저장한다.
            _startAnchoredPosition = _rectTransform.anchoredPosition;
            if (_layoutElement != null)
            {
                _layoutElement.ignoreLayout = true;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rectTransform == null)
            {
                return;
            }

            // 캔버스 스케일을 반영해 이동한다.
            var scale = _canvas != null ? _canvas.scaleFactor : 1f;
            _rectTransform.anchoredPosition += eventData.delta / scale;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_layoutElement != null)
            {
                _layoutElement.ignoreLayout = false;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
            }

            if (_returnToStart && _rectTransform != null)
            {
                _rectTransform.anchoredPosition = _startAnchoredPosition;
            }
        }

        /// <summary>
        /// 드래그할 대상을 지정합니다.
        /// </summary>
        public void SetDragTarget(RectTransform dragTarget)
        {
            _dragTarget = dragTarget;
            CacheReferences();
        }

        private void CacheReferences()
        {
            // 대상과 관련 컴포넌트를 찾아 둔다.
            if (_dragTarget == null)
            {
                _dragTarget = GetComponent<RectTransform>();
            }

            _rectTransform = _dragTarget;
            _canvas = GetComponentInParent<Canvas>();

            if (_dragTarget != null)
            {
                _canvasGroup = _dragTarget.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = _dragTarget.gameObject.AddComponent<CanvasGroup>();
                }

                _layoutElement = _dragTarget.GetComponent<LayoutElement>();
            }
        }
    }
}
