using System.Collections.Generic;
using MyProject.Common.Bootstrap;
using UnityEngine;

namespace MyProject.Common.UI
{
    /// <summary>
    /// UGUI용 싱글턴 UI 매니저입니다.
    /// </summary>
    public sealed class UIManager : MonoBehaviour, IManager
    {
        private enum UILayer
        {
            Page,
            Popup,
            System
        }

        /// <summary>
        /// 현재 UIManager 인스턴스입니다.
        /// </summary>
        public static UIManager Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private UIRoot _rootPrefab;
        [SerializeField] private UIRegistry _registry;
        [SerializeField] private bool _dontDestroyOnLoad = true;

        private readonly Dictionary<UIBase, UIBase> _instances = new();
        private UIRoot _rootInstance;
        private UIPopupBase _activePopup;
        private int _pageOrder;
        private int _popupOrder;
        private int _systemOrder;
        private bool _initialized;

        /// <summary>
        /// 현재 루트 인스턴스입니다.
        /// </summary>
        public UIRoot Root => _rootInstance;

        /// <summary>
        /// 매니저를 명시적으로 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            // 싱글턴을 보장합니다.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _initialized = true;

            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            // 루트가 없으면 생성합니다.
            EnsureRoot();
        }

        private void OnDestroy()
        {
            // 싱글턴 해제를 보장합니다.
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Page UI를 엽니다.
        /// </summary>
        public T OpenPage<T>(T prefab) where T : UIPageBase
        {
            // 페이지 레이어로 열어줍니다.
            return Open(prefab, UILayer.Page) as T;
        }

        /// <summary>
        /// Page UI를 타입으로 엽니다.
        /// </summary>
        public T OpenPage<T>() where T : UIPageBase
        {
            // 등록된 프리팹을 찾아 페이지로 엽니다.
            return OpenByType<T>(UILayer.Page);
        }

        /// <summary>
        /// Popup UI를 엽니다.
        /// </summary>
        public T OpenPopup<T>(T prefab) where T : UIPopupBase
        {
            // 팝업 레이어로 열어줍니다.
            return Open(prefab, UILayer.Popup) as T;
        }

        /// <summary>
        /// Popup UI를 타입으로 엽니다.
        /// </summary>
        public T OpenPopup<T>() where T : UIPopupBase
        {
            // 등록된 프리팹을 찾아 팝업으로 엽니다.
            return OpenByType<T>(UILayer.Popup);
        }

        /// <summary>
        /// System UI를 엽니다.
        /// </summary>
        public T OpenSystem<T>(T prefab) where T : UISystemBase
        {
            // 시스템 레이어로 열어줍니다.
            return Open(prefab, UILayer.System) as T;
        }

        /// <summary>
        /// System UI를 타입으로 엽니다.
        /// </summary>
        public T OpenSystem<T>() where T : UISystemBase
        {
            // 등록된 프리팹을 찾아 시스템으로 엽니다.
            return OpenByType<T>(UILayer.System);
        }

        /// <summary>
        /// UI 인스턴스를 닫습니다.
        /// </summary>
        public void Close(UIBase instance)
        {
            if (instance == null)
            {
                return;
            }

            // 비활성화만 하고 인스턴스는 보관합니다.
            instance.gameObject.SetActive(false);

            if (_activePopup == instance)
            {
                _activePopup = null;
            }
        }

        /// <summary>
        /// 현재 활성화된 팝업을 닫습니다.
        /// </summary>
        public void ClosePopup()
        {
            if (_activePopup == null)
            {
                return;
            }

            // 활성 팝업만 비활성화합니다.
            _activePopup.gameObject.SetActive(false);
            _activePopup = null;
        }

        private T OpenByType<T>(UILayer layer) where T : UIBase
        {
            if (_registry == null)
            {
                Debug.LogWarning($"UIRegistry가 설정되지 않았습니다: {typeof(T).Name}");
                return null;
            }

            if (!_registry.TryGetPrefab<T>(out var prefab) || prefab == null)
            {
                Debug.LogWarning($"UI 프리팹을 찾을 수 없습니다: {typeof(T).Name}");
                return null;
            }

            return Open(prefab, layer) as T;
        }

        private UIBase Open(UIBase prefab, UILayer layer)
        {
            if (prefab == null)
            {
                return null;
            }

            EnsureRoot();
            var root = ResolveRoot(layer);
            if (root == null)
            {
                return null;
            }

            if (!ValidateLayer(prefab, layer))
            {
                return null;
            }

            if (!_instances.TryGetValue(prefab, out var instance) || instance == null)
            {
                // 최초 생성 시 프리팹을 인스턴스화합니다.
                instance = Instantiate(prefab, root);
                _instances[prefab] = instance;
            }
            else
            {
                // 재사용 시 부모만 갱신합니다.
                instance.transform.SetParent(root, false);
            }

            if (layer == UILayer.Popup && _activePopup != null && _activePopup != instance)
            {
                _activePopup.gameObject.SetActive(false);
            }

            instance.gameObject.SetActive(true);
            instance.transform.SetAsLastSibling();
            ApplySorting(instance, layer);

            if (layer == UILayer.Popup)
            {
                _activePopup = instance as UIPopupBase;
            }

            return instance;
        }

        private void EnsureRoot()
        {
            if (_rootInstance != null)
            {
                return;
            }

            // 씬에 이미 있는 루트를 우선 사용합니다.
            _rootInstance = FindFirstObjectByType<UIRoot>();
            if (_rootInstance == null && _rootPrefab != null)
            {
                _rootInstance = Instantiate(_rootPrefab);
            }

            if (_rootInstance != null && _dontDestroyOnLoad)
            {
                _rootInstance.transform.position = Vector3.zero;
                DontDestroyOnLoad(_rootInstance.gameObject);
            }
        }

        private RectTransform ResolveRoot(UILayer layer)
        {
            if (_rootInstance == null)
            {
                return null;
            }

            return layer switch
            {
                UILayer.Page => _rootInstance.PageRoot,
                UILayer.Popup => _rootInstance.PopupRoot,
                UILayer.System => _rootInstance.SystemRoot,
                _ => null
            };
        }

        private Canvas ResolveCanvas(UILayer layer)
        {
            if (_rootInstance == null)
            {
                return null;
            }

            return layer switch
            {
                UILayer.Page => _rootInstance.PageCanvas,
                UILayer.Popup => _rootInstance.PopupCanvas,
                UILayer.System => _rootInstance.SystemCanvas,
                _ => null
            };
        }

        private void ApplySorting(UIBase instance, UILayer layer)
        {
            var canvas = instance.GetComponent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            // 루트 캔버스의 기준 오더에 누적 오더를 더합니다.
            var rootCanvas = ResolveCanvas(layer);
            var baseOrder = rootCanvas != null ? rootCanvas.sortingOrder : 0;
            var order = NextOrder(layer);

            canvas.overrideSorting = true;
            canvas.sortingOrder = baseOrder + order;
        }

        private int NextOrder(UILayer layer)
        {
            return layer switch
            {
                UILayer.Page => ++_pageOrder,
                UILayer.Popup => ++_popupOrder,
                UILayer.System => ++_systemOrder,
                _ => 0
            };
        }

        private bool ValidateLayer(UIBase prefab, UILayer layer)
        {
            var isPage = prefab is UIPageBase;
            var isPopup = prefab is UIPopupBase;
            var isSystem = prefab is UISystemBase;

            var valid = layer switch
            {
                UILayer.Page => isPage,
                UILayer.Popup => isPopup,
                UILayer.System => isSystem,
                _ => false
            };

            if (valid)
            {
                return true;
            }

            Debug.LogWarning($"{prefab.name} UI는 {layer} 레이어에 맞지 않습니다.");
            return false;
        }
    }
}
