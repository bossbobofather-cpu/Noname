using UnityEngine;
using UnityEngine.UI;

namespace Noname.GameAbilitySystem.DebugTool
{
    /// <summary>
    /// 툴팁 UI를 표시하는 컴포넌트입니다.
    /// </summary>
    public sealed class AbilityDebugTooltip : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Vector2 _offset = new Vector2(12f, -12f);
        [SerializeField] private float _maxWidth = 320f;

        private Canvas _canvas;
        private RectTransform _canvasRect;

        /// <summary>
        /// 현재 표시 중인지 여부입니다.
        /// </summary>
        public bool IsVisible => _root != null && _root.gameObject.activeSelf;

        /// <summary>
        /// 기본 UI 구성 요소를 준비합니다.
        /// </summary>
        public void EnsureBuilt(Font font, Color textColor, Color backgroundColor)
        {
            if (_root == null)
            {
                // 루트와 기본 컴포넌트를 보장한다.
                _root = GetComponent<RectTransform>();
                if (_root == null)
                {
                    _root = gameObject.AddComponent<RectTransform>();
                }

                _root.anchorMin = Vector2.zero;
                _root.anchorMax = Vector2.zero;
                _root.pivot = new Vector2(0f, 1f);
                _root.anchoredPosition = Vector2.zero;

                var image = GetComponent<Image>();
                if (image == null)
                {
                    image = gameObject.AddComponent<Image>();
                }
                image.color = backgroundColor;

                var layout = GetComponent<VerticalLayoutGroup>();
                if (layout == null)
                {
                    layout = gameObject.AddComponent<VerticalLayoutGroup>();
                }
                layout.padding = new RectOffset(8, 8, 6, 6);
                layout.spacing = 4f;
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;

                var fitter = GetComponent<ContentSizeFitter>();
                if (fitter == null)
                {
                    fitter = gameObject.AddComponent<ContentSizeFitter>();
                }
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            _titleText = EnsureText(_titleText, "Title", font, textColor, 12, FontStyle.Bold);
            _descriptionText = EnsureText(_descriptionText, "Description", font, textColor, 11, FontStyle.Normal);
            SetupWrapping(_titleText);
            SetupWrapping(_descriptionText);

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 툴팁을 표시합니다.
        /// </summary>
        public void Show(string title, string description, Vector2 screenPosition)
        {
            if (_root == null)
            {
                return;
            }

            if (_titleText != null)
            {
                _titleText.text = title ?? string.Empty;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = description ?? string.Empty;
            }

            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_root);
            SetPosition(screenPosition);
        }

        /// <summary>
        /// 툴팁을 숨깁니다.
        /// </summary>
        public void Hide()
        {
            if (_root != null)
            {
                // 표시를 끈다.
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 화면 좌표를 기준으로 위치를 갱신합니다.
        /// </summary>
        public void SetPosition(Vector2 screenPosition)
        {
            if (_root == null)
            {
                return;
            }

            // 캔버스 기준 좌표로 변환한다.
            EnsureCanvas();
            if (_canvasRect == null)
            {
                return;
            }

            var camera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : null;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPosition, camera, out var localPoint);
            _root.anchoredPosition = localPoint + _offset;
            ClampToCanvas();
        }

        private Text EnsureText(Text current, string name, Font font, Color color, int fontSize, FontStyle style)
        {
            if (current == null)
            {
                // 텍스트 오브젝트를 새로 만든다.
                var obj = new GameObject(name, typeof(RectTransform));
                obj.transform.SetParent(transform, false);
                current = obj.AddComponent<Text>();
            }

            current.font = font;
            current.fontSize = fontSize;
            current.fontStyle = style;
            current.color = color;
            current.alignment = TextAnchor.UpperLeft;
            current.raycastTarget = false;
            return current;
        }

        private void SetupWrapping(Text text)
        {
            if (text == null)
            {
                return;
            }

            // 줄바꿈과 너비 제한을 설정한다.
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var element = text.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = text.gameObject.AddComponent<LayoutElement>();
            }

            element.preferredWidth = _maxWidth;
            element.flexibleWidth = 0f;
        }

        private void EnsureCanvas()
        {
            if (_canvas != null && _canvasRect != null)
            {
                return;
            }

            // 부모 캔버스를 찾아 저장한다.
            _canvas = GetComponentInParent<Canvas>();
            _canvasRect = _canvas != null ? _canvas.transform as RectTransform : null;
        }

        private void ClampToCanvas()
        {
            if (_canvasRect == null)
            {
                return;
            }

            // 캔버스 영역을 넘지 않도록 위치를 보정한다.
            var rect = _canvasRect.rect;
            var size = _root.rect.size;
            var pivot = _root.pivot;
            var pos = _root.anchoredPosition;

            var minX = rect.xMin + size.x * pivot.x;
            var maxX = rect.xMax - size.x * (1f - pivot.x);
            var minY = rect.yMin + size.y * pivot.y;
            var maxY = rect.yMax - size.y * (1f - pivot.y);

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            _root.anchoredPosition = pos;
        }
    }
}
