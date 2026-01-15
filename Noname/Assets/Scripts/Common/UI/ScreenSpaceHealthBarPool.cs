using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Common.UI
{
    [DisallowMultipleComponent]
    public sealed class ScreenSpaceHealthBarPool : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private ScreenSpaceHealthBar _prefab;
        [SerializeField] private int _prewarmCount;

        [Header("Canvas")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private bool _autoCreateCanvas = true;
        [SerializeField] private RectTransform _activeRoot;
        [SerializeField] private RectTransform _poolRoot;

        private readonly Queue<ScreenSpaceHealthBar> _pool = new();

        public static ScreenSpaceHealthBarPool Instance { get; private set; }

        public Canvas Canvas => _canvas;
        public RectTransform ActiveRoot => _activeRoot != null ? _activeRoot : (_canvas != null ? _canvas.transform as RectTransform : null);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureCanvas();
            EnsureRoots();
            Prewarm();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public ScreenSpaceHealthBar Acquire()
        {
            EnsureCanvas();
            EnsureRoots();

            if (_canvas == null)
            {
                Debug.LogWarning("ScreenSpaceHealthBarPool requires a Canvas.");
                return null;
            }

            if (_prefab == null)
            {
                Debug.LogWarning("ScreenSpaceHealthBarPool prefab is missing.");
                return null;
            }

            ScreenSpaceHealthBar bar;
            if (_pool.Count > 0)
            {
                bar = _pool.Dequeue();
            }
            else
            {
                bar = Instantiate(_prefab, _activeRoot != null ? _activeRoot : transform);
            }

            if (_activeRoot != null)
            {
                bar.transform.SetParent(_activeRoot, false);
            }

            bar.gameObject.SetActive(true);
            return bar;
        }

        public void Release(ScreenSpaceHealthBar bar)
        {
            if (bar == null)
            {
                return;
            }

            bar.Unbind();
            bar.gameObject.SetActive(false);

            if (_poolRoot != null)
            {
                bar.transform.SetParent(_poolRoot, false);
            }

            _pool.Enqueue(bar);
        }

        private void Prewarm()
        {
            if (_prefab == null || _prewarmCount <= 0 || _canvas == null)
            {
                return;
            }

            for (var i = 0; i < _prewarmCount; i++)
            {
                var bar = Instantiate(_prefab, _poolRoot != null ? _poolRoot : transform);
                bar.gameObject.SetActive(false);
                _pool.Enqueue(bar);
            }
        }

        private void EnsureCanvas()
        {
            if (_canvas != null)
            {
                return;
            }

            if (!_autoCreateCanvas)
            {
                return;
            }

            var obj = new GameObject("HealthBarCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = obj.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void EnsureRoots()
        {
            if (_canvas == null)
            {
                return;
            }

            if (_activeRoot == null)
            {
                var obj = new GameObject("HealthBarActiveRoot", typeof(RectTransform));
                _activeRoot = obj.GetComponent<RectTransform>();
                _activeRoot.SetParent(_canvas.transform, false);
                _activeRoot.anchorMin = Vector2.zero;
                _activeRoot.anchorMax = Vector2.one;
                _activeRoot.sizeDelta = Vector2.zero;
            }

            if (_poolRoot == null)
            {
                var obj = new GameObject("HealthBarPoolRoot", typeof(RectTransform));
                _poolRoot = obj.GetComponent<RectTransform>();
                _poolRoot.SetParent(_canvas.transform, false);
                _poolRoot.anchorMin = Vector2.zero;
                _poolRoot.anchorMax = Vector2.one;
                _poolRoot.sizeDelta = Vector2.zero;
                _poolRoot.gameObject.SetActive(false);
            }
        }
    }
}
