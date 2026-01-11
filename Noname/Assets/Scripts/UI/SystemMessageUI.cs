using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;
using UnityEngine.UI;

namespace MergeGame.UI
{
    [DisallowMultipleComponent]
    public sealed class SystemMessageUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private RectTransform _container;
        [SerializeField] private Font _font;
        [SerializeField] private int _fontSize = 16;
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private Color _backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Vector2 _padding = new Vector2(10f, 4f);
        [SerializeField] private float _rowSpacing = 2f;

        [Header("Behavior")]
        [SerializeField] private float _messageDuration = 2f;
        [SerializeField] private float _fadeDuration = 0.25f;
        [SerializeField] private int _maxVisible = 5;
        [SerializeField] private bool _useUnscaledTime = true;

        [Header("Auto Setup")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private bool _autoCreateCanvas = true;

        private readonly List<MessageRow> _activeRows = new();
        private readonly Queue<MessageRow> _pool = new();

        private sealed class MessageRow
        {
            public GameObject Root;
            public Text Label;
            public CanvasGroup Group;
            public float StartTime;
            public float EndTime;
        }

        private float Now => _useUnscaledTime ? Time.unscaledTime : Time.time;

        private void Awake()
        {
            EnsureCanvas();
            EnsureContainer();
            EnsureFont();
        }

        private void OnEnable()
        {
            SystemMessageBus.MessagePublished += HandleMessage;
        }

        private void OnDisable()
        {
            SystemMessageBus.MessagePublished -= HandleMessage;
        }

        private void Update()
        {
            if (_activeRows.Count == 0)
            {
                return;
            }

            var now = Now;
            for (var i = _activeRows.Count - 1; i >= 0; i--)
            {
                var row = _activeRows[i];
                if (row == null)
                {
                    continue;
                }

                var timeLeft = row.EndTime - now;
                if (timeLeft <= -_fadeDuration)
                {
                    RecycleRow(row);
                    _activeRows.RemoveAt(i);
                    continue;
                }

                if (row.Group == null)
                {
                    continue;
                }

                var alpha = 1f;
                if (_fadeDuration > 0f && timeLeft < _fadeDuration)
                {
                    alpha = Mathf.Clamp01(timeLeft / _fadeDuration);
                }

                row.Group.alpha = alpha;
            }
        }

        public static void Post(string message)
        {
            SystemMessageBus.Publish(message);
        }

        private void HandleMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var row = GetOrCreateRow();
            row.Label.text = message;
            row.StartTime = Now;
            row.EndTime = row.StartTime + Mathf.Max(0.1f, _messageDuration);
            if (row.Group != null)
            {
                row.Group.alpha = 1f;
            }

            row.Root.transform.SetAsFirstSibling();
            _activeRows.Insert(0, row);
            TrimOverflow();
        }

        private MessageRow GetOrCreateRow()
        {
            if (_pool.Count > 0)
            {
                var pooled = _pool.Dequeue();
                pooled.Root.SetActive(true);
                return pooled;
            }

            return CreateRow();
        }

        private MessageRow CreateRow()
        {
            var root = new GameObject("SystemMessageRow", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(_container, false);
            rect.localScale = Vector3.one;
            rect.sizeDelta = Vector2.zero;

            var background = root.AddComponent<Image>();
            background.color = _backgroundColor;

            var layout = root.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(
                Mathf.RoundToInt(_padding.x),
                Mathf.RoundToInt(_padding.x),
                Mathf.RoundToInt(_padding.y),
                Mathf.RoundToInt(_padding.y));
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            var element = root.AddComponent<LayoutElement>();
            element.preferredHeight = _fontSize + _padding.y * 2f;

            var labelObj = new GameObject("Label", typeof(RectTransform));
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.SetParent(rect, false);
            labelRect.localScale = Vector3.one;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            var text = labelObj.AddComponent<Text>();
            text.font = _font;
            text.fontSize = _fontSize;
            text.color = _textColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var group = root.AddComponent<CanvasGroup>();

            return new MessageRow
            {
                Root = root,
                Label = text,
                Group = group
            };
        }

        private void TrimOverflow()
        {
            var max = Mathf.Max(1, _maxVisible);
            while (_activeRows.Count > max)
            {
                var row = _activeRows[_activeRows.Count - 1];
                _activeRows.RemoveAt(_activeRows.Count - 1);
                RecycleRow(row);
            }
        }

        private void RecycleRow(MessageRow row)
        {
            if (row == null || row.Root == null)
            {
                return;
            }

            row.Root.SetActive(false);
            _pool.Enqueue(row);
        }

        private void EnsureCanvas()
        {
            if (_container != null)
            {
                return;
            }

            if (_canvas == null)
            {
                _canvas = FindFirstObjectByType<Canvas>();
            }

            if (_canvas == null && _autoCreateCanvas)
            {
                var obj = new GameObject("SystemMessageCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                _canvas = obj.GetComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = obj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        private void EnsureContainer()
        {
            if (_container != null)
            {
                return;
            }

            if (_canvas == null)
            {
                return;
            }

            var obj = new GameObject("SystemMessageContainer", typeof(RectTransform));
            _container = obj.GetComponent<RectTransform>();
            _container.SetParent(_canvas.transform, false);
            _container.anchorMin = new Vector2(0.5f, 1f);
            _container.anchorMax = new Vector2(0.5f, 1f);
            _container.pivot = new Vector2(0.5f, 1f);
            _container.anchoredPosition = new Vector2(0f, -16f);
            _container.sizeDelta = new Vector2(600f, 0f);

            var layout = obj.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = _rowSpacing;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var fitter = obj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void EnsureFont()
        {
            if (_font != null)
            {
                return;
            }

            var text = GetComponentInChildren<Text>(true);
            if (text != null && text.font != null)
            {
                _font = text.font;
                return;
            }

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
