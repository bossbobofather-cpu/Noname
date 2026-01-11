using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MergeGame.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class CanvasObjectLabel : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private string _title;
        [Header("UI")]
        [SerializeField] private RectTransform _root;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TMP_FontAsset _font;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _parent;
        [SerializeField] private bool _autoCreateCanvas = true;

        [Header("Layout")]
        [SerializeField] private float _titleSize = 20f;
        [SerializeField] private float _descriptionSize = 14f;
        [SerializeField] private float _lineSpacing;
        [SerializeField] private float _maxWidth = 240f;
        [SerializeField] private bool _boldTitle = true;
        [SerializeField] private Color _titleColor = Color.white;

        [Header("Follow")]
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private Vector2 _screenOffset = new Vector2(0f, 40f);
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private bool _hideWhenOffscreen = true;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            if (CanCreateUI())
            {
                EnsureUI();
            }

            ApplyText();
        }

        private void OnValidate()
        {
            if (CanCreateUI())
            {
                EnsureUI();
            }
            ApplyText();
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void EnsureUI()
        {
            if (_root == null)
            {
                EnsureCanvas();
                var parent = _parent != null
                    ? _parent
                    : _canvas != null ? _canvas.transform as RectTransform : null;
                if (parent == null)
                {
                    return;
                }

                var rootObj = new GameObject("ObjectLabel", typeof(RectTransform), typeof(CanvasGroup), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                _root = rootObj.GetComponent<RectTransform>();
                _root.SetParent(parent, false);
                _root.localScale = Vector3.one;
                _root.anchoredPosition = Vector2.zero;
                _root.pivot = new Vector2(0.5f, 0.5f);

                _canvasGroup = rootObj.GetComponent<CanvasGroup>();
                var layout = rootObj.GetComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.spacing = 2f;

                var fitter = rootObj.GetComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                _canvasGroup = _root.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = _root.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (_titleText == null)
            {
                _titleText = CreateText("Title", _root);
            }

            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }
        }

        private bool CanCreateUI()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var scene = gameObject.scene;
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    return false;
                }
            }
#endif
            return true;
        }

        private TextMeshProUGUI CreateText(string name, RectTransform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;

            var text = obj.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        private void ApplyText()
        {
            ApplyText(_titleText, _title, _titleSize, _titleColor, _boldTitle);
        }

        private void ApplyText(TextMeshProUGUI text, string value, float size, Color color, bool bold)
        {
            if (text == null)
            {
                return;
            }

            text.gameObject.SetActive(!string.IsNullOrWhiteSpace(value));
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (_font != null)
            {
                text.font = _font;
            }

            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.lineSpacing = _lineSpacing;
            text.textWrappingMode = TextWrappingModes.Normal;

            if (_maxWidth > 0f)
            {
                var rect = text.rectTransform;
                if (rect != null)
                {
                    var sizeDelta = rect.sizeDelta;
                    sizeDelta.x = _maxWidth;
                    rect.sizeDelta = sizeDelta;
                }
            }
        }

        private void UpdatePosition()
        {
            if (_root == null)
            {
                return;
            }

            var camera = _worldCamera != null ? _worldCamera : Camera.main;
            if (camera == null)
            {
                return;
            }

            var screen = camera.WorldToScreenPoint(transform.position + _worldOffset);
            var visible = screen.z > 0f
                && screen.x >= 0f
                && screen.x <= Screen.width
                && screen.y >= 0f
                && screen.y <= Screen.height;

            if (_hideWhenOffscreen && _canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.blocksRaycasts = visible;
                _canvasGroup.interactable = visible;
            }

            if (!visible)
            {
                return;
            }

            var parent = _root.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            var offsetScreen = screen + new Vector3(_screenOffset.x, _screenOffset.y, 0f);
            var canvas = _canvas != null ? _canvas : parent.GetComponentInParent<Canvas>();
            var camForConversion = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? camera : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, offsetScreen, camForConversion, out var localPoint);
            _root.anchoredPosition = localPoint;
        }

        private void EnsureCanvas()
        {
            if (_parent != null)
            {
                return;
            }

            if (_canvas == null)
            {
                _canvas = FindFirstObjectByType<Canvas>();
            }

            if (_canvas == null && _autoCreateCanvas)
            {
                var obj = new GameObject("ObjectLabelCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                _canvas = obj.GetComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = obj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (_parent == null && _canvas != null)
            {
                _parent = _canvas.transform as RectTransform;
            }
        }
    }
}
