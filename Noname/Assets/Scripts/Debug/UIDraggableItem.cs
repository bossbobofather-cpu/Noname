using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MergeGame.Debug
{
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
            CacheReferences();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_rectTransform == null)
            {
                return;
            }

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

        public void SetDragTarget(RectTransform dragTarget)
        {
            _dragTarget = dragTarget;
            CacheReferences();
        }

        private void CacheReferences()
        {
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
