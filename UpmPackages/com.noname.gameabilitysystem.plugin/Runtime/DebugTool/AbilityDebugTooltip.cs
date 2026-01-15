using UnityEngine;
using UnityEngine.UI;

namespace Noname.GameAbilitySystem.DebugTool
{
    public sealed class AbilityDebugTooltip : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Vector2 _offset = new Vector2(12f, -12f);
        [SerializeField] private float _maxWidth = 320f;

        private Canvas _canvas;
        private RectTransform _canvasRect;

        public bool IsVisible => _root != null && _root.gameObject.activeSelf;

        public void EnsureBuilt(Font font, Color textColor, Color backgroundColor)
        {
            if (_root == null)
            {
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

        public void Hide()
        {
            if (_root != null)
            {
                gameObject.SetActive(false);
            }
        }

        public void SetPosition(Vector2 screenPosition)
        {
            if (_root == null)
            {
                return;
            }

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

            _canvas = GetComponentInParent<Canvas>();
            _canvasRect = _canvas != null ? _canvas.transform as RectTransform : null;
        }

        private void ClampToCanvas()
        {
            if (_canvasRect == null)
            {
                return;
            }

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
