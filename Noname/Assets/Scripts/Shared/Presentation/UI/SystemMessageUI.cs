using System.Collections.Generic;
using Noname.GameAbilitySystem;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Common.UI
{
    /// <summary>
    /// 화면 중앙 상단 등에 시스템 메시지를 띄우는 UI 매니저입니다.
    /// SystemMessageBus 이벤트를 구독하여 메시지를 표시합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SystemMessageUI : MonoBehaviour
    {
        [Header("UI")]
        /// <summary>
        /// 메시지가 생성될 부모 컨테이너입니다.
        /// </summary>
        [SerializeField] private RectTransform _container;
        
        /// <summary>
        /// 메시지에 사용할 폰트입니다.
        /// </summary>
        [SerializeField] private Font _font;
        
        /// <summary>
        /// 메시지 텍스트 크기입니다.
        /// </summary>
        [SerializeField] private int _fontSize = 24;
        
        /// <summary>
        /// 메시지 텍스트 색상입니다.
        /// </summary>
        [SerializeField] private Color _textColor = Color.white;
        
        /// <summary>
        /// 메시지 배경 색상입니다.
        /// </summary>
        [SerializeField] private Color _backgroundColor = new Color(0f, 0f, 0f, 0.6f);
        
        /// <summary>
        /// 메시지 내부 여백(padding)입니다.
        /// </summary>
        [SerializeField] private Vector2 _padding = new Vector2(10f, 4f);
        
        /// <summary>
        /// 메시지 간의 간격입니다.
        /// </summary>
        [SerializeField] private float _rowSpacing = 2f;

        [Header("Behavior")]
        /// <summary>
        /// 화면에 동시에 표시될 최대 메시지 개수입니다.
        /// </summary>
        [SerializeField] private int _maxVisible = 30;

        [Header("Auto Setup")]
        /// <summary>
        /// UI가 배치될 캔버스입니다.
        /// </summary>
        [SerializeField] private Canvas _canvas;
        
        /// <summary>
        /// 캔버스가 없을 경우 자동 생성 여부입니다.
        /// </summary>
        [SerializeField] private bool _autoCreateCanvas = true;

        private readonly List<MessageRow> _activeRows = new();
        private readonly Queue<MessageRow> _pool = new();

        private sealed class MessageRow
        {
            public GameObject Root;
            public Text Label;
            public Image Background;
            public CanvasGroup Group;
        }

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

        // Update는 개수 기반으로만 동작하므로 제거

        /// <summary>
        /// 시스템 메시지를 게시합니다.
        /// </summary>
        /// <param name="message">표시할 메시지 내용</param>
        public static void Post(string message)
        {
            SystemMessageBus.Publish(message);
        }

        private void HandleMessage(SystemMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Text))
            {
                return;
            }

            var row = GetOrCreateRow();
            row.Label.text = message.Text;

            // 배경색 적용
            if (row.Background != null)
            {
                row.Background.color = message.BackgroundColor;
            }

            if (row.Group != null)
            {
                row.Group.alpha = 1f;
            }

            // 최신 메시지를 하단에 추가 (위로 쌓임)
            row.Root.transform.SetAsLastSibling();
            _activeRows.Add(row);
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
            // 메시지 행(Row) UI 동적 생성
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
                Background = background,
                Group = group
            };
        }

        private void TrimOverflow()
        {
            // 최대 표시 개수를 초과하면 가장 오래된 메시지부터 제거 (맨 위부터)
            var max = Mathf.Max(1, _maxVisible);
            while (_activeRows.Count > max)
            {
                var row = _activeRows[0];
                _activeRows.RemoveAt(0);
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
                if (_autoCreateCanvas)
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

            // 화면 최하단 중앙에 배치 (위로 쌓이는 스택형)
            var obj = new GameObject("SystemMessageContainer", typeof(RectTransform));
            _container = obj.GetComponent<RectTransform>();
            _container.SetParent(_canvas.transform, false);
            _container.anchorMin = new Vector2(0.5f, 0f);
            _container.anchorMax = new Vector2(0.5f, 0f);
            _container.pivot = new Vector2(0.5f, 0f);
            _container.anchoredPosition = new Vector2(0f, 10f); // 하단에서 10px 위
            _container.sizeDelta = new Vector2(1200f, 0f);

            var layout = obj.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerCenter;
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
