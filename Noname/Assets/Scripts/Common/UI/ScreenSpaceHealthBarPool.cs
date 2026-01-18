using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Common.UI
{
    /// <summary>
    /// ScreenSpaceHealthBar 객체를 재사용하기 위한 풀링 시스템입니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScreenSpaceHealthBarPool : MonoBehaviour
    {
        [Header("Prefab")]
        /// <summary>
        /// 생성할 체력바의 프리팹입니다.
        /// </summary>
        [SerializeField] private ScreenSpaceHealthBar _prefab;

        /// <summary>
        /// 초기 생성 시 미리 만들어둘 개수입니다.
        /// </summary>
        [SerializeField] private int _prewarmCount;

        [Header("Canvas")]
        /// <summary>
        /// UI를 배치할 캔버스입니다.
        /// </summary>
        [SerializeField] private Canvas _canvas;

        /// <summary>
        /// 캔버스가 없을 경우 자동으로 생성할지 여부입니다.
        /// </summary>
        [SerializeField] private bool _autoCreateCanvas = true;

        /// <summary>
        /// 활성화된 체력바가 위치할 루트입니다.
        /// </summary>
        [SerializeField] private RectTransform _activeRoot;

        /// <summary>
        /// 비활성화된 체력바가 위치할 루트입니다.
        /// </summary>
        [SerializeField] private RectTransform _poolRoot;

        private readonly Queue<ScreenSpaceHealthBar> _pool = new();

        /// <summary>
        /// 싱글톤 인스턴스입니다.
        /// </summary>
        public static ScreenSpaceHealthBarPool Instance { get; private set; }

        /// <summary>
        /// 현재 사용 중인 캔버스입니다.
        /// </summary>
        public Canvas Canvas => _canvas;

        /// <summary>
        /// 활성 객체가 배치되는 루트 트랜스폼입니다.
        /// </summary>
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

        /// <summary>
        /// 체력바 객체를 풀에서 가져오거나 생성합니다.
        /// </summary>
        /// <returns>활성화된 ScreenSpaceHealthBar 인스턴스</returns>
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

        /// <summary>
        /// 사용이 끝난 체력바 객체를 풀에 반환합니다.
        /// </summary>
        /// <param name="bar">반환할 체력바 인스턴스</param>
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

            // 캔버스가 없으면 1920x1080 해상도 기준으로 오버레이 캔버스 생성
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
